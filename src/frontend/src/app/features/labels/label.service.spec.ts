import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Label, LabelListResponse } from './label';
import { LabelService } from './label.service';

describe('LabelService', () => {
  let service: LabelService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(LabelService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('ruft getPaged mit page und pageSize als Query-Parameter auf', () => {
    // arrange
    const response: LabelListResponse = {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20,
      totalPages: 0,
    };
    let result: LabelListResponse | undefined;

    // act
    service.getPaged(1, 20).subscribe((value) => (result = value));
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/labels' &&
        req.params.get('page') === '1' &&
        req.params.get('pageSize') === '20',
    );
    request.flush(response);

    // assert
    expect(result).toEqual(response);
  });

  it('hängt den name-Filter nur an, wenn er gesetzt ist', () => {
    // arrange
    // act
    service.getPaged(1, 20, 'Rough Trade').subscribe();
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/labels' && req.params.get('name') === 'Rough Trade',
    );
    request.flush({ items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });

    // assert
    expect(request.request.params.has('name')).toBe(true);
  });

  it('hängt den countryId-Filter nur an, wenn er gesetzt ist', () => {
    // arrange
    // act
    service.getPaged(1, 20, undefined, 42).subscribe();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/labels' && req.params.get('countryId') === '42',
    );
    request.flush({ items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });

    // assert
    expect(request.request.params.has('countryId')).toBe(true);
  });

  it('kombiniert name- und countryId-Filter', () => {
    // arrange
    // act
    service.getPaged(1, 20, 'Rough Trade', 42).subscribe();
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/labels' &&
        req.params.get('name') === 'Rough Trade' &&
        req.params.get('countryId') === '42',
    );
    request.flush({ items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });

    // assert
    expect(request.request.params.has('name')).toBe(true);
    expect(request.request.params.has('countryId')).toBe(true);
  });

  it('sendet create als POST mit name, countryId und information im Body', () => {
    // arrange
    const created: Label = {
      id: 1,
      name: 'Rough Trade',
      countryId: 1,
      countryName: 'Vereinigtes Königreich',
      information: null,
    };
    let result: Label | undefined;

    // act
    service
      .create({ name: 'Rough Trade', countryId: 1, information: null })
      .subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/labels');
    request.flush(created);

    // assert
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ name: 'Rough Trade', countryId: 1, information: null });
    expect(result).toEqual(created);
  });

  it('sendet update als PUT gegen die Id-Route', () => {
    // arrange
    const updated: Label = {
      id: 1,
      name: 'Rough Trade Records',
      countryId: 1,
      countryName: 'Vereinigtes Königreich',
      information: null,
    };
    let result: Label | undefined;

    // act
    service
      .update(1, { name: 'Rough Trade Records', countryId: 1, information: null })
      .subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/labels/1');
    request.flush(updated);

    // assert
    expect(request.request.method).toBe('PUT');
    expect(result).toEqual(updated);
  });

  it('sendet delete als DELETE gegen die Id-Route', () => {
    // arrange
    let completed = false;

    // act
    service.delete(1).subscribe({ complete: () => (completed = true) });
    const request = httpTesting.expectOne('https://api.test/api/labels/1');
    request.flush(null);

    // assert
    expect(request.request.method).toBe('DELETE');
    expect(completed).toBe(true);
  });

  it('propagiert HTTP-Fehler an den Aufrufer', () => {
    // arrange
    let error: HttpErrorResponse | undefined;

    // act
    service.getPaged(1, 20).subscribe({ error: (err: HttpErrorResponse) => (error = err) });
    const request = httpTesting.expectOne((req) => req.url === 'https://api.test/api/labels');
    request.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );

    // assert
    expect(error?.status).toBe(500);
  });
});
