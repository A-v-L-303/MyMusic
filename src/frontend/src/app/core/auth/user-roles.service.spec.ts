import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { of } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';

import { UserRolesService } from './user-roles.service';

function injectServiceWithPayload(payload: Record<string, unknown>): UserRolesService {
  TestBed.configureTestingModule({
    providers: [
      {
        provide: OidcSecurityService,
        useValue: {
          authenticated: signal({ isAuthenticated: true }),
          getPayloadFromAccessToken: vi.fn().mockReturnValue(of(payload)),
        },
      },
    ],
  });

  const service = TestBed.inject(UserRolesService);
  TestBed.tick();
  return service;
}

describe('UserRolesService', () => {
  it('liefert eine leere Rollenliste, wenn der Access-Token-Payload keine realm_access-Claims enthält', () => {
    // arrange
    const service = injectServiceWithPayload({});

    // act & assert
    expect(service.roles()).toEqual([]);
  });

  it('liefert die Rollen aus realm_access.roles unverändert', () => {
    // arrange
    const service = injectServiceWithPayload({ realm_access: { roles: ['User', 'Admin'] } });

    // act & assert
    expect(service.roles()).toEqual(['User', 'Admin']);
  });

  it('isAdmin ist false, wenn die Rolle Admin fehlt', () => {
    // arrange
    const service = injectServiceWithPayload({ realm_access: { roles: ['User'] } });

    // act & assert
    expect(service.isAdmin()).toBe(false);
  });

  it('isAdmin ist true, wenn die Rolle Admin vorhanden ist', () => {
    // arrange
    const service = injectServiceWithPayload({ realm_access: { roles: ['User', 'Admin'] } });

    // act & assert
    expect(service.isAdmin()).toBe(true);
  });

  it('liest den Payload erneut, wenn sich der Anmeldestatus ändert', () => {
    // arrange
    const authenticatedSignal = signal({ isAuthenticated: false });
    const getPayloadFromAccessToken = vi
      .fn()
      .mockReturnValueOnce(of({}))
      .mockReturnValueOnce(of({ realm_access: { roles: ['Admin'] } }));

    TestBed.configureTestingModule({
      providers: [
        {
          provide: OidcSecurityService,
          useValue: { authenticated: authenticatedSignal, getPayloadFromAccessToken },
        },
      ],
    });
    const service = TestBed.inject(UserRolesService);
    TestBed.tick();
    expect(service.isAdmin()).toBe(false);

    // act
    authenticatedSignal.set({ isAuthenticated: true });
    TestBed.tick();

    // assert
    expect(service.isAdmin()).toBe(true);
    expect(getPayloadFromAccessToken).toHaveBeenCalledTimes(2);
  });
});
