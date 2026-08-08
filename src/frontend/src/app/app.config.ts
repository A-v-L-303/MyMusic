import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import {
  INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
  includeBearerTokenInterceptor,
  provideKeycloak,
} from 'keycloak-angular';

import { routes } from './app.routes';
import { buildBearerTokenCondition, buildKeycloakConfig } from './core/auth/keycloak.config';
import { RUNTIME_CONFIG, RuntimeConfig } from './core/runtime-config/runtime-config.service';

export function buildAppConfig(runtimeConfig: RuntimeConfig): ApplicationConfig {
  return {
    providers: [
      provideBrowserGlobalErrorListeners(),
      provideRouter(routes),
      { provide: RUNTIME_CONFIG, useValue: runtimeConfig },
      provideKeycloak({
        config: buildKeycloakConfig(runtimeConfig.keycloakUrl),
        initOptions: {
          onLoad: 'check-sso',
          silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
        },
      }),
      provideHttpClient(withInterceptors([includeBearerTokenInterceptor])),
      {
        provide: INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
        useValue: [buildBearerTokenCondition(runtimeConfig.apiBaseUrl)],
      },
    ],
  };
}
