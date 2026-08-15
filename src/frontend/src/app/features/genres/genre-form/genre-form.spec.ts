import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of, throwError } from 'rxjs';

import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { Genre } from '../genre';
import { GenreService } from '../genre.service';
import { GenreForm } from './genre-form';

describe('GenreForm', () => {
  let genreServiceMock: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let errorModalServiceMock: { showFromHttpError: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    genreServiceMock = { create: vi.fn(), update: vi.fn() };
    errorModalServiceMock = { showFromHttpError: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: GenreService, useValue: genreServiceMock },
        { provide: ErrorModalService, useValue: errorModalServiceMock },
      ],
    });
  });

  function createFixture(genre: Genre | null = null) {
    const fixture = TestBed.createComponent(GenreForm);

    if (genre) {
      fixture.componentRef.setInput('genre', genre);
    }

    fixture.detectChanges();
    return fixture;
  }

  function typeName(fixture: ReturnType<typeof createFixture>, value: string): void {
    const input = (fixture.nativeElement as HTMLElement).querySelector(
      '#genre-name',
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
    typeName(fixture, 'Ro');

    // assert
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent,
    ).toContain('mindestens 3 Zeichen');
  });

  it('zeigt einen Fehler bei zu langem Namen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, 'R'.repeat(51));

    // assert
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent,
    ).toContain('höchstens 50 Zeichen');
  });

  it('zeigt einen Fehler bei verbotenen Zeichen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, 'Rock!');

    // assert
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent,
    ).toContain('Buchstaben, Zahlen, Leerzeichen');
  });

  it('ruft im Anlegen-Modus GenreService.create auf und emittiert saved', async () => {
    // arrange
    genreServiceMock.create.mockReturnValue(of({ id: 1, name: 'Rock' }));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Rock');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(genreServiceMock.create).toHaveBeenCalledWith({ name: 'Rock' });
    expect(savedHandler).toHaveBeenCalledTimes(1);
  });

  it('befüllt das Namensfeld im Bearbeiten-Modus mit dem bestehenden Namen vor', () => {
    // arrange
    // act
    const fixture = createFixture({ id: 5, name: 'Rock' });

    // assert
    const input = (fixture.nativeElement as HTMLElement).querySelector(
      '#genre-name',
    ) as HTMLInputElement;
    expect(input.value).toBe('Rock');
  });

  it('ruft im Bearbeiten-Modus GenreService.update mit der Id des übergebenen Genres auf', async () => {
    // arrange
    genreServiceMock.update.mockReturnValue(of({ id: 5, name: 'Jazz' }));
    const fixture = createFixture({ id: 5, name: 'Rock' });
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Jazz');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(genreServiceMock.update).toHaveBeenCalledWith(5, { name: 'Jazz' });
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
    genreServiceMock.create.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Rock');

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
    genreServiceMock.create.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Rock');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(error, 'Genre');
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
