import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { RUNTIME_CONFIG, RuntimeConfigService } from './runtime-config.service';

describe('RuntimeConfigService', () => {
  it('stellt apiBaseUrl und keycloakUrl aus der injizierten Konfiguration bereit', () => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: RUNTIME_CONFIG,
          useValue: { apiBaseUrl: 'https://localhost:5001', keycloakUrl: 'http://localhost:8080' },
        },
      ],
    });

    const service = TestBed.inject(RuntimeConfigService);

    expect(service.apiBaseUrl).toBe('https://localhost:5001');
    expect(service.keycloakUrl).toBe('http://localhost:8080');
  });
});
