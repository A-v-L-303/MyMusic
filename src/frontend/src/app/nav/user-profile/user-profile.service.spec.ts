import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { UserProfileService } from './user-profile.service';

describe('UserProfileService', () => {
  let service: UserProfileService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(UserProfileService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('sendet updateEmail als PUT mit der neuen E-Mail-Adresse im Body', () => {
    // arrange
    let completed = false;

    // act
    service.updateEmail('neu@example.com').subscribe({ complete: () => (completed = true) });
    const request = httpTesting.expectOne('https://api.test/api/me/email');
    request.flush(null);

    // assert
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ email: 'neu@example.com' });
    expect(completed).toBe(true);
  });

  it('sendet changePassword als PUT mit dem neuen Passwort im Body', () => {
    // arrange
    let completed = false;

    // act
    service.changePassword('einSicheresPasswort1').subscribe({ complete: () => (completed = true) });
    const request = httpTesting.expectOne('https://api.test/api/me/password');
    request.flush(null);

    // assert
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ newPassword: 'einSicheresPasswort1' });
    expect(completed).toBe(true);
  });

  it('propagiert HTTP-Fehler an den Aufrufer', () => {
    // arrange
    let error: HttpErrorResponse | undefined;

    // act
    service.updateEmail('vergeben@example.com').subscribe({ error: (err: HttpErrorResponse) => (error = err) });
    const request = httpTesting.expectOne('https://api.test/api/me/email');
    request.flush(
      { title: 'Konflikt', status: 409, detail: 'Diese E-Mail-Adresse wird bereits von einem anderen Konto verwendet.' },
      { status: 409, statusText: 'Conflict' },
    );

    // assert
    expect(error?.status).toBe(409);
  });
});
