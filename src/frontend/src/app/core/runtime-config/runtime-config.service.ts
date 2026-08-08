import { Injectable, signal } from '@angular/core';

export interface RuntimeConfig {
  apiBaseUrl: string;
}

@Injectable({ providedIn: 'root' })
export class RuntimeConfigService {
  private readonly config = signal<RuntimeConfig | null>(null);

  async load(): Promise<void> {
    const response = await fetch('/runtime-config.json');

    if (!response.ok) {
      throw new Error(`runtime-config.json konnte nicht geladen werden (Status ${response.status}).`);
    }

    this.config.set((await response.json()) as RuntimeConfig);
  }

  get apiBaseUrl(): string {
    const config = this.config();

    if (!config) {
      throw new Error('RuntimeConfigService.load() wurde noch nicht aufgerufen.');
    }

    return config.apiBaseUrl;
  }
}
