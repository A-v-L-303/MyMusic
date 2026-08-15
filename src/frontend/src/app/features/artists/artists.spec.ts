import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { Artist, ArtistListResponse } from './artist';
import { Artists } from './artists';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

describe('Artists', () => {
  let httpTesting: HttpTestingController;
  let errorModalService: ErrorModalService;

  const milesDavis: Artist = { id: 1, name: 'Miles Davis' };
  const emptyPage: ArtistListResponse = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 1,
  };
  const onePage: ArtistListResponse = {
    items: [milesDavis],
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
      (req) => req.method === 'GET' && req.url === 'https://api.test/api/artists',
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

  async function createLoadedFixture(response: ArtistListResponse = onePage) {
    const fixture = TestBed.createComponent(Artists);
    fixture.detectChanges();
    expectListRequest().flush(response);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  it('lädt beim Start die erste Seite und zeigt sie an', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    expect(compiled(fixture).textContent).toContain('Miles Davis');
    expect(compiled(fixture).querySelector('.spinner')).toBeNull();
  });

  it('hat Tooltips am Anzahl-Badge und am Anlegen-Button', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    expect(compiled(fixture).querySelector('.badge')?.getAttribute('title')).toBe(
      'Anzahl der gefundenen Artists',
    );
    expect(findButton(compiled(fixture), 'Anlegen').title).toBe('Neuen Artist anlegen');
  });

  it('filtert live nach Namenseingabe und setzt die Seite dabei zurück', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const input = compiled(fixture).querySelector('input[type="text"]') as HTMLInputElement;

    // act
    input.value = 'Mi';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    fixture.detectChanges();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/artists' && req.params.get('name') === 'Mi',
    );

    // assert
    expect(request.request.params.get('page')).toBe('1');
    request.flush(onePage);
    await fixture.whenStable();
  });

  it('lädt bei Seitenwechsel die gewählte Seite', async () => {
    // arrange
    const twoPages: ArtistListResponse = {
      items: [milesDavis],
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
      (req) => req.url === 'https://api.test/api/artists' && req.params.get('page') === '2',
    );

    // assert
    expect(request.request.params.get('page')).toBe('2');
    request.flush({ ...twoPages, page: 2 });
    await fixture.whenStable();
  });

  it('legt einen neuen Artist an und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture(emptyPage);
    findButton(compiled(fixture), 'Anlegen').click();
    fixture.detectChanges();
    const nameInput = compiled(fixture).querySelector('#artist-name') as HTMLInputElement;
    nameInput.value = 'John Coltrane';
    nameInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;

    // act
    form.dispatchEvent(new Event('submit', { cancelable: true }));
    const createRequest = httpTesting.expectOne(
      (req) => req.method === 'POST' && req.url === 'https://api.test/api/artists',
    );
    createRequest.flush({ id: 2, name: 'John Coltrane' });
    // Ein Tick für die Promise-Kette (firstValueFrom → saved.emit → onFormSaved → reload()),
    // bevor der dadurch ausgelöste Neuladen-Request erwartet werden kann. whenStable() darf hier
    // noch nicht aufgerufen werden, da es sonst auf genau diesen noch nicht geflushten Request wartet.
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush({
      items: [{ id: 2, name: 'John Coltrane' }],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(createRequest.request.body).toEqual({ name: 'John Coltrane' });
    expect(compiled(fixture).querySelector('#artist-name')).toBeNull();
    expect(compiled(fixture).textContent).toContain('John Coltrane');
  });

  it('bearbeitet einen bestehenden Artist', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Artist Miles Davis bearbeiten"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();
    const nameInput = compiled(fixture).querySelector('#artist-name') as HTMLInputElement;
    nameInput.value = 'Miles Davis Quintet';
    nameInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;

    // act
    form.dispatchEvent(new Event('submit', { cancelable: true }));
    const updateRequest = httpTesting.expectOne(
      (req) => req.method === 'PUT' && req.url === 'https://api.test/api/artists/1',
    );
    updateRequest.flush({ id: 1, name: 'Miles Davis Quintet' });
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush({ ...onePage, items: [{ id: 1, name: 'Miles Davis Quintet' }] });
    await fixture.whenStable();

    // assert
    expect(updateRequest.request.body).toEqual({ name: 'Miles Davis Quintet' });
  });

  it('löscht einen Artist nach Bestätigung und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Artist Miles Davis löschen"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/artists/1',
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
        '[aria-label="Artist Miles Davis löschen"]',
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

  it('zeigt beim Löschen einen 409-Konflikt (Record- oder Track-Referenz) über den ErrorModalService an', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector(
        '[aria-label="Artist Miles Davis löschen"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/artists/1',
    );
    deleteRequest.flush(
      {
        title: 'Konflikt',
        detail:
          "Artist 'Miles Davis' kann nicht gelöscht werden, da er noch von mindestens einem Record verwendet wird.",
        status: 409,
      },
      { status: 409, statusText: 'Conflict' },
    );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(
      expect.objectContaining({
        kind: 'conflict',
        message:
          "Artist 'Miles Davis' kann nicht gelöscht werden, da er noch von mindestens einem Record verwendet wird.",
      }),
    );
  });

  it('zeigt einen 404 beim Laden über den ErrorModalService mit Retry-Callback an', async () => {
    // arrange
    const fixture = TestBed.createComponent(Artists);

    // act
    fixture.detectChanges();
    expectListRequest().flush(
      { title: 'Nicht gefunden', status: 404 },
      { status: 404, statusText: 'Not Found' },
    );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'not-found' }));
  });
});
