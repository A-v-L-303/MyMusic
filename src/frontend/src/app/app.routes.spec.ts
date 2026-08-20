import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { of } from 'rxjs';
import { describe, expect, it } from 'vitest';

import { adminGuard } from './core/auth/admin.guard';
import { authGuard } from './core/auth/auth.guard';
import { routes } from './app.routes';
import { RuntimeConfigService } from './core/runtime-config/runtime-config.service';
import { UserRolesService } from './core/auth/user-roles.service';
import { Admin } from './features/admin/admin';
import { Artists } from './features/artists/artists';
import { Dashboard } from './features/dashboard/dashboard';
import { Genres } from './features/genres/genres';
import { Labels } from './features/labels/labels';
import { Records } from './features/records/records';
import { Search } from './features/search/search';
import { Landing } from './core/shell/landing/landing';

// Für die Navigations-Tests wird der echte authGuard (autoLoginPartialRoutesGuard)
// durch einen trivialen Passthrough ersetzt - er bräuchte sonst eine vollständige,
// echte OIDC-Konfiguration. Die Verdrahtung selbst (dass authGuard/adminGuard an den
// richtigen Routen hängen) wird unten separat gegen den unveränderten routes-Import
// geprüft, `children` bleibt unverändert (adminGuard bleibt aktiv). `routes[0]` ist die
// unbewachte Landing-Route (path ''), `routes[1]` der bewachte Eltern-Knoten.
const testRoutes = [routes[0], { ...routes[1], canActivate: [() => true] }];

const routingTestProviders = [
  provideRouter(testRoutes),
  provideHttpClient(),
  provideHttpClientTesting(),
  { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
  {
    provide: OidcSecurityService,
    useValue: {
      authenticated: signal({ isAuthenticated: true }),
      getPayloadFromAccessToken: () => of({ sub: 'own-id' }),
    },
  },
];

function routingTestProvidersWithAdminRole(isAdmin: boolean) {
  return [
    ...routingTestProviders,
    { provide: UserRolesService, useValue: { isAdmin: () => isAdmin } },
    {
      provide: OidcSecurityService,
      useValue: {
        authenticated: signal({ isAuthenticated: true }),
        getPayloadFromAccessToken: () => of({ sub: 'own-id' }),
      },
    },
  ];
}

function routingTestProvidersUnauthenticated() {
  return [
    provideRouter(testRoutes),
    provideHttpClient(),
    provideHttpClientTesting(),
    { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
    {
      provide: OidcSecurityService,
      useValue: { authenticated: signal({ isAuthenticated: false }) },
    },
  ];
}

describe('app.routes', () => {
  it('verdrahtet authGuard auf dem geschützten Eltern-Knoten', () => {
    // arrange & act
    const guardedRoot = routes[1];

    // assert
    expect(guardedRoot.canActivate).toEqual([authGuard]);
  });

  it('verdrahtet adminGuard auf der /admin-Route', () => {
    // arrange & act
    const adminRoute = routes[1].children?.find((route) => route.path === 'admin');

    // assert
    expect(adminRoute?.canActivate).toEqual([adminGuard]);
  });

  it('verdrahtet die unbewachte Landing-Komponente auf dem Wurzelpfad', () => {
    // arrange & act
    const landingRoute = routes[0];

    // assert
    expect(landingRoute.path).toBe('');
    expect(landingRoute.pathMatch).toBe('full');
    expect(landingRoute.component).toBe(Landing);
    expect(landingRoute.canActivate).toBeUndefined();
  });

  it('lädt /admin, wenn die Rolle Admin vorhanden ist', async () => {
    // arrange
    TestBed.configureTestingModule({ providers: routingTestProvidersWithAdminRole(true) });
    const harness = await RouterTestingHarness.create();

    // act
    const component = await harness.navigateByUrl('/admin', Admin);

    // assert
    expect(component).toBeInstanceOf(Admin);
  });

  it('leitet /admin auf /dashboard um, wenn die Rolle Admin fehlt', async () => {
    // arrange
    TestBed.configureTestingModule({ providers: routingTestProvidersWithAdminRole(false) });
    const harness = await RouterTestingHarness.create();

    // act
    await harness.navigateByUrl('/admin', Dashboard);

    // assert
    expect(TestBed.inject(Router).url).toBe('/dashboard');
  });

  it('redirectet den Wurzelpfad auf /dashboard, wenn bereits angemeldet', async () => {
    // arrange
    TestBed.configureTestingModule({ providers: routingTestProviders });
    const harness = await RouterTestingHarness.create();

    // act
    await harness.navigateByUrl('/', Dashboard);

    // assert
    expect(TestBed.inject(Router).url).toBe('/dashboard');
  });

  it('bleibt auf der Landing-Seite, wenn nicht angemeldet - Login/Registrieren bleiben erreichbar', async () => {
    // arrange
    TestBed.configureTestingModule({ providers: routingTestProvidersUnauthenticated() });
    const harness = await RouterTestingHarness.create();

    // act
    await harness.navigateByUrl('/', Landing);

    // assert
    expect(TestBed.inject(Router).url).toBe('/');
  });

  it('redirectet einen unbekannten Pfad auf /dashboard', async () => {
    // arrange
    TestBed.configureTestingModule({ providers: routingTestProviders });
    const harness = await RouterTestingHarness.create();

    // act & assert: wirft, falls nicht Dashboard geladen wird
    await expect(harness.navigateByUrl('/nicht-vorhanden', Dashboard)).resolves.toBeInstanceOf(
      Dashboard,
    );
  });

  it.each([
    ['/dashboard', Dashboard],
    ['/records', Records],
    ['/artists', Artists],
    ['/labels', Labels],
    ['/genres', Genres],
    ['/search', Search],
  ])('lädt für %s die erwartete Feature-Komponente', async (url, componentType) => {
    // arrange
    TestBed.configureTestingModule({ providers: routingTestProviders });
    const harness = await RouterTestingHarness.create();

    // act
    const component = await harness.navigateByUrl(url, componentType);

    // assert
    expect(component).toBeInstanceOf(componentType);
  });
});
