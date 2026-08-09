import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';

import { RuntimeConfigService } from '../runtime-config/runtime-config.service';
import { keycloakConfigLoaderFactory } from './keycloak-config.factory';

describe('keycloakConfigLoaderFactory', () => {
  it('baut eine OpenIdConfiguration aus der RuntimeConfigService', async () => {
    const runtimeConfigServiceStub = {
      load: vi.fn().mockResolvedValue(undefined),
      apiBaseUrl: 'https://localhost:5001',
      keycloakAuthority: 'http://localhost:8080/realms/mymusic',
    };

    TestBed.configureTestingModule({
      providers: [{ provide: RuntimeConfigService, useValue: runtimeConfigServiceStub }],
    });

    const loader = TestBed.runInInjectionContext(() => keycloakConfigLoaderFactory());
    const configs = await firstValueFrom(loader.loadConfigs());
    const config = configs[0];

    // assert: Authority/Client stammen aus der RuntimeConfigService
    expect(config.authority).toBe('http://localhost:8080/realms/mymusic');
    expect(config.clientId).toBe('mymusic-angular');

    // assert: Redirect-URIs entsprechen dem Realm-Import (Wildcard auf window.location.origin)
    expect(config.redirectUrl).toBe(window.location.origin);
    expect(config.postLogoutRedirectUri).toBe(window.location.origin);

    // assert: Authorization Code + PKCE, Refresh-Token-basierte stille Erneuerung
    expect(config.responseType).toBe('code');
    expect(config.useRefreshToken).toBe(true);
    expect(config.silentRenew).toBe(true);
    expect(config.disablePkce).not.toBe(true);

    // assert: scope bewusst ohne offline_access (Sitzungsbindung an SSO-Session-Grenzen)
    expect(config.scope).toBe('openid profile email');
    expect(config.scope).not.toContain('offline_access');

    // assert: keine erzwungenen Auth-Request-Parameter (z. B. prompt=login), damit eine gültige
    // SSO-Session beim erneuten App-Aufruf automatisch durchgereicht wird
    expect(config.customParamsAuthRequest).toBeUndefined();

    // assert: Token wird ausschließlich an die MyMusic-API angehängt
    expect(config.secureRoutes).toEqual(['https://localhost:5001']);
  });

  it('lädt die RuntimeConfigService nur einmal, auch wenn sie bereits geladen war', async () => {
    const loadMock = vi.fn().mockResolvedValue(undefined);
    const runtimeConfigServiceStub = {
      load: loadMock,
      apiBaseUrl: 'https://localhost:5001',
      keycloakAuthority: 'http://localhost:8080/realms/mymusic',
    };

    TestBed.configureTestingModule({
      providers: [{ provide: RuntimeConfigService, useValue: runtimeConfigServiceStub }],
    });

    const loader = TestBed.runInInjectionContext(() => keycloakConfigLoaderFactory());

    await firstValueFrom(loader.loadConfigs());

    expect(loadMock).toHaveBeenCalledTimes(1);
  });
});
