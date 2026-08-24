import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { DashboardStats } from './dashboard-stats';
import { DashboardService } from './dashboard.service';

describe('DashboardService', () => {
  let service: DashboardService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
      ],
    });

    service = TestBed.inject(DashboardService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('ruft getDashboard als GET gegen /api/dashboard auf', () => {
    // arrange
    const stats: DashboardStats = {
      recordsTotal: 5,
      artistsTotal: 3,
      labelsTotal: 2,
      genresTotal: 4,
      formatDistribution: [],
      topArtists: [],
      topLabels: [],
      yearDistribution: [],
    };
    let result: DashboardStats | undefined;

    // act
    service.getDashboard().subscribe((value) => (result = value));
    const request = httpTesting.expectOne('https://api.test/api/dashboard');
    request.flush(stats);

    // assert
    expect(request.request.method).toBe('GET');
    expect(result).toEqual(stats);
  });
});
