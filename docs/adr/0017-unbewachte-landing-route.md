# ADR 0017 — Unbewachte Landing-Route für nicht angemeldete Benutzer

**Status**: Angenommen
**Datum**: 2026-08-20
**Betrifft**: `src/frontend`

## Kontext

Block 7g (Registrierung) fügte einen „Registrieren"-Button neben dem bestehenden
„Login"-Button in der Kopfzeile hinzu. Bei der Live-Verifikation zeigte sich: Beide
Buttons waren über normale Navigation praktisch nicht erreichbar.

`app.routes.ts` (Block 0g) hängt `canActivate: [authGuard]`
(`autoLoginPartialRoutesGuard` aus `angular-auth-oidc-client`) an den Wurzelknoten;
`''` redirectete auf das ebenfalls geschützte `/dashboard`. Ein nicht angemeldeter
Aufruf der Anwendung löste dadurch sofort `authorize()` aus (Redirect zu Keycloak),
bevor ein Klick auf einen Header-Button überhaupt möglich war — bestätigt durch
Live-Test: Aufruf von `localhost:4200` landete direkt auf der Keycloak-Anmeldeseite,
keine sichtbare Zwischenansicht der `NavComponent`.

Betroffen war nicht nur der neue Registrieren-Button, sondern auch der bereits seit
Block 7a bestehende Login-Button sowie das in US-AU7 beschriebene Verhalten nach dem
Logout („Kopfzeile zeigt wieder den Button Login") — beides technisch korrekt
implementiert, aber praktisch nie beobachtbar, da derselbe Guard-Mechanismus griff.

## Entscheidung

Der Wurzelpfad `''` wird aus dem geschützten Routenbaum herausgelöst und erhält eine
eigene, unbewachte Route mit einer neuen Komponente `Landing`
(`src/frontend/src/app/core/shell/landing/`):

```ts
export const routes: Routes = [
  { path: '', pathMatch: 'full', component: Landing },
  {
    path: '',
    canActivate: [authGuard],
    children: [ /* dashboard, records, artists, labels, genres, search, admin, ** */ ],
  },
];
```

`Landing` prüft beim Erstellen synchron `OidcSecurityService.authenticated()`
(zuverlässig aufgelöst, da `withAppInitializerAuthCheck()` den Auth-Check bereits vor
dem Bootstrapping der Anwendung abschließt, siehe ADR 0010):

- **Bereits angemeldet**: sofortiger `router.navigate(['/dashboard'], { replaceUrl:
  true })` — unverändertes Verhalten, kein zusätzlicher Klick nötig.
- **Nicht angemeldet**: keine Navigation. Die `NavComponent` (liegt außerhalb des
  `router-outlet`, unverändert immer sichtbar) zeigt Login/Registrieren-Buttons, die
  jetzt tatsächlich anklickbar sind.

Alle übrigen Routen (`/dashboard`, `/records`, `/records/:id`, …) bleiben unverändert
vollständig geschützt — ein direkter Aufruf einer geschützten Route ohne Anmeldung
leitet weiterhin sofort zu Keycloak weiter, ohne die Zielseite kurz zu zeigen
(US-AU3 unverändert erfüllt). Nur der reine Wurzelpfad verhält sich anders.

## Begründung

Alternative (im Chat mit dem Projektinhaber verworfen): Guard-Verhalten unverändert
lassen, Registrierung ausschließlich über den ohnehin vorhandenen Link auf Keycloaks
eigener Anmeldeseite anbieten. Verworfen, weil dann sowohl der neue
Registrieren-Button als auch der bestehende Login-Button dauerhaft totem Code
entsprächen — kein Widerspruch zu US-AU1/US-AU7, aber auch keine tatsächlich nutzbare
Umsetzung dieser Stories.

## Konsequenzen

- US-AU1, US-AU2, US-AU7 (Wiki `user-stories-authentifizierung.md`) sind durch diese
  Änderung erstmals auch live tatsächlich beobachtbar, nicht nur technisch korrekt.
- Neue Datei-Trias `core/shell/landing/landing.ts`/`.html`/`.spec.ts`, analog zum in
  Block 0g entfernten `core/shell/home-placeholder/`.
- `app.routes.spec.ts` musste angepasst werden: `routes[0]` ist jetzt die unbewachte
  Landing-Route, `routes[1]` der vormals alleinige, geschützte Wurzelknoten
  (Indexverschiebung in den bestehenden „verdrahtet"-Tests).
- Kein neues Sicherheitsrisiko: Die `Landing`-Komponente selbst zeigt keine
  fachlichen Inhalte, nur die ohnehin immer sichtbare `NavComponent`.
