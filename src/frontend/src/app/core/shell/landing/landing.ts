import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-landing',
  templateUrl: './landing.html',
})
export class Landing {
  private readonly router = inject(Router);
  private readonly oidcSecurityService = inject(OidcSecurityService);

  constructor() {
    if (this.oidcSecurityService.authenticated().isAuthenticated) {
      this.router.navigate(['/dashboard'], { replaceUrl: true });
    }
  }
}
