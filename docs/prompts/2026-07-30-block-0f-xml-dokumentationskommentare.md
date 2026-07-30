# Block 0f — XML-Dokumentationskommentare: Pflichtregel und Bestandscode

## Kontext

Beim vollständig implementierten und wieder gelöschten Genre-Slice (siehe
Block 0e) fehlten laut Nutzerangabe auch XML-Dokumentationskommentare zu
Methoden — ein Implementierer konnte dadurch nicht auf einen Blick erkennen,
welche Properties/Parameter eine Methode bzw. ein Command/Query/DTO hat.

CLAUDE.md §9 und die gespiegelte Wiki-Regel (`entwicklung/codierrichtlinien.md`,
Zeile 15) erlauben XML-Dokumentationskommentare zwar ausdrücklich als
Ausnahme vom generellen Kommentarverbot, aber:

- Der Wortlaut spricht nur von „XML-Dokumentationskommentare für Methoden" —
  Properties, Records/DTOs, Parameter und Interfaces werden nicht erwähnt.
- Es ist nirgends eine **Pflicht**, nur eine **Erlaubnis** formuliert.
- Verifiziert: Im gesamten `src/`-Verzeichnis existiert aktuell keine einzige
  `///`-Zeile — die Regel wurde nie einmal angewendet, auch nicht im
  bestehenden Code aus Block 0b.
- Swagger/OpenAPI ist noch nicht verdrahtet (kein `AddSwaggerGen`, kein
  `Microsoft.AspNetCore.OpenApi`, kein `<GenerateDocumentationFile>` in
  irgendeiner `.csproj`) — XML-Doc-Kommentare wirken aktuell ausschließlich
  als IDE-Tooltip, nicht als Swagger-Anreicherung. Das bleibt unverändert;
  Swagger-Verdrahtung ist nicht Teil dieser Runde.

Entschieden (Rückfrage beantwortet):

- Sprache der XML-Docs: **Deutsch** (konsistent mit Fehlermeldungen/Log/Wiki).
- Bestehender Code aus Block 0b wird **jetzt nachgerüstet** (6 Dateien).
- **Keine** technische Durchsetzung (kein `GenerateDocumentationFile`/CS1591)
  in dieser Runde — nur Konvention, geprüft von `implementer`/`reviewer`.

## Freigegebener Umfang

### Regel: welche Elemente XML-Doc-Kommentare brauchen

| Element | Pflicht | Inhalt |
| --- | --- | --- |
| Commands, Queries (Records) | Ja | `<summary>` auf dem Typ, `<param>` je Property |
| Response-DTOs | Ja | `<summary>` auf dem Typ, `<param>` je Property |
| Domain: public Methoden (Create/Update/…) | Ja | `<summary>`, `<param>`, `<returns>`, `<exception>` je Validierungs-Exception |
| Public Interfaces und ihre Member | Ja | `<summary>` je Member |
| Endpoint-Klassen | Ja | `<summary>` auf `Map{Entität}Endpoints` und je Handler-Methode |
| Handler-Klassen | Ja, nur Klassenebene | `<summary>` auf der Klasse, keine `HandleAsync`-Detaildoku |
| ResponseBuilder, Repository-Implementierung, private Hilfsmethoden, Tests | Nein | Vertrag bereits am Interface/DTO dokumentiert |

Inhaltliche Anforderung: fachliche Bedeutung beschreiben, nicht den
Bezeichner wiederholen. Details, Beispiel und Tabelle: siehe Plan-Dokument
dieses Blocks.

### Verankerung

- `wiki/entwicklung/codierrichtlinien.md`: neue Sektion „XML-Dokumentationskommentare".
- `wiki/entwicklung/vorgaben-checkliste-neue-entitaet.md`: neuer Checklistenpunkt.
- `wiki/log.md`, `wiki/index.md` (falls nötig).
- `CLAUDE.md` §9: Kurzfassung aktualisiert.
- `.claude/agents/implementer.md`: Abschlussprüfung erweitert.
- `.claude/agents/reviewer.md`: vierter Prüfpunkt „XML-Doc-Konformität".

### Rückwirkende Anwendung (6 Dateien)

- `src/MyMusic.Domain/Contracts/Repository/IRepository.cs`
- `src/MyMusic.Application/Common/Services/ICurrentUserService.cs`
- `src/MyMusic.Application/Features/System/CurrentUser/ResponseDtos/CurrentUserResponse.cs`
- `src/MyMusic.Application/Features/System/CurrentUser/Queries/GetCurrentUser/GetCurrentUserQuery.cs`
- `src/MyMusic.Application/Features/System/CurrentUser/Queries/GetCurrentUser/GetCurrentUserQueryHandler.cs`
- `src/MyMusic.Api/MeEndpoints.cs`

Exakte Inhalte: siehe Plan-Dokument dieses Blocks. Reine
Kommentar-Ergänzung, keine Verhaltensänderung.

### TASK.md

Neuer Block „0f. XML-Dokumentationskommentare: Pflichtregel definieren und
Bestandscode nachziehen" zwischen 0e und Abschnitt 1.

## Nicht-Ziele

- Swagger/OpenAPI-Verdrahtung — eigener, separat freizugebender Schritt.
- Technische Durchsetzung via `GenerateDocumentationFile`/CS1591.
- XML-Docs für `Application/Common/CQRS/`-Interfaces und
  `ExceptionManager`/`GlobalExceptionHandler` — nicht Teil der
  abgestimmten 6-Dateien-Liste, eigener Folgeschritt.
- Korrektur des Nebenbefunds, dass `projekt/planungsschritte.md` Swagger
  bereits als erledigt (`[x]`) führt — nur als Beobachtung notiert.

## Betroffene Dateien/Module

Siehe Aufzählung oben; vollständige Diffs im Plan-Dokument dieses Blocks
(`C:\Users\A-v-L\.claude\plans\es-gibt-hier-im-witty-widget.md` zum
Zeitpunkt der Freigabe).

## Sicherheitsanforderungen

Keine — reine Dokumentations-/Kommentaränderung, kein Verhaltens-, Schema-
oder Zugriffsänderung.

## Verifikation

- `dotnet build` — muss unverändert grün bleiben.
- `dotnet test` für die vier Unit-Test-Projekte — unverändert grün.
- `dotnet format --verify-no-changes`, Zeilenlängen-Check.
- `git diff --check`.
- Wiki-Links gegen tatsächliche Dateinamen geprüft; `wiki/log.md` aktualisiert.

## Abnahmekriterien

- Wiki-Regel definiert Pflicht/Nicht-Pflicht je Element mit Beispiel.
- `CLAUDE.md`, `implementer.md`, `reviewer.md` verweisen konsistent auf die
  Regel.
- Alle 6 Bestandsdateien tragen die vorgeschriebenen XML-Doc-Kommentare;
  Build/Tests unverändert grün.

## Risiken und offene Punkte

- CQRS-Framework-Interfaces (`IMediator` etc.) und `ExceptionManager`
  bleiben vorerst undokumentiert, obwohl sie kategorisch unter „Public
  Interfaces" fallen würden — bewusst außerhalb des abgestimmten Umfangs,
  Folgeschritt.
