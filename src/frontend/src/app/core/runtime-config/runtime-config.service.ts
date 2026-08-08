import { Injectable, InjectionToken, inject } from '@angular/core';

export interface RuntimeConfig {
  apiBaseUrl: string;
  keycloakUrl: string;
}

export const RUNTIME_CONFIG = new InjectionToken<RuntimeConfig>('RUNTIME_CONFIG');

@Injectable({ providedIn: 'root' })
export class RuntimeConfigService {
  private readonly config = inject(RUNTIME_CONFIG);

  get apiBaseUrl(): string {
    return this.config.apiBaseUrl;
  }

  get keycloakUrl(): string {
    return this.config.keycloakUrl;
  }
}
