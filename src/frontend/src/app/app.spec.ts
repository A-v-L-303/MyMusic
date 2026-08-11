import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { App } from './app';
import { ThemeService } from './core/theme/theme.service';

describe('App', () => {
  let authenticatedSignal: ReturnType<typeof signal<{ isAuthenticated: boolean }>>;
  let userDataSignal: ReturnType<
    typeof signal<{ userData: { preferred_username?: string } | undefined }>
  >;
  let authorizeMock: ReturnType<typeof vi.fn>;
  let logoffMock: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    authenticatedSignal = signal({ isAuthenticated: false });
    userDataSignal = signal({ userData: undefined });
    authorizeMock = vi.fn();
    logoffMock = vi.fn().mockReturnValue(of(undefined));

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        {
          provide: OidcSecurityService,
          useValue: {
            authenticated: authenticatedSignal,
            userData: userDataSignal,
            authorize: authorizeMock,
            logoff: logoffMock,
          },
        },
        { provide: ThemeService, useValue: { effectiveTheme: signal('light'), toggle: vi.fn() } },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the MyMusic wordmark', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('MyMusic');
  });

  it('zeigt den Login-Button, wenn nicht angemeldet, und löst authorize() aus', () => {
    // arrange
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('button.btn-secondary');

    // act
    button?.click();

    // assert
    expect(button?.textContent).toContain('Login');
    expect(authorizeMock).toHaveBeenCalledTimes(1);
  });

  it('zeigt den Logout-Button, wenn angemeldet, und löst logoff() aus', () => {
    // arrange
    authenticatedSignal.set({ isAuthenticated: true });
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('button.btn-secondary');

    // act
    button?.click();

    // assert
    expect(button?.textContent).toContain('Logout');
    expect(logoffMock).toHaveBeenCalledTimes(1);
  });

  it('zeigt den Benutzernamen in der Kopfzeile, wenn angemeldet', () => {
    // arrange
    authenticatedSignal.set({ isAuthenticated: true });
    userDataSignal.set({ userData: { preferred_username: 'block7a-manueller-test' } });
    const fixture = TestBed.createComponent(App);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('block7a-manueller-test');
  });

  it('zeigt keinen Benutzernamen, solange die Anmeldedaten noch nicht geladen sind', () => {
    // arrange
    authenticatedSignal.set({ isAuthenticated: true });
    const fixture = TestBed.createComponent(App);

    // act
    fixture.detectChanges();

    // assert: kein Absturz, nur der Logout-Button ohne Namenstext
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('button.btn-secondary')?.textContent).toContain('Logout');
  });
});
