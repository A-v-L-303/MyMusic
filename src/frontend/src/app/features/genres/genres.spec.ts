import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { Genre, GenreListResponse } from './genre';
import { Genres } from './genres';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

describe('Genres', () => {
  let httpTesting: HttpTestingController;
  let errorModalService: ErrorModalService;

  const rock: Genre = { id: 1, name: 'Rock' };
  const emptyPage: GenreListResponse = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 1,
  };
  const onePage: GenreListResponse = {
    items: [rock],
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
      (req) => req.method === 'GET' && req.url === 'https://api.test/api/genres',
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

  async function createLoadedFixture(response: GenreListResponse = onePage) {
    const fixture = TestBed.createComponent(Genres);
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
    expect(compiled(fixture).textContent).toContain('Rock');
    expect(compiled(fixture).querySelector('.spinner')).toBeNull();
  });

  it('filtert live nach Namenseingabe und setzt die Seite dabei zurück', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const input = compiled(fixture).querySelector('input[type="text"]') as HTMLInputElement;

    // act
    input.value = 'Ro';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    fixture.detectChanges();
    const request = httpTesting.expectOne(
      (req) => req.url === 'https://api.test/api/genres' && req.params.get('name') === 'Ro',
    );

    // assert
    expect(request.request.params.get('page')).toBe('1');
    request.flush(onePage);
    await fixture.whenStable();
  });

  it('lädt bei Seitenwechsel die gewählte Seite', async () => {
    // arrange
    const twoPages: GenreListResponse = {
      items: [rock],
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
      (req) => req.url === 'https://api.test/api/genres' && req.params.get('page') === '2',
    );

    // assert
    expect(request.request.params.get('page')).toBe('2');
    request.flush({ ...twoPages, page: 2 });
    await fixture.whenStable();
  });

  it('legt ein neues Genre an und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture(emptyPage);
    findButton(compiled(fixture), 'Anlegen').click();
    fixture.detectChanges();
    const nameInput = compiled(fixture).querySelector('#genre-name') as HTMLInputElement;
    nameInput.value = 'Jazz';
    nameInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;

    // act
    form.dispatchEvent(new Event('submit', { cancelable: true }));
    const createRequest = httpTesting.expectOne(
      (req) => req.method === 'POST' && req.url === 'https://api.test/api/genres',
    );
    createRequest.flush({ id: 2, name: 'Jazz' });
    // Ein Tick für die Promise-Kette (firstValueFrom → saved.emit → onFormSaved → reload()),
    // bevor der dadurch ausgelöste Neuladen-Request erwartet werden kann. whenStable() darf hier
    // noch nicht aufgerufen werden, da es sonst auf genau diesen noch nicht geflushten Request wartet.
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush({
      items: [{ id: 2, name: 'Jazz' }],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(createRequest.request.body).toEqual({ name: 'Jazz' });
    expect(compiled(fixture).querySelector('#genre-name')).toBeNull();
    expect(compiled(fixture).textContent).toContain('Jazz');
  });

  it('bearbeitet ein bestehendes Genre', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector('[aria-label="Genre Rock bearbeiten"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();
    const nameInput = compiled(fixture).querySelector('#genre-name') as HTMLInputElement;
    nameInput.value = 'Rock Pop';
    nameInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;

    // act
    form.dispatchEvent(new Event('submit', { cancelable: true }));
    const updateRequest = httpTesting.expectOne(
      (req) => req.method === 'PUT' && req.url === 'https://api.test/api/genres/1',
    );
    updateRequest.flush({ id: 1, name: 'Rock Pop' });
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush({ ...onePage, items: [{ id: 1, name: 'Rock Pop' }] });
    await fixture.whenStable();

    // assert
    expect(updateRequest.request.body).toEqual({ name: 'Rock Pop' });
  });

  it('löscht ein Genre nach Bestätigung und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector('[aria-label="Genre Rock löschen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/genres/1',
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
      compiled(fixture).querySelector('[aria-label="Genre Rock löschen"]') as HTMLButtonElement
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
      compiled(fixture).querySelector('[aria-label="Genre Rock löschen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/genres/1',
    );
    deleteRequest.flush(
      { title: 'Konflikt', detail: 'Genre wird noch von einem Track referenziert.', status: 409 },
      { status: 409, statusText: 'Conflict' },
    );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(
      expect.objectContaining({
        kind: 'conflict',
        message: 'Genre wird noch von einem Track referenziert.',
      }),
    );
  });

  it('zeigt einen 404 beim Laden über den ErrorModalService mit Retry-Callback an', async () => {
    // arrange
    const fixture = TestBed.createComponent(Genres);

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
