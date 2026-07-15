# Block 0a — Solution- und Aspire-Fundament

## Kontext

Das Repository `01 Repos/MyMusic/` enthält bisher nur die Arbeitsstruktur (CLAUDE.md,
README.md, TASK.md, leere Ordner `src/`, `tests/`, `docs/`). Die fachliche Planung im Wiki
ist weit fortgeschritten, aber es existiert keine Zeile Anwendungscode.

Block 0 der TASK.md („Walking Skeleton") wurde auf Wunsch in drei Teilblöcke zerlegt, damit
jeder Schritt für sich prüfbar und committebar ist. **Dieser Plan deckt ausschließlich 0a ab**:
das Solution- und Orchestrierungs-Fundament. Ziel ist ein Stand, bei dem alle Container über
den AppHost in der dokumentierten Reihenfolge hochkommen, der Migrator-Job durchläuft und
sich beendet, und die leere API einen Health-Check beantwortet.

Nicht Teil von 0a: CQRS-Framework, Repository, ExceptionManager und der Auth-Smoke-Test (0b);
Angular-Workspace (0c); jede Fachlogik (ab Block 2).

## Getroffene Entscheidungen

| Thema | Entscheidung |
|---|---|
| Layer-Projekte | `MyMusic.Domain`, `MyMusic.Application`, `MyMusic.Infrastructure`, `MyMusic.Api` |
| Testprojekte | `MyMusic.Domain.Tests`, `MyMusic.Application.Tests`, `MyMusic.Api.Tests`, `MyMusic.IntegrationTests` |
| DB-Rechte | Berechtigungskonzept wird in 0a umgesetzt (eigene DML-Rolle für die API) |
| Auth in 0a | Nur Keycloak-Container + Realm-Import; JWT-Verdrahtung und Smoke-Test in 0b |

## Zu prüfende Umgebung (bereits verifiziert)

- .NET SDK 10.0.204 vorhanden.
- Docker 29.4.1, Daemon erreichbar.
- Node v22.16.0 vorhanden (erst für 0c relevant).
- **`aspire` CLI ist nicht installiert** — siehe Schritt 1.

## Vorgeschlagene Schritte

### 1. Aspire-CLI und Solution-Gerüst

- `aspire` CLI global installieren (`dotnet tool install -g Aspire.Cli`). Alternative ohne CLI:
  `dotnet new install Aspire.ProjectTemplates` und Projekte einzeln anlegen. Empfehlung: CLI,
  da sie ab Aspire 13 der dokumentierte Weg ist (`aspire init` / `aspire new`).
- Solution `MyMusic.sln` im Repo-Root anlegen.
- Aspire-Zielversion: aktuelle 13.x (zum Planungszeitpunkt 13.4/13.5) — exakte Version beim
  Anlegen ermitteln und in allen Projekten einheitlich pinnen.

### 2. Projekte anlegen

Struktur gemäß CLAUDE.md §4.1:

```
src/
├── MyMusic.AppHost/          # Aspire.AppHost.Sdk/13.x (neues SDK-Format ab Aspire 13)
├── MyMusic.ServiceDefaults/  # OpenTelemetry, Health Checks, Service Discovery
├── MyMusic.Migrator/         # Konsolen-Projekt, einmaliger Job
├── MyMusic.Domain/           # leer in 0a
├── MyMusic.Application/      # leer in 0a
├── MyMusic.Infrastructure/   # nur MyMusicDbContext (leer, ohne DbSets)
└── MyMusic.Api/              # Minimal API, nur Health-Check
tests/
├── MyMusic.Domain.Tests/
├── MyMusic.Application.Tests/
├── MyMusic.Api.Tests/
└── MyMusic.IntegrationTests/
```

Referenzrichtung strikt von außen nach innen (CLAUDE.md §4.2). `MyMusic.Api` und
`MyMusic.Migrator` referenzieren `MyMusic.ServiceDefaults` und rufen `AddServiceDefaults()` auf.

Der `MyMusicDbContext` kommt in 0a bewusst leer (ohne DbSets) ins Infrastructure-Projekt —
der Migrator braucht einen Kontext, um die Kette zu beweisen. Entitäten und `IRepository<T>`
folgen in 0b.

### 3. AppHost: Ressourcen und Boot-Reihenfolge

Gemäß `wiki/architektur/aspire-orchestrierung.md`:

- `AddPostgres()` + `AddDatabase()`, mit `WithDataVolume()` (Container sonst ephemer).
- `AddSeq()` mit Datenvolume und EULA-Akzeptanz.
- Keycloak per `AddContainer()` mit offiziellem Image 26.5, Start mit `--import-realm`,
  Realm-JSON aus `/keycloak/` gemountet, HTTP-Health-Check auf `/health/ready`.
- Admin-Credentials als Aspire-Parameter mit `secret: true`, Werte über
  `dotnet user-secrets` im AppHost — nie im Repository.
- `WaitFor`-Kette: Migrator → WaitFor(PostgreSQL); API → WaitFor(Migrator), WaitFor(Keycloak).

Hinweis: Es gibt eine offizielle `Aspire.Hosting.Keycloak`-Integration. Das Wiki hat sich
bewusst für `AddContainer()` entschieden; dieser Plan folgt dem Wiki.

### 4. Keycloak-Realm als JSON

Versioniert unter `/keycloak/mymusic-realm.json`:

- Realm `mymusic`.
- Public Client für Angular (Authorization Code + PKCE, kein Client Secret).
- Realm Roles `User` und `Admin`.
- Token-Lebensdauern gemäß Sicherheitskonzept: Access 5 min, Refresh 30 min (Sliding),
  SSO Session Max 8 h.

Keine Benutzer, keine echten Zugangsdaten in der Datei.

### 5. DB-Berechtigungskonzept

Umsetzung von „Migrator = DDL+DML, API = nur DML" (`aspire-orchestrierung.md`):

- Init-Skript via `WithInitFiles()` am PostgreSQL-Resource. Als **`.sh`-Skript**, nicht `.sql`,
  damit das API-Passwort aus einer Umgebungsvariablen gelesen werden kann statt im
  Repository zu stehen.
- Skript legt Rolle `mymusic_api` mit `LOGIN` an, gibt `CONNECT` auf die Datenbank und
  `USAGE` auf Schema `public`.
- **Entscheidend**: `ALTER DEFAULT PRIVILEGES FOR ROLE <migrator-rolle> IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO mymusic_api;` plus `USAGE, SELECT ON
  SEQUENCES`. Ohne Default Privileges hätte die API keinerlei Rechte auf die Tabellen, die
  der Migrator erst später anlegt.
- API-Passwort als Aspire-Parameter (`secret: true`) über User Secrets; der AppHost baut den
  API-Connection-String zusammen und übergibt ihn per `WithEnvironment()`.
- Der Migrator behält den privilegierten, von Aspire injizierten Connection-String.

**Stolperstein, der in die Doku gehört**: Das PostgreSQL-Image führt Init-Skripte nur aus,
wenn das Datenverzeichnis leer ist. In Kombination mit `WithDataVolume()` läuft das Skript
also genau einmal. Nach Änderungen am Skript muss das Volume verworfen werden — das wird in
der README als Entwicklerhinweis festgehalten.

### 6. Serilog (offener Punkt aus dem Wiki — Vorschlag)

`wiki/tech-stack/serilog-seq.md` lässt die Sink-Konfiguration ausdrücklich offen. Die
Recherche hat ergeben, dass die offizielle Aspire-Client-Integration (`Aspire.Seq` /
`AddSeqEndpoint()`) Logs per **OpenTelemetry-OTLP** an Seq exportiert — also nicht über
Serilog-Sinks. Damit gibt es zwei Wege, und sie sollten nicht vermischt werden.

**Vorschlag für 0a**: Serilog als Logging-Provider mit `Serilog.AspNetCore`, Sinks nach
Console und Seq (`Serilog.Sinks.Console`, `Serilog.Sinks.Seq`). Die Seq-URL kommt aus der von
Aspire injizierten Verbindungsinformation. `UseSerilogRequestLogging()` mit Ausschluss der
Header `Authorization` und `Cookie` (Sicherheitskonzept §5.4). Traces und Metriken laufen
unverändert über OpenTelemetry aus den ServiceDefaults ans Aspire-Dashboard.

Begründung: Ein Pfad statt zwei, entspricht dem Wiki wörtlich („Serilog mit Seq"), und der
geforderte Header-Ausschluss braucht `Serilog.AspNetCore` ohnehin. Bewusster Nachteil: Seq
sieht keine OTel-Traces, es gibt dort also keine Log-Trace-Korrelation. Falls die später
gewünscht ist, wäre der Wechsel auf `Serilog.Sinks.OpenTelemetry` bzw. `Aspire.Seq` eine
eigene Entscheidung samt Wiki-Update.

Die getroffene Entscheidung wird im Wiki nachgetragen und als ADR festgehalten.

### 7. Migrator

- Konsolen-Projekt, ruft `AddServiceDefaults()` auf, führt `Database.MigrateAsync()` aus und
  beendet sich mit Exit-Code 0.
- **In 0a wird bewusst keine Migration erzeugt.** Ohne Entitäten legt `MigrateAsync()` nur die
  Tabelle `__EFMigrationsHistory` an — das genügt, um Verbindung und DDL-Rechte zu beweisen.
  Die erste echte Migration ist laut CLAUDE.md §6 ein eigener, separat freizugebender Schritt
  und gehört zum Genre-Slice.

### 8. API

- Minimal API, `AddServiceDefaults()`, `MapDefaultEndpoints()` für die Health-Checks.
- Health-Check bleibt anonym. Keine JWT-Verdrahtung, keine fachlichen Endpunkte (das ist 0b).

### 9. Dokumentation

- ADR `docs/adr/0001-projektnamen-onion-layer.md` — die vier Layer-Namen plus Testprojekt-Zuschnitt.
- ADR `docs/adr/0002-serilog-sink-konfiguration.md` — Entscheidung aus Schritt 6.
- `README.md` um Abschnitt „Lokale Entwicklung" ergänzen (Voraussetzungen, User-Secrets-Setup,
  Start über AppHost, Volume-Hinweis aus Schritt 5).
- `TASK.md` nach Abschluss aktualisieren (Block 0a erledigt, Zerlegung 0a/0b/0c abbilden).
- Wiki-Korrekturen siehe Abschnitt „Zu meldende Wiki-Abweichungen".

## Benötigte NuGet-Pakete (Freigabe erforderlich, CLAUDE.md §12)

| Paket | Projekt | Zweck | Alternative |
|---|---|---|---|
| `Aspire.Hosting.PostgreSQL` | AppHost | PostgreSQL als Aspire-Ressource | `AddContainer()` von Hand — mehr Eigenbau, keine Service-Discovery |
| `Aspire.Hosting.Seq` | AppHost | Seq als Aspire-Ressource | `AddContainer()` von Hand |
| `Serilog.AspNetCore` | Api | Serilog als Provider + Request-Logging mit Header-Ausschluss | `Microsoft.Extensions.Logging` allein — erfüllt Wiki-Vorgabe nicht |
| `Serilog.Sinks.Seq` | Api | Log-Versand an Seq | `Aspire.Seq` (OTLP-Weg, siehe Schritt 6) |
| `Serilog.Sinks.Console` | Api | Startup-Logs im Aspire-Dashboard | nur Seq-Sink — frühe Logs gingen verloren |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Infrastructure | EF-Core-Provider | keine (gesetzt durch Tech-Stack) |
| `Microsoft.EntityFrameworkCore.Design` | Migrator | Migrations-Tooling | keine |
| `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk` | Testprojekte | Test-Framework | keine (gesetzt durch Tech-Stack) |
| `Aspire.Hosting.Testing` | IntegrationTests | AppHost in Tests starten | manuelles Container-Setup |

Dazu das globale Tool `Aspire.Cli` (Schritt 1).

## Zu meldende Wiki-Abweichungen

Diese Punkte werden **nicht stillschweigend aufgelöst**, sondern nach der Umsetzung als
Korrektur im Wiki vorgeschlagen:

1. **Widerspruch DB-Rechte** — `aspire-orchestrierung.md:62` sagt, die API bekomme ihren
   Connection-String automatisch von Aspire injiziert; zehn Zeilen später fordert das
   Berechtigungskonzept, dass die API keine DDL-Rechte hat. Der automatisch injizierte String
   gehört dem Superuser. Nach Umsetzung von Schritt 5 muss die Service-Discovery-Tabelle auf
   „manuell zusammengesetzt aus Aspire-Parameter" korrigiert werden.
2. **`AddNpmApp()` existiert in Aspire 13 nicht mehr** — `aspire-orchestrierung.md:24` nennt
   es für das Angular-Frontend. Es wurde durch `AddJavaScriptApp()` ersetzt. Betrifft 0c,
   sollte aber jetzt schon korrigiert werden.
3. **Serilog-Sink-Konfiguration** — `serilog-seq.md:11` markiert das Thema selbst als offen.
   Wird durch Schritt 6 geschlossen.
4. **Aspire-Version** — Wiki nennt „Aspire 13"; aktuell ist 13.4/13.5. Die konkret verwendete
   Minor-Version sollte im Wiki festgehalten werden, da laut `offene-themen.md` Breaking
   Changes zwischen Minor-Versionen erwartet werden.

## Verifikation

1. `dotnet build` über die Solution — fehlerfrei.
2. `dotnet test` — läuft durch (die Unit-Test-Projekte sind in 0a noch weitgehend leer).
3. AppHost starten (`aspire run` bzw. `dotnet run --project src/MyMusic.AppHost`) und im
   Dashboard prüfen:
   - PostgreSQL, Seq, Keycloak erreichen den Zustand „Running"/„Healthy".
   - Keycloak meldet `/health/ready`; der Realm `mymusic` ist in der Admin-UI vorhanden.
   - Der Migrator läuft **einmal**, beendet sich mit Exit-Code 0 und bleibt beendet.
   - Die API startet erst nach dem Migrator und antwortet auf `/health` mit 200.
4. Log-Nachweis: Ein Start-Log der API ist in Seq sichtbar; im Request-Log taucht **kein**
   `Authorization`-Header auf.
5. Rechte-Nachweis (der eigentliche Punkt von Schritt 5): mit dem API-Connection-String
   `psql` verbinden und ein `CREATE TABLE` versuchen — muss mit „permission denied"
   scheitern. Ein `SELECT` auf `__EFMigrationsHistory` muss funktionieren.
6. Neustart-Nachweis: AppHost stoppen und erneut starten — die PostgreSQL-Daten überleben
   (Volume greift), der Migrator läuft erneut ohne Fehler durch.

Punkt 5 wird zusätzlich als automatisierter Integrationstest in `MyMusic.IntegrationTests`
festgehalten, damit die Rechtetrennung nicht später unbemerkt aufweicht.

## Risiken und offene Punkte

- **Aspire 13 ist neu und bewegt sich schnell** (13.0 → 13.5 mit Breaking Changes, u. a. der
  Wegfall von `AddNpmApp()`). Die konkreten API-Aufrufe in diesem Plan sind aus der aktuellen
  Doku abgeleitet, aber nicht auf dieser Maschine erprobt. Abweichungen beim Umsetzen sind
  wahrscheinlich und werden gemeldet, nicht umgangen.
- **Init-Skript läuft nur bei leerem Datenverzeichnis** (siehe Schritt 5) — der häufigste
  Grund, warum die Rechtetrennung „plötzlich nicht mehr geht".
- **Keycloak-Realm-Import und Health-Check** sind erfahrungsgemäß der fragilste Teil der
  Kette; hier ist mit Nacharbeit zu rechnen.
- Production-TLS und Production-Secret-Management bleiben laut Wiki offen und sind bewusst
  nicht Teil dieses Plans.
