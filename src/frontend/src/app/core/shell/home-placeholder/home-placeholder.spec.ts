import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../runtime-config/runtime-config.service';
import { HomePlaceholder } from './home-placeholder';

describe('HomePlaceholder', () => {
  const apiBaseUrl = 'https://localhost:5001';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HomePlaceholder],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl } },
      ],
    });
  });

  it('ruft GET /api/me auf und zeigt den angemeldeten Benutzer', async () => {
    // arrange
    const fixture = TestBed.createComponent(HomePlaceholder);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    const request = httpMock.expectOne(`${apiBaseUrl}/api/me`);

    // act
    request.flush({ userId: 'b3f6f7a0-testbenutzer' });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('b3f6f7a0-testbenutzer');

    httpMock.verify();
  });

  it('zeigt eine Fehlermeldung, wenn /api/me fehlschlägt', async () => {
    // arrange
    const fixture = TestBed.createComponent(HomePlaceholder);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    const request = httpMock.expectOne(`${apiBaseUrl}/api/me`);

    // act
    request.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
    await fixture.whenStable();
    fixture.detectChanges();

    // assert: keine Ausnahme, Fehlerzustand wird angezeigt statt die Komponente abstürzen zu lassen
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('nicht geladen werden');

    httpMock.verify();
  });
});
