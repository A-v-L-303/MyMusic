import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Genre, GenreListResponse } from './genre';
import { GenreService } from './genre.service';

describe('GenreService', () => {
  let service: GenreService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(GenreService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('ruft getPaged mit page und pageSize als Query-Parameter auf', () => {
    // arrange
    const response: GenreListResponse = {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20,
      totalPages: 0,
    };
    let result: GenreListResponse | undefined;

    // act
    service.getPaged(1, 20).subscribe((value) => (result = value));
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/genres' &&
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
    service.getPaged(1, 20, 'Rock').subscribe();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/genres' && req.params.get('name') === 'Rock',
    );
    request.flush({ items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });

    // assert
    expect(request.request.params.has('name')).toBe(true);
  });

  it('ruft getAll ohne Query-Parameter gegen die all-Route auf', () => {
    // arrange
    const genres: Genre[] = [
      { id: 1, name: 'Jazz' },
      { id: 2, name: 'Rock' },
    ];
    let result: Genre[] | undefined;

    // act
    service.getAll().subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/genres/all');
    request.flush(genres);

    // assert
    expect(request.request.method).toBe('GET');
    expect(result).toEqual(genres);
  });

  it('sendet create als POST mit dem Namen im Body', () => {
    // arrange
    const created: Genre = { id: 1, name: 'Rock' };
    let result: Genre | undefined;

    // act
    service.create({ name: 'Rock' }).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/genres');
    request.flush(created);

    // assert
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ name: 'Rock' });
    expect(result).toEqual(created);
  });

  it('sendet update als PUT gegen die Id-Route', () => {
    // arrange
    const updated: Genre = { id: 1, name: 'Rock/Pop' };
    let result: Genre | undefined;

    // act
    service.update(1, { name: 'Rock/Pop' }).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/genres/1');
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
    const request = httpTesting.expectOne('https://api.test/api/genres/1');
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
    const request = httpTesting.expectOne((req) => req.url === 'https://api.test/api/genres');
    request.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );

    // assert
    expect(error?.status).toBe(500);
  });
});
