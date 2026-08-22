import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { ErrorModalService } from './error-modal.service';

describe('ErrorModalService', () => {
  let service: ErrorModalService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ErrorModalService);
  });

  it('mappt Status 0 auf network mit Retry-Callback', () => {
    // arrange
    const error = new HttpErrorResponse({ status: 0 });
    const onRetry = vi.fn();

    // act
    service.showFromHttpError(error, 'Genre', onRetry);

    // assert
    expect(service.current()).toEqual(expect.objectContaining({ kind: 'network', onRetry }));
  });

  it('mappt Status 404 auf not-found mit entitätsspezifischer Meldung', () => {
    // arrange
    const error = new HttpErrorResponse({ status: 404 });

    // act
    service.showFromHttpError(error, 'Genre');

    // assert
    expect(service.current()).toEqual(
      expect.objectContaining({ kind: 'not-found', message: 'Genre wurde nicht gefunden.' }),
    );
  });

  it('mappt Status 409 auf conflict und übernimmt das detail-Feld des Bodys', () => {
    // arrange
    const error = new HttpErrorResponse({
      status: 409,
      error: {
        title: 'Konflikt',
        detail: 'Genre wird noch von einem Track referenziert.',
        status: 409,
      },
    });

    // act
    service.showFromHttpError(error, 'Genre');

    // assert
    expect(service.current()).toEqual(
      expect.objectContaining({
        kind: 'conflict',
        message: 'Genre wird noch von einem Track referenziert.',
      }),
    );
  });

  it('mappt Status 502 auf discogs mit Hinweis zur manuellen Eingabe', () => {
    // arrange
    const error = new HttpErrorResponse({ status: 502 });

    // act
    service.showFromHttpError(error, 'Discogs');

    // assert
    expect(service.current()).toEqual(
      expect.objectContaining({
        kind: 'discogs',
        message: 'Discogs ist aktuell nicht erreichbar. Bitte die Daten manuell eingeben.',
      }),
    );
  });

  it('mappt Status 429 auf rate-limit', () => {
    // arrange
    const error = new HttpErrorResponse({ status: 429 });

    // act
    service.showFromHttpError(error, 'Genre');

    // assert
    expect(service.current()).toEqual(expect.objectContaining({ kind: 'rate-limit' }));
  });

  it('mappt Status 500 auf server', () => {
    // arrange
    const error = new HttpErrorResponse({ status: 500 });

    // act
    service.showFromHttpError(error, 'Genre');

    // assert
    expect(service.current()).toEqual(expect.objectContaining({ kind: 'server' }));
  });

  it('ignoriert Status 401 und 403 (bereits vom Interceptor behandelt)', () => {
    // arrange
    const error401 = new HttpErrorResponse({ status: 401 });
    const error403 = new HttpErrorResponse({ status: 403 });

    // act
    service.showFromHttpError(error401, 'Genre');
    service.showFromHttpError(error403, 'Genre');

    // assert
    expect(service.current()).toBeNull();
  });

  it('setzt den Zustand bei dismiss() zurück', () => {
    // arrange
    service.showFromHttpError(new HttpErrorResponse({ status: 500 }), 'Genre');

    // act
    service.dismiss();

    // assert
    expect(service.current()).toBeNull();
  });
});
