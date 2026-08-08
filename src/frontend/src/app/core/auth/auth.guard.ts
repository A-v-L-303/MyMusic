import { CanActivateChildFn } from '@angular/router';
import { createAuthGuard } from 'keycloak-angular';

export const authGuard: CanActivateChildFn = createAuthGuard<CanActivateChildFn>(
  async (_route, state, authData) => {
    const { authenticated, keycloak } = authData;

    if (authenticated) {
      return true;
    }

    await keycloak.login({ redirectUri: `${window.location.origin}${state.url}` });
    return false;
  },
);
