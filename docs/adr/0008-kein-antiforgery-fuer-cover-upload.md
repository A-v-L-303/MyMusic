# ADR 0008 — Kein Antiforgery-Schutz für den Cover-Upload-Endpunkt

**Status**: Angenommen
**Datum**: 2026-08-07
**Betrifft**: `MyMusic.Api` (`POST /api/records/{id}/cover`)

## Kontext

Block 6b führt mit dem Cover-Upload-Endpunkt (`POST /api/records/{id}/cover`,
`multipart/form-data`) den ersten Datei-Upload und die erste Bindung eines
Formularparameters (`IFormFile`) in diesem Projekt ein. Seit .NET 8 lehnen
Minimal-API-Endpunkte, die Formulardaten binden, eine Anfrage standardmäßig
ab, sofern der Endpunkt nicht entweder Antiforgery-Tokens validiert oder
explizit `.DisableAntiforgery()` aufruft — dieser Mechanismus schützt
klassischerweise Cookie-basierte Formulare vor Cross-Site-Request-Forgery.

Diese API verwendet ausschließlich JWT-Bearer-Authentifizierung
(`AddAuthentication().AddJwtBearer()`, siehe ADR 0004 und
Sicherheitskonzept §5.1) — es gibt keine Cookie-basierte Session, gegen die
ein Cross-Site-Request überhaupt greifen könnte. Ein Angreifer, der ein Bearer
Token nicht kennt, kann den Endpunkt nicht aufrufen; ein automatisch
mitgesendetes Cookie, das ein CSRF-Angriff ausnutzen würde, existiert nicht.

## Entscheidung

Der Cover-Upload-Endpunkt ruft `.DisableAntiforgery()` explizit auf, statt
eine Antiforgery-Infrastruktur (Cookie, Token-Erzeugung, Validierung) neu
aufzubauen.

## Begründung

- Antiforgery-Tokens schützen gegen Angriffe, die auf automatisch
  mitgesendeten Anmeldeinformationen (Cookies) beruhen. Diese API sendet
  keine Cookies; die einzige Anmeldeinformation ist ein vom Client explizit
  im `Authorization`-Header mitgesendetes Bearer-Token, das ein
  browserbasierter Cross-Site-Angriff nicht automatisch mitschicken kann.
- Eine zusätzliche Antiforgery-Infrastruktur nur für einen einzelnen
  Upload-Endpunkt einzuführen, obwohl kein Cookie-basierter Angriffsvektor
  besteht, wäre Mehraufwand ohne Sicherheitsgewinn und ein Fremdkörper in
  einer sonst durchgängig zustandslosen, Bearer-Token-basierten API.

## Konsequenzen

- `.DisableAntiforgery()` gilt ausschließlich für diesen einen Endpunkt, nicht
  projektweit — künftige Formular-/Datei-Upload-Endpunkte müssen dieselbe
  Prüfung individuell durchführen und ggf. dieselbe Begründung erneut
  anwenden.
- Sollte die Anwendung künftig zusätzlich Cookie-basierte Authentifizierung
  einführen (aktuell nicht geplant), ist diese Entscheidung neu zu bewerten.
