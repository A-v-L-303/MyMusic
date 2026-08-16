import { Injectable, computed, inject } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { switchMap } from 'rxjs';

const ADMIN_ROLE = 'Admin';

interface AccessTokenClaims {
  realm_access?: { roles?: string[] };
}

@Injectable({ providedIn: 'root' })
export class UserRolesService {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  private readonly accessTokenPayload = toSignal(
    toObservable(this.oidcSecurityService.authenticated).pipe(
      switchMap(() => this.oidcSecurityService.getPayloadFromAccessToken()),
    ),
    { initialValue: {} as AccessTokenClaims },
  );

  readonly roles = computed<readonly string[]>(
    () => this.accessTokenPayload().realm_access?.roles ?? [],
  );

  readonly isAdmin = computed(() => this.roles().includes(ADMIN_ROLE));
}
