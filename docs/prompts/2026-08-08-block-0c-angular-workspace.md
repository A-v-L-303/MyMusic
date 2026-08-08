# Block 0c: Angular-Workspace

## Kontext

`TASK.md` führte Block 0c (Angular-Workspace) als letzten offenen Punkt aus dem
MVP-Fundament. Er ist Voraussetzung für alle bereits zurückgestellten
Angular-Features (`genres/`, `labels/`, `artists/`, `records/`) sowie für
Abschnitt 7 (Login-Flow). Analog zu Block 0a („Walking Skeleton") blieb dieser
Block bewusst minimal: lauffähiges Fundament, keine Feature-UI.

## Entscheidung (mit dem Projektinhaber geklärt)

Inter/JetBrains Mono werden **self-hosted über npm** eingebunden
(`@fontsource/inter`, `@fontsource/jetbrains-mono`) statt per Google-Fonts-
CDN-Link — konsistent mit der bereits getroffenen Linie bei Lucide-Icons
(„CDN nur für Previews, Production self-hosted").

## Umgesetzte Schritte

1. Branch `block-0c-angular-workspace` von `main`.
2. **Node.js-Aktualisierung** (mit Freigabe): Node.js war mit v22.16.0
   installiert, Angular CLI 22 verlangt jedoch mindestens v22.22.3/v24.15.0/v26.
   Kein Node-Versionsmanager vorhanden — Node systemweit per offiziellem
   MSI-Installer auf v22.23.2 aktualisiert.
3. Angular-22-Workspace unter `src/frontend/` gescaffoldet:
   ```
   npx --yes @angular/cli@22 new frontend --directory=src/frontend `
     --routing --style=css --ssr=false --skip-git --package-manager=npm --strict
   ```
   Tatsächliches Ergebnis: Angular 22 verwendet standardmäßig **Vitest** statt
   Karma/Jasmine und ist **zoneless** (kein `zone.js` in den Dependencies); der
   Static-Assets-Ordner heißt `public/`.
4. Tailwind CSS **3** (`tailwindcss@3`, nicht das npm-`latest` 4.x) plus
   PostCSS/Autoprefixer installiert; `tailwind.config.js` unverändert aus
   `../../02 Wiki/MyMusic Wiki/raw/design_system/tailwind/tailwind.config.js`
   übernommen; `postcss.config.js` neu angelegt.
5. Design-Tokens eingebunden: `colors_and_type.css`/`components.css`
   unverändert nach `src/frontend/src/styles/design-system/` kopiert,
   `mark.svg` in `public/` übernommen. Globale `src/styles.css`: Tokens vor
   Komponenten vor Tailwind-Direktiven.
6. Schriften self-hosted: `@fontsource/inter` (400/500/600/700),
   `@fontsource/jetbrains-mono` (400) installiert und in `src/styles.css`
   importiert; Build bestätigt, dass die `.woff2`-Dateien ins Bundle
   übernommen werden.
7. Minimale App-Shell: `AppComponent` zeigt Marke + „MyMusic"-Wortmarke +
   Platzhaltertext (Tailwind-/Token-Klassen). `app.spec.ts` entsprechend
   angepasst.
8. Runtime-Config-Mechanismus:
   - `RuntimeConfigService` (`src/app/core/runtime-config/`) lädt
     `runtime-config.json` per `fetch()` über `provideAppInitializer()`.
   - Statische Platzhalter-`runtime-config.json` (`{ "apiBaseUrl": "" }`) im
     `public/`-Ordner.
   - `scripts/write-runtime-config.mjs` schreibt die Datei aus der
     Umgebungsvariable `MYMUSIC_API_BASE_URL`, verdrahtet als `prestart`/
     `prebuild` in `package.json`.
   - Unit-Tests für `RuntimeConfigService` (Erfolgsfall, HTTP-Fehler,
     Zugriff vor `load()`).
   - Entscheidung mit Alternativenabwägung dokumentiert in
     `docs/adr/0009-angular-runtime-config-mechanismus.md`.
9. AppHost-Integration (`src/MyMusic.AppHost/AppHost.cs`):
   - Die tatsächliche Aspire-13.4.6-API wurde **nicht geraten**, sondern
     verifiziert: Reflection gegen die installierten NuGet-Assemblies zeigte,
     dass `AddNpmApp()`/`AddJavaScriptApp()` **nicht** im Kernpaket
     `Aspire.Hosting` liegen; `dotnet package search Aspire.Hosting` deckte
     das tatsächliche, neue Paket `Aspire.Hosting.JavaScript` 13.4.6 auf
     (Nachfolger von `Aspire.Hosting.NodeJs`, das bei 9.5.2 eingefroren ist).
     Die exakte Signatur von `AddJavaScriptApp` wurde über ein Scratch-Projekt
     mit echtem `dotnet build` gegen die reale `MyMusic.Api`-Projektreferenz
     verifiziert; zusätzlich wurde der offizielle Aspire-Sample
     `playground/AspireWithJavaScript/AspireJavaScript.AppHost/AppHost.cs`
     (Repository `dotnet/aspire`, Tag `v13.4.6`) über die GitHub-API gelesen,
     um das reale Verwendungsmuster (inkl. `PORT`-Env-Var-Konvention) zu
     bestätigen.
   - `AppHost.cs`: `api`-Ressourcen-Bauteil erstmals in eine Variable gefasst
     (vorher inline), neue `frontend`-Ressource ergänzt:
     ```csharp
     builder.AddJavaScriptApp("frontend", "../frontend", runScriptName: "start")
         .WithReference(api)
         .WaitFor(api)
         .WithEnvironment("MYMUSIC_API_BASE_URL", api.GetEndpoint("https"))
         .WithHttpEndpoint(env: "PORT")
         .WithExternalHttpEndpoints();
     ```
   - `src/frontend/package.json`: `start`-Skript nutzt (wie im offiziellen
     Aspire-Angular-Sample) `run-script-os` für plattformabhängige
     `PORT`-Interpolation (`ng serve --port %PORT%` bzw. `--port $PORT`), da
     `ng serve` selbst keine `PORT`-Umgebungsvariable liest.
10. Dokumentation: `TASK.md` (Block 0c abgeschlossen, Branch-Zeile), `README.md`
    (neuer Abschnitt „Frontend (Block 0c)"), ADR 0009, Wiki-Korrekturen
    (`aspire-orchestrierung.md`, `angular.md`, `log.md`), dieses Prompt-Archiv.

## Verifikation

1. `npm run build` (Production-Build) in `src/frontend` — fehlerfrei, Fonts
   als `.woff2` im Output bestätigt.
2. `npm test -- --watch=false` — 5/5 Tests grün (App-Shell, RuntimeConfigService).
3. `dotnet build MyMusic.slnx --no-restore` — fehlerfrei, inkl. neuer
   `Aspire.Hosting.JavaScript`-Abhängigkeit und geänderter `AppHost.cs`.
4. `dotnet format MyMusic.slnx --verify-no-changes` — grün.
5. AppHost-Start über PowerShell und Browser-Prüfung: siehe Risiken/offene
   Punkte unten.

## Risiken und offene Punkte

- Der npm-Audit meldet 3 „moderate"-Schwachstellen ausschließlich in einer
  MCP-Abhängigkeit der Angular-CLI-Tooling-Kette (`@hono/node-server`, nur
  Dev-Zeit, nicht im ausgelieferten Code). Ein Fix würde `@angular/cli` auf
  21.0.4 downgraden — widerspricht der Tech-Stack-Entscheidung Angular 22.
  Bewusst nicht behoben.
- Ein direkter `ng serve` außerhalb des AppHosts (ohne `npm start`) läuft ohne
  gesetzte `PORT`-Variable ins Leere — identisches Verhalten wie im
  offiziellen Aspire-Angular-Sample, im README dokumentiert.
- Kein Production-Publish-Pfad (`PublishAsDockerFile()`) für das Frontend
  ergänzt — nur lokale Aspire-Entwicklung war Teil des Auftrags.
