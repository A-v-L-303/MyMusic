import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { DashboardStats } from './dashboard-stats';
import { Dashboard } from './dashboard';

describe('Dashboard', () => {
  let httpTesting: HttpTestingController;

  function createFixture() {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    httpTesting = TestBed.inject(HttpTestingController);

    return TestBed.createComponent(Dashboard);
  }

  afterEach(() => {
    httpTesting.verify();
  });

  it('zeigt die vier Kennzahlen-Kacheln mit den geladenen Zahlen', async () => {
    // arrange
    const stats: DashboardStats = {
      recordsTotal: 12,
      artistsTotal: 5,
      labelsTotal: 3,
      genresTotal: 4,
      formatDistribution: [],
      topArtists: [],
      topLabels: [],
      yearDistribution: [],
    };
    const fixture = createFixture();

    // act
    fixture.detectChanges();
    httpTesting.expectOne('https://api.test/api/dashboard').flush(stats);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('12');
    expect(compiled.textContent).toContain('5');
    expect(compiled.textContent).toContain('3');
    expect(compiled.textContent).toContain('4');
  });

  it('zeigt die leeren Detail-Kacheln unabhängig voneinander bei einer leeren Sammlung', async () => {
    // arrange
    const stats: DashboardStats = {
      recordsTotal: 0,
      artistsTotal: 0,
      labelsTotal: 0,
      genresTotal: 0,
      formatDistribution: [],
      topArtists: [],
      topLabels: [],
      yearDistribution: [],
    };
    const fixture = createFixture();

    // act
    fixture.detectChanges();
    httpTesting.expectOne('https://api.test/api/dashboard').flush(stats);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    const emptyMessages = compiled.querySelectorAll('.empty');
    expect(emptyMessages.length).toBe(4);
  });
});
