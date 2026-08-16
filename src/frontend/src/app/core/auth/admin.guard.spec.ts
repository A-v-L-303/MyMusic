import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { describe, expect, it } from 'vitest';

import { adminGuard } from './admin.guard';
import { UserRolesService } from './user-roles.service';

function configureWithIsAdmin(isAdmin: boolean): Router {
  TestBed.configureTestingModule({
    providers: [
      provideRouter([]),
      { provide: UserRolesService, useValue: { isAdmin: () => isAdmin } },
    ],
  });

  return TestBed.inject(Router);
}

function runGuard(): boolean | UrlTree {
  return TestBed.runInInjectionContext(() =>
    adminGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
  ) as boolean | UrlTree;
}

describe('adminGuard', () => {
  it('erlaubt den Zugriff, wenn die Rolle Admin vorhanden ist', () => {
    // arrange
    configureWithIsAdmin(true);

    // act
    const result = runGuard();

    // assert
    expect(result).toBe(true);
  });

  it('leitet auf /dashboard um, wenn die Rolle Admin fehlt', () => {
    // arrange
    const router = configureWithIsAdmin(false);

    // act
    const result = runGuard();

    // assert: kein stiller Boolean, sondern ein UrlTree auf /dashboard
    expect(router.serializeUrl(result as UrlTree)).toBe('/dashboard');
  });
});
