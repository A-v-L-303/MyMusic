# Block 6b — Album-Cover-Upload (Backend)

## Kontext

Block 6a (Record-Backend ohne Cover, ohne Tracks) ist abgeschlossen und auf
`main` gemerged (`31492f1`). Laut `TASK.md` Abschnitt 6b ist Album-Cover-Upload
der nächste Backend-Slice. Vor Umsetzung wurden mit dem Projektinhaber zwei
Punkte geklärt, die laut `TASK.md`/`wiki/user-stories/user-stories-record.md`
(US-R8) ausdrücklich vor Umsetzung von 6b zu klären waren:

- Nächster Slice ist **6b** (nicht 6c/6d).
- Fehlerdarstellung bei ungültigem Cover-Upload (falsches Format, Datei
  > 5 MB): **Modal**, nicht Inline — Ausnahme von der allgemeinen 400-Regel
  in `wiki/architektur/fehler-und-ausnahmekonzept.md` (Datei-Uploads sind
  dort nicht als eigene Kategorie geführt). Beide Wiki-Seiten entsprechend
  aktualisiert.

Ziel dieses Blocks ist ausschließlich das Backend — analog zu allen
bisherigen Slices bleibt eine Angular-Anbindung bis Block 0c zurückgestellt.

## Referenzimplementierung

Als Vorlage dienten (vollständig gelesen): `Record.cs`,
`UpdateRecordCommand(Handler)`, `UpdateLabelCommand(Validator)` (Muster für
Wiederverwendung von Domain-Konstanten im Validator), `RecordResponse(Builder)`,
`RecordEndpoints.cs`, `ApplicationServiceCollectionExtensions.cs`,
`IRepository.cs`, `GlobalUsing.cs` (Domain/Application/Api/Tests) sowie
`RecordTests.cs`, `UpdateRecordCommandHandlerTests.cs`,
`UpdateRecordCommandValidatorTests.cs`, `RecordResponseBuilderTests.cs` und
`RecordEndpointsTests.cs`. Migration `20260807111556_CreateRecordTable.cs`
zeigte, dass `album_cover bytea` (nullable) bereits seit Block 6a existiert —
**keine neue Migration** in diesem Block.

## Designentscheidungen

- **Domain**: `Record.SetAlbumCover(byte[] albumCover)` — analog zu
  `Update(...)`, gibt neue Instanz zurück. Neue Konstante
  `MaxAlbumCoverSizeBytes` (5 MB) und statische Methode
  `DetectAlbumCoverContentType(byte[])` (liefert `"image/jpeg"`,
  `"image/png"` oder `null` anhand der Magic Bytes), von Domain-Guard **und**
  Validator gemeinsam genutzt (gleiches Wiederverwendungsmuster wie
  `LabelEntity.NamePattern`).
- **Application**: neuer Ordner
  `Features/Sammlung/Record/Commands/UploadCover/` mit `UploadRecordCoverCommand`
  (`Id`, `UserId`, `FileContent` als `byte[]`), Validator (`NotEmpty`, Größe,
  Format — alles über `RecordEntity`-Konstanten/-Methode) und Handler (Load →
  404 bei fremd/unbekannt → `SetAlbumCover(...)` → `Update`/`SaveChangesAsync`
  → Label-/Artist-Namen nachladen → `RecordResponseBuilder.Build(...)`).
- **`RecordResponse`** erhält `AlbumCoverDataUrl` (`string?`).
  `RecordResponseBuilder.Build(...)` baut bei vorhandenem Cover eine
  vollständige Data-URL (`data:image/jpeg;base64,...` bzw. `image/png`) über
  `DetectAlbumCoverContentType(...)` + `Convert.ToBase64String(...)` — nicht
  wörtlich im Wiki spezifiziert (dort nur „API liefert Base64"), aber
  notwendig, da der Content-Type nicht in der DB gespeichert wird. Da `Build`
  sowohl für Einzelabruf als auch je Item in `BuildPaged` verwendet wird,
  erscheint das Cover automatisch in Card- **und** Detailansicht
  (US-R1/US-R7).
- **API**: `POST /api/records/{id}/cover` in der bestehenden
  `RecordEndpoints`-Gruppe, `[FromForm] IFormFile file`-Parameter,
  `.DisableAntiforgery()` (seit .NET 8 für formularbindende Minimal-API-
  Endpunkte erforderlich; unkritisch, da die API ausschließlich JWT-Bearer
  statt Cookies nutzt — siehe ADR 0008).
- Kein Thumbnail/Resizing, keine Kestrel-/`RequestSizeLimit`-Änderung (Default
  deutlich über 5 MB ausreichend) — bewusst nicht Teil dieses Blocks.

## Umgesetzt

1. **Domain** (`MyMusic.Domain`): `Record.cs` um `MaxAlbumCoverSizeBytes`,
   `_jpegSignature`/`_pngSignature`, `DetectAlbumCoverContentType(...)` und
   `SetAlbumCover(byte[])` erweitert.
2. **Application** (`MyMusic.Application`): `UploadRecordCoverCommand.cs`,
   `UploadRecordCoverCommandValidator.cs`, `UploadRecordCoverCommandHandler.cs`
   neu; `RecordResponse.cs` und `RecordResponseBuilder.cs` um
   `AlbumCoverDataUrl` erweitert; `GlobalUsing.cs` (Application, Api,
   Application.Tests) um den neuen `Commands.UploadCover`-Namespace ergänzt.
3. **API** (`MyMusic.Api`): `RecordEndpoints.cs` um
   `POST /{id:int}/cover` mit `.DisableAntiforgery()` erweitert.
4. **ADR**: `docs/adr/0008-kein-antiforgery-fuer-cover-upload.md` neu.
5. **Tests**:
   - Domain: `RecordTests.cs` — `SetAlbumCover` (gültiges JPEG/PNG,
     Immutabilität, leer, zu groß, ungültige Signatur),
     `DetectAlbumCoverContentType` (JPEG, unbekannt).
   - Application: `UploadRecordCoverCommandValidatorTests.cs`,
     `UploadRecordCoverCommandHandlerTests.cs` (Happy Path, 404 bei
     unbekannt/fremd), `RecordResponseBuilderTests.cs` um Data-URL-Fälle
     ergänzt.
   - Integration: `RecordEndpointsTests.cs` um Cover-Upload-Schritte im
     bestehenden CRUD-Testfluss ergänzt (401 ohne Token, 400 bei
     ungültigem Format, 400 bei zu großer Datei, 404 bei fremdem Record,
     200 mit Data-URL, Persistenz über erneuten Abruf geprüft);
     `TestSupport/RecordResponseDto.cs` um `AlbumCoverDataUrl` ergänzt.
6. **Wiki**: `user-stories-record.md` (Modal-Entscheidung dokumentiert,
   offener Punkt aufgelöst), `fehler-und-ausnahmekonzept.md`
   (Ausnahme-Regel für Datei-Uploads ergänzt), `log.md` (neuer Eintrag).

## Bewusst nicht Teil dieses Blocks

- Angular-Anbindung (Upload-UI, Modal-Fehlerdarstellung im Frontend) — folgt
  erst nach Block 0c.
- Thumbnail-/Resize-Verarbeitung.
- Streaming-Validierung vor vollständigem Einlesen der Datei.
- `RequestSizeLimit`-Verschärfung über die 5-MB-Geschäftsregel hinaus.

## Abnahmekriterium

Für einen eigenen Record kann ein Cover hochgeladen werden (JPEG/PNG,
max. 5 MB) und erscheint danach in Card- und Detailansicht; fremde oder
unbekannte Records liefern 404; ungültiges Format/zu große Datei liefern 400
(im Frontend künftig als Modal darzustellen).
