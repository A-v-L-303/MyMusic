import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { of } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { Admin } from './admin';
import { AdminUser, AdminUserListResponse } from './admin-user';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

describe('Admin', () => {
  let httpTesting: HttpTestingController;
  let errorModalService: ErrorModalService;

  const self: AdminUser = { id: 'own-id', username: 'admin', email: 'admin@example.com', role: 'Admin' };
  const other: AdminUser = { id: 'other-id', username: 'erika', email: 'erika@example.com', role: 'User' };
  const onePage: AdminUserListResponse = {
    items: [self, other],
    totalCount: 2,
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
        {
          provide: OidcSecurityService,
          useValue: {
            authenticated: signal({ isAuthenticated: true }),
            getPayloadFromAccessToken: vi.fn().mockReturnValue(of({ sub: 'own-id' })),
          },
        },
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
      (req) => req.method === 'GET' && req.url === 'https://api.test/api/admin/users',
    );
  }

  function expectRequestWithParams(params: Record<string, string>) {
    return httpTesting.expectOne(
      (req) =>
        req.method === 'GET' &&
        req.url === 'https://api.test/api/admin/users' &&
        Object.entries(params).every(([key, value]) => req.params.get(key) === value),
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

  async function createLoadedFixture(response: AdminUserListResponse = onePage) {
    const fixture = TestBed.createComponent(Admin);
    fixture.detectChanges();
    expectListRequest().flush(response);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  it('lädt beim Start die erste Seite und zeigt Benutzername, E-Mail und Rolle an', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    expect(compiled(fixture).textContent).toContain('erika');
    expect(compiled(fixture).textContent).toContain('erika@example.com');
    expect(compiled(fixture).textContent).toContain('User');
  });

  it('zeigt bei der eigenen Zeile kein Löschen-Icon', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    expect(
      compiled(fixture).querySelector('[aria-label="Benutzer admin löschen"]'),
    ).toBeNull();
    expect(
      compiled(fixture).querySelector('[aria-label="Benutzer erika löschen"]'),
    ).not.toBeNull();
  });

  it('löscht einen fremden Benutzer nach Bestätigung und lädt die Liste danach neu', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector('[aria-label="Benutzer erika löschen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/admin/users/other-id',
    );
    deleteRequest.flush(null);
    await wait(0);
    fixture.detectChanges();
    expectListRequest().flush({ ...onePage, items: [self], totalCount: 1 });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).textContent).not.toContain('erika');
  });

  it('bricht das Löschen ohne HTTP-Aufruf ab', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    (
      compiled(fixture).querySelector('[aria-label="Benutzer erika löschen"]') as HTMLButtonElement
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
      compiled(fixture).querySelector('[aria-label="Benutzer erika löschen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/admin/users/other-id',
    );
    deleteRequest.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'server' }));
  });

  it('zeigt einen Fehler beim Laden über den ErrorModalService mit Retry-Callback an', async () => {
    // arrange
    const fixture = TestBed.createComponent(Admin);

    // act
    fixture.detectChanges();
    expectListRequest().flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'server' }));
  });

  it('lädt bei Seitenwechsel die gewählte Seite', async () => {
    // arrange
    const twoPages: AdminUserListResponse = {
      items: [self, other],
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
      (req) => req.url === 'https://api.test/api/admin/users' && req.params.get('page') === '2',
    );

    // assert
    expect(request.request.params.get('page')).toBe('2');
    request.flush({ ...twoPages, page: 2 });
    await fixture.whenStable();
  });

  it('zeigt Autocomplete-Vorschläge nach Sucheingabe an', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const searchInput = compiled(fixture).querySelector(
      '[aria-label="Benutzer suchen"]',
    ) as HTMLInputElement;

    // act
    searchInput.value = 'eri';
    searchInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    fixture.detectChanges();
    const suggestionRequest = expectRequestWithParams({ pageSize: '10', search: 'eri' });
    suggestionRequest.flush({ items: [other], totalCount: 1, page: 1, pageSize: 10, totalPages: 1 });
    const listRequest = expectRequestWithParams({ pageSize: '20', search: 'eri' });
    listRequest.flush({ items: [other], totalCount: 1, page: 1, pageSize: 20, totalPages: 1 });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).textContent).toContain('erika (erika@example.com)');
  });

  it('filtert nach Auswahl eines Autocomplete-Vorschlags auf genau einen Benutzer', async () => {
    // arrange
    const fixture = await createLoadedFixture();
    const searchInput = compiled(fixture).querySelector(
      '[aria-label="Benutzer suchen"]',
    ) as HTMLInputElement;
    searchInput.value = 'eri';
    searchInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    fixture.detectChanges();
    expectRequestWithParams({ pageSize: '10', search: 'eri' }).flush({
      items: [other],
      totalCount: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    });
    expectRequestWithParams({ pageSize: '20', search: 'eri' }).flush({
      items: [other],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });
    await fixture.whenStable();
    fixture.detectChanges();

    // act
    const option = compiled(fixture).querySelector('li[role="option"]') as HTMLLIElement;
    option.dispatchEvent(new MouseEvent('mousedown', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
    const selectedRequest = expectRequestWithParams({ pageSize: '20', search: 'other-id' });
    selectedRequest.flush({ items: [other], totalCount: 1, page: 1, pageSize: 20, totalPages: 1 });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert: die Auswahl filtert per Benutzer-ID, nicht per Freitext
    expect(selectedRequest.request.params.get('search')).toBe('other-id');
  });
});
