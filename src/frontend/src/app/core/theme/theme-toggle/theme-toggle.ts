import { Component, computed, inject } from '@angular/core';

import { ThemeService } from '../theme.service';

@Component({
  selector: 'app-theme-toggle',
  templateUrl: './theme-toggle.html',
})
export class ThemeToggle {
  protected readonly themeService = inject(ThemeService);

  protected readonly isDark = computed(() => this.themeService.effectiveTheme() === 'dark');
  protected readonly ariaLabel = computed(() =>
    this.isDark() ? 'Zum hellen Design wechseln' : 'Zum dunklen Design wechseln',
  );
}
