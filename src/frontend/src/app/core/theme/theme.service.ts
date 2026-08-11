import { Injectable, computed, effect, signal } from '@angular/core';

export type Theme = 'light' | 'dark';

const STORAGE_KEY = 'mymusic-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly darkMediaQuery = matchMedia('(prefers-color-scheme: dark)');

  private readonly userPreference = signal<Theme | null>(this.readStoredPreference());
  private readonly osPrefersDark = signal(this.darkMediaQuery.matches);

  readonly effectiveTheme = computed<Theme>(
    () => this.userPreference() ?? (this.osPrefersDark() ? 'dark' : 'light'),
  );

  constructor() {
    this.darkMediaQuery.addEventListener('change', (event) =>
      this.osPrefersDark.set(event.matches),
    );

    effect(() => {
      const explicit = this.userPreference();

      if (explicit) {
        document.documentElement.setAttribute('data-theme', explicit);
        localStorage.setItem(STORAGE_KEY, explicit);
      } else {
        document.documentElement.removeAttribute('data-theme');
      }
    });
  }

  toggle(): void {
    const next: Theme = this.effectiveTheme() === 'dark' ? 'light' : 'dark';
    this.userPreference.set(next);
  }

  private readStoredPreference(): Theme | null {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === 'light' || stored === 'dark' ? stored : null;
  }
}
