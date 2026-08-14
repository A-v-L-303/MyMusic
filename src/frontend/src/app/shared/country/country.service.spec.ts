import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Country } from './country';
import { CountryService } from './country.service';

describe('CountryService', () => {
  let service: CountryService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(CountryService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('ruft getAll ohne Query-Parameter gegen /api/countries auf', () => {
    // arrange
    const response: Country[] = [
      { id: 1, name: 'Deutschland', code: 'DE' },
      { id: 2, name: 'Frankreich', code: 'FR' },
    ];
    let result: Country[] | undefined;

    // act
    service.getAll().subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/countries');
    request.flush(response);

    // assert
    expect(request.request.method).toBe('GET');
    expect(result).toEqual(response);
  });

  it('propagiert HTTP-Fehler an den Aufrufer', () => {
    // arrange
    let error: HttpErrorResponse | undefined;

    // act
    service.getAll().subscribe({ error: (err: HttpErrorResponse) => (error = err) });
    const request = httpTesting.expectOne('https://api.test/api/countries');
    request.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );

    // assert
    expect(error?.status).toBe(500);
  });
});
