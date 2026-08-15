# Block 7f — Keycloak-Login-Theme "mymusic"

## Kontext

TASK.md Abschnitt 7 listet als offenen Punkt „Keycloak-Custom-Theme der Anmeldeseite" (US-AU8,
`02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-authentifizierung.md`). Die Story wurde bei
Block 7a bewusst ausgeklammert („eigener, späterer Schritt", siehe
`docs/prompts/2026-08-09-block-7a-login-flow.md`). Der Projektinhaber hat Abschnitt 7 in sechs
Teilblöcke (7a–7f) zerlegt und 7f als ersten davon freigegeben:

> Als Benutzer möchte ich, dass die Anmeldeseite wie ein Teil von MyMusic aussieht, damit der
> Anmeldevorgang nicht wie ein fremdes System wirkt.

Akzeptanzkriterien: eigenes Keycloak-Theme statt Standard-Theme; Farben/Typografie entsprechen den
Design-Tokens; `mark.svg` + Wortmarke „MyMusic" erscheinen auf der Seite. Betroffen ist ausschließlich
die Login-Seite (Registrierung ist im Realm deaktiviert, `resetPasswordAllowed` nicht gesetzt) — kein
Angular-Code, keine Account-Console, keine E-Mail-Templates.

## Ist-Stand (verifiziert)

- `keycloak/mymusic-realm.json`: vor diesem Block kein `loginTheme`-Feld gesetzt, kein `themes/`-Ordner
  im Repo, keine Altlasten eines früheren Versuchs (per Grep über das ganze Repo geprüft).
- `src/MyMusic.AppHost/AppHost.cs`: Keycloak läuft als
  `AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.5")` mit
  `.WithBindMount("../../keycloak", "/opt/keycloak/data/import", isReadOnly: true)` für den
  Realm-Import, `.WithVolume("mymusic-keycloak-data", "/opt/keycloak/data")` für Persistenz, Start via
  `.WithArgs("start-dev", "--import-realm")`. Kein eigenes Docker-Image im Projekt — ausschließlich
  Bind-Mounts auf das offizielle Image.
- Design-Tokens: `src/frontend/src/styles/design-system/colors_and_type.css` (Emerald
  `#10b981`/`#059669`/`#047857`, vollständige Light-/Dark-Semantik als CSS Custom Properties, Font
  Inter). `mark.svg` liegt unter `src/frontend/public/mark.svg`; Inter-woff2-Dateien liegen bereits
  lokal unter `src/frontend/node_modules/@fontsource/inter/files/` (keine neue Abhängigkeit, nur
  Dateikopie).
- Keine automatisierten Tests decken Keycloak-Theme-Rendering ab; `MyMusic.IntegrationTests` prüft nur
  die Admin-REST-API und Token-Endpunkte, kein HTML/CSS. Verifikation ist damit wie bei Block 0f/0g
  ausschließlich manuell/live gegen den laufenden Aspire-AppHost.

### Empirische Prüfung des tatsächlichen Keycloak-26.5-Theme-Mechanismus

Offizielle Doku und Web-Recherche allein reichten nicht aus, um die exakte Struktur sicher zu belegen
(insbesondere PatternFly-Variablennamen und CSS-Ladereihenfolge). Deshalb wurde das reale
`quay.io/keycloak/keycloak:26.5`-Image lokal per `docker create`/`docker cp` inspiziert
(`keycloak-themes-26.5.7.jar`, `keycloak-themes-vendor-26.5.7.jar`, temporär, `--rm`, keine
Auswirkung auf Projekt-Container/-Volumes):

- Aktives Standard-Login-Theme ist `keycloak.v2` (PatternFly-v5-basiert), `parent=base`. Ein
  Custom-Theme setzt `parent=keycloak.v2`.
- `keycloak.v2/login/theme.properties` deklariert `styles=css/styles.css`,
  `stylesCommon=vendor/patternfly-v5/patternfly.min.css vendor/patternfly-v5/patternfly-addons.css`,
  `darkMode=true`.
- `keycloak.v2/login/resources/css/styles.css` (116 Zeilen) legt ein vollflächiges Hintergrundbild auf
  `<body>` (`--keycloak-bg-logo-url`) und stylt `#kc-header-wrapper` mit `color: ... !important`,
  Großbuchstaben, Letter-Spacing — passt nicht zum MyMusic-Kartenlayout. **Entscheidung**: `mymusic`
  importiert `css/styles.css` bewusst NICHT, sondern nur `stylesCommon` (PatternFly-Komponenten,
  zwingend nötig) und `import=common/keycloak` (liefert nur `favicon.ico` + PatternFly-Icon-Font,
  unkritisch) sowie eine eigene `css/mymusic.css`.
- `keycloak.v2/login/template.ftl`: Der Marken-Header (`#kc-header-wrapper`) rendert ausschließlich
  `${msg("loginTitleHtml", realm.displayNameHtml)}` — kein `<img>`-Slot. Reines CSS reicht für das
  Akzeptanzkriterium „Markenzeichen erscheint" nicht aus; Keycloak erlaubt nur vollständige
  Template-Overrides, keine partiellen Patches. `template.ftl` wurde deshalb 1:1 aus dem realen Image
  kopiert, einzige Änderung: der `#kc-header-wrapper`-Inhalt bekommt zusätzlich ein `<img>` mit
  `mark.svg` (leeres `alt`, da die textuelle Wortmarke direkt daneben denselben Inhalt trägt).
- PatternFly v5 löst Theming über globale CSS Custom Properties: `.pf-v5-c-login__main` verwendet
  `background-color: var(--pf-v5-c-login__main--BackgroundColor)`, das an
  `var(--pf-v5-global--BackgroundColor--light-100)` gebunden ist; dieselbe Variable wird innerhalb von
  `:where(.pf-v5-theme-dark)` auf einen anderen (dunklen) Wert umdefiniert. Primärfarbe läuft über
  `--pf-v5-global--primary-color--100` (Buttons, Fokus-Rahmen der Formularfelder binden direkt darauf).
  `:where()` hat Spezifität 0 — eine eigene, später geladene Regel mit denselben Selektoren gewinnt
  zuverlässig ohne `!important`. Dark Mode wird von `template.ftl` selbst per
  `prefers-color-scheme`-Listener gesteuert (JS setzt/entfernt die Klasse `pf-v5-theme-dark` auf
  `<html>`), kein eigener Mechanismus nötig.
- `start-dev` deaktiviert Theme-/Template-Caching automatisch — kein `kc.sh build`-Schritt nötig,
  Änderungen wirken ohne Neustart.
- Realm-Feld heißt exakt `loginTheme` (Top-Level-String in `RealmRepresentation`).

## Entscheidungen mit Empfehlung

1. **Font-Strategie: Inter self-hosten** (400+600, aus vorhandenem `@fontsource/inter`-Paket kopiert)
   statt System-Font oder CDN — keine Netzwerkabhängigkeit auf der Login-Seite, kein CSP-Sonderfall,
   konsistent zur Angular-Entscheidung (Block 0c).
2. **Dark Mode: unterstützt**, über denselben `prefers-color-scheme`-Mechanismus, den `keycloak.v2`
   bereits mitbringt (`darkMode=true`) — kein eigener Toggle nötig oder möglich (vor dem Login existiert
   keine gespeicherte Präferenz).
3. **`template.ftl` wird vollständig kopiert und lokal gepflegt** (nur der Header-Block geändert), da
   Keycloak keine partiellen Template-Overrides erlaubt. Bekanntes Risiko: bei künftigem
   Keycloak-Versionsupgrade muss diese Datei manuell mit der neuen `keycloak.v2/login/template.ftl`
   abgeglichen werden.
4. **`css/styles.css` des Parent-Themes wird nicht importiert** (siehe oben) — vermeidet Konflikte mit
   dem vollflächigen Hintergrundbild und dem `!important`-Header-Styling, ohne PatternFly selbst
   (`stylesCommon`) zu verlieren.

## Schritte

1. `keycloak/themes/mymusic/login/theme.properties` — `parent=keycloak.v2`, `import=common/keycloak`,
   `styles=css/mymusic.css`, `stylesCommon=vendor/patternfly-v5/patternfly.min.css
   vendor/patternfly-v5/patternfly-addons.css`, `darkMode=true`.
2. `keycloak/themes/mymusic/login/template.ftl` — 1:1-Kopie aus dem realen 26.5-Image, Header-Block um
   `<img src="${url.resourcesPath}/img/mark.svg" alt="" class="mymusic-mark" width="28" height="28" />`
   ergänzt, Wrapper-Klasse `mymusic-brand` zusätzlich zu `pf-v5-c-brand`.
3. `keycloak/themes/mymusic/login/resources/css/mymusic.css` — PatternFly-Globals
   (`--pf-v5-global--primary-color--100`, `--pf-v5-global--link--Color[--hover]`,
   `--pf-v5-global--BackgroundColor--light-100`, `--pf-v5-global--Color--100`) auf die MyMusic-Tokens
   gemappt, jeweils für `:root` (Light) und `:where(.pf-v5-theme-dark)` (Dark); Karten-Rahmen/-Radius/
   -Schatten, Marken-Layout (`.mymusic-brand`/`.mymusic-mark`/`.mymusic-brand-word`), Inter-`@font-face`.
4. `keycloak/themes/mymusic/login/resources/img/mark.svg` — unveränderte Kopie aus
   `src/frontend/public/mark.svg`.
5. `keycloak/themes/mymusic/login/resources/fonts/inter-latin-400-normal.woff2`,
   `inter-latin-600-normal.woff2` — unveränderte Kopie aus
   `src/frontend/node_modules/@fontsource/inter/files/`.
6. `keycloak/mymusic-realm.json`: `"loginTheme": "mymusic"` ergänzen.
7. `src/MyMusic.AppHost/AppHost.cs`: neuer Bind-Mount
   `.WithBindMount("../../keycloak/themes/mymusic", "/opt/keycloak/themes/mymusic", isReadOnly: true)`
   auf der `keycloak`-Ressource.
8. ADR `docs/adr/0014-keycloak-login-theme.md`.
9. Live-Verifikation über den Aspire-AppHost.
10. Wiki-Nachtrag (`wiki/tech-stack/keycloak.md`,
    `wiki/architektur/aspire-orchestrierung.md`, US-AU8-Status in
    `wiki/user-stories/user-stories-authentifizierung.md`) und `TASK.md` Abschnitt 7f.

## Verifikation

| Schritt | Prüfung |
|---|---|
| Aspire-AppHost starten (`dotnet run --project src/MyMusic.AppHost`, PowerShell) | Kein Fehler beim Keycloak-Start, Realm-Import ohne Fehler |
| Keycloak-Container-Log im Aspire-Dashboard | Theme `mymusic` wird erkannt, kein „theme not found" |
| Browser: Login-Seite via Angular-Redirect öffnen | `mark.svg` + „MyMusic" sichtbar, Emerald-Akzent, Inter-Font geladen (Netzwerk-Tab: woff2 aus Theme-Resources, kein Google-Fonts-Request) |
| DevTools: `prefers-color-scheme: dark` simulieren | Dark-Tokens greifen sichtbar |
| Login mit bestehendem/neu angelegtem Testbenutzer | Technisch unverändert funktionsfähig (Redirect zurück zu Angular, gültiges Token) |
| Fehleranmeldung (falsches Passwort) | Fehlermeldung erscheint weiterhin, im MyMusic-Design |

## Risiken und offene Punkte

- `template.ftl` als lokale Vollkopie → Drift-Risiko bei künftigem Keycloak-Upgrade (im ADR
  dokumentiert).
- Die CSS-Variablen-Überschreibung deckt die wichtigsten, empirisch bestätigten Stellen ab
  (Primärfarbe, Links, Kartenhintergrund, Haupttextfarbe); Detailabweichungen an selteneren
  Komponenten (z. B. Zwei-Faktor-/Recovery-Code-Ansichten) sind nicht einzeln durchgeprüft, da für
  diesen Realm nicht aktiv (kein OTP/WebAuthn konfiguriert).
- Kein automatisierter Test für Theme-Rendering (Projektkonvention, kein neues Risiko).
- Falls im Keycloak-Datenvolume kein Testbenutzer mehr vorhanden ist, muss vor der Login-Verifikation
  manuell einer angelegt werden (keine Zugangsdaten werden committet).
