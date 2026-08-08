# Block 6d — Nachträge aus Block 2/4/5 (Backend)

## Kontext

Block 6c (Track-Backend) ist abgeschlossen und auf `main` gemergt (PR #32).
Laut `TASK.md` Abschnitt 6d sind vier Nachträge offen, die bewusst aus den
Slices Genre (Block 2), Label (Block 4) und Artist (Block 5) zurückgestellt
wurden, weil die dafür nötigen Tabellen `record`/`record_track` zum
jeweiligen Umsetzungszeitpunkt noch nicht existierten:

1. `DeleteGenreCommandHandler` — Referenzprüfung gegen `record_track`
   ergänzen (US-G5, HTTP 409 wenn mind. ein Track das Genre referenziert).
2. `DeleteLabelCommandHandler` — Referenzprüfung gegen `record` ergänzen
   (US-L5, HTTP 409 wenn mind. ein Record das Label referenziert).
3. `DeleteArtistCommandHandler` — Referenzprüfung gegen `record` **und**
   `record_track` ergänzen (US-A5, zwei getrennte Existenzabfragen, HTTP 409
   bei jeweils mind. einem Treffer).
4. `GetPagedArtistsQuery`/`GetPagedArtistsQueryHandler`/`ArtistEndpoints` —
   `labelId`-Filter ergänzen (US-A2). `artist` hat keine `label_id`-Spalte,
   die Beziehung besteht nur indirekt über `record.artist_id →
   record.label_id`.

Abnahmekriterium laut TASK.md: Die drei Delete-Handler verhindern das
Löschen real referenzierter Datensätze (HTTP 409); `GET /artists`
unterstützt den `labelId`-Filter. Mit Abschluss ist Block 6 (Record/Tracks)
vollständig fertig.

Alle vier Punkte sind rein mechanisch — sie übernehmen ein bereits im Code
etabliertes Muster und wenden es auf drei weitere Handler an. **Keine neue
Entität, keine Migration, kein neues NuGet-Paket, kein neuer ADR** (kein
neuer Präzedenzfall, keine neue Grundsatzentscheidung).

## Referenzimplementierung

Als Vorlage dienen (vollständig gelesen, Muster bereits im Code vorhanden):

- **Existenzprüfung**: `CreateRecordTrackCommandHandler.cs`
  (`Application/Features/Sammlung/RecordTrack/Commands/Create/`) — nutzt
  `IRepository<T>.GetPagedAsync(filter, orderBy, page: 1, pageSize: 1,
  cancellationToken)`, wertet nur `TotalCount` aus, wirft bei Treffer
  `exceptionManager.Conflict(message)`.
- **Zwischenmengen-Filter**: `GetPagedRecordsQueryHandler.
  ResolveLabelIdsForCountryAsync`
  (`Application/Features/Sammlung/Record/Queries/GetPaged/`) — löst über
  eine andere Tabelle eine `HashSet<int>?` von IDs auf, filtert dann per
  `Contains(...)`.
- Aktueller Ist-Stand der drei Delete-Handler (`DeleteGenreCommandHandler.cs`,
  `DeleteLabelCommandHandler.cs`, `DeleteArtistCommandHandler.cs`): laden →
  Ownership-Check (404) → `repository.Remove(...)` → `SaveChangesAsync`,
  jeweils mit einem erklärenden Kommentar, warum die Referenzprüfung noch
  fehlt — dieser Kommentar entfällt bei der Umsetzung vollständig.
- `ExceptionManager.Conflict(string message)` ist der einzige Overload — die
  deutsche Meldung wird vom Aufrufer selbst formuliert (kein Overload mit
  Entitätsname/Id wie bei `NotFound`).
- `IRepository<>` ist in `Program.cs` als offenes Generic registriert
  (`builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));`)
  — neue `IRepository<T>`-Konstruktorparameter brauchen keine zusätzliche
  DI-Registrierung.
- `RecordEntity`/`RecordTrackEntity` sind über `GlobalUsing.cs` in
  `MyMusic.Application` und `MyMusic.Application.Tests` bereits als Alias
  verfügbar — kein neuer using-Eintrag nötig.
- Fremdschlüssel-Nullability (verifiziert): `RecordEntity.LabelId` ist
  `int` (Pflicht), `RecordEntity.ArtistId` ist `int?` (nullable, EF/LINQ
  übersetzt `record.ArtistId == command.Id` korrekt, kein `!= null`-Zweig
  nötig). `RecordTrackEntity.ArtistId`/`GenreId` sind beide `int` (Pflicht).
- Die Referenzprüfung braucht **keinen** zusätzlichen `UserId`-Filter: Die
  Id ist bereits über den vorherigen Ownership-Check auf den angemeldeten
  Benutzer geprüft, analog zum bestehenden Muster in
  `CreateRecordTrackCommandHandler`.

## Vorgeschlagene Schritte

### 1. Domain (`MyMusic.Domain`)

Entfällt — keine neue Entität, keine Änderung an bestehenden Entitäten.

### 2. Infrastructure (`MyMusic.Infrastructure`)

Entfällt — keine Schemaänderung, keine Migration.

### 3. Application (`MyMusic.Application`)

**`DeleteGenreCommandHandler.cs`**
(`Features/Stammdaten/Genre/Commands/Delete/`): zusätzliche Abhängigkeit
`IRepository<RecordTrackEntity> recordTrackRepository`. Nach dem
Ownership-Check, vor `repository.Remove(genre)`:

```csharp
var (_, referencingTrackCount) = await recordTrackRepository.GetPagedAsync(
    track => track.GenreId == command.Id,
    query => query.OrderBy(track => track.Id),
    page: 1, pageSize: 1, cancellationToken);

if (referencingTrackCount > 0)
    throw exceptionManager.Conflict(
        $"Genre '{genre.Name}' kann nicht gelöscht werden, da es noch von mindestens einem Track " +
        "verwendet wird.");
```

**`DeleteLabelCommandHandler.cs`**
(`Features/Stammdaten/Label/Commands/Delete/`): analog mit
`IRepository<RecordEntity> recordRepository`, Filter
`record => record.LabelId == command.Id`, Meldung „Label '{label.Name}'
kann nicht gelöscht werden, da es noch von mindestens einem Record
verwendet wird."

**`DeleteArtistCommandHandler.cs`**
(`Features/Stammdaten/Artist/Commands/Delete/`): zwei zusätzliche
Abhängigkeiten `IRepository<RecordEntity>` und
`IRepository<RecordTrackEntity>`, zwei getrennte, nacheinander ausgeführte
Prüfungen (erst Record, dann Track — Kurzschluss, zweite Abfrage läuft nur
wenn die erste keinen Treffer hatte), je eigene Meldung:

- `record => record.ArtistId == command.Id` → „Artist '{artist.Name}' kann
  nicht gelöscht werden, da er noch von mindestens einem Record verwendet
  wird."
- `track => track.ArtistId == command.Id` → „Artist '{artist.Name}' kann
  nicht gelöscht werden, da er noch von mindestens einem Track verwendet
  wird."

Begründung getrennte statt kombinierte Meldung: TASK.md verlangt
ausdrücklich zwei getrennte Existenzabfragen; die Meldung sollte diese
Unterscheidung spiegeln. Kein Mehraufwand, da die zwei `if`-Blöcke ohnehin
nötig sind. (Wortlaut ist Empfehlung, im Wiki nicht vorgegeben — bei Bedarf
anpassbar.)

**`labelId`-Filter**:

- `GetPagedArtistsQuery.cs` (`Features/Stammdaten/Artist/Queries/GetPaged/`):
  zusätzlicher Parameter `int? LabelId`.
- `GetPagedArtistsQueryHandler.cs`: zusätzliche Abhängigkeit
  `IRepository<RecordEntity> recordRepository`, neue Methode
  `ResolveArtistIdsForLabelAsync(userId, labelId, cancellationToken)` — 1:1
  nach dem Muster von `ResolveLabelIdsForCountryAsync`, nur invertiert
  (Artist über Record zu Label). `RecordEntity.ArtistId` ist `int?` — beim
  Sammeln der HashSet-Werte `null` herausfiltern:
  `.Where(r => r.ArtistId is not null).Select(r => r.ArtistId!.Value)`.
  Haupt-Filter erhält zusätzlich
  `&& (artistIdsForLabel == null || artistIdsForLabel.Contains(artist.Id))`.
- Bewusste Analogie-Entscheidung: Wie bei `countryId` bei `GET /records`
  wird `labelId` nicht gegen Existenz/Mandantenzugehörigkeit geprüft — eine
  fremde/unbekannte `labelId` liefert eine leere Ergebnisliste, kein 400.

### 4. API (`MyMusic.Api`)

`ArtistEndpoints.cs` (`Endpoints/Stammdaten/Artist/`): `GetPagedArtistsAsync`
bekommt einen zusätzlichen `int? labelId`-Parameter (implizites Model
Binding wie bei `RecordEndpoints.GetPagedRecordsAsync`, kein
`[AsParameters]`), durchgereicht an den `GetPagedArtistsQuery`-Konstruktor.

### 5. Tests

**Application.Tests** (NSubstitute, xUnit, bestehendes 3-Test-Muster je
Delete-Handler-Testdatei übernehmen — Ist-Stand aller drei geprüft):

- `DeleteGenreCommandHandlerTests.cs`, `DeleteLabelCommandHandlerTests.cs`,
  `DeleteArtistCommandHandlerTests.cs`: bestehende Konstruktoraufrufe um die
  neuen `Substitute.For<IRepository<...>>()` ergänzen, für den „löscht
  erfolgreich"-Fall explizit `.Returns((Items: leere Liste, TotalCount: 0))`
  konfigurieren (nicht auf unkonfiguriertes NSubstitute-Default-Verhalten
  verlassen). Neuer Test je Referenztyp mit `TotalCount: 1` →
  `ConflictException` erwarten, plus `repository.DidNotReceive().Remove(...)`:
  - Genre: `HandleAsync_GenreReferenziertVonTrack_WirftConflictException`
  - Label: `HandleAsync_LabelReferenziertVonRecord_WirftConflictException`
  - Artist: `HandleAsync_ArtistReferenziertVonRecord_WirftConflictException`
    und `HandleAsync_ArtistReferenziertVonTrack_WirftConflictException`
    (Record-Fall: Track-Repository liefert 0 und wird — Kurzschluss — gar
    nicht aufgerufen, per `DidNotReceive()` absichern).
- `GetPagedArtistsQueryHandlerTests.cs` (Ist-Stand: 2 Tests): Konstruktor um
  `IRepository<RecordEntity>` ergänzen, bestehende
  `GetPagedArtistsQuery(...)`-Aufrufe um `LabelId: null` ergänzen (positional
  record). Neue Tests analog zum bestehenden `GetPagedRecordsQueryHandlerTests`-
  Muster für `CountryId`:
  `HandleAsync_LabelIdGesetzt_LoestRecordsDesLabelsMandantengefiltertAuf`
  (Filter-Expression auf `recordRepository.GetPagedAsync` per `Arg.Do<...>`
  capturen, kompilieren, gegen eigene/fremde/nicht-passende Records prüfen)
  sowie `HandleAsync_OhneLabelId_RuftRecordRepositoryNichtAuf`.
- Ein Unit-Test bis zum finalen `artistIdsForLabel.Contains(artist.Id)` auf
  Artist-Ebene ist nicht sinnvoll umsetzbar (`ArtistEntity.Create`/
  `RecordEntity.Create` liefern in Tests stets `Id == 0`) — bereits bekannte,
  dokumentierte Testbarkeitsgrenze, keine neue Lücke. Abgesichert wird
  dieser letzte Schritt über den Integrationstest.

**IntegrationTests** (kein bestehender 409-Testfall für „Löschen wegen
Referenz verweigert" vorhanden — neu zu schreiben):

- Neuer dritter `[Fact]` in `RecordEndpointsTests.cs` (Ist-Stand geprüft:
  besitzt bereits alle nötigen Hilfsmethoden — `GetTwoCountryIdsAsync`,
  `CreateLabelAsync`, `CreateArtistAsync`, `CreateGenreAsync`,
  `PostRecordAsync`, `PostRecordTrackAsync`), z. B.
  `ReferenzielleIntegritaet_VerhindertLoeschenVonGenreLabelUndArtistBeiVerwendung`.
  Ablauf: Genre+Label+Artist anlegen → Record damit anlegen → Track damit
  anlegen → `DELETE /genres/{id}` → 409 → `DELETE /labels/{id}` → 409 →
  `DELETE /artists/{id}` → 409 (wegen Record) → Track löschen →
  Artist-Delete weiterhin 409 (wegen Record) → Record löschen → alle drei
  Deletes → 204. Dafür drei neue Ein-Zeiler-Hilfsmethoden
  (`DeleteGenreAsync`/`DeleteLabelAsync`/`DeleteArtistAsync`), analog zum
  bereits vorhandenen `DeleteGenreAsync`-Muster in `GenreEndpointsTests.cs`
  (Ist-Stand geprüft, existiert dort bereits).
  Begründung für die Bündelung in `RecordEndpointsTests.cs` statt drei
  separaten Tests in den jeweiligen Entity-Testdateien: keine
  Code-Duplikation, kein zusätzlicher AppHost-Start (Postgres+Keycloak+
  Migrator+API, der Laufzeit-dominante Faktor je Testklasse) — bewusster
  Trade-off, vor Umsetzung kurz zu bestätigen.
- `labelId`-Filter: als zusätzliche Schritte im bestehenden
  `ArtistEndpoints_CrudPaginierungUndMandantentrennung`-Test in
  `ArtistEndpointsTests.cs` (Label + Record anlegen,
  `GET /api/artists?labelId=` prüfen) — gehört fachlich zum
  Artist-Endpoint-Test, kein Track nötig.

### 6. Dokumentation

- `TASK.md`: Kopfzeile (Stand-Datum, „Block 6d umgesetzt und verifiziert"),
  „Aktuell nicht umgesetzt"-Liste bereinigen, Abschnitt 6 als vollständig
  abgeschlossen markieren, Abschnitt 6d von „Status: offen" auf „Status:
  **abgeschlossen** (Datum)" mit Umsetzt-Liste, Branch-Zeile aktualisieren.
- `README.md` — bestehende Abschnitte ergänzen, keine neue Sektion:
  - Genre-Slice: Satz zu HTTP 409 bei Track-Referenz ergänzen.
  - Label-Slice: Satz zu HTTP 409 bei Record-Referenz ergänzen.
  - Artist-Slice: Endpoint-Tabelle um `&labelId=` ergänzen; den jetzt
    veralteten Satz „Ein `labelId`-Filter fehlt in diesem Slice bewusst: …"
    ersetzen durch die neue Filterbeschreibung plus HTTP-409-Hinweis
    (Record/Track, zwei getrennte Prüfungen).

## Benötigte NuGet-Pakete

Keine.

## Verifikation

PowerShell, kein Git Bash (CLAUDE.md §11):

```powershell
dotnet restore
dotnet build --no-restore
dotnet format --verify-no-changes
dotnet test tests\MyMusic.Domain.Tests --no-build
dotnet test tests\MyMusic.Application.Tests --no-build
dotnet test tests\MyMusic.Api.Tests --no-build
dotnet test tests\MyMusic.IntegrationTests
git diff --check
```

Erst die drei schnellen Unit-Test-Projekte, danach die deutlich
langsameren Integrationstests (echter AppHost-Start mit Postgres/Keycloak/
Migrator). Zeilenlängen-Check (120 Zeichen) läuft automatisiert nur in CI —
neue/geänderte Zeilen manuell prüfen.

## Risiken und offene Punkte

- Die vier vorgeschlagenen deutschen Fehlermeldungstexte sind eine
  Empfehlung (Wiki gibt keinen Wortlaut vor, nur „Fehler-Modal mit
  entsprechender Meldung") — bei Bedarf vor/während der Umsetzung anpassbar.
- Bündelung der drei Delete-Referenz-Integrationstests in einem neuen Fact
  in `RecordEndpointsTests.cs` statt in den drei Entity-eigenen
  Testdateien ist ein bewusster Laufzeit/Duplikations-Trade-off — sollte
  vor Umsetzung kurz bestätigt werden, nicht stillschweigend entschieden.
- `labelId`-Filter ohne Existenz-/Mandantenprüfung (Analogie zu
  `countryId` bei Records) — bewusste Konsistenzentscheidung, kein 400 bei
  fremder/unbekannter Id.
- `GetPagedAsync`s Erfolgsfall bleibt wie bei Genre/Country/Label/Artist
  bereits etabliert nicht sinnvoll per NSubstitute unit-testbar — Nachweis
  über Integrationstest.
- Branch-Strategie (CLAUDE.md §2.4): Vor der ersten Änderung Feature-Branch
  `block-6d-nachtraege` von `main` anlegen (aktueller Branch bei
  Prompt-Erstellung: `main`) — Anlegen des Branches bleibt freigabepflichtig.
