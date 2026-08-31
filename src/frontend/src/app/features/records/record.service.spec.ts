import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Record, RecordListResponse, RecordTrack } from './record';
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

  it('ruft getById gegen die Id-Route auf', () => {
    // arrange
    const record: Record = {
      id: 1,
      collectionNumber: 1,
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
    let result: Record | undefined;

    // act
    service.getById(1).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/records/1');
    request.flush(record);

    // assert
    expect(request.request.method).toBe('GET');
    expect(result).toEqual(record);
  });

  it('propagiert einen 404-Fehler von getById an den Aufrufer', () => {
    // arrange
    let error: HttpErrorResponse | undefined;

    // act
    service.getById(999).subscribe({ error: (err: HttpErrorResponse) => (error = err) });
    const request = httpTesting.expectOne('https://api.test/api/records/999');
    request.flush(
      { title: 'Nicht gefunden', status: 404 },
      { status: 404, statusText: 'Not Found' },
    );

    // assert
    expect(error?.status).toBe(404);
  });

  it('sendet create als POST mit allen Feldern im Body', () => {
    // arrange
    const created: Record = {
      id: 1,
      collectionNumber: 1,
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
      collectionNumber: 1,
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

  it('sendet uploadCover als POST mit FormData gegen die Cover-Route', () => {
    // arrange
    const updated: Record = {
      id: 1,
      collectionNumber: 1,
      labelId: 1,
      labelName: 'Columbia',
      artistId: 2,
      artistName: 'Miles Davis',
      format: 'Album',
      albumName: 'Kind of Blue',
      releaseYear: 1959,
      condition: 'Vg',
      information: null,
      albumCoverDataUrl: 'data:image/jpeg;base64,abc',
      tracks: [],
    };
    const file = new File(['content'], 'cover.jpg', { type: 'image/jpeg' });
    let result: Record | undefined;

    // act
    service.uploadCover(1, file).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/records/1/cover');
    request.flush(updated);

    // assert
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeInstanceOf(FormData);
    expect((request.request.body as FormData).get('file')).toBe(file);
    expect(result).toEqual(updated);
  });

  it('propagiert einen 400-Fehler von uploadCover an den Aufrufer', () => {
    // arrange
    const file = new File(['content'], 'cover.txt', { type: 'text/plain' });
    let error: HttpErrorResponse | undefined;

    // act
    service.uploadCover(1, file).subscribe({ error: (err: HttpErrorResponse) => (error = err) });
    const request = httpTesting.expectOne('https://api.test/api/records/1/cover');
    request.flush(
      { title: 'Validierungsfehler', status: 400, errors: { FileContent: ['ungültig'] } },
      { status: 400, statusText: 'Bad Request' },
    );

    // assert
    expect(error?.status).toBe(400);
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

  const track: RecordTrack = {
    id: 1,
    recordId: 1,
    artistId: 2,
    artistName: 'Miles Davis',
    genreId: 3,
    genreName: 'Jazz',
    trackName: 'So What',
    recordSide: 'A',
    trackNumber: 1,
    information: null,
  };

  it('sendet createTrack als POST gegen die Tracks-Route', () => {
    // arrange
    const request_ = {
      artistId: 2,
      genreId: 3,
      trackName: 'So What',
      recordSide: 'A',
      trackNumber: 1,
      information: null,
    };
    let result: RecordTrack | undefined;

    // act
    service.createTrack(1, request_).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/records/1/tracks');
    request.flush(track);

    // assert
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(request_);
    expect(result).toEqual(track);
  });

  it('sendet updateTrack als PUT gegen die Track-Id-Route', () => {
    // arrange
    const request_ = {
      artistId: 2,
      genreId: 3,
      trackName: 'So What (Remaster)',
      recordSide: 'A',
      trackNumber: 1,
      information: null,
    };
    let result: RecordTrack | undefined;

    // act
    service.updateTrack(1, 1, request_).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/records/1/tracks/1');
    request.flush({ ...track, trackName: 'So What (Remaster)' });

    // assert
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(request_);
    expect(result?.trackName).toBe('So What (Remaster)');
  });

  it('sendet deleteTrack als DELETE gegen die Track-Id-Route', () => {
    // arrange
    let completed = false;

    // act
    service.deleteTrack(1, 1).subscribe({ complete: () => (completed = true) });
    const request = httpTesting.expectOne('https://api.test/api/records/1/tracks/1');
    request.flush(null);

    // assert
    expect(request.request.method).toBe('DELETE');
    expect(completed).toBe(true);
  });

  it('propagiert einen 409-Fehler von createTrack an den Aufrufer', () => {
    // arrange
    let error: HttpErrorResponse | undefined;

    // act
    service
      .createTrack(1, {
        artistId: 2,
        genreId: 3,
        trackName: 'So What',
        recordSide: 'A',
        trackNumber: 1,
        information: null,
      })
      .subscribe({ error: (err: HttpErrorResponse) => (error = err) });
    const request = httpTesting.expectOne('https://api.test/api/records/1/tracks');
    request.flush(
      { title: 'Konflikt', status: 409, detail: 'Track A1 ist bereits vergeben.' },
      { status: 409, statusText: 'Conflict' },
    );

    // assert
    expect(error?.status).toBe(409);
  });
});
