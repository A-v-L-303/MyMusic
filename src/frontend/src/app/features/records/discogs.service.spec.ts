import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { DiscogsRelease, DiscogsSearchResult } from './discogs';
import { DiscogsService } from './discogs.service';

describe('DiscogsService', () => {
  let service: DiscogsService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(DiscogsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('sendet search als GET mit dem Suchbegriff als Query-Parameter', () => {
    // arrange
    const results: DiscogsSearchResult[] = [
      { id: 1, title: 'Nevermind', year: 1991, label: 'DGC', thumbnailUrl: null },
    ];
    let result: DiscogsSearchResult[] | undefined;

    // act
    service.search('Nevermind').subscribe((value) => (result = value));
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/discogs/search' && req.params.get('q') === 'Nevermind',
    );
    request.flush(results);

    // assert
    expect(request.request.method).toBe('GET');
    expect(result).toEqual(results);
  });

  it('sendet getRelease als GET gegen die Release-Id-Route', () => {
    // arrange
    const release: DiscogsRelease = {
      id: 1,
      title: 'Nevermind',
      year: 1991,
      artists: ['Nirvana'],
      labels: ['DGC'],
      genres: ['Rock'],
      styles: ['Grunge'],
      formats: [],
      coverImageUrl: null,
      tracklist: [],
      country: null,
    };
    let result: DiscogsRelease | undefined;

    // act
    service.getRelease(1).subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/discogs/releases/1');
    request.flush(release);

    // assert
    expect(request.request.method).toBe('GET');
    expect(result).toEqual(release);
  });

  it('propagiert HTTP-Fehler an den Aufrufer', () => {
    // arrange
    let error: HttpErrorResponse | undefined;

    // act
    service.search('ab').subscribe({ error: (err: HttpErrorResponse) => (error = err) });
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/discogs/search',
    );
    request.flush(
      { title: 'Discogs nicht erreichbar', status: 502 },
      { status: 502, statusText: 'Bad Gateway' },
    );

    // assert
    expect(error?.status).toBe(502);
  });
});
