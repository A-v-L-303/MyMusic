import { HttpClient } from '@angular/common/http';
import { Component, effect, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { CurrentUserService } from './core/auth/current-user.service';
import { RuntimeConfigService } from './core/runtime-config/runtime-config.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly currentUser = inject(CurrentUserService);
  private readonly http = inject(HttpClient);
  private readonly runtimeConfig = inject(RuntimeConfigService);

  protected readonly meCheck = signal<'idle' | 'ok' | 'error'>('idle');

  constructor() {
    effect(() => {
      if (this.currentUser.isAuthenticated()) {
        this.checkMe();
      }
    });
  }

  protected login(): void {
    void this.currentUser.login();
  }

  protected logout(): void {
    void this.currentUser.logout();
  }

  private checkMe(): void {
    this.http.get(`${this.runtimeConfig.apiBaseUrl}/api/me`).subscribe({
      next: () => this.meCheck.set('ok'),
      error: () => this.meCheck.set('error'),
    });
  }
}
