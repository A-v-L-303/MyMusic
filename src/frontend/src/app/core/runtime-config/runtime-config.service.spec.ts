import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { RuntimeConfigService } from './runtime-config.service';

describe('RuntimeConfigService', () => {
  let service: RuntimeConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RuntimeConfigService);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('lädt die Konfiguration und stellt apiBaseUrl und keycloakAuthority bereit', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: () =>
          Promise.resolve({
            apiBaseUrl: 'https://localhost:5001',
            keycloakAuthority: 'http://localhost:8080/realms/mymusic',
          }),
      }),
    );

    await service.load();

    expect(service.apiBaseUrl).toBe('https://localhost:5001');
    expect(service.keycloakAuthority).toBe('http://localhost:8080/realms/mymusic');
  });

  it('wirft einen Fehler, wenn runtime-config.json nicht geladen werden kann', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }));

    await expect(service.load()).rejects.toThrow();
  });

  it('wirft einen Fehler, wenn apiBaseUrl vor load() gelesen wird', () => {
    expect(() => service.apiBaseUrl).toThrow();
  });

  it('wirft einen Fehler, wenn keycloakAuthority vor load() gelesen wird', () => {
    expect(() => service.keycloakAuthority).toThrow();
  });

  it('ruft fetch bei mehrfachem load()-Aufruf nur einmal auf', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () =>
        Promise.resolve({
          apiBaseUrl: 'https://localhost:5001',
          keycloakAuthority: 'http://localhost:8080/realms/mymusic',
        }),
    });

    vi.stubGlobal('fetch', fetchMock);

    await Promise.all([service.load(), service.load()]);
    await service.load();

    expect(fetchMock).toHaveBeenCalledTimes(1);
  });
});
