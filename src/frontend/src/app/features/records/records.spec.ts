import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { Artist } from '../artists/artist';
import { Country } from '../../shared/country/country';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Record, RecordListResponse } from './record';
import { Records } from './records';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

describe('Records', () => {
  let httpTesting: HttpTestingController;
  let errorModalService: ErrorModalService;

  const countries: Country[] = [{ id: 1, name: 'Vereinigtes Königreich', code: 'GB' }];

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

  const emptyPage: RecordListResponse = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 1,
  };
  const onePage: RecordListResponse = {
    items: [abbeyRoad],
    totalCount: 1,
    page: 1,
    pageSize: 20,
    totalPages: 1,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    httpTesting = TestBed.inject(HttpTestingController);
    errorModalService = TestBed.inject(ErrorModalService);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function expectListRequest() {
    return httpTesting.expectOne(
      (req) => req.method === 'GET' && req.url === 'https://api.test/api/records',
    );
  }

  function expectCountriesRequest() {
    return httpTesting.expectOne(
      (req) => req.method === 'GET' && req.url === 'https://api.test/api/countries',
    );
  }

  function compiled(fixture: { nativeElement: unknown }): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  async function createLoadedFixture(response: RecordListResponse = onePage) {
    const fixture = TestBed.createComponent(Records);
    fixture.detectChanges();
    expectListRequest().flush(response);
    expectCountriesRequest().flush(countries);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  it('lädt beim Start die erste Seite und die Länderliste und zeigt die Records an, ohne Künstler/Label vorab zu laden', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    expect(compiled(fixture).textContent).toContain('Abbey Road');
    expect(compiled(fixture).textContent).toContain('The Beatles');
    expect(compiled(fixture).querySelector('.spinner')).toBeNull();
    // httpTesting.verify() im afterEach deckt einen unerwarteten Aufruf von
    // /api/artists/all oder /api/labels/all beim Start auf.
  });

  it('zeigt "Keine Daten vorhanden" ohne Treffer', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture(emptyPage);

    // assert
    expect(compiled(fixture).querySelector('.empty')?.textContent).toContain(
      'Keine Daten vorhanden',
    );
  });

  it('filtert live nach Namenseingabe und setzt die Seite dabei zurück', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const input = compiled(fixture).querySelector('input[type="text"]') as HTMLInputElement;

    // act
    input.value = 'Abbey';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    fixture.detectChanges();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/records' && req.params.get('name') === 'Abbey',
    );

    // assert
    expect(request.request.params.get('page')).toBe('1');
    request.flush(onePage);
    await fixture.whenStable();
  });

  it('fragt Künstler-Vorschläge erst ab, sobald in den Autosuggest getippt wird, und übernimmt die Auswahl in den Filter', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const artistInput = compiled(fixture).querySelectorAll(
      'app-autocomplete input',
    )[0] as HTMLInputElement;

    // act
    artistInput.value = 'Beatl';
    artistInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    fixture.detectChanges();
    const suggestionRequest = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/artists' &&
        req.params.get('name') === 'Beatl' &&
        req.params.get('pageSize') === '10',
    );
    const artists: Artist[] = [{ id: 10, name: 'The Beatles' }];
    suggestionRequest.flush({
      items: artists,
      totalCount: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    });
    await fixture.whenStable();
    fixture.detectChanges();

    // act: Vorschlag auswählen
    const option = compiled(fixture).querySelector('li[role="option"]') as HTMLLIElement;
    option.dispatchEvent(new MouseEvent('mousedown', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
    const listRequest = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/records' && req.params.get('artistId') === '10',
    );

    // assert
    expect(listRequest.request.params.get('page')).toBe('1');
    listRequest.flush(onePage);
    await fixture.whenStable();
  });

  it('filtert sofort nach Formatauswahl, ohne Debounce, und setzt die Seite dabei zurück', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const select = compiled(fixture).querySelector(
      'select[aria-label="Nach Format filtern"]',
    ) as HTMLSelectElement;

    // act
    select.value = 'CdAlbum';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/records' && req.params.get('format') === 'CdAlbum',
    );

    // assert
    expect(request.request.params.get('page')).toBe('1');
    request.flush(onePage);
    await fixture.whenStable();
  });

  it('lädt bei Seitenwechsel die gewählte Seite', async () => {
    // arrange
    const twoPages: RecordListResponse = {
      items: [abbeyRoad],
      totalCount: 21,
      page: 1,
      pageSize: 20,
      totalPages: 2,
    };
    const fixture = await createLoadedFixture(twoPages);
    const pageButtons = compiled(fixture).querySelectorAll('button.btn-sm:not(.btn-icon)');

    // act
    (pageButtons[1] as HTMLButtonElement).click();
    fixture.detectChanges();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/records' && req.params.get('page') === '2',
    );

    // assert
    expect(request.request.params.get('page')).toBe('2');
    request.flush({ ...twoPages, page: 2 });
    await fixture.whenStable();
  });

  it('öffnet keine Detailansicht bei Klick auf eine Karte (noch nicht Teil dieses Blocks)', async () => {
    // arrange
    const fixture = await createLoadedFixture();

    // act
    compiled(fixture)
      .querySelector('.record-card')
      ?.dispatchEvent(new Event('click', { bubbles: true }));
    fixture.detectChanges();

    // assert
    // httpTesting.verify() im afterEach deckt einen unerwarteten Folge-Request auf.
    expect(compiled(fixture).textContent).toContain('Abbey Road');
  });

  it('zeigt einen Fehler beim Laden der Records über den ErrorModalService mit Retry-Callback an', async () => {
    // arrange
    const fixture = TestBed.createComponent(Records);

    // act
    fixture.detectChanges();
    expectListRequest().flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );
    expectCountriesRequest().flush(countries);
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'server' }));
  });

  it('zeigt einen Fehler beim Laden der Länderliste über den ErrorModalService an', async () => {
    // arrange
    const fixture = TestBed.createComponent(Records);

    // act
    fixture.detectChanges();
    expectListRequest().flush(onePage);
    expectCountriesRequest().flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'server' }));
  });
});
