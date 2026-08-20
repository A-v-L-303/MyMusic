import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { describe, expect, it, vi } from 'vitest';

import { Landing } from './landing';

describe('Landing', () => {
  it('leitet auf /dashboard um, wenn bereits angemeldet', () => {
    // arrange
    const navigateMock = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: { navigate: navigateMock } },
        {
          provide: OidcSecurityService,
          useValue: { authenticated: () => ({ isAuthenticated: true }) },
        },
      ],
    });

    // act
    TestBed.createComponent(Landing);

    // assert
    expect(navigateMock).toHaveBeenCalledWith(['/dashboard'], { replaceUrl: true });
  });

  it('navigiert nicht, wenn nicht angemeldet - Login/Registrieren bleiben erreichbar', () => {
    // arrange
    const navigateMock = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: { navigate: navigateMock } },
        {
          provide: OidcSecurityService,
          useValue: { authenticated: () => ({ isAuthenticated: false }) },
        },
      ],
    });

    // act
    TestBed.createComponent(Landing);

    // assert
    expect(navigateMock).not.toHaveBeenCalled();
  });
});
