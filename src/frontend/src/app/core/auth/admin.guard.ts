import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { UserRolesService } from './user-roles.service';

export const adminGuard: CanActivateFn = () => {
  const userRolesService = inject(UserRolesService);
  const router = inject(Router);

  return userRolesService.isAdmin() ? true : router.createUrlTree(['/dashboard']);
};
