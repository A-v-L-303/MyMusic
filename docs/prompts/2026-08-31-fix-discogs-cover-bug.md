# Fix: Discogs-Cover-Bugs (Trefferliste + Übernahme)

## Kontext

`03 Ressourcen/2026-08-30-discogs-cover-bug-untersuchung.md` (Analyse-Notiz,
nicht dauerhaft archiviert) beschreibt zwei Symptome aus einem Live-Test:

1. **Bug 1**: Discogs-Suchtrefferliste zeigt keine Vorschaubilder mehr.
2. **Bug 2**: Ein aus Discogs angelegter Record wird ohne Cover gespeichert.

Die Notiz hat beide Ursachen bereits lokalisiert, für Bug 2 aber explizit
**keine** Ursache verifiziert (fehlendes Logging). Eigene Wiki-/Code-Analyse
in dieser Session hat für Bug 2 eine konkrete, in der Notiz nicht genannte
Ursache identifiziert (siehe unten) — wird vor dem endgültigen Fix live
verifiziert, nicht nur angenommen.

## Ist-Zustand (verifiziert)

**Bug 1** — `discogs-search.html:37-41` rendert `result.thumbnailUrl` direkt
als `<img [src]>`. `thumbnailUrl` kommt unverändert von Discogs' Such-API
(`DiscogsClient.MapSearchResult`, `DiscogsClient.cs:48-55`) — ein direkter
Link auf Discogs' Bild-CDN. ADR 0020 dokumentiert bereits, dass Discogs
sowas ohne passenden User-Agent/Referer blockiert; der dortige Fix
(serverseitiger Download + Data-URL-Embedding) wurde nur für
`GetReleaseAsync` umgesetzt, nicht für `SearchAsync`.

**Bug 2** — `DiscogsClient.GetReleaseAsync` lädt das Cover serverseitig
herunter und liefert es als `data:`-URL in `CoverImageUrl` (ADR 0020). Das
Frontend (`record-form.ts:549-572`, `applyDiscogsCover`) ruft
`fetch(coverImageUrl)` auf dieser Data-URL auf, um daraus ein `File`-Objekt
zu bauen. Neuer Befund (nicht in der Notiz): Block 7j hat am 2026-08-26 —
vier Tage nach Block 8b (2026-08-22) — eine CSP eingeführt
(`write-csp-meta.mjs`, ADR 0025), deren `connect-src` nur `'self'` + API-
+ Keycloak-Origin erlaubt. `wiki/sicherheit/sicherheitskonzept.md`
reserviert `data:` ausdrücklich nur für `img-src`, nicht für `connect-src`
(`| connect-src | 'self' + Keycloak-URL + API-URL |` vs.
`| img-src | 'self' data:' |`). Ein `fetch()` auf eine `data:`-URL wird vom
Browser gegen `connect-src` geprüft (unabhängig vom URL-Schema) — die
Blöcke 8b und 7j wurden nie zusammen gegen diese Interaktion verifiziert.
Widerspruch zwischen den zugehörigen ADRs 0020 und 0025, der gemeldet wird,
nicht stillschweigend aufgelöst.

Der serverseitige Catch-Block in `DownloadCoverImageAsDataUrlAsync`
(`DiscogsClient.cs:122-126`) verschluckt Fehler ohne Log-Eintrag —
bestätigt in der Notiz, unabhängig von der CSP-Hypothese ein eigener
Mangel.

**Weitere verifizierte Fakten:**

- `DiscogsSearchResult`/`DiscogsRelease` behandeln `ThumbnailUrl`/
  `CoverImageUrl` als reinen String — Frontend-Vertrag ändert sich nicht,
  ob dort eine `https://`- oder `data:`-URL steht.
- Keine bestehenden Unit-Tests für `DiscogsClient` (nur
  `DiscogsEndpointsTests` in `IntegrationTests`, prüft nur 401 vor
  Handler-Erreichen). Kein `HttpMessageHandler`-Test-Double existiert
  bisher im Projekt.
- Test-Doubles: `NSubstitute` (nicht Moq) ist in
  `MyMusic.Infrastructure.Tests` referenziert.
- Kein `ILogger`/Serilog-Aufruf existiert bisher in Application/
  Infrastructure — `AddSerilog` in `Program.cs:5` stellt `ILogger<T>` aber
  bereits per DI bereit.
- `GlobalUsing.cs` (Infrastructure) hat noch kein
  `global using Microsoft.Extensions.Logging;`.
- Bildhost: Such-Thumbnails/Cover kommen von Discogs' Bild-CDN, nicht von
  `api.discogs.com` (dem ratenlimitierten Endpunkt) — zusätzliche
  Bild-Downloads pro Suche zählen nicht gegen das API-Rate-Limit.

## Geplanter Fix

### Bug 1 — Suchtreffer-Thumbnails serverseitig einbetten

`DiscogsClient.SearchAsync`: nach dem Mapping der Ergebnisse für jedes
Element parallel (`Task.WhenAll`) das Thumbnail über eine gemeinsame,
aus `DownloadCoverImageAsDataUrlAsync` extrahierte Hilfsmethode
(z. B. `DownloadImageAsDataUrlAsync(string? url, ...)`, wiederverwendet von
Cover **und** Thumbnail) als Data-URL laden und einbetten. Schlägt der
Download für ein einzelnes Ergebnis fehl, bleibt `ThumbnailUrl` für genau
dieses Element `null` (Frontend zeigt bereits das Platzhalter-Icon) — die
Suche als Ganzes schlägt nicht fehl.

### Logging ergänzen

- `GlobalUsing.cs` (Infrastructure): `global using Microsoft.Extensions.Logging;`
  ergänzen.
- `DiscogsClient` erhält `ILogger<DiscogsClient>` per Konstruktor-Injection.
- Im Catch-Block der gemeinsamen Download-Hilfsmethode: strukturierter
  `LogWarning` auf Deutsch mit Bild-URL und Fehlermeldung, keine Tokens/PII.

### Bug 2 — Ursache live verifizieren, dann gezielt fixen

Vor dem endgültigen Fix: Aspire-AppHost starten, mit Claude-in-Chrome einen
Discogs-Treffer im RecordForm übernehmen und die Browser-Konsole prüfen, ob
dort eine CSP-Verletzung auf `connect-src` beim `fetch()`-Aufruf auf die
`data:`-URL erscheint.

- **Falls bestätigt** (erwarteter Fall): `applyDiscogsCover` in
  `record-form.ts` wird von `fetch(coverImageUrl)` auf eine
  netzwerkfreie Data-URL→`File`-Konvertierung umgestellt (Base64-Teil
  manuell dekodieren, `Uint8Array` → `File`) — kein `fetch()` mehr nötig,
  da die Bilddaten bereits vollständig inline vorliegen. Umgeht den
  CSP-Konflikt an der Fehlerstelle, ohne die im Wiki dokumentierten
  CSP-Mindest-Direktiven (`connect-src` ohne `data:`) aufzuweichen. Neue
  ADR-Datei hält die Erkenntnis fest (Interaktion zwischen ADR 0020 und
  ADR 0025, die keiner der beiden ADRs vorausgesehen hat) sowie die
  verworfene Alternative (`connect-src` um `data:` erweitern — hätte die
  Wiki-Mindestvorgabe aufgeweicht und mehr als die aktuell fehlerhafte
  Stelle betroffen).
- **Falls nicht bestätigt**: Das neue Logging zeigt die tatsächliche
  Ursache im Backend; Fix wird dann entsprechend dem tatsächlichen Befund
  neu geplant — in diesem Fall wird vor weiterer Umsetzung erneut kurz
  Rücksprache gehalten, da der Rest dieses Plans auf der CSP-Hypothese
  aufbaut.

### Tests

- Neue `tests/MyMusic.Infrastructure.Tests/ExternalServices/Discogs/DiscogsClientTests.cs`:
  kleines `HttpMessageHandler`-Test-Double (neues Muster im Projekt),
  Fälle: Thumbnail-Download erfolgreich → Data-URL, Thumbnail-Download
  schlägt fehl → `null` + `LogWarning` (`ILogger` via NSubstitute),
  bestehendes Cover-Verhalten weiterhin grün.
- `discogs-search.spec.ts`: bestehende Erwartungen bleiben gültig (String
  bleibt String), ggf. Testdaten-Kommentar anpassen, falls dort explizit
  eine `https://`-URL als Beispiel vorkommt.
- Je nach Ausgang der Bug-2-Verifikation: `record-form.spec.ts`/vorhandene
  Discogs-Cover-Tests auf das neue Konvertierungsverhalten anpassen (kein
  `fetch`-Mock mehr nötig für diesen Pfad).

### Dokumentation

- Ggf. neue ADR (siehe Bug-2-Abschnitt).
- `TASK.md` Abschnitt „8. Discogs-Integration": neuer
  „Nachbesserungen"-Absatz analog zum bestehenden vom 2026-08-22, mit
  Verweis auf diesen Fix-Prompt.
- `CLAUDE.md` (Repo-Wurzel): neuer „Stand"-Absatz.
- Notiz `03 Ressourcen/2026-08-30-discogs-cover-bug-untersuchung.md` bleibt
  unverändert (laut Auftrag nicht dauerhaft archiviert, keine Pflege
  vorgesehen).

## Geplante Verifikation

1. `dotnet build`, `dotnet test` (Infrastructure, Application, Api, Domain),
   `dotnet format --verify-no-changes`, Zeilenlängen-Check.
2. `ng lint`, `ng test --watch=false` im Frontend-Workspace.
3. Aspire-AppHost lokal starten; mit Claude-in-Chrome:
   - Discogs-Suche ausführen → Vorschaubilder erscheinen in der
     Trefferliste (Bug 1).
   - Treffer übernehmen und Record speichern → Cover wird übernommen und
     gespeichert, sichtbar auf RecordCard/Detailansicht (Bug 2).
   - Browser-Konsole auf neue Log-Einträge bzw. Abwesenheit von
     CSP-Fehlern prüfen.
4. Diff vor Commit zeigen und Freigabe für Commit/Push/PR gesondert
   einholen.

## Bekannte Risiken und offene Punkte

- Die CSP-Hypothese für Bug 2 ist bis zur Live-Verifikation nicht
  bestätigt — der Plan verzweigt an dieser Stelle ausdrücklich.
- Ob Discogs' Bild-CDN für parallele Thumbnail-Downloads eigene
  Rate-Limits hat, ist nicht verifiziert — wird bei der Live-Prüfung
  sichtbar (HTTP-429/-Fehler bei vielen Treffern).
