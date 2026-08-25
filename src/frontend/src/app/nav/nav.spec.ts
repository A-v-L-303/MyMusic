import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { UserRolesService } from '../core/auth/user-roles.service';
import { ThemeService } from '../core/theme/theme.service';
import { Nav } from './nav';

@Component({ selector: 'app-dummy', template: '' })
class Dummy {}

describe('Nav', () => {
  let authenticatedSignal: ReturnType<typeof signal<{ isAuthenticated: boolean }>>;
  let userDataSignal: ReturnType<
    typeof signal<{ userData: { preferred_username?: string } | undefined }>
  >;
  let authorizeMock: ReturnType<typeof vi.fn>;
  let logoffMock: ReturnType<typeof vi.fn>;
  let isAdminSignal: ReturnType<typeof signal<boolean>>;
  let router: Router;

  beforeEach(async () => {
    authenticatedSignal = signal({ isAuthenticated: false });
    userDataSignal = signal({ userData: undefined });
    authorizeMock = vi.fn();
    logoffMock = vi.fn().mockReturnValue(of(undefined));
    isAdminSignal = signal(false);

    await TestBed.configureTestingModule({
      imports: [Nav],
      providers: [
        provideRouter([
          { path: 'dashboard', component: Dummy },
          { path: 'records', component: Dummy },
          { path: 'artists', component: Dummy },
          { path: 'labels', component: Dummy },
          { path: 'genres', component: Dummy },
          { path: 'search', component: Dummy },
        ]),
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
        { provide: UserRolesService, useValue: { isAdmin: isAdminSignal } },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
  });

  it('zeigt die Markenbezeichnung MyMusic', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('MyMusic');
  });

  it('markiert den Records-Tab als aktiv, wenn /records aufgerufen ist', async () => {
    // arrange
    await router.navigateByUrl('/records');
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    await fixture.whenStable();

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    const recordsTab = compiled.querySelector('a[routerLink="/records"]');
    expect(recordsTab?.classList.contains('is-active')).toBe(true);
  });

  it('markiert den Option-Trigger als aktiv, wenn /artists aufgerufen ist', async () => {
    // arrange
    await router.navigateByUrl('/artists');
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    await fixture.whenStable();

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    const trigger = compiled.querySelector('button.tab');
    expect(trigger?.classList.contains('is-active')).toBe(true);
  });

  it('hat einen Tooltip am Option-Trigger', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();

    // act
    const trigger = (fixture.nativeElement as HTMLElement).querySelector('button.tab');

    // assert
    expect(trigger?.getAttribute('title')).toBe('Weitere Bereiche anzeigen');
  });

  it('öffnet und schließt das Option-Dropdown per Klick auf den Trigger', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const trigger = compiled.querySelector<HTMLButtonElement>('button.tab');

    // act: erster Klick öffnet
    trigger?.click();
    fixture.detectChanges();

    // assert
    expect(compiled.querySelector('a[routerLink="/artists"]')).toBeTruthy();

    // act: zweiter Klick schließt wieder
    trigger?.click();
    fixture.detectChanges();

    // assert
    expect(compiled.querySelector('a[routerLink="/artists"]')).toBeFalsy();
  });

  it('schließt das Option-Dropdown bei Klick außerhalb', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const trigger = compiled.querySelector<HTMLButtonElement>('button.tab');
    trigger?.click();
    fixture.detectChanges();
    expect(compiled.querySelector('a[routerLink="/artists"]')).toBeTruthy();

    // act
    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    fixture.detectChanges();

    // assert
    expect(compiled.querySelector('a[routerLink="/artists"]')).toBeFalsy();
  });

  it('schließt das Option-Dropdown bei Klick auf einen Eintrag', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const trigger = compiled.querySelector<HTMLButtonElement>('button.tab');
    trigger?.click();
    fixture.detectChanges();
    const artistsLink = compiled.querySelector<HTMLAnchorElement>('a[routerLink="/artists"]');

    // act
    artistsLink?.click();
    fixture.detectChanges();

    // assert
    expect(compiled.querySelector('a[routerLink="/artists"]')).toBeFalsy();
  });

  it('navigiert bei Sucheingabe und Submit zu /search mit dem Suchbegriff', () => {
    // arrange
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const input = compiled.querySelector<HTMLInputElement>('form.search input');
    const form = compiled.querySelector<HTMLFormElement>('form.search');
    if (input) {
      input.value = 'jazz';
      input.dispatchEvent(new Event('input'));
    }
    fixture.detectChanges();

    // act
    form?.dispatchEvent(new Event('submit', { cancelable: true }));

    // assert
    expect(navigateSpy).toHaveBeenCalledWith(['/search'], { queryParams: { q: 'jazz' } });
  });

  it('navigiert nicht bei leerer Sucheingabe und zeigt keine Fehlermeldung', () => {
    // arrange
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const form = compiled.querySelector<HTMLFormElement>('form.search');

    // act
    form?.dispatchEvent(new Event('submit', { cancelable: true }));
    fixture.detectChanges();

    // assert
    expect(navigateSpy).not.toHaveBeenCalled();
    expect(compiled.querySelector('form.search .hint.is-error')).toBeFalsy();
  });

  it('zeigt eine Fehlermeldung und navigiert nicht bei zu kurzer Sucheingabe', () => {
    // arrange
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const input = compiled.querySelector<HTMLInputElement>('form.search input');
    const form = compiled.querySelector<HTMLFormElement>('form.search');
    if (input) {
      input.value = 'a';
      input.dispatchEvent(new Event('input'));
    }
    fixture.detectChanges();

    // act
    form?.dispatchEvent(new Event('submit', { cancelable: true }));
    fixture.detectChanges();

    // assert
    expect(navigateSpy).not.toHaveBeenCalled();
    expect(compiled.querySelector('form.search .hint.is-error')?.textContent).toContain(
      'mindestens 2 Zeichen',
    );
  });

  it('zeigt eine Fehlermeldung und navigiert nicht bei verbotenen Sonderzeichen', () => {
    // arrange
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const input = compiled.querySelector<HTMLInputElement>('form.search input');
    const form = compiled.querySelector<HTMLFormElement>('form.search');
    if (input) {
      input.value = 'jazz!';
      input.dispatchEvent(new Event('input'));
    }
    fixture.detectChanges();

    // act
    form?.dispatchEvent(new Event('submit', { cancelable: true }));
    fixture.detectChanges();

    // assert
    expect(navigateSpy).not.toHaveBeenCalled();
    expect(compiled.querySelector('form.search .hint.is-error')?.textContent).toContain(
      'Buchstaben, Zahlen',
    );
  });

  it('zeigt keinen Admin-Button, wenn die Rolle Admin fehlt', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('a[routerLink="/admin"]')).toBeFalsy();
  });

  it('zeigt den Admin-Button, wenn die Rolle Admin vorhanden ist', () => {
    // arrange
    isAdminSignal.set(true);
    const fixture = TestBed.createComponent(Nav);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    const adminLink = compiled.querySelector<HTMLAnchorElement>('a[routerLink="/admin"]');
    expect(adminLink?.textContent?.trim()).toBe('Admin');
    expect(adminLink?.title).toBe('Admin-Bereich öffnen');
  });

  it('zeigt den Theme-Toggle', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);

    // act
    fixture.detectChanges();

    // assert
    expect(fixture.nativeElement.querySelector('app-theme-toggle')).toBeTruthy();
  });

  it('zeigt den Login-Button, wenn nicht angemeldet, und löst authorize() aus', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('button[title="Anmelden"]');

    // act
    button?.click();

    // assert
    expect(button?.textContent).toContain('Login');
    expect(authorizeMock).toHaveBeenCalledTimes(1);
    expect(authorizeMock).toHaveBeenCalledWith();
  });

  it('zeigt den Registrieren-Button, wenn nicht angemeldet, und leitet zur Keycloak-Registrierung um', () => {
    // arrange
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('button[title="Registrieren"]');
    const originalLocation = window.location;
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: { ...originalLocation, href: '' },
    });

    // act
    button?.click();
    const authOptions = authorizeMock.mock.calls[0]?.[1] as {
      urlHandler: (url: string) => void;
    };
    authOptions.urlHandler(
      'http://localhost:8080/realms/mymusic/protocol/openid-connect/auth?client_id=mymusic-angular',
    );

    // assert: nur der Pfad wechselt auf die Registrierungsseite, der Rest der URL bleibt gleich
    expect(button?.textContent).toContain('Registrieren');
    expect(authorizeMock).toHaveBeenCalledWith(undefined, { urlHandler: expect.any(Function) });
    expect(window.location.href).toBe(
      'http://localhost:8080/realms/mymusic/protocol/openid-connect/registrations?client_id=mymusic-angular',
    );

    // cleanup
    Object.defineProperty(window, 'location', { configurable: true, value: originalLocation });
  });

  it('zeigt den Logout-Button und den Benutzernamen, wenn angemeldet, und löst logoff() aus', () => {
    // arrange
    authenticatedSignal.set({ isAuthenticated: true });
    userDataSignal.set({ userData: { preferred_username: 'nav-test-user' } });
    const fixture = TestBed.createComponent(Nav);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('button.btn-secondary');

    // act
    button?.click();

    // assert
    expect(compiled.textContent).toContain('nav-test-user');
    expect(button?.textContent).toContain('Logout');
    expect(button?.title).toBe('Abmelden');
    expect(logoffMock).toHaveBeenCalledTimes(1);
  });
});
