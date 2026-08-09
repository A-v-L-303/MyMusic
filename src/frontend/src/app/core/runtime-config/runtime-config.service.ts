import { Injectable, signal } from '@angular/core';

export interface RuntimeConfig {
  apiBaseUrl: string;
  keycloakAuthority: string;
}

@Injectable({ providedIn: 'root' })
export class RuntimeConfigService {
  private readonly config = signal<RuntimeConfig | null>(null);
  private loadPromise: Promise<void> | null = null;

  load(): Promise<void> {
    if (!this.loadPromise) {
      this.loadPromise = this.fetchConfig();
    }

    return this.loadPromise;
  }

  private async fetchConfig(): Promise<void> {
    const response = await fetch('/runtime-config.json');

    if (!response.ok) {
      throw new Error(
        `runtime-config.json konnte nicht geladen werden (Status ${response.status}).`,
      );
    }

    this.config.set((await response.json()) as RuntimeConfig);
  }

  get apiBaseUrl(): string {
    return this.requireConfig().apiBaseUrl;
  }

  get keycloakAuthority(): string {
    return this.requireConfig().keycloakAuthority;
  }

  private requireConfig(): RuntimeConfig {
    const config = this.config();

    if (!config) {
      throw new Error('RuntimeConfigService.load() wurde noch nicht aufgerufen.');
    }

    return config;
  }
}
