import { inject, isDevMode } from '@angular/core';
import {
  LogLevel,
  OpenIdConfiguration,
  StsConfigHttpLoader,
  StsConfigLoader,
} from 'angular-auth-oidc-client';
import { from } from 'rxjs';
import { map } from 'rxjs/operators';

import { RuntimeConfigService } from '../runtime-config/runtime-config.service';

export function keycloakConfigLoaderFactory(): StsConfigLoader {
  const runtimeConfigService = inject(RuntimeConfigService);

  const config$ = from(runtimeConfigService.load()).pipe(
    map(() => buildOpenIdConfiguration(runtimeConfigService)),
  );

  return new StsConfigHttpLoader(config$);
}

function buildOpenIdConfiguration(runtimeConfigService: RuntimeConfigService): OpenIdConfiguration {
  return {
    authority: runtimeConfigService.keycloakAuthority,
    clientId: 'mymusic-angular',
    redirectUrl: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    responseType: 'code',
    scope: 'openid profile email',
    useRefreshToken: true,
    silentRenew: true,
    secureRoutes: [runtimeConfigService.apiBaseUrl],
    logLevel: isDevMode() ? LogLevel.Warn : LogLevel.None,
  };
}
