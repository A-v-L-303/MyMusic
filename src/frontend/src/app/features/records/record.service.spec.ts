import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { RecordListResponse } from './record';
import { RecordService } from './record.service';

describe('RecordService', () => {
  let service: RecordService;
  let httpTesting: HttpTestingController;

  const emptyResponse: RecordListResponse = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(RecordService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('ruft getPaged mit page und pageSize als Query-Parameter auf', () => {
    // arrange
    let result: RecordListResponse | undefined;

    // act
    service.getPaged(1, 20).subscribe((value) => (result = value));
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/records' &&
        req.params.get('page') === '1' &&
        req.params.get('pageSize') === '20',
    );
    request.flush(emptyResponse);

    // assert
    expect(result).toEqual(emptyResponse);
  });

  it('hängt den name-Filter nur an, wenn er gesetzt ist', () => {
    // arrange
    // act
    service.getPaged(1, 20, 'Abbey Road').subscribe();
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/records' && req.params.get('name') === 'Abbey Road',
    );
    request.flush(emptyResponse);

    // assert
    expect(request.request.params.has('name')).toBe(true);
  });

  it('hängt den artistId-Filter nur an, wenn er gesetzt ist', () => {
    // arrange
    // act
    service.getPaged(1, 20, undefined, 7).subscribe();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/records' && req.params.get('artistId') === '7',
    );
    request.flush(emptyResponse);

    // assert
    expect(request.request.params.has('artistId')).toBe(true);
  });

  it('hängt den labelId-Filter nur an, wenn er gesetzt ist', () => {
    // arrange
    // act
    service.getPaged(1, 20, undefined, undefined, 3).subscribe();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/records' && req.params.get('labelId') === '3',
    );
    request.flush(emptyResponse);

    // assert
    expect(request.request.params.has('labelId')).toBe(true);
  });

  it('hängt yearFrom und yearTo nur an, wenn sie gesetzt sind', () => {
    // arrange
    // act
    service.getPaged(1, 20, undefined, undefined, undefined, 1960, 1969).subscribe();
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/records' &&
        req.params.get('yearFrom') === '1960' &&
        req.params.get('yearTo') === '1969',
    );
    request.flush(emptyResponse);

    // assert
    expect(request.request.params.has('yearFrom')).toBe(true);
    expect(request.request.params.has('yearTo')).toBe(true);
  });

  it('hängt den countryId-Filter nur an, wenn er gesetzt ist', () => {
    // arrange
    // act
    service.getPaged(1, 20, undefined, undefined, undefined, undefined, undefined, 5).subscribe();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/records' && req.params.get('countryId') === '5',
    );
    request.flush(emptyResponse);

    // assert
    expect(request.request.params.has('countryId')).toBe(true);
  });

  it('hängt den format-Filter nur an, wenn er gesetzt ist', () => {
    // arrange
    // act
    service
      .getPaged(1, 20, undefined, undefined, undefined, undefined, undefined, undefined, 'CdAlbum')
      .subscribe();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/records' && req.params.get('format') === 'CdAlbum',
    );
    request.flush(emptyResponse);

    // assert
    expect(request.request.params.has('format')).toBe(true);
  });

  it('hängt sortBy und sortDirection nur an, wenn sie gesetzt sind', () => {
    // arrange
    // act
    service
      .getPaged(
        1,
        20,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        'releaseYear',
        'desc',
      )
      .subscribe();
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/records' &&
        req.params.get('sortBy') === 'releaseYear' &&
        req.params.get('sortDirection') === 'desc',
    );
    request.flush(emptyResponse);

    // assert
    expect(request.request.params.has('sortBy')).toBe(true);
    expect(request.request.params.has('sortDirection')).toBe(true);
  });

  it('propagiert HTTP-Fehler an den Aufrufer', () => {
    // arrange
    let error: HttpErrorResponse | undefined;

    // act
    service.getPaged(1, 20).subscribe({ error: (err: HttpErrorResponse) => (error = err) });
    const request = httpTesting.expectOne((req) => req.url === 'https://api.test/api/records');
    request.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );

    // assert
    expect(error?.status).toBe(500);
  });
});
