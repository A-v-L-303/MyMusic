# Fix: Discogs-Integration – Auto-Übernahme ohne Rückfrage, Suchfeld-Fokus

## Kontext

Notwendige-Korrekturen-Liste des Projektinhabers, Abschnitt „Discogs Integration":

- Suchfeld muss Fokus haben.
- Wenn aus der Discogs-Suche Artist, Label oder Genre noch nicht in den
  Stammdaten vorhanden sind, werden diese ohne Rückfrage automatisch
  hinzugefügt. Das Land für ein neues Label wird aus den Discogs-Daten
  übernommen (wenn möglich, ansonsten besprechen).

Aktuell (Block 8b, dokumentiert in `user-stories-discogs.md` US-DI3 und
`discogs-api.md`) erscheint für jeden neuen Artist/Label/Genre aus einem
Discogs-Treffer eine Rückfrage, bevor der Datensatz angelegt wird — bei Label
sogar das volle `LabelForm`-Modal, weil `Label.countryId` ein Pflichtfeld ist
und Discogs-Länderdaten bisher gar nicht durch die Anwendung propagiert
werden. Der Korrekturauftrag kehrt das um: Regelfall ist Auto-Anlage ohne
Interaktion; nur wenn für ein neues Label kein Land ermittelbar ist, bleibt
eine Rückfrage nötig.

**Mit dem Projektinhaber geklärt (2026-08-31):**
- Länderzuordnung: statische Zuordnungstabelle (Discogs-Länder-/Regionstext
  → ISO-Code), abgeleitet aus der bereits im Wiki gepflegten Referenzliste
  (`country-referenzdaten.md`, 238 Zeilen Code + deutscher Name). Kein
  NuGet-/npm-Paket, keine Teilliste. Mehrdeutige/nicht zuordenbare
  Discogs-Werte (Regionen wie „Europe"/„UK & Europe", nicht in unserer Liste
  geführte historische Staaten wie „Czechoslovakia"/„USSR") bleiben bewusst
  ungemappt, kein Raten.
- Fallback bei keinem Treffer: das bestehende `LabelForm`-Modal öffnet sich
  weiterhin vorbefüllt mit dem (bereinigten) Namen, Land wird manuell
  gewählt — exakt das heutige Verhalten, nur nicht mehr der Normalfall.
- Scope-Grenze: Betrifft ausschließlich Referenzen aus der Discogs-Übernahme
  (Record-Artist, Track-Artist, Genre, Label). Die manuelle Artist-Eingabe im
  RecordForm (Freitext + Blur, unabhängig von Discogs) bleibt unverändert mit
  Rückfrage.

## Ist-Stand (verifiziert)

- `DiscogsReleaseRepresentation`/`DiscogsRelease`/`DiscogsReleaseResponse`
  (Backend) enthalten kein Länderfeld — Discogs' Release-Feld `country` wird
  aktuell nirgends geparst oder durchgereicht.
- `record-form.ts`:
  - `resolveArtistId` wird sowohl von `onArtistBlur` (manuelle Eingabe) als
    auch von `onDiscogsReleaseApplied`/`importDiscogsTracksIfAny`
    (Discogs-Import) verwendet und öffnet bei fehlendem Treffer immer ein
    `ConfirmModal`.
  - `resolveGenreId` wird ausschließlich vom Discogs-Import aufgerufen,
    öffnet bei fehlendem Treffer ein `ConfirmModal`.
  - `resolveLabelId` wird ausschließlich vom Discogs-Import aufgerufen,
    öffnet bei fehlendem Treffer das `LabelForm`-Modal (vorbefüllt mit dem
    Namen, Land manuell zu wählen).
- `discogs-search.ts`/`.html`: Das Sucheingabefeld hat kein Autofokus-Verhalten.
- `Label.countryId` ist ein Pflicht-Fremdschlüssel (`NOT NULL`, siehe Wiki
  `label.md`) — eine automatische Label-Anlage ohne Land ist nicht möglich.
- `country.md`/`country-referenzdaten.md`: `country_name` ist Deutsch,
  `country_code` überwiegend ISO 3166-1 alpha-2 — kein direkter Textabgleich
  gegen Discogs' englische Ländertexte möglich.

## Geplanter Fix

### Backend — Discogs-Land durchreichen (reines Pass-through, kein Mapping)

1. `src/MyMusic.Infrastructure/ExternalServices/Discogs/DiscogsReleaseRepresentation.cs`:
   neues Feld `string? Country`.
2. `src/MyMusic.Infrastructure/ExternalServices/Discogs/DiscogsClient.cs`
   (`MapRelease`): `release.Country` durchreichen.
3. `src/MyMusic.Application/Common/Services/DiscogsRelease.cs`: neues Feld
   `string? Country`.
4. `src/MyMusic.Application/Features/Integration/Discogs/ResponseDtos/DiscogsReleaseResponse.cs`:
   neues Feld `string? Country`.
5. `src/MyMusic.Application/Features/Integration/Discogs/ResponseDtos/Builder/DiscogsResponseBuilder.cs`
   (`BuildRelease`): `release.Country` durchreichen.

`DiscogsSearchResult`/`DiscogsSearchResultResponse` bleiben unverändert — das
Land ist laut Wiki nur im Volldaten-Abruf relevant.

Tests: `DiscogsClientTests.cs`, `DiscogsResponseBuilderTests.cs`,
`GetDiscogsReleaseQueryHandlerTests.cs`, `DiscogsEndpointsTests.cs` um das
neue Feld ergänzen (Fixture-JSON + Durchreichungs-Assertion).

### Frontend — Länder-Zuordnungstabelle

Neue Datei `src/frontend/src/app/features/records/discogs-country-mapping.ts`:

- `DISCOGS_COUNTRY_TO_ISO_CODE: Record<string, string>` — üblicher
  englischer Ländername (lowercase) → ISO-Code, aufgebaut aus den 238
  Code/Name-Paaren der Wiki-Referenzliste, plus die auf Discogs verbreiteten
  eindeutigen Kurzformen „uk"/„us"/„usa".
- `resolveDiscogsCountryId(discogsCountry: string | null, countries: Country[]): number | null`
  — normalisiert (trim/lowercase), schlägt in der Tabelle nach, sucht den
  passenden Eintrag in `countries` über `code`; `null` bei keinem Treffer.
- `DiscogsRelease`-Interface (`discogs.ts`) bekommt `country: string | null`.

### Frontend — `record-form.ts`: Auto-Anlage statt Rückfrage

- **Genre** (`resolveGenreId`): Rückfrage-Zweig entfernen, bei fehlendem
  Treffer direkt `genreService.create({ name: cleanedName })`. Entfernt:
  `pendingGenreConfirmName`, `pendingGenreResolve`, `onGenreCreateConfirmed`,
  `onGenreCreateCancelled`, zugehöriger `ConfirmModal`-Block in
  `record-form.html`.
- **Artist**: `resolveArtistId` bleibt unverändert für `onArtistBlur`. Neue
  private Methode `resolveOrCreateArtistId(name: string): Promise<number | null>`
  (gleiche Bereinigung/Existenzprüfung, aber direkte Anlage ohne Rückfrage) —
  wird von `onDiscogsReleaseApplied` und `importDiscogsTracksIfAny` anstelle
  von `resolveArtistId` aufgerufen. `ConfirmModal`-Block für Artist bleibt
  (weiterhin für die manuelle Eingabe).
- **Label** (`resolveLabelId`): Signatur wird
  `resolveLabelId(name: string, discogsCountry: string | null)`.
  1. Existenzprüfung wie bisher.
  2. Kein Treffer → `resolveDiscogsCountryId(discogsCountry, this.countries())`.
     - Land ermittelt → direkt `labelService.create({ name, countryId, information: null })`.
     - Kein Land ermittelt → bestehender Fallback unverändert (`LabelForm`-Modal).
  Aufrufstelle in `onDiscogsReleaseApplied` übergibt `release.country`.

### Frontend — Discogs-Suchfeld: Autofokus

`discogs-search.ts`/`.html`: Template-Referenz `#queryInput` auf das
bestehende `<input>`; `viewChild<ElementRef<HTMLInputElement>>('queryInput')`
+ `afterNextRender(() => this.queryInput()?.nativeElement.focus())` im
Constructor (Komponente wird bei jedem Öffnen des Modals neu erzeugt).

## Geplante Verifikation

1. Backend: `dotnet build`, `dotnet test` für `MyMusic.Infrastructure.Tests`,
   `MyMusic.Application.Tests`, `MyMusic.IntegrationTests`,
   `dotnet format --verify-no-changes`.
2. Frontend: `ng test --watch=false` für geänderte/neue Specs
   (`discogs-country-mapping.spec.ts`, `record-form.spec.ts`,
   `discogs-search.spec.ts`), `ng lint`, `ng build`.
3. Zeilenlängen-Check (≤120 Zeichen) der geänderten Zeilen.
4. Manuelle Live-Prüfung gegen den laufenden Aspire-AppHost: ein
   Discogs-Release mit eindeutig zuordenbarem Land (Label wird automatisch
   ohne Rückfrage angelegt, korrektes Land) und ein Release ohne
   Mapping-Treffer (LabelForm-Modal öffnet sich wie bisher); zusätzlich
   prüfen, dass ein neuer Artist/Genre ohne Rückfrage angelegt wird und dass
   das Discogs-Suchfeld beim Öffnen fokussiert ist.

## Bekannte Risiken und offene Punkte

- Die Zuordnungstabelle deckt Standard-Ländernamen ab; ob Discogs für
  einzelne, seltenere Releases abweichende Schreibweisen verwendet, die nicht
  in der Tabelle enthalten sind, kann nur die manuelle Live-Prüfung mit
  echten Discogs-Daten zeigen — solche Fälle fallen korrekt in den
  LabelForm-Fallback, sind aber nicht einzeln vorab getestet.
- `resolveArtistId` und `resolveOrCreateArtistId` teilen sich Bereinigung und
  Existenzprüfung (kleine Code-Duplizierung) — bewusst nicht zu einer
  gemeinsamen Methode zusammengeführt, um den Scope auf die freigegebene
  Änderung zu begrenzen.
- Kein Refresh von `artistsResource`/`labelsResource`/`genresResource` nach
  einer Auto-Anlage — entspricht dem bereits heute bestehenden Verhalten
  (auch die bisherige `LabelForm`-Übernahme lädt die Liste nicht neu) und ist
  kein neues Risiko dieses Fixes.
