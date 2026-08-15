import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { ErrorModal } from './error-modal';
import { ErrorModalService } from './error-modal.service';

describe('ErrorModal', () => {
  let service: ErrorModalService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ErrorModalService);
  });

  it('zeigt nichts an, solange kein Fehler gesetzt ist', () => {
    // arrange
    const fixture = TestBed.createComponent(ErrorModal);

    // act
    fixture.detectChanges();

    // assert
    expect((fixture.nativeElement as HTMLElement).querySelector('.scrim')).toBeNull();
  });

  it('zeigt Titel und Meldung für einen Serverfehler', () => {
    // arrange
    service.showFromHttpError(new HttpErrorResponse({ status: 500 }), 'Genre');
    const fixture = TestBed.createComponent(ErrorModal);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Serverfehler');
    expect(compiled.textContent).toContain('Es ist ein unerwarteter Serverfehler aufgetreten.');
  });

  it('zeigt bei einem Validierungsfehler (400) die erste Fehlermeldung aus errors', () => {
    // arrange
    service.showFromHttpError(
      new HttpErrorResponse({
        status: 400,
        error: { title: 'Validierungsfehler', status: 400, errors: { FileContent: ['ungültig'] } },
      }),
      'Album-Cover',
    );
    const fixture = TestBed.createComponent(ErrorModal);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Ungültige Eingabe');
    expect(compiled.textContent).toContain('ungültig');
  });

  it('zeigt bei einem Validierungsfehler (400) ohne errors einen Fallback-Text', () => {
    // arrange
    service.showFromHttpError(new HttpErrorResponse({ status: 400 }), 'Album-Cover');
    const fixture = TestBed.createComponent(ErrorModal);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Die Eingabe ist ungültig.');
  });

  it('zeigt eine manuell gesetzte Validierungsmeldung', () => {
    // arrange
    service.showValidationMessage('Es sind nur JPEG- oder PNG-Dateien bis 5 MB erlaubt.');
    const fixture = TestBed.createComponent(ErrorModal);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Ungültige Eingabe');
    expect(compiled.textContent).toContain('Es sind nur JPEG- oder PNG-Dateien bis 5 MB erlaubt.');
  });

  it('zeigt bei einem Netzwerkfehler die Schaltfläche „Erneut versuchen" und ruft onRetry auf', () => {
    // arrange
    const onRetry = vi.fn();
    service.showFromHttpError(new HttpErrorResponse({ status: 0 }), 'Genre', onRetry);
    const fixture = TestBed.createComponent(ErrorModal);
    fixture.detectChanges();
    const retryButton = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ).find((button) => button.textContent?.includes('Erneut versuchen')) as HTMLButtonElement;

    // act
    retryButton.click();

    // assert
    expect(onRetry).toHaveBeenCalledTimes(1);
    expect(service.current()).toBeNull();
  });

  it('hat Tooltips an Schließen- und Erneut-versuchen-Button bei Netzwerkfehler', () => {
    // arrange
    service.showFromHttpError(new HttpErrorResponse({ status: 0 }), 'Genre');
    const fixture = TestBed.createComponent(ErrorModal);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    // act
    const buttons = Array.from(compiled.querySelectorAll('button')) as HTMLButtonElement[];

    // assert
    expect(buttons.find((button) => button.textContent?.includes('Schließen'))?.title).toBe(
      'Schließen',
    );
    expect(
      buttons.find((button) => button.textContent?.includes('Erneut versuchen'))?.title,
    ).toBe('Erneut versuchen');
  });

  it('hat einen Tooltip am OK-Button', () => {
    // arrange
    service.showFromHttpError(new HttpErrorResponse({ status: 404 }), 'Genre');
    const fixture = TestBed.createComponent(ErrorModal);
    fixture.detectChanges();

    // act
    const okButton = (fixture.nativeElement as HTMLElement).querySelector(
      '.btn-primary',
    ) as HTMLButtonElement;

    // assert
    expect(okButton.title).toBe('OK');
  });

  it('schließt den Fehler beim Klick auf OK', () => {
    // arrange
    service.showFromHttpError(new HttpErrorResponse({ status: 404 }), 'Genre');
    const fixture = TestBed.createComponent(ErrorModal);
    fixture.detectChanges();
    const okButton = (fixture.nativeElement as HTMLElement).querySelector(
      '.btn-primary',
    ) as HTMLButtonElement;

    // act
    okButton.click();

    // assert
    expect(service.current()).toBeNull();
  });
});
