import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { Country } from './country';
import { Label, LabelListResponse } from './label';
import { Labels } from './labels';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

describe('Labels', () => {
  let httpTesting: HttpTestingController;
  let errorModalService: ErrorModalService;

  const countries: Country[] = [
    { id: 1, name: 'Deutschland', code: 'DE' },
    { id: 2, name: 'Vereinigtes Königreich', code: 'GB' },
  ];

  const roughTrade: Label = {
    id: 1,
    name: 'Rough Trade',
    countryId: 2,
    countryName: 'Vereinigtes Königreich',
    information: null,
  };
  const emptyPage: LabelListResponse = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 1,
  };
  const onePage: LabelListResponse = {
    items: [roughTrade],
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
      (req) => req.method === 'GET' && req.url === 'https://api.test/api/labels',
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

  function findButton(root: HTMLElement, text: string): HTMLButtonElement {
    return Array.from(root.querySelectorAll('button')).find((button) =>
      button.textContent?.trim().includes(text),
    ) as HTMLButtonElement;
  }

  async function createLoadedFixture(response: LabelListResponse = onePage) {
    const fixture = TestBed.createComponent(Labels);
    fixture.detectChanges();
    expectListRequest().flush(response);
    expectCountriesRequest().flush(countries);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  it('lädt beim Start die erste Seite und die Länderliste und zeigt sie an', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    expect(compiled(fixture).textContent).toContain('Rough Trade');
    expect(compiled(fixture).textContent).toContain('Vereinigtes Königreich');
    expect(compiled(fixture).querySelector('.spinner')).toBeNull();
  });

  it('filtert live nach Namenseingabe und setzt die Seite dabei zurück', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const input = compiled(fixture).querySelector('input[type="text"]') as HTMLInputElement;

    // act
    input.value = 'Rough';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    fixture.detectChanges();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/labels' && req.params.get('name') === 'Rough',
    );

    // assert
    expect(request.request.params.get('page')).toBe('1');
    request.flush(onePage);
    await fixture.whenStable();
  });

  it('filtert sofort nach Länderauswahl, ohne Debounce, und setzt die Seite dabei zurück', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const select = compiled(fixture).querySelector('select') as HTMLSelectElement;

    // act
    select.value = '2';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/labels' && req.params.get('countryId') === '2',
    );

    // assert
    expect(request.request.params.get('page')).toBe('1');
    request.flush(onePage);
    await fixture.whenStable();
  });

  it('lädt bei Seitenwechsel die gewählte Seite', async () => {
    // arrange
    const twoPages: LabelListResponse = {
      items: [roughTrade],
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
      (req) => req.url === 'https://api.test/api/labels' && req.params.get('page') === '2',
    );

    // assert
    expect(request.request.params.get('page')).toBe('2');
    request.flush({ ...twoPages, page: 2 });
    await fixture.whenStable();
  });

  it('legt ein neues Label an und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture(emptyPage);
    findButton(compiled(fixture), 'Anlegen').click();
    fixture.detectChanges();
    const nameInput = compiled(fixture).querySelector('#label-name') as HTMLInputElement;
    nameInput.value = 'Rough Trade';
    nameInput.dispatchEvent(new Event('input'));
    const countrySelect = compiled(fixture).querySelector('#label-country') as HTMLSelectElement;
    countrySelect.value = '2';
    countrySelect.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;

    // act
    form.dispatchEvent(new Event('submit', { cancelable: true }));
    const createRequest = httpTesting.expectOne(
      (req) => req.method === 'POST' && req.url === 'https://api.test/api/labels',
    );
    createRequest.flush(roughTrade);
    // Ein Tick für die Promise-Kette (firstValueFrom → saved.emit → onFormSaved → reload()),
    // bevor der dadurch ausgelöste Neuladen-Request erwartet werden kann.
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush(onePage);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(createRequest.request.body).toEqual({
      name: 'Rough Trade',
      countryId: 2,
      information: null,
    });
    expect(compiled(fixture).querySelector('#label-name')).toBeNull();
    expect(compiled(fixture).textContent).toContain('Rough Trade');
  });

  it('bearbeitet ein bestehendes Label', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Label Rough Trade bearbeiten"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();
    const nameInput = compiled(fixture).querySelector('#label-name') as HTMLInputElement;
    nameInput.value = 'Rough Trade Records';
    nameInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;

    // act
    form.dispatchEvent(new Event('submit', { cancelable: true }));
    const updateRequest = httpTesting.expectOne(
      (req) => req.method === 'PUT' && req.url === 'https://api.test/api/labels/1',
    );
    updateRequest.flush({ ...roughTrade, name: 'Rough Trade Records' });
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush({
      ...onePage,
      items: [{ ...roughTrade, name: 'Rough Trade Records' }],
    });
    await fixture.whenStable();

    // assert
    expect(updateRequest.request.body).toEqual({
      name: 'Rough Trade Records',
      countryId: 2,
      information: null,
    });
  });

  it('löscht ein Label nach Bestätigung und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Label Rough Trade löschen"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/labels/1',
    );
    deleteRequest.flush(null);
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush(emptyPage);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('.btn-danger')).toBeNull();
  });

  it('bricht das Löschen ohne HTTP-Aufruf ab', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Label Rough Trade löschen"]',
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

  it('zeigt beim Löschen einen 409-Konflikt über den ErrorModalService an', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Label Rough Trade löschen"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/labels/1',
    );
    const conflictDetail =
      "Label 'Rough Trade' kann nicht gelöscht werden, da es noch von mindestens einem Record verwendet wird.";
    deleteRequest.flush(
      { title: 'Konflikt', detail: conflictDetail, status: 409 },
      { status: 409, statusText: 'Conflict' },
    );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(
      expect.objectContaining({ kind: 'conflict', message: conflictDetail }),
    );
  });

  it('zeigt einen 404 beim Laden der Labels über den ErrorModalService mit Retry-Callback an', async () => {
    // arrange
    const fixture = TestBed.createComponent(Labels);

    // act
    fixture.detectChanges();
    expectListRequest().flush(
      { title: 'Nicht gefunden', status: 404 },
      { status: 404, statusText: 'Not Found' },
    );
    expectCountriesRequest().flush(countries);
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'not-found' }));
  });

  it('zeigt einen Fehler beim Laden der Länderliste über den ErrorModalService an', async () => {
    // arrange
    const fixture = TestBed.createComponent(Labels);

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
