# Sammlungsnummer für Record (CollectionNumber)

## Kontext

`2026-08-28-notwendige-korrekturen.md` listet unter „Records" noch einen
offenen Punkt: Record braucht ein zusätzliches Feld — eine bei jedem
Benutzer bei 1 beginnende fortlaufende Ganzzahl, die auf der Record-Card im
Frontend angezeigt wird. Zweck laut Korrekturtext: Dem Benutzer wird damit
die Möglichkeit gegeben, seine physischen Tonträger real durchsuchbar zu
machen — z. B. durch Aufkleber mit dieser Nummer auf den Tonträgern selbst.

Diese Anforderung war im Wiki nirgends dokumentiert (geprüft: `record.md`,
`tabellenschema.md`, `er-modell.md`, `user-stories-record.md`,
`api-endpunkte.md`, `glossar.md`) — eine neue fachliche Anforderung, kein
bereits geplantes Feature.

**Mit dem Projektinhaber geklärt (2026-08-31):**

- **Feldname**: `CollectionNumber` (nicht „RecordId" wie im Korrekturtext,
  da das mit der vorhandenen Primärschlüssel-Spalte `id` und der
  FK-Konvention `record_id` kollidieren würde).
- **Löschverhalten**: Lücke bleibt bestehen. Die Nummer wird einmalig beim
  Anlegen vergeben (nächste freie Nummer je Benutzer) und danach nie mehr
  verändert — auch nicht, wenn ein Record mit einer niedrigeren Nummer
  gelöscht wird.
- **Anzeige**: Card **und** Detailansicht (nicht nur Card wie im
  Korrekturtext wörtlich beschrieben), als Badge.
- **Sortierung**: Die Sammlungsnummer aufsteigend wird die neue
  **Standard-Sortierung** der Record-Liste — löst die bisherige
  Standard-Sortierung „Albumname aufsteigend" ab (US-R1/US-R3,
  `api-endpunkte.md`). Jede andere Sortierung (Name, Erscheinungsjahr,
  Format) bleibt weiterhin explizit wählbar; `collectionNumber` wird als
  vierte, explizit wählbare `sortBy`-Option ergänzt.
- **Filterung**: Keine Filterung nach der Sammlungsnummer.

Explizit **nicht** Teil dieses Umfangs: Anzeige/Bearbeitbarkeit im
Record-Formular, Anzeige im Dashboard.

## Ist-Stand (verifiziert)

- `Record.cs` (Domain) hat keine benutzerbezogene Zählnummer, nur den
  globalen Primärschlüssel `Id`.
- `record`-Tabelle (`tabellenschema.md`, `RecordConfiguration.cs`) hat keine
  entsprechende Spalte oder einen entsprechenden Unique-Constraint.
- `RecordResponse`/`SearchResultResponse` und die zugehörigen Builder
  enthalten das Feld nicht.
- `GetPagedRecordsQueryHandler.BuildOrderBy` sortiert ohne `sortBy` bzw. bei
  unbekanntem Wert nach `AlbumName` aufsteigend; `record-filter.ts` hat den
  gleichen Default (`sortBy: 'name'`) und kennt nur die drei Optionen
  `name`/`releaseYear`/`format`.
- `GetPagedSearchQueryHandler` sortiert fest nach `AlbumName`, kein
  `sortBy`-Parameter — bleibt unverändert.
- `record-card.html`/`record-detail.html` zeigen die Nummer nirgends an.

## Geplanter Fix

### Domain

`src/MyMusic.Domain/DomainModels/Sammlung/Record/Record.cs`:

- Neue Property `CollectionNumber` (`int`, `private init`).
- Konstruktor und `Create(...)` bekommen zusätzlichen Parameter
  `collectionNumber`, Validierung `collectionNumber <= 0` →
  `ArgumentException`.
- `Update(...)` und `SetAlbumCover(...)` übernehmen `CollectionNumber`
  unverändert aus der bestehenden Instanz.

### Infrastructure / Migration

`RecordConfiguration.cs`: Mapping `HasColumnName("collection_number")`,
`IsRequired()`; neuer `HasIndex(record => new { record.UserId,
record.CollectionNumber }).IsUnique()`.

Migration `AddCollectionNumberToRecord`
(`dotnet ef migrations add AddCollectionNumberToRecord --project
src/MyMusic.Infrastructure --startup-project src/MyMusic.Migrator
--output-dir Persistence/Migrations`), `Up()` manuell ergänzt um:

1. Spalte zunächst nullable anlegen.
2. Backfill bestehender Records je Benutzer nach `id` aufsteigend:
   ```sql
   UPDATE record r
   SET collection_number = sub.rn
   FROM (SELECT id, ROW_NUMBER() OVER (PARTITION BY user_id ORDER BY id) AS rn FROM record) sub
   WHERE r.id = sub.id;
   ```
3. Spalte auf `NOT NULL` setzen.
4. Unique-Index `(user_id, collection_number)` anlegen.

`Down()` entfernt Index und Spalte wieder.

### Application

- `CreateRecordCommandHandler.cs`: nächste freie Nummer je Benutzer über
  `IRepository<T>.GetProjectedAsync` ermitteln (gleiches Muster wie ADR
  0021):
  ```csharp
  var existingNumbers = await repository.GetProjectedAsync(
      r => r.UserId == command.UserId, r => r.CollectionNumber, cancellationToken);
  var nextCollectionNumber = existingNumbers.Count == 0 ? 1 : existingNumbers.Max() + 1;
  ```
  `Max() + 1`, nicht `Count() + 1` — sonst würden Lücken wieder aufgefüllt.
- `RecordResponse.cs`: neues Feld `int CollectionNumber` (nach `Id`).
- `RecordResponseBuilder.cs`: `record.CollectionNumber` durchreichen.
- `SearchResultResponse.cs` + `SearchResponseBuilder.cs`: gleiches Feld
  ergänzen (eigene DTOs wegen Feature-Kapselung, Block 10); Sortierung von
  `GET /api/search` bleibt unverändert.
- `GetPagedRecordsQueryHandler.cs` (`BuildOrderBy`): neuer Case
  `"collectionnumber"`; Default-Fall wechselt von `AlbumName` auf
  `CollectionNumber`.

### Frontend

- `record.ts`: `collectionNumber: number` im `Record`-Interface (nicht in
  `CreateRecordRequest`/`UpdateRecordRequest` — serverseitig vergeben).
- `record-card.html`/`.ts`: Badge `.badge.badge-neutral.tnum` mit `#{{ … }}`,
  eigene Zeile am Anfang von `.record-meta`, oberhalb von `.record-title`.
- `record-detail.html`: gleicher Badge-Stil in der bestehenden Badge-Zeile
  neben Format-Badge und Grade-Badge.
- `record-filter.ts`: `RecordSortBy`-Union um `'collectionNumber'`
  erweitert; Default in `filterModel` von `'name'` auf `'collectionNumber'`.
- `record-filter.html`: neue Option `collectionNumber` (Sammlungsnummer) im
  Sortier-`<select>`, als erste Option.

Betroffene Spec-Dateien: `record-card.spec.ts`, `record-detail.spec.ts`,
`records.spec.ts`, `record.service.spec.ts`, `search.service.spec.ts` —
Fixtures um `collectionNumber` ergänzen, neue Assertion für den Badge.

### Tests — mechanische Signatur-Anpassung

`RecordEntity.Create(...)` bekommt einen Pflichtparameter mehr — betrifft
alle Stellen, die einen Test-Record über diese Factory bauen (u. a.
`RecordTests.cs`, `CreateRecordCommandHandlerTests.cs`,
`UpdateRecordCommandHandlerTests.cs`, `DeleteRecordCommandHandlerTests.cs`,
`GetRecordByIdQueryHandlerTests.cs`, `GetPagedRecordsQueryHandlerTests.cs`,
`RecordResponseBuilderTests.cs`, `UploadRecordCoverCommandHandlerTests.cs`,
`SearchResponseBuilderTests.cs`, `GetPagedSearchQueryHandlerTests.cs`,
`GetDashboardQueryHandlerTests.cs`, `CreateRecordTrackCommandHandlerTests.cs`,
`DeleteLabelCommandHandlerTests.cs`, `DeleteArtistCommandHandlerTests.cs`,
`DeleteUserCommandHandlerTests.cs`, `GetPagedArtistsQueryHandlerTests.cs`) —
jeweils einen gültigen Dummy-Wert ergänzen.

Neue, fachlich aussagekräftige Tests:

- `RecordTests.cs`: `Create_UngueltigeCollectionNumber_WirftArgumentException`
  (0, negativ) + Assertion, dass `Update`/`SetAlbumCover` die Nummer
  unverändert lassen.
- `CreateRecordCommandHandlerTests.cs`: erster Record eines Benutzers erhält
  `1`; ein weiterer Record erhält `Max + 1`; mit einer Lücke im Bestand
  (z. B. vorhandene Nummern `1, 2, 4`) wird `5` vergeben, nicht `3`.
- `RecordEndpointsTests.cs` (Integrationstest): zwei Records anlegen →
  Nummern `1`/`2`; ersten löschen, dritten anlegen → neue Nummer ist `3`.
  `RecordResponseDto.cs` (Test-Support) um `CollectionNumber` ergänzen.
- `GetPagedRecordsQueryHandlerTests.cs`: Test für den geänderten Default
  (kein `sortBy` → Ergebnis nach `CollectionNumber` aufsteigend) und für den
  neuen expliziten Fall `sortBy=collectionNumber`.

### Dokumentation

- Wiki: `record.md`, `tabellenschema.md`, `er-modell.md`, `api-endpunkte.md`
  (`sortBy` um `collectionNumber` erweitern, Default ändern),
  `user-stories-record.md` (US-R1, US-R3, neuer Abschnitt „Geklärt am
  2026-08-31"), `wiki/index.md`, `wiki/log.md`.
- Neuer ADR `docs/adr/0030-record-collection-number-vergabe.md`: begründet
  MAX+1-Vergabe je Benutzer über die vorhandene Projektions-Query statt
  einer DB-Sequenz pro Partition oder einer separaten Zähler-Tabelle, plus
  den akzeptierten, vernachlässigbaren Risikofall einer Race Condition bei
  zwei gleichzeitigen Create-Requests desselben Benutzers (Unique-Constraint
  als reine Absicherung, kein Retry).
- `TASK.md` und root `CLAUDE.md`: Stand-Absatz nach Merge und
  Live-Verifikation.
- `2026-08-28-notwendige-korrekturen.md`: Record-Punkt nach Merge
  durchstreichen und mit „Erledigt — behoben in PR #…" versehen.

## Geplante Verifikation

1. `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`.
2. `ng test --watch=false`, `ng lint`.
3. Live-Verifikation gegen laufenden Aspire-AppHost: Migration läuft sauber
   gegen die vorhandene Datenbank durch (Backfill korrekt), neuer Record
   bekommt die nächste freie Nummer, Löschen erzeugt eine bleibende Lücke,
   Card und Detailansicht zeigen die Nummer, Standard-Sortierung ist
   Sammlungsnummer aufsteigend, alle vier Sortieroptionen funktionieren.

## Bekannte Risiken und offene Punkte

- Theoretische Race Condition bei zwei gleichzeitigen Create-Requests
  desselben Benutzers (beide lesen denselben Höchstwert) — durch den
  Unique-Constraint auf Datenbankebene abgefangen (führt zu einem
  Fehlschlag des zweiten Requests statt einer doppelt vergebenen Nummer),
  keine Sonderbehandlung/Retry implementiert. Bei einer
  Einzelbenutzer-Sammlungsanwendung als vernachlässigbar eingestuft, siehe
  ADR 0030.
- Der Hinweis auf „Katalognummer" in `komponenten-klassen.md`
  (`.record-sub`) bezieht sich auf ein nie umgesetztes, anderes Konzept und
  wird durch diesen Fix nicht berührt.
