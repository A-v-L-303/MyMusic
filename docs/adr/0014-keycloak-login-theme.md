# ADR 0014 — Keycloak-Login-Theme "mymusic"

**Status**: Angenommen
**Datum**: 2026-08-15
**Betrifft**: `keycloak/`, `src/MyMusic.AppHost`

## Kontext

US-AU8 (`wiki/user-stories/user-stories-authentifizierung.md`) verlangt, dass die
Keycloak-Anmeldeseite wie ein Teil von MyMusic wirkt: eigenes Theme statt Standard-Theme,
Farben/Typografie aus den Design-Tokens, `mark.svg` + Wortmarke „MyMusic" sichtbar. Die Story
wurde bei Block 7a bewusst ausgeklammert und in Block 7f nachgeholt.

Keycloak 26.5 verwendet als aktives Standard-Login-Theme `keycloak.v2` (PatternFly-v5-basiert,
`parent=base`). Offizielle Doku und allgemeine Tutorials beschreiben den Theming-Mechanismus nur
grob; die konkreten CSS-Variablennamen und die Ladereihenfolge waren daraus nicht sicher zu
belegen. Deshalb wurde das reale `quay.io/keycloak/keycloak:26.5`-Image lokal per `docker
create`/`docker cp` inspiziert (Themes liegen als JAR-Ressourcen unter
`/opt/keycloak/lib/lib/main/org.keycloak.keycloak-themes-26.5.7.jar` und
`...-themes-vendor-26.5.7.jar`, temporär entpackt, keine Auswirkung auf Projekt-Container).

## Entscheidung

1. **Eigenes Theme `mymusic` mit `parent=keycloak.v2`**, unter `keycloak/themes/mymusic/login/`,
   nur für den Theme-Typ `login` (kein `account`-/`email`-Theme — beides nicht Teil von US-AU8).
   Der Realm setzt `"loginTheme": "mymusic"`.

2. **`css/styles.css` des Parent-Themes wird bewusst nicht vollständig importiert.** Diese Datei
   legt ein vollflächiges Hintergrundbild auf `<body>` und stylt `#kc-header-wrapper` mit
   `color: ... !important`, Großbuchstaben und Letter-Spacing — passt nicht zum
   MyMusic-Kartenlayout. `theme.properties` importiert stattdessen nur `stylesCommon`
   (PatternFly-v5-Komponenten-CSS, zwingend nötig) und `import=common/keycloak` (liefert nur
   `favicon.ico` + PatternFly-Icon-Font, unkritisch), plus eine eigene `css/mymusic.css`. Diese
   Datei enthält allerdings nicht nur Kosmetik, sondern auch die einzige Struktur-Regel
   `.pf-v5-c-login__container { grid-template-columns: 34rem; grid-template-areas: "header" "main" }`
   — ohne sie stand der Marken-Header bei der Live-Prüfung frei neben statt über der Karte.
   `mymusic.css` übernimmt genau diese eine Regel unverändert, der Rest von `styles.css` bleibt
   draußen.

3. **Theming über direkte Eigenschaften auf konkreten Komponentenselektoren, nicht primär über
   globale PatternFly-CSS-Variablen.** Ursprünglich war geplant, ausschließlich globale Variablen
   wie `--pf-v5-global--primary-color--100`/`--pf-v5-global--BackgroundColor--light-100` zu
   überschreiben. Die Live-Prüfung zeigte zwei Probleme damit: Erstens hat `:where(.pf-v5-theme-dark)`
   Spezifität 0 und verlor gegen den eigenen `:root`-Block — der Dark-Mode-Override griff dadurch
   nie. Zweites, tieferes Problem: PatternFly bindet mehrere Komponentenfarben im Dark Mode intern
   an eigene, anders benannte Varianten (z. B. den Primärbutton an
   `--pf-v5-global--primary-color--dark-100`, eine eigene Variable, nicht einfach denselben Namen
   mit anderem Wert) — ein reines Überschreiben der „hellen" Variable griff deshalb selbst mit
   korrekter Spezifität nicht durchgängig. `mymusic.css` überschreibt deshalb `background-color`,
   `border`, `box-shadow`, `color` direkt auf `.pf-v5-c-login__main`, `.pf-v5-c-title`,
   `.pf-v5-c-button.pf-m-primary` usw. — mit eigenen `--mm-*`-Tokens, die unter `:root` (Light) und
   `html.pf-v5-theme-dark` (Dark, Typ+Klassenselektor statt `:where()`) gesetzt werden. Global
   überschrieben bleibt nur `--pf-v5-global--primary-color--100`, ohne nachweisbaren Effekt auf den
   Fokus-Rahmen der Formularfelder (siehe Konsequenzen) — dort bleibt bewusst die
   PatternFly-Standardfarbe (Blau) bestehen, da die genaue interne Variable dafür nicht gefunden
   wurde und der Aufwand für dieses kosmetische Detail nicht gerechtfertigt war.

4. **Marke im Header über einen `template.ftl`-Override, nicht per CSS.** Der Marken-Header
   (`#kc-header-wrapper`) rendert im Parent-Theme nur `${msg("loginTitleHtml", ...)}` — keinen
   `<img>`-Slot. Keycloak erlaubt nur vollständige Template-Overrides, keine partiellen Patches.
   `keycloak/themes/mymusic/login/template.ftl` ist deshalb eine 1:1-Kopie aus dem realen
   26.5-Image; einzige Änderung ist ein zusätzliches `<img>` mit `mark.svg` neben der bestehenden,
   weiterhin lokalisierten Wortmarke (`realm.displayNameHtml`, im Realm bereits „MyMusic").

5. **Dark Mode ohne eigenen Mechanismus.** `keycloak.v2` bringt bereits `darkMode=true` mit:
   `template.ftl` setzt per `prefers-color-scheme`-Listener die Klasse `pf-v5-theme-dark` auf
   `<html>`. `mymusic` übernimmt dieses Verhalten unverändert und überschreibt nur die Farbwerte
   innerhalb dieser Klasse — kein expliziter Toggle (vor dem Login existiert keine gespeicherte
   Präferenz, analog zum OS-Fallback in ADR 0011).

6. **Inter self-hosted statt System-Font oder CDN.** Zwei Gewichte (400, 600, `latin`, `woff2`)
   aus dem bereits im Projekt vorhandenen `@fontsource/inter`-Paket in
   `keycloak/themes/mymusic/login/resources/fonts/` kopiert (keine neue Abhängigkeit).

7. **Bind-Mount statt eigenem Docker-Image.** `AppHost.cs` erhält einen zusätzlichen
   `.WithBindMount("../../keycloak/themes/mymusic", "/opt/keycloak/themes/mymusic",
   isReadOnly: true)` auf der `keycloak`-Ressource, analog zum bestehenden Realm-Import-Bind-Mount.
   Kein `kc.sh build`-Schritt nötig — `start-dev` deaktiviert Theme-/Template-Caching automatisch,
   Änderungen wirken ohne Neustart.

8. **Deutsche Anmeldeseite (`internationalizationEnabled: true`, `supportedLocales: ["de"]`,
   `defaultLocale: "de"`).** Bei der Live-Prüfung fiel auf, dass Keycloak ohne aktivierte
   Internationalisierung ausnahmslos englische Standardtexte liefert — unabhängig vom Theme.
   Nachträglich mit dem Projektinhaber abgestimmt (nicht Teil des ursprünglichen US-AU8-Scopes,
   aber konsistent mit der projektweiten Regel „UI-Texte deutsch",
   `design-system-überblick.md`). Keycloak liefert deutsche Übersetzungen für den Theme-Typ
   `login` bereits vollständig mit (`base/login/theme.properties`, `locales=...,de,...`) — keine
   eigenen Übersetzungsdateien nötig. Nur `de` als unterstützte Sprache, kein Sprachumschalter
   (die Angular-Anwendung selbst ist ebenfalls einsprachig Deutsch ohne i18n-Mechanismus).

## Begründung

Ein vollständig neues Theme ohne `parent` zu schreiben hieße, sämtliche Login-, Fehler- und
Formular-Templates (Zwei-Faktor, WebAuthn, Passwort-Reset, Registrierung …) selbst zu pflegen —
unverhältnismäßiger Aufwand für eine Anforderung, die nur Farben/Typografie/Marke betrifft.
Das Erweitern von `keycloak.v2` über CSS-Variablen folgt dem von PatternFly selbst vorgesehenen
Theming-Mechanismus und bleibt robust gegenüber internen Markup-Details einzelner Komponenten.
Der punktuelle `template.ftl`-Override ist auf das technisch zwingend Notwendige (Marke im Header)
begrenzt, statt das gesamte Template ohne Not zu verändern.

## Konsequenzen

- `template.ftl` ist eine lokal gepflegte Vollkopie: Bei einem künftigen Keycloak-Versionsupgrade
  muss diese Datei manuell mit der dann aktuellen `keycloak.v2/login/template.ftl` abgeglichen
  werden (Drift-Risiko). Betrifft ausschließlich diese eine Datei — `theme.properties` und
  `mymusic.css` sind reine Erweiterungen und upgrade-stabiler.
- Nur der Theme-Typ `login` ist abgedeckt. Ein künftiger, eigener Block müsste `account`- oder
  `email`-Theming separat entscheiden, falls das je gefordert wird — nicht durch diesen ADR
  vorweggenommen.
- Kein automatisierter Test deckt das Theme-Rendering ab (Projektkonvention für rein
  visuelle/Infrastruktur-Änderungen, siehe Block 0f/0g) — Verifikation bleibt manuell/live.
- Bekannte kosmetische Abweichung: Der Fokus-Rahmen der Formularfelder (dünne Unterstreichung bei
  Klick ins Feld) bleibt PatternFly-Blau statt Emerald — die dafür zuständige interne
  PatternFly-Variable wurde live nicht gefunden. Kein Blocker für US-AU8 (Marke, Kartenfarben,
  Primärbutton, Typografie sind korrekt), aber bei Gelegenheit nachbesserbar.
- Realm-Änderungen (`loginTheme`, `internationalizationEnabled`, `supportedLocales`,
  `defaultLocale`) wirken bei einem bereits einmal importierten Realm nicht automatisch erneut,
  da `--import-realm` mit der Strategie `IGNORE_EXISTING` läuft (siehe Log: „Realm 'mymusic'
  already exists. Import skipped"). In bestehenden lokalen Entwicklungsumgebungen mit
  persistiertem `mymusic-keycloak-data`-Volume müssen solche Feldänderungen einmalig manuell
  nachgezogen werden (Admin-Konsole oder `kcadm.sh update realms/mymusic -s <feld>=<wert>`) — nur
  ein frisches Volume übernimmt sie automatisch aus der JSON.
