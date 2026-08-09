import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import {
  authInterceptor,
  provideAuth,
  StsConfigLoader,
  withAppInitializerAuthCheck,
} from 'angular-auth-oidc-client';

import { routes } from './app.routes';
import { keycloakConfigLoaderFactory } from './core/auth/keycloak-config.factory';
import { unauthorizedRedirectInterceptor } from './core/auth/unauthorized-redirect.interceptor';
import { RuntimeConfigService } from './core/runtime-config/runtime-config.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAppInitializer(() => inject(RuntimeConfigService).load()),
    provideHttpClient(withInterceptors([unauthorizedRedirectInterceptor, authInterceptor()])),
    provideAuth(
      {
        loader: {
          provide: StsConfigLoader,
          useFactory: keycloakConfigLoaderFactory,
        },
      },
      // autoLoginPartialRoutesGuard prüft nur, ob bereits gültige Tokens im Storage liegen —
      // ohne withAppInitializerAuthCheck() wird der OIDC-Callback (code/state) nie verarbeitet
      // und der Guard leitet nach jedem Rücksprung von Keycloak sofort wieder neu um (Endlosschleife).
      withAppInitializerAuthCheck(),
    ),
  ],
};
