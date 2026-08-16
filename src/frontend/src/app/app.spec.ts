import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { App } from './app';
import { ThemeService } from './core/theme/theme.service';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        {
          provide: OidcSecurityService,
          useValue: {
            authenticated: signal({ isAuthenticated: false }),
            userData: signal({ userData: undefined }),
            authorize: vi.fn(),
            logoff: vi.fn().mockReturnValue(of(undefined)),
            getPayloadFromAccessToken: vi.fn().mockReturnValue(of({})),
          },
        },
        { provide: ThemeService, useValue: { effectiveTheme: signal('light'), toggle: vi.fn() } },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    // arrange
    const fixture = TestBed.createComponent(App);

    // act
    const app = fixture.componentInstance;

    // assert
    expect(app).toBeTruthy();
  });

  it('rendert die Navigation und den Router-Outlet', () => {
    // arrange
    const fixture = TestBed.createComponent(App);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-nav')).toBeTruthy();
    expect(compiled.querySelector('router-outlet')).toBeTruthy();
  });
});
