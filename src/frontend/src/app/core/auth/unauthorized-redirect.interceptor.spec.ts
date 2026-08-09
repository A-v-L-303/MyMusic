import { HttpErrorResponse, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { firstValueFrom, throwError } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { RuntimeConfigService } from '../runtime-config/runtime-config.service';
import { unauthorizedRedirectInterceptor } from './unauthorized-redirect.interceptor';

describe('unauthorizedRedirectInterceptor', () => {
  const apiBaseUrl = 'https://localhost:5001';

  let authorizeMock: ReturnType<typeof vi.fn>;
  let consoleErrorSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    authorizeMock = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        { provide: RuntimeConfigService, useValue: { apiBaseUrl } },
        { provide: OidcSecurityService, useValue: { authorize: authorizeMock } },
      ],
    });

    consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    consoleErrorSpy.mockRestore();
  });

  function runInterceptor(url: string, status: number): Promise<unknown> {
    const request = new HttpRequest('GET', url);
    const error = new HttpErrorResponse({ status, url });
    const next: HttpHandlerFn = () => throwError(() => error);

    return TestBed.runInInjectionContext(() =>
      firstValueFrom(unauthorizedRedirectInterceptor(request, next)).catch(
        (caught: unknown) => caught,
      ),
    );
  }

  it('leitet bei HTTP 401 von der API zur Anmeldung um und loggt im Dev-Modus', async () => {
    // arrange / act
    await runInterceptor(`${apiBaseUrl}/api/genres`, 401);

    // assert
    expect(authorizeMock).toHaveBeenCalledTimes(1);
    expect(consoleErrorSpy).toHaveBeenCalledTimes(1);
  });

  it('leitet bei HTTP 403 von der API zur Anmeldung um', async () => {
    // arrange / act
    await runInterceptor(`${apiBaseUrl}/api/genres`, 403);

    // assert
    expect(authorizeMock).toHaveBeenCalledTimes(1);
  });

  it('leitet bei anderen Fehlercodes nicht um und reicht den Fehler weiter', async () => {
    // arrange / act
    const result = await runInterceptor(`${apiBaseUrl}/api/genres`, 404);

    // assert: kein Redirect, Fehler bleibt für nachgelagerte Fehlerbehandlung erhalten
    expect(authorizeMock).not.toHaveBeenCalled();
    expect(result).toBeInstanceOf(HttpErrorResponse);
  });

  it('leitet bei HTTP 401 von einer fremden URL (nicht die MyMusic-API) nicht um', async () => {
    // arrange / act
    await runInterceptor('http://localhost:8080/realms/mymusic/protocol/openid-connect/token', 401);

    // assert
    expect(authorizeMock).not.toHaveBeenCalled();
  });
});
