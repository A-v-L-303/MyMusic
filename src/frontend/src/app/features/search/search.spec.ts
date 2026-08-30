import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Record, RecordListResponse } from '../records/record';
import { Search } from './search';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

class StubIntersectionObserver implements IntersectionObserver {
  readonly root: Element | Document | null = null;
  readonly rootMargin = '';
  readonly scrollMargin = '';
  readonly thresholds: ReadonlyArray<number> = [];

  observe = vi.fn();
  unobserve = vi.fn();
  disconnect = vi.fn();
  takeRecords = vi.fn((): IntersectionObserverEntry[] => []);
}

describe('Search', () => {
  let httpTesting: HttpTestingController;
  let router: Router;

  const abbeyRoad: Record = {
    id: 1,
    labelId: 1,
    labelName: 'Apple Records',
    artistId: 10,
    artistName: 'The Beatles',
    format: 'Album',
    albumName: 'Abbey Road',
    releaseYear: 1969,
    condition: 'Nm',
    information: null,
    albumCoverDataUrl: null,
    tracks: [],
  };

  const emptyResult: RecordListResponse = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
  };

  const onePage: RecordListResponse = {
    items: [abbeyRoad],
    totalCount: 1,
    page: 1,
    pageSize: 20,
    totalPages: 1,
  };

  function configureTestBed(query: string | null): void {
    vi.stubGlobal('IntersectionObserver', StubIntersectionObserver);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
        {
          provide: ActivatedRoute,
          useValue: { queryParamMap: of(convertToParamMap(query ? { q: query } : {})) },
        },
      ],
    });

    httpTesting = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  }

  afterEach(() => {
    httpTesting.verify();
    vi.unstubAllGlobals();
  });

  function compiled(fixture: { nativeElement: unknown }): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function findButton(root: HTMLElement, text: string): HTMLButtonElement {
    return Array.from(root.querySelectorAll('button')).find((button) =>
      button.textContent?.trim().includes(text),
    ) as HTMLButtonElement;
  }

  function expectSearchRequest(query = 'abbey') {
    return httpTesting.expectOne(
      (req) =>
        req.method === 'GET' &&
        req.url === 'https://api.test/api/search' &&
        req.params.get('q') === query,
    );
  }

  function flushRecordFormReferenceLists(): void {
    httpTesting
      .expectOne((req) => req.method === 'GET' && req.url === 'https://api.test/api/countries')
      .flush([]);
    httpTesting
      .expectOne((req) => req.method === 'GET' && req.url === 'https://api.test/api/artists/all')
      .flush([]);
    httpTesting
      .expectOne((req) => req.method === 'GET' && req.url === 'https://api.test/api/labels/all')
      .flush([]);
    httpTesting
      .expectOne((req) => req.method === 'GET' && req.url === 'https://api.test/api/genres/all')
      .flush([]);
  }

  it('zeigt ohne Suchbegriff einen Hinweistext und ruft das Backend nicht auf', () => {
    // arrange
    configureTestBed(null);
    const fixture = TestBed.createComponent(Search);

    // act
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('.empty')?.textContent).toContain(
      'Bitte einen Suchbegriff eingeben',
    );
    // httpTesting.verify() im afterEach deckt einen unerwarteten GET /api/search auf.
  });

  it('lädt und zeigt Treffer als Cards bei vorhandenem Suchbegriff', async () => {
    // arrange
    configureTestBed('abbey');
    const fixture = TestBed.createComponent(Search);

    // act
    fixture.detectChanges();
    expectSearchRequest().flush(onePage);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).textContent).toContain('Abbey Road');
    expect(compiled(fixture).querySelector('.record-card')).not.toBeNull();
  });

  it('zeigt "Keine Daten vorhanden" ohne Treffer', async () => {
    // arrange
    configureTestBed('abbey');
    const fixture = TestBed.createComponent(Search);

    // act
    fixture.detectChanges();
    expectSearchRequest().flush(emptyResult);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('.empty')?.textContent).toContain(
      'Keine Daten vorhanden',
    );
  });

  it('navigiert beim Klick auf eine Karte zur Detailansicht (inkl. Tracks)', async () => {
    // arrange
    configureTestBed('abbey');
    const fixture = TestBed.createComponent(Search);
    fixture.detectChanges();
    expectSearchRequest().flush(onePage);
    await fixture.whenStable();
    fixture.detectChanges();
    const navigateSpy = vi.spyOn(router, 'navigate');

    // act
    compiled(fixture)
      .querySelector('.record-card')
      ?.dispatchEvent(new Event('click', { bubbles: true }));
    fixture.detectChanges();

    // assert
    expect(navigateSpy).toHaveBeenCalledWith(['/records', 1]);
  });

  it('lädt beim Klick auf "Mehr laden" die nächste Seite nach und hängt die Treffer an', async () => {
    // arrange
    const letItBe: Record = { ...abbeyRoad, id: 2, albumName: 'Let It Be' };
    configureTestBed('abbey');
    const fixture = TestBed.createComponent(Search);
    fixture.detectChanges();
    expectSearchRequest().flush({ ...onePage, totalCount: 21, totalPages: 2 });
    await fixture.whenStable();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Mehr laden').click();
    fixture.detectChanges();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/search' && req.params.get('page') === '2',
    );
    request.flush({ ...onePage, page: 2, items: [letItBe] });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(request.request.params.get('q')).toBe('abbey');
    expect(compiled(fixture).textContent).toContain('Abbey Road');
    expect(compiled(fixture).textContent).toContain('Let It Be');
  });

  it('zeigt keinen "Mehr laden"-Button, wenn nur eine Seite vorhanden ist', async () => {
    // arrange
    configureTestBed('abbey');
    const fixture = TestBed.createComponent(Search);

    // act
    fixture.detectChanges();
    expectSearchRequest().flush(onePage);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(findButton(compiled(fixture), 'Mehr laden')).toBeUndefined();
  });

  it('setzt die Ergebnisliste nach dem Löschen von Seite 2 aus auf Seite 1 zurück', async () => {
    // arrange
    const letItBe: Record = { ...abbeyRoad, id: 2, albumName: 'Let It Be' };
    configureTestBed('abbey');
    const fixture = TestBed.createComponent(Search);
    fixture.detectChanges();
    expectSearchRequest().flush({ ...onePage, totalCount: 21, totalPages: 2 });
    await fixture.whenStable();
    fixture.detectChanges();
    findButton(compiled(fixture), 'Mehr laden').click();
    fixture.detectChanges();
    httpTesting
      .expectOne(
        (req) => req.url === 'https://api.test/api/search' && req.params.get('page') === '2',
      )
      .flush({ ...onePage, page: 2, items: [letItBe] });
    await fixture.whenStable();
    fixture.detectChanges();
    (
      compiled(fixture).querySelector(
        '[aria-label="Record Abbey Road löschen"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/records/1',
    );
    deleteRequest.flush(null);
    await wait(0);
    fixture.detectChanges();
    const reloadRequest = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/search' && req.params.get('page') === '1',
    );
    reloadRequest.flush(onePage);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert: Seite-2-Treffer sind nach dem Reset verschwunden, nur noch Seite 1 sichtbar
    expect(compiled(fixture).textContent).toContain('Abbey Road');
    expect(compiled(fixture).textContent).not.toContain('Let It Be');
  });

  it('bearbeitet einen Treffer aus der Suchergebnisliste und lädt danach neu', async () => {
    // arrange
    configureTestBed('abbey');
    const fixture = TestBed.createComponent(Search);
    fixture.detectChanges();
    expectSearchRequest().flush(onePage);
    await fixture.whenStable();
    fixture.detectChanges();

    (
      compiled(fixture).querySelector(
        '[aria-label="Record Abbey Road bearbeiten"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();
    flushRecordFormReferenceLists();
    const albumInput = compiled(fixture).querySelector('#record-album-name') as HTMLInputElement;
    albumInput.value = 'Abbey Road (Remastered)';
    albumInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;

    // act
    form.dispatchEvent(new Event('submit', { cancelable: true }));
    const updateRequest = httpTesting.expectOne(
      (req) => req.method === 'PUT' && req.url === 'https://api.test/api/records/1',
    );
    updateRequest.flush({ ...abbeyRoad, albumName: 'Abbey Road (Remastered)' });
    await wait(0);
    fixture.detectChanges();
    expectSearchRequest().flush({
      ...onePage,
      items: [{ ...abbeyRoad, albumName: 'Abbey Road (Remastered)' }],
    });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('#record-album-name')).toBeNull();
    expect(compiled(fixture).textContent).toContain('Abbey Road (Remastered)');
  });

  it('löscht einen Treffer nach Bestätigung und lädt die Ergebnisliste danach neu', async () => {
    // arrange
    configureTestBed('abbey');
    const fixture = TestBed.createComponent(Search);
    fixture.detectChanges();
    expectSearchRequest().flush(onePage);
    await fixture.whenStable();
    fixture.detectChanges();

    (
      compiled(fixture).querySelector(
        '[aria-label="Record Abbey Road löschen"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    (
      Array.from(compiled(fixture).querySelectorAll('button')).find((button) =>
        button.textContent?.trim().includes('Löschen'),
      ) as HTMLButtonElement
    ).click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/records/1',
    );
    deleteRequest.flush(null);
    await wait(0);
    fixture.detectChanges();
    expectSearchRequest().flush(emptyResult);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('.empty')?.textContent).toContain(
      'Keine Daten vorhanden',
    );
  });
});
