import { IncludeBearerTokenCondition, createInterceptorCondition } from 'keycloak-angular';
import { KeycloakConfig } from 'keycloak-js';

export const KEYCLOAK_REALM = 'mymusic';
export const KEYCLOAK_CLIENT_ID = 'mymusic-angular';

export function buildKeycloakConfig(keycloakUrl: string): KeycloakConfig {
  return {
    url: keycloakUrl,
    realm: KEYCLOAK_REALM,
    clientId: KEYCLOAK_CLIENT_ID,
  };
}

export function buildBearerTokenCondition(apiBaseUrl: string): IncludeBearerTokenCondition {
  return createInterceptorCondition<IncludeBearerTokenCondition>({
    urlPattern: new RegExp(`^${escapeRegExp(apiBaseUrl)}(/.*)?$`, 'i'),
  });
}

function escapeRegExp(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}
