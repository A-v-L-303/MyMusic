import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of, throwError } from 'rxjs';

import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { Artist } from '../artist';
import { ArtistService } from '../artist.service';
import { ArtistForm } from './artist-form';

describe('ArtistForm', () => {
  let artistServiceMock: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let errorModalServiceMock: { showFromHttpError: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    artistServiceMock = { create: vi.fn(), update: vi.fn() };
    errorModalServiceMock = { showFromHttpError: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: ArtistService, useValue: artistServiceMock },
        { provide: ErrorModalService, useValue: errorModalServiceMock },
      ],
    });
  });

  function createFixture(artist: Artist | null = null) {
    const fixture = TestBed.createComponent(ArtistForm);

    if (artist) {
      fixture.componentRef.setInput('artist', artist);
    }

    fixture.detectChanges();
    return fixture;
  }

  function typeName(fixture: ReturnType<typeof createFixture>, value: string): void {
    const input = (fixture.nativeElement as HTMLElement).querySelector(
      '#artist-name',
    ) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function submitForm(fixture: ReturnType<typeof createFixture>): void {
    const form = (fixture.nativeElement as HTMLElement).querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit', { cancelable: true }));
  }

  it('zeigt einen Pflichtfeld-Fehler bei leerem Namen nach Blur', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, '');

    // assert
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent,
    ).toContain('erforderlich');
  });

  it('zeigt einen Fehler bei zu kurzem Namen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, 'Mi');

    // assert
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent,
    ).toContain('mindestens 3 Zeichen');
  });

  it('zeigt einen Fehler bei zu langem Namen (mehr als 120 Zeichen)', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, 'M'.repeat(121));

    // assert
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent,
    ).toContain('höchstens 120 Zeichen');
  });

  it('zeigt einen Fehler bei verbotenen Zeichen (Klammern)', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, 'Miles (Live)');

    // assert
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent,
    ).toContain('Buchstaben, Zahlen, Leerzeichen');
  });

  it('akzeptiert Punkt und Schrägstrich im Namen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, 'AC/DC feat. Bon Scott');

    // assert
    expect((fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')).toBeNull();
  });

  it('ruft im Anlegen-Modus ArtistService.create auf und emittiert saved', async () => {
    // arrange
    artistServiceMock.create.mockReturnValue(of({ id: 1, name: 'Miles Davis' }));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Miles Davis');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(artistServiceMock.create).toHaveBeenCalledWith({ name: 'Miles Davis' });
    expect(savedHandler).toHaveBeenCalledTimes(1);
  });

  it('befüllt das Namensfeld im Bearbeiten-Modus mit dem bestehenden Namen vor', () => {
    // arrange
    // act
    const fixture = createFixture({ id: 5, name: 'Miles Davis' });

    // assert
    const input = (fixture.nativeElement as HTMLElement).querySelector(
      '#artist-name',
    ) as HTMLInputElement;
    expect(input.value).toBe('Miles Davis');
  });

  it('ruft im Bearbeiten-Modus ArtistService.update mit der Id des übergebenen Artists auf', async () => {
    // arrange
    artistServiceMock.update.mockReturnValue(of({ id: 5, name: 'John Coltrane' }));
    const fixture = createFixture({ id: 5, name: 'Miles Davis' });
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'John Coltrane');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(artistServiceMock.update).toHaveBeenCalledWith(5, { name: 'John Coltrane' });
    expect(savedHandler).toHaveBeenCalledTimes(1);
  });

  it('hängt eine 400-Serverantwort inline ins Namensfeld ein, ohne das ErrorModal zu öffnen', async () => {
    // arrange
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        errors: { Name: ['Der Name ist bereits vergeben.'] },
        title: 'Validierungsfehler',
        status: 400,
      },
    });
    artistServiceMock.create.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Miles Davis');

    // act
    submitForm(fixture);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent,
    ).toContain('Der Name ist bereits vergeben.');
    expect(errorModalServiceMock.showFromHttpError).not.toHaveBeenCalled();
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('leitet 409/404/500/Netzwerkfehler an den ErrorModalService weiter, ohne saved zu emittieren', async () => {
    // arrange
    const error = new HttpErrorResponse({ status: 409 });
    artistServiceMock.create.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Miles Davis');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(error, 'Artist');
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('hat Tooltips an Abbrechen- und Speichern-Button', () => {
    // arrange
    const fixture = createFixture();
    const compiled = fixture.nativeElement as HTMLElement;

    // act
    const cancelButton = compiled.querySelector('.btn-secondary') as HTMLButtonElement;
    const submitButton = compiled.querySelector('.btn-primary') as HTMLButtonElement;

    // assert
    expect(cancelButton.title).toBe('Abbrechen');
    expect(submitButton.title).toBe('Speichern');
  });

  it('emittiert cancelled beim Abbrechen', () => {
    // arrange
    const fixture = createFixture();
    const cancelledHandler = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelledHandler);

    // act
    (
      (fixture.nativeElement as HTMLElement).querySelector('.btn-secondary') as HTMLButtonElement
    ).click();

    // assert
    expect(cancelledHandler).toHaveBeenCalledTimes(1);
  });
});
