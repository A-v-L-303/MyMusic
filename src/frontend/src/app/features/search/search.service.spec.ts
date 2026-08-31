import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { RecordListResponse } from '../records/record';
import { SearchService } from './search.service';

describe('SearchService', () => {
  let service: SearchService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(SearchService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('ruft getPaged mit page, pageSize und q als Query-Parameter auf', () => {
    // arrange
    let result: RecordListResponse | undefined;

    // act
    service.getPaged(1, 20, 'abbey').subscribe((value) => (result = value));
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/search' &&
        req.params.get('page') === '1' &&
        req.params.get('pageSize') === '20' &&
        req.params.get('q') === 'abbey',
    );
    request.flush({ items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });

    // assert
    expect(result?.items).toEqual([]);
  });

  it('ergänzt jedes Ergebnis um ein leeres tracks-Array, da die Suchantwort keine Tracks liefert', () => {
    // arrange
    let result: RecordListResponse | undefined;

    // act
    service.getPaged(1, 20, 'abbey').subscribe((value) => (result = value));
    const request = httpTesting.expectOne((req) => req.url === 'https://api.test/api/search');
    request.flush({
      items: [
        {
          id: 1,
          collectionNumber: 1,
          labelId: 1,
          labelName: 'Apple Records',
          artistId: 2,
          artistName: 'The Beatles',
          format: 'Album',
          albumName: 'Abbey Road',
          releaseYear: 1969,
          condition: 'Nm',
          information: null,
          albumCoverDataUrl: null,
        },
      ],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });

    // assert
    expect(result?.items[0].tracks).toEqual([]);
    expect(result?.items[0].albumName).toBe('Abbey Road');
  });
});
