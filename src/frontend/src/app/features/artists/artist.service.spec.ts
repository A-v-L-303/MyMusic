import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Artist, ArtistListResponse } from './artist';
import { ArtistService } from './artist.service';

describe('ArtistService', () => {
  let service: ArtistService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(ArtistService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('ruft getPaged mit page und pageSize als Query-Parameter auf', () => {
    // arrange
    const response: ArtistListResponse = {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20,
      totalPages: 0,
    };
    let result: ArtistListResponse | undefined;

    // act
    service.getPaged(1, 20).subscribe((value) => (result = value));
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/artists' &&
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
    service.getPaged(1, 20, 'Miles Davis').subscribe();
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/artists' && req.params.get('name') === 'Miles Davis',
    );
    request.flush({ items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });

    // assert
    expect(request.request.params.has('name')).toBe(true);
  });

  it('ruft getAll ohne Query-Parameter gegen die all-Route auf', () => {
    // arrange
    const artists: Artist[] = [
      { id: 1, name: 'AC/DC' },
      { id: 2, name: 'Nirvana' },
    ];
    let result: Artist[] | undefined;

    // act
    service.getAll().subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/artists/all');
    request.flush(artists);

    // assert
    expect(request.request.method).toBe('GET');
    expect(result).toEqual(artists);
  });

  it('sendet create als POST mit dem Namen im Body', () => {
    // arrange
    const created: Artist = { id: 1, name: 'AC/DC' };
    let result: Artist | undefined;

    // act
    service.create({ name: 'AC/DC' }).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/artists');
    request.flush(created);

    // assert
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ name: 'AC/DC' });
    expect(result).toEqual(created);
  });

  it('sendet update als PUT gegen die Id-Route', () => {
    // arrange
    const updated: Artist = { id: 1, name: "Guns N' Roses" };
    let result: Artist | undefined;

    // act
    service.update(1, { name: "Guns N' Roses" }).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/artists/1');
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
    const request = httpTesting.expectOne('https://api.test/api/artists/1');
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
    const request = httpTesting.expectOne((req) => req.url === 'https://api.test/api/artists');
    request.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );

    // assert
    expect(error?.status).toBe(500);
  });
});
