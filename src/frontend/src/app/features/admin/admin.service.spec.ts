import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { AdminUserListResponse } from './admin-user';
import { AdminService } from './admin.service';

describe('AdminService', () => {
  let service: AdminService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(AdminService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('ruft getPaged mit page und pageSize als Query-Parameter auf', () => {
    // arrange
    const response: AdminUserListResponse = {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20,
      totalPages: 0,
    };
    let result: AdminUserListResponse | undefined;

    // act
    service.getPaged(1, 20).subscribe((value) => (result = value));
    const request = httpTesting.expectOne(
      (req) =>
        req.url === 'https://api.test/api/admin/users' &&
        req.params.get('page') === '1' &&
        req.params.get('pageSize') === '20',
    );
    request.flush(response);

    // assert
    expect(result).toEqual(response);
  });

  it('sendet deleteUser als DELETE gegen die Id-Route', () => {
    // arrange
    let completed = false;

    // act
    service.deleteUser('user-1').subscribe({ complete: () => (completed = true) });
    const request = httpTesting.expectOne('https://api.test/api/admin/users/user-1');
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
    const request = httpTesting.expectOne((req) => req.url === 'https://api.test/api/admin/users');
    request.flush(
      { title: 'Serverfehler', status: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );

    // assert
    expect(error?.status).toBe(500);
  });
});
