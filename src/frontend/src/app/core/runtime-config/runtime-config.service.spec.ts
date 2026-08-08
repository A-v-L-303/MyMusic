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

  it('lädt die Konfiguration und stellt apiBaseUrl bereit', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: () => Promise.resolve({ apiBaseUrl: 'https://localhost:5001' })
      })
    );

    await service.load();

    expect(service.apiBaseUrl).toBe('https://localhost:5001');
  });

  it('wirft einen Fehler, wenn runtime-config.json nicht geladen werden kann', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ ok: false, status: 404 })
    );

    await expect(service.load()).rejects.toThrow();
  });

  it('wirft einen Fehler, wenn apiBaseUrl vor load() gelesen wird', () => {
    expect(() => service.apiBaseUrl).toThrow();
  });
});
