import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { Artist } from '../artists/artist';
import { Country } from '../../shared/country/country';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Record, RecordListResponse } from './record';
import { Records } from './records';

@Component({ selector: 'app-dummy', template: '' })
class Dummy {}

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
        provideRouter([
          { path: 'records', component: Dummy },
          { path: 'records/:id', component: Dummy },
        ]),
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

  function flushRecordFormReferenceLists(): void {
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

  function compiled(fixture: { nativeElement: unknown }): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function findButton(root: HTMLElement, text: string): HTMLButtonElement {
    return Array.from(root.querySelectorAll('button')).find((button) =>
      button.textContent?.trim().includes(text),
    ) as HTMLButtonElement;
  }

  async function selectFormLabel(
    fixture: {
      nativeElement: unknown;
      detectChanges: () => void;
      whenStable: () => Promise<unknown>;
    },
    name: string,
    id: number,
  ): Promise<void> {
    const formHost = compiled(fixture).querySelector('app-record-form') as HTMLElement;
    const input = formHost.querySelectorAll('app-autocomplete input')[0] as HTMLInputElement;
    input.value = name;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/labels' && req.params.get('name') === name,
    );
    request.flush({
      items: [{ id, name, countryId: 1, countryName: 'USA', information: null }],
      totalCount: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    });
    await fixture.whenStable();
    fixture.detectChanges();
    const option = formHost.querySelector('li[role="option"]') as HTMLLIElement;
    option.dispatchEvent(new MouseEvent('mousedown', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
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

  it('hat Tooltips am Anzahl-Badge und am Anlegen-Button', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    expect(compiled(fixture).querySelector('.badge')?.getAttribute('title')).toBe(
      'Anzahl der gefundenen Records',
    );
    expect(findButton(compiled(fixture), 'Anlegen').title).toBe('Neuen Record anlegen');
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

  it('navigiert beim Klick auf eine Karte zur Detailansicht', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate');

    // act
    compiled(fixture)
      .querySelector('.record-card')
      ?.dispatchEvent(new Event('click', { bubbles: true }));
    fixture.detectChanges();

    // assert
    expect(navigateSpy).toHaveBeenCalledWith(['/records', 1]);
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

  it('legt einen neuen Record an und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture(emptyPage);
    findButton(compiled(fixture), 'Anlegen').click();
    fixture.detectChanges();
    expectCountriesRequest().flush(countries);
    flushRecordFormReferenceLists();
    await selectFormLabel(fixture, 'Apple Records', 1);
    const formatSelect = compiled(fixture).querySelector('#record-format') as HTMLSelectElement;
    formatSelect.value = 'Album';
    formatSelect.dispatchEvent(new Event('input'));
    const albumInput = compiled(fixture).querySelector('#record-album-name') as HTMLInputElement;
    albumInput.value = 'Abbey Road';
    albumInput.dispatchEvent(new Event('input'));
    const yearInput = compiled(fixture).querySelector('#record-release-year') as HTMLInputElement;
    yearInput.value = '1969';
    yearInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;

    // act
    form.dispatchEvent(new Event('submit', { cancelable: true }));
    const createRequest = httpTesting.expectOne(
      (req) => req.method === 'POST' && req.url === 'https://api.test/api/records',
    );
    createRequest.flush(abbeyRoad);
    // Ein Tick für die Promise-Kette (firstValueFrom → saved.emit → onFormSaved → reload()),
    // bevor der dadurch ausgelöste Neuladen-Request erwartet werden kann.
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush(onePage);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(createRequest.request.body).toEqual({
      labelId: 1,
      artistId: null,
      format: 'Album',
      albumName: 'Abbey Road',
      releaseYear: 1969,
      condition: 'Vg',
      information: null,
    });
    expect(compiled(fixture).querySelector('#record-album-name')).toBeNull();
    expect(compiled(fixture).textContent).toContain('Abbey Road');
  });

  it('bearbeitet einen bestehenden Record und übernimmt Label/Künstler vorbefüllt', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Record Abbey Road bearbeiten"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();
    expectCountriesRequest().flush(countries);
    flushRecordFormReferenceLists();
    const formHost = compiled(fixture).querySelector('app-record-form') as HTMLElement;
    const autocompleteInputs = formHost.querySelectorAll(
      'app-autocomplete input',
    ) as NodeListOf<HTMLInputElement>;
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
    expectListRequest().flush({
      ...onePage,
      items: [{ ...abbeyRoad, albumName: 'Abbey Road (Remastered)' }],
    });
    await fixture.whenStable();

    // assert: keine HTTP-Anfrage für Label/Künstler nötig, da über initialQuery vorbefüllt
    expect(autocompleteInputs[0].value).toBe('Apple Records');
    expect(autocompleteInputs[1].value).toBe('The Beatles');
    expect(updateRequest.request.body).toEqual({
      labelId: 1,
      artistId: 10,
      format: 'Album',
      albumName: 'Abbey Road (Remastered)',
      releaseYear: 1969,
      condition: 'Nm',
      information: null,
    });
  });

  it('löscht einen Record nach Bestätigung und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture();
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
    expectListRequest().flush(emptyPage);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('.btn-danger')).toBeNull();
    expect(compiled(fixture).querySelector('.empty')).not.toBeNull();
  });

  it('bricht das Löschen ohne HTTP-Aufruf ab', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Record Abbey Road löschen"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Abbrechen').click();
    fixture.detectChanges();

    // assert
    // httpTesting.verify() im afterEach deckt einen unerwarteten DELETE-Request auf.
    expect(compiled(fixture).querySelector('.btn-danger')).toBeNull();
  });

  it('zeigt einen Serverfehler beim Löschen über den ErrorModalService an', async () => {
    // arrange
    const fixture = await createLoadedFixture();
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
    deleteRequest.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'server' }));
  });
});
