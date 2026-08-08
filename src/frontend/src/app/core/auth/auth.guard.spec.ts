import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import Keycloak from 'keycloak-js';
import { describe, expect, it, vi } from 'vitest';

import { authGuard } from './auth.guard';

describe('authGuard', () => {
  function setup(authenticated: boolean) {
    const keycloak = {
      authenticated,
      login: vi.fn().mockResolvedValue(undefined),
    } as unknown as Keycloak;

    TestBed.configureTestingModule({
      providers: [{ provide: Keycloak, useValue: keycloak }],
    });

    return { keycloak };
  }

  it('erlaubt den Zugriff, wenn der Benutzer angemeldet ist', async () => {
    const { keycloak } = setup(true);

    const result = await TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/dashboard' } as RouterStateSnapshot),
    );

    expect(result).toBe(true);
    expect(keycloak.login).not.toHaveBeenCalled();
  });

  it('leitet nicht angemeldete Benutzer zu Keycloak weiter und blockiert die Route', async () => {
    const { keycloak } = setup(false);

    const result = await TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/records' } as RouterStateSnapshot),
    );

    expect(result).toBe(false);
    expect(keycloak.login).toHaveBeenCalledWith({
      redirectUri: `${window.location.origin}/records`,
    });
  });
});
