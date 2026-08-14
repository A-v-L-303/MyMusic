import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Record, RecordListResponse } from './record';
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

  it('sendet create als POST mit allen Feldern im Body', () => {
    // arrange
    const created: Record = {
      id: 1,
      labelId: 1,
      labelName: 'Columbia',
      artistId: 2,
      artistName: 'Miles Davis',
      format: 'Album',
      albumName: 'Kind of Blue',
      releaseYear: 1959,
      condition: 'Vg',
      information: null,
      albumCoverDataUrl: null,
      tracks: [],
    };
    const request_ = {
      labelId: 1,
      artistId: 2,
      format: 'Album' as const,
      albumName: 'Kind of Blue',
      releaseYear: 1959,
      condition: 'Vg' as const,
      information: null,
    };
    let result: Record | undefined;

    // act
    service.create(request_).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/records');
    request.flush(created);

    // assert
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(request_);
    expect(result).toEqual(created);
  });

  it('sendet update als PUT gegen die Id-Route', () => {
    // arrange
    const updated: Record = {
      id: 1,
      labelId: 1,
      labelName: 'Columbia',
      artistId: 2,
      artistName: 'Miles Davis',
      format: 'Album',
      albumName: 'Kind of Blue (Deluxe)',
      releaseYear: 1959,
      condition: 'Vg',
      information: null,
      albumCoverDataUrl: null,
      tracks: [],
    };
    let result: Record | undefined;

    // act
    service
      .update(1, {
        labelId: 1,
        artistId: 2,
        format: 'Album',
        albumName: 'Kind of Blue (Deluxe)',
        releaseYear: 1959,
        condition: 'Vg',
        information: null,
      })
      .subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/records/1');
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
    const request = httpTesting.expectOne('https://api.test/api/records/1');
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
    const request = httpTesting.expectOne((req) => req.url === 'https://api.test/api/records');
    request.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );

    // assert
    expect(error?.status).toBe(500);
  });
});
