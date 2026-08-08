import { WritableSignal, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { KEYCLOAK_EVENT_SIGNAL, KeycloakEvent, KeycloakEventType } from 'keycloak-angular';
import Keycloak from 'keycloak-js';
import { describe, expect, it, vi } from 'vitest';

import { CurrentUserService } from './current-user.service';

describe('CurrentUserService', () => {
  function setup(keycloakOverrides: Partial<Keycloak> = {}) {
    const eventSignal: WritableSignal<KeycloakEvent> = signal({
      type: KeycloakEventType.KeycloakAngularNotInitialized,
    });
    const keycloak = {
      authenticated: false,
      login: vi.fn().mockResolvedValue(undefined),
      logout: vi.fn().mockResolvedValue(undefined),
      ...keycloakOverrides,
    } as unknown as Keycloak;

    TestBed.configureTestingModule({
      providers: [
        { provide: Keycloak, useValue: keycloak },
        { provide: KEYCLOAK_EVENT_SIGNAL, useValue: eventSignal },
      ],
    });

    return { service: TestBed.inject(CurrentUserService), keycloak, eventSignal };
  }

  it('meldet isAuthenticated=false, solange kein Auth-Event eingetroffen ist', () => {
    const { service } = setup({ authenticated: false });

    expect(service.isAuthenticated()).toBe(false);
    expect(service.username()).toBeUndefined();
  });

  it('meldet isAuthenticated=true und den Benutzernamen nach einem Auth-Erfolg', () => {
    const { service, eventSignal } = setup({
      authenticated: true,
      tokenParsed: { preferred_username: 'max.mustermann' },
    } as Partial<Keycloak>);

    eventSignal.set({ type: KeycloakEventType.AuthSuccess });

    expect(service.isAuthenticated()).toBe(true);
    expect(service.username()).toBe('max.mustermann');
  });

  it('login() ruft keycloak.login mit der übergebenen redirectUri auf', async () => {
    const { service, keycloak } = setup();

    await service.login('https://localhost:4200/records');

    expect(keycloak.login).toHaveBeenCalledWith({ redirectUri: 'https://localhost:4200/records' });
  });

  it('logout() ruft keycloak.logout mit der übergebenen redirectUri auf', async () => {
    const { service, keycloak } = setup();

    await service.logout('https://localhost:4200/');

    expect(keycloak.logout).toHaveBeenCalledWith({ redirectUri: 'https://localhost:4200/' });
  });
});
