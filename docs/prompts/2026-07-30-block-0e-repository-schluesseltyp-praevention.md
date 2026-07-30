# Block 0e — Generischer Repository-Schlüsseltyp und Prozess-Absicherung

## Kontext

Am 29.07.2026 wurde Block 2 (Slice "Genre") **vollständig implementiert**.
Nach anschließender Prüfung stellte sich heraus, dass dabei durchgängig
Wiki-Vorgaben missachtet wurden — der Nutzer hat die komplette
Implementierung daraufhin bewusst wieder gelöscht. Im Repository blieben
dadurch kein Commit und nur leere Verzeichnis-Stubs zurück (Git trackt leere
Verzeichnisse nicht, daher unsichtbar in `git status`).

**Wichtig zur Faktenlage**: Da nichts committet wurde, lässt sich der Inhalt
der gelöschten Implementierung nicht mehr aus dem Repository rekonstruieren.
Die folgenden drei Fehler sind die direkte Angabe des Nutzers nach eigener
Prüfung, nicht unabhängig am Code verifizierbar:

1. **ID-Typ**: Alle Entitäten wurden mit `Guid` angelegt. Das ist falsch —
   nur `user_id` darf `Guid` sein, alle Stammdaten (Genre, Country, Label,
   Artist, Record, RecordTrack) müssen laut Wiki (`datenbank/tabellenschema.md`)
   `int` (`INTEGER GENERATED ALWAYS AS IDENTITY`) verwenden. Dieser Fehler
   lässt sich unabhängig am aktuellen Code erklären: Das seit Block 0b
   bestehende `IRepository<TEntity>` und `ExceptionManager.NotFound` sind
   hart auf `Guid` fixiert — wer mit diesem Baustein arbeitet, landet ohne
   Gegenmaßnahme zwangsläufig bei `Guid` für jede Entität. Das ist der
   technische Kern des Problems und unabhängig belegt.
2. **Keine DomainExceptions angelegt**: Laut Nutzerangabe wurden keine
   domänenspezifischen Exceptions erzeugt. Das Wiki verlangt dafür keine
   eigene `DomainException`-Basisklasse pro Entität, sondern durchgängige
   Nutzung des bereits vorhandenen, generischen `ExceptionManager`
   (`NotFoundException`/`ConflictException`/`ValidationException`) — dieses
   Muster wurde offenbar nicht (vollständig) angewendet.
3. **Keine Verzeichnisse im API-Layer angelegt**: Laut Nutzerangabe wurde die
   vorgeschriebene Struktur `Endpoints/{Kategorie}/{Entität}/`
   (`architektur/application-layer.md`/`tech-stack/minimal-api.md`) nicht
   verwendet.

Bemerkenswert: `architektur/application-layer.md` — wo die Verzeichnisstruktur
für den Application-Layer beschrieben ist — stand bereits **vor** diesem
Vorfall in der Pflicht-Leseliste des `implementer`-Subagenten. Fehler 2 und 3
sind also nicht allein durch fehlendes Wissen erklärbar, sondern deuten
darauf hin, dass eine reine Leseliste am Anfang der Arbeit nicht reicht, wenn
es keine Prüfung am Ende gibt, ob das Ergebnis den gelesenen Vorgaben
tatsächlich entspricht.

Die eigentliche Prozess-Lücke bleibt davon unberührt: Bei den abgeschlossenen
Blöcken 0a/0b/CI-Gate wurde immer ein konkreter, Wiki-referenzierter
Arbeits-Prompt erstellt, freigegeben und unter `docs/prompts/` archiviert,
bevor implementiert wurde. Für Genre fehlt das komplett. Zusätzlich
referenzierte die Pflicht-Leseliste im `implementer`-Subagenten
`datenbank/tabellenschema.md` (wo der ID-Typ steht) nicht, obwohl CLAUDE.md
§6 diese Seite als verbindliche Quelle nennt.

Nebenbefund: Die Wiki-Seite `tech-stack/minimal-api.md` behauptet fälschlich,
`MeEndpoints.cs` sei bereits verschoben worden — das Wiki wurde während des
Versuchs so aktualisiert, als wäre die neue Struktur bereits produktiv,
obwohl die gesamte Implementierung später verworfen wurde.

**Ziel dieses Blocks**: Nur Prävention (Technik-Fix + Prozess-Absicherung).
Die eigentliche Genre-Implementierung ist **nicht** Teil dieses Prompts und
folgt später als eigener, separat freigegebener Arbeits-Prompt (weiterhin
Block 2 der TASK.md).

## Freigegebener Umfang

### A. Generischer Schlüsseltyp im Repository

`IRepository<TEntity>` → `IRepository<TEntity, TKey>` (zweiter Typparameter,
`where TKey : notnull`; kein Marker-Interface, keine separaten Repositories
je Schlüsseltyp):

- `src/MyMusic.Domain/Contracts/Repository/IRepository.cs`
- `src/MyMusic.Infrastructure/Persistence/Repositories/Repository.cs`
- `src/MyMusic.Application/Common/Exceptions/ExceptionManager/ExceptionManager.cs`:
  `NotFound(string, Guid)` → generisch `NotFound<TId>(string, TId)`.

Betroffene Bestandstests:
- `tests/MyMusic.Infrastructure.Tests/Persistence/Repositories/RepositoryTests.cs`
  (`Repository<TestEntity>` → `Repository<TestEntity, Guid>`).
- `tests/MyMusic.Application.Tests/Common/Exceptions/ExceptionManagerTests.cs`
  (zusätzlicher `int`-Testfall für `NotFound<TId>`).

Konsequenz-Updates: `CLAUDE.md` §4.2/§10.2, `README.md` (Signatur-Erwähnung).

`IRepository<T>` bleibt weiterhin nicht per DI verdrahtet (unverändert
gegenüber der 0b-Entscheidung, folgt mit der echten Genre-Implementierung).

### B. ADR 0006 — ID-Typ-Strategie

Neue Datei `docs/adr/0006-repository-id-typ-strategie.md`: Guid nur für
`user_id`, `int`/IDENTITY für alle Stammdaten (siehe Entscheidungsinhalt im
Plan-Dokument dieses Blocks).

### C. Subagent-Konfiguration

- `.claude/agents/implementer.md`: Pflicht-Leseliste um
  `entwicklung/vorgaben-checkliste-neue-entitaet.md`,
  `architektur/fehler-und-ausnahmekonzept.md`, `datenbank/er-modell.md`,
  `datenbank/tabellenschema.md` und die entitätsspezifische
  `domain/{entität}.md`-Seite erweitert; zusätzlich neue Abschlussprüfung
  (Verzeichnisstruktur- und Exception-Konformität explizit gegen die
  Wiki-Seiten abgleichen, nicht nur zu Beginn lesen).
- `.claude/agents/planner.md`: Pflichtabschnitt „Wiki-Referenzen" im
  Arbeits-Prompt-Format ergänzt.
- `.claude/agents/reviewer.md`: drei explizite Prüfpunkte ergänzt —
  Datenmodell-, Verzeichnisstruktur- und Exception-Konformität.

### D. Neue gebündelte Wiki-Checkliste

`wiki/entwicklung/vorgaben-checkliste-neue-entitaet.md` (neu), plus
Pflichtaktualisierung `wiki/index.md` und `wiki/log.md`.

### E. Wiki-Korrekturen

- `wiki/tech-stack/minimal-api.md`: falsche Behauptung zur bereits erfolgten
  `MeEndpoints.cs`-Verschiebung richtiggestellt.
- `offene-themen.md`: falscher Pfadverweis auf `wiki/user-stories/genre.md`
  korrigiert zu `wiki/user-stories/user-stories-genre.md`.

### F. Aufräumen leerer Verzeichnis-Stubs

Reine Dateisystem-Löschung (kein `git clean`) der sieben leeren, nie von Git
getrackten Verzeichnis-Wurzeln aus der gelöschten Genre-Implementierung
(u. a. `src/MyMusic.Domain/DomainModels/`,
`src/MyMusic.Application/Features/Stammdaten/`,
`src/MyMusic.Api/Endpoints/Stammdaten/`,
`src/MyMusic.Api/Endpoints/System/CurrentUser/`,
`src/MyMusic.Infrastructure/Migrations/` und die zugehörigen Test-Ordner).

Der leere, lokale Branch `block-2-genre-slice` bleibt unangetastet
(Branch-Löschung ist laut CLAUDE.md ausnahmslos verboten).

### G. TASK.md

Neuer Block „0e. Technische Vorbereitung: generischer
Repository-Schlüsseltyp & Prozess-Absicherung" zwischen 0d und Abschnitt 1.

## Nicht-Ziele

- Genre-Slice-Implementierung selbst (Domain-Entität, Commands/Queries,
  Endpoints, Migration, Angular-Feature) — eigener, separat freigegebener
  Arbeits-Prompt.
- Nachträgliche Korrektur historischer `docs/prompts/*`-Archive
  (Zeitpunkt-Snapshots, keine lebenden Dokumente).
- Beiläufige, nicht faktisch falsche `IRepository<T>`-Prosa-Erwähnungen in
  `repository-pattern.md`/`application-layer.md` — bei nächster inhaltlicher
  Änderung dieser Seiten mitziehen, nicht separat jagen.

## Betroffene Dateien/Module

Siehe Aufzählung A–G oben; vollständige Liste im Plan-Dokument dieses
Blocks (`C:\Users\A-v-L\.claude\plans\es-gibt-hier-im-witty-widget.md` zum
Zeitpunkt der Freigabe).

## Sicherheitsanforderungen

Keine — reine Code-Contract-, Dokumentations- und Prozessänderung, keine
Migration, kein Datenbankzugriff, keine Secrets betroffen.

## Verifikation

- `dotnet build` für die gesamte Solution.
- `dotnet test` für `MyMusic.Domain.Tests`, `MyMusic.Infrastructure.Tests`,
  `MyMusic.Application.Tests`, `MyMusic.Api.Tests` (insbesondere
  `RepositoryTests` und `ExceptionManagerTests` inkl. neuem `int`-Testfall).
- `dotnet format --verify-no-changes`, Zeilenlängen-Check (CLAUDE.md §11).
- `git diff --check`.
- Verzeichnislisting bestätigt: die 7 leeren Stub-Wurzeln existieren nicht
  mehr.
- Wiki-Links in der neuen Checkliste gegen tatsächliche Dateinamen unter
  `wiki/` geprüft; `wiki/index.md`/`log.md` aktualisiert.

## Abnahmekriterien

- `dotnet build`/`dotnet test` grün mit angepasster Signatur; ADR 0006
  vorhanden; `implementer.md` enthält Pflicht-Leseliste inkl.
  Datenbank-Seiten UND die neue Abschlussprüfung; `planner.md` verlangt den
  Abschnitt „Wiki-Referenzen"; `reviewer.md` enthält alle drei
  Konformitäts-Prüfpunkte; keine leeren Verzeichnis-Stubs mehr vorhanden;
  Wiki-Korrekturen umgesetzt.

## Risiken und offene Punkte

- `TId`-Constraint bei `ExceptionManager.NotFound<TId>`: bewusst kein
  Constraint (jeder Typ hat `ToString()`); geringe Relevanz.
- Kategorisierung der neuen Wiki-Checkliste (`entwicklung/` vs.
  `architektur/`) beim tatsächlichen Anlegen im Wiki nochmal bestätigen
  lassen, wie es der Wiki-eigene Workflow verlangt.
- Da die gelöschte Genre-Implementierung nicht mehr im Repository vorliegt,
  beruhen Fehler 2 und 3 (fehlende DomainExceptions, fehlende
  API-Verzeichnisse) auf der Angabe des Nutzers und sind nicht unabhängig am
  Code nachprüfbar.
