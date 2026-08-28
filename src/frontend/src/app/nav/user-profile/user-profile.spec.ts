import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of, throwError } from 'rxjs';

import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { UserProfileService } from './user-profile.service';
import { UserProfile } from './user-profile';

describe('UserProfile', () => {
  let userProfileServiceMock: { updateEmail: ReturnType<typeof vi.fn>; changePassword: ReturnType<typeof vi.fn> };
  let errorModalServiceMock: { showFromHttpError: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    userProfileServiceMock = { updateEmail: vi.fn(), changePassword: vi.fn() };
    errorModalServiceMock = { showFromHttpError: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: UserProfileService, useValue: userProfileServiceMock },
        { provide: ErrorModalService, useValue: errorModalServiceMock },
      ],
    });
  });

  function createFixture(username = 'erika', email = 'erika@example.com') {
    const fixture = TestBed.createComponent(UserProfile);
    fixture.componentRef.setInput('username', username);
    fixture.componentRef.setInput('email', email);
    fixture.detectChanges();
    return fixture;
  }

  function typeInto(fixture: ReturnType<typeof createFixture>, selector: string, value: string): void {
    const input = (fixture.nativeElement as HTMLElement).querySelector(selector) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function submitForm(fixture: ReturnType<typeof createFixture>, formIndex: number): void {
    const forms = (fixture.nativeElement as HTMLElement).querySelectorAll('form');
    forms[formIndex].dispatchEvent(new Event('submit', { cancelable: true }));
  }

  function hintFor(fixture: ReturnType<typeof createFixture>, inputSelector: string): HTMLElement | null {
    const input = (fixture.nativeElement as HTMLElement).querySelector(inputSelector);
    return input?.closest('.field')?.querySelector('.hint') ?? null;
  }

  it('zeigt den Benutzernamen als reinen Text und befüllt die E-Mail vor', () => {
    // arrange
    // act
    const fixture = createFixture('erika', 'erika@example.com');

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('erika');
    expect((compiled.querySelector('#profile-email') as HTMLInputElement).value).toBe(
      'erika@example.com',
    );
  });

  it('zeigt einen Pflichtfeld-Fehler bei leerer E-Mail-Adresse', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeInto(fixture, '#profile-email', '');

    // assert
    expect(hintFor(fixture, '#profile-email')?.textContent).toContain('erforderlich');
  });

  it('zeigt einen Formatfehler bei ungültiger E-Mail-Adresse', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeInto(fixture, '#profile-email', 'keine-email');

    // assert
    expect(hintFor(fixture, '#profile-email')?.textContent).toContain('gültiges Format');
  });

  it('ruft UserProfileService.updateEmail auf, emittiert emailChanged und zeigt eine Erfolgsmeldung', async () => {
    // arrange
    userProfileServiceMock.updateEmail.mockReturnValue(of(undefined));
    const fixture = createFixture();
    const emailChangedHandler = vi.fn();
    fixture.componentInstance.emailChanged.subscribe(emailChangedHandler);
    typeInto(fixture, '#profile-email', 'neu@example.com');

    // act
    submitForm(fixture, 0);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(userProfileServiceMock.updateEmail).toHaveBeenCalledWith('neu@example.com');
    expect(emailChangedHandler).toHaveBeenCalledWith('neu@example.com');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('E-Mail-Adresse wurde geändert.');
  });

  it('hängt eine 400-Serverantwort inline ins E-Mail-Feld ein, ohne das ErrorModal zu öffnen', async () => {
    // arrange
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        errors: { Email: ['Diese E-Mail-Adresse hat kein gültiges Format.'] },
        title: 'Validierungsfehler',
        status: 400,
      },
    });
    userProfileServiceMock.updateEmail.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    typeInto(fixture, '#profile-email', 'neu@example.com');

    // act
    submitForm(fixture, 0);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(hintFor(fixture, '#profile-email')?.textContent).toContain(
      'Diese E-Mail-Adresse hat kein gültiges Format.',
    );
    expect(errorModalServiceMock.showFromHttpError).not.toHaveBeenCalled();
  });

  it('leitet einen 409-Konflikt an den ErrorModalService weiter', async () => {
    // arrange
    const error = new HttpErrorResponse({ status: 409 });
    userProfileServiceMock.updateEmail.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    typeInto(fixture, '#profile-email', 'vergeben@example.com');

    // act
    submitForm(fixture, 0);
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(error, 'E-Mail-Adresse');
  });

  it('zeigt einen Fehler, wenn die Passwort-Wiederholung nicht übereinstimmt', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeInto(fixture, '#profile-new-password', 'einSicheresPasswort1');
    typeInto(fixture, '#profile-new-password-confirmation', 'einAnderesPasswort');

    // assert
    expect(hintFor(fixture, '#profile-new-password-confirmation')?.textContent).toContain(
      'stimmen nicht überein',
    );
  });

  it('ruft UserProfileService.changePassword auf und leert die Felder nach Erfolg', async () => {
    // arrange
    userProfileServiceMock.changePassword.mockReturnValue(of(undefined));
    const fixture = createFixture();
    typeInto(fixture, '#profile-new-password', 'einSicheresPasswort1');
    typeInto(fixture, '#profile-new-password-confirmation', 'einSicheresPasswort1');

    // act
    submitForm(fixture, 1);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(userProfileServiceMock.changePassword).toHaveBeenCalledWith('einSicheresPasswort1');
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('#profile-new-password') as HTMLInputElement,
    ).toHaveProperty('value', '');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Passwort wurde geändert.');
  });

  it('reserviert den Hint-Platz auch ohne Meldung, damit Buttons nicht die Position wechseln', () => {
    // arrange
    // act
    const fixture = createFixture();

    // assert: die Hint-Elemente sind immer im DOM vorhanden (nur unsichtbar), damit
    // die darunterliegenden Speichern-Buttons nicht je nach Meldung nach unten rutschen
    expect(hintFor(fixture, '#profile-email')).toBeTruthy();
    expect(hintFor(fixture, '#profile-email')?.classList.contains('invisible')).toBe(true);
    expect(hintFor(fixture, '#profile-new-password')).toBeTruthy();
    expect(hintFor(fixture, '#profile-new-password')?.classList.contains('invisible')).toBe(true);
    expect(hintFor(fixture, '#profile-new-password-confirmation')).toBeTruthy();
    expect(
      hintFor(fixture, '#profile-new-password-confirmation')?.classList.contains('invisible'),
    ).toBe(true);
  });

  it('emittiert closed beim Klick auf Schließen', () => {
    // arrange
    const fixture = createFixture();
    const closedHandler = vi.fn();
    fixture.componentInstance.closed.subscribe(closedHandler);

    // act
    (
      (fixture.nativeElement as HTMLElement).querySelector('.modal-foot .btn-secondary') as HTMLButtonElement
    ).click();

    // assert
    expect(closedHandler).toHaveBeenCalledTimes(1);
  });
});
