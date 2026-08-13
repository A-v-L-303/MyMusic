import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { describe, expect, it, vi } from 'vitest';

vi.mock('angular-auth-oidc-client', () => ({
  autoLoginPartialRoutesGuard: () => true,
}));

import { authGuard } from './core/auth/auth.guard';
import { routes } from './app.routes';
import { Artists } from './features/artists/artists';
import { Dashboard } from './features/dashboard/dashboard';
import { Genres } from './features/genres/genres';
import { Labels } from './features/labels/labels';
import { Records } from './features/records/records';
import { Search } from './features/search/search';

describe('app.routes', () => {
  it('verdrahtet authGuard auf dem Eltern-Knoten', () => {
    // arrange & act
    const root = routes[0];

    // assert
    expect(root.canActivate).toEqual([authGuard]);
  });

  it('redirectet den Wurzelpfad auf /dashboard', async () => {
    // arrange
    TestBed.configureTestingModule({ providers: [provideRouter(routes)] });
    const harness = await RouterTestingHarness.create();

    // act
    await harness.navigateByUrl('/', Dashboard);

    // assert
    expect(TestBed.inject(Router).url).toBe('/dashboard');
  });

  it('redirectet einen unbekannten Pfad auf /dashboard', async () => {
    // arrange
    TestBed.configureTestingModule({ providers: [provideRouter(routes)] });
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
    TestBed.configureTestingModule({ providers: [provideRouter(routes)] });
    const harness = await RouterTestingHarness.create();

    // act
    const component = await harness.navigateByUrl(url, componentType);

    // assert
    expect(component).toBeInstanceOf(componentType);
  });
});
