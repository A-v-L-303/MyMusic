import { Component, computed, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

interface OidcUserClaims {
  preferred_username?: string;
}

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  protected readonly authenticated = this.oidcSecurityService.authenticated;
  protected readonly username = computed(() => {
    const claims = this.oidcSecurityService.userData().userData as OidcUserClaims | undefined;
    return claims?.preferred_username;
  });

  protected login(): void {
    this.oidcSecurityService.authorize();
  }

  protected logout(): void {
    this.oidcSecurityService.logoff().subscribe();
  }
}
