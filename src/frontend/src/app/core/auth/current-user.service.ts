import { Injectable, computed, inject } from '@angular/core';
import { KEYCLOAK_EVENT_SIGNAL } from 'keycloak-angular';
import Keycloak from 'keycloak-js';

@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  private readonly keycloak = inject(Keycloak);
  private readonly keycloakEvent = inject(KEYCLOAK_EVENT_SIGNAL);

  readonly isAuthenticated = computed(() => {
    this.keycloakEvent();
    return this.keycloak.authenticated ?? false;
  });

  readonly username = computed(() => {
    this.keycloakEvent();
    return this.keycloak.tokenParsed?.['preferred_username'] as string | undefined;
  });

  login(redirectUri: string = window.location.href): Promise<void> {
    return this.keycloak.login({ redirectUri });
  }

  logout(redirectUri: string = window.location.origin): Promise<void> {
    return this.keycloak.logout({ redirectUri });
  }
}
