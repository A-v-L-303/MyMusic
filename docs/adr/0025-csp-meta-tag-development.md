# ADR 0025 — Content Security Policy per Meta-Tag (Development/lokal)

**Status**: Angenommen
**Datum**: 2026-08-26
**Betrifft**: `src/frontend`

## Kontext

`wiki/sicherheit/sicherheitskonzept.md` verlangt eine Content Security Policy
für den Angular-Client, lokal per `<meta http-equiv="Content-Security-Policy">`
in `index.html` (Angular Dev Server unterstützt keine HTTP-Header-
Konfiguration), in Production per HTTP-Response-Header vom Webserver (Nginx).
Mindest-Direktiven laut Wiki: `default-src 'self'`, `script-src 'self'`,
`style-src 'self'`, `connect-src 'self'` + Keycloak-URL + API-URL,
`img-src 'self' data:'`.

Im Repository existiert keinerlei Production-Infrastruktur (kein Docker
Compose, kein Nginx — siehe `wiki/projekt/deployment-konzept.md`, Hosting-
Anbieter noch nicht entschieden). Mit dem Projektinhaber vor Planbeginn
geklärt (Block 7j): Dieser ADR und die zugehörige Umsetzung decken
ausschließlich die Development/lokal-Variante (Meta-Tag) ab. Die
Production-Variante (Nginx-Header) bleibt bewusst offen und hängt vom noch
nicht begonnenen Production-/Docker-Compose-Setup ab.

Zwei technische Hürden mussten vor einer wörtlichen Umsetzung der
Wiki-Mindest-Direktiven gelöst werden (beide gegen den tatsächlichen
Repository- bzw. Angular-Stand verifiziert, nicht angenommen):

1. `index.html` enthielt ein Inline-`<script>` (Block 0f, FOUC-Vermeidung:
   liest `localStorage.getItem('mymusic-theme')` und setzt `data-theme` vor
   dem ersten Rendering). `script-src 'self'` blockiert Inline-Skripte ohne
   Nonce/Hash — das hätte die Theme-Infrastruktur aus Block 0f sichtbar
   gebrochen.
2. Angular injiziert für `ViewEncapsulation` komponenteneigene `<style>`-Tags
   zur Laufzeit. `style-src 'self'` ohne Weiteres hätte jede Komponenten-
   Formatierung blockiert. Angular stellt dafür offiziell einen
   `CSP_NONCE`-Mechanismus bereit: Wird kein `CSP_NONCE`-DI-Token gesetzt,
   liest Angular den Wert aus dem `ngCspNonce`-Attribut des Root-Knotens
   (`@angular/core/types/core.d.ts`, Kommentar zu `CSP_NONCE` — installierte
   Version 22.1.x, verifiziert).

## Entscheidung

- **Theme-Script ausgelagert**: Der bisherige Inline-Code liegt jetzt in
  `public/theme-init.js`, eingebunden über `<script src="theme-init.js">` an
  derselben Stelle im `<head>` — bleibt render-blocking und synchron vor dem
  ersten Rendering, `script-src 'self'` deckt same-origin-Dateien ohne
  weitere Ausnahme ab.
- **Nonce-basiertes `style-src`**: `<app-root ngCspNonce="...">` erhält einen
  pro Build zufällig erzeugten Nonce-Wert (`crypto.randomBytes(16).
  toString('base64')`); derselbe Wert steht im Meta-Tag unter
  `style-src 'self' 'nonce-...'`.
- **Build-Zeit-Injektion nach bestehendem Muster**: Neues Skript
  `scripts/write-csp-meta.mjs`, eigenständig neben dem bestehenden
  `scripts/write-runtime-config.mjs` (ADR 0009) — beide laufen über dieselben
  `prestart`/`prebuild`-npm-Hooks und lesen dieselben
  `MYMUSIC_API_BASE_URL`/`MYMUSIC_KEYCLOAK_AUTHORITY`-Umgebungsvariablen, aus
  denen `connect-src` per `new URL(...).origin` die beiden Origins ableitet
  (nur Origin, nicht der volle Pfad wie `/realms/mymusic`). Der Platzhalter-
  Kommentar `<!-- CSP_META_PLACEHOLDER -->` sowie `ngCspNonce="__CSP_NONCE__"`
  bleiben als eingecheckte Baseline in `index.html` (analog zur eingecheckten
  Platzhalter-`runtime-config.json` aus ADR 0009) — ein `ng build`/`ng serve`
  ohne vorherigen Skriptlauf liefert damit weiterhin gültiges, wenn auch ohne
  CSP-Schutz ausgeliefertes HTML.
- Fehlen `MYMUSIC_API_BASE_URL`/`MYMUSIC_KEYCLOAK_AUTHORITY` oder sind sie
  keine gültige URL, fällt `connect-src` auf `'self'` allein zurück (kein
  Skriptabbruch) — entspricht dem bestehenden leeren-Platzhalter-Verhalten
  von `runtime-config.json`.

## Verworfene Alternativen

- **Hash-Pinning für das Theme-Script** (`script-src 'self'
  'sha256-...'` statt Auslagerung): hätte den Hash bei jeder Änderung am
  Skriptinhalt manuell/über Tooling nachziehen müssen und wäre fehleranfälliger
  als eine same-origin-Datei, die `script-src 'self'` ohnehin abdeckt.
- **`'unsafe-inline'` für `style-src`**: würde den Schutzzweck der CSP gegen
  XSS über injizierte Styles weitgehend aufheben — genau die Angriffsklasse,
  die die Wiki-Vorgabe adressieren soll. Der offizielle Angular-Nonce-
  Mechanismus existiert genau für diesen Fall.
- **CSP-Injektion in `write-runtime-config.mjs` statt eigenem Skript**: hätte
  dessen bereits verifiziertes, seit Block 0c genutztes Verhalten (reines
  Schreiben einer JSON-Datei) um eine strukturell andere Aufgabe
  (HTML-Textersetzung) erweitert — ein eigenständiges Skript mit derselben
  Env-Var-Quelle hält beide Verantwortlichkeiten getrennt.

## Konsequenzen

- Nach jedem lokalen `npm start`/`npm run build` zeigt `git status`
  `index.html` als geändert an (neuer Nonce, aufgelöste `connect-src`-Origins)
  — analog zu `runtime-config.json` (ADR 0009) erwartetes, nicht zu
  committendes Arbeitsverzeichnis-Rauschen.
- `write-csp-meta.mjs` ersetzt beim erneuten Lauf sowohl ein bereits
  vorhandenes CSP-Meta-Tag als auch einen bereits gesetzten `ngCspNonce`-Wert
  per Regex (nicht nur den ursprünglichen Platzhalter) — das Skript ist damit
  über beliebig viele aufeinanderfolgende Läufe hinweg wiederholbar, ohne
  doppelte Tags oder veraltete Nonces zu hinterlassen (manuell verifiziert:
  zwei aufeinanderfolgende Läufe mit unterschiedlichen Umgebungsvariablen).
- Ob Angulars Dev-Server (`ng serve`, über Aspire per `AddJavaScriptApp`
  gestartet) für HMR/Live-Reload zusätzliche CSP-Erlaubnisse braucht (z. B.
  `connect-src` für einen WebSocket, `'unsafe-eval'` für Sourcemaps), wird
  bei der Live-Verifikation gegen den laufenden Aspire-AppHost sichtbar
  (Konsolenfehler bei CSP-Verstößen) und bei Bedarf in einer Nachtrags-Notiz
  zu diesem ADR nachgezogen, ohne die Grundarchitektur zu ändern.
- CSP-Production (HTTP-Header vom Nginx) ist explizit **nicht** Teil dieses
  ADRs und bleibt in TASK.md als offener Punkt geführt, abhängig vom noch
  nicht begonnenen Production-/Docker-Compose-Setup.
