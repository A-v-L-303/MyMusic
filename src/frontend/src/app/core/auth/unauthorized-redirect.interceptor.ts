import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, isDevMode } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { catchError, throwError } from 'rxjs';

import { RuntimeConfigService } from '../runtime-config/runtime-config.service';

export const unauthorizedRedirectInterceptor: HttpInterceptorFn = (request, next) => {
  const runtimeConfigService = inject(RuntimeConfigService);
  const oidcSecurityService = inject(OidcSecurityService);

  return next(request).pipe(
    catchError((error: unknown) => {
      const istUnauthorisierteApiAntwort =
        error instanceof HttpErrorResponse &&
        request.url.startsWith(runtimeConfigService.apiBaseUrl) &&
        (error.status === 401 || error.status === 403);

      if (istUnauthorisierteApiAntwort) {
        if (isDevMode()) {
          const status = (error as HttpErrorResponse).status;

          console.error(
            `Sitzung abgelaufen oder nicht autorisiert (HTTP ${status}) bei ${request.url} — ` +
              'Weiterleitung zur Anmeldung.',
          );
        }

        oidcSecurityService.authorize();
      }

      return throwError(() => error);
    }),
  );
};
