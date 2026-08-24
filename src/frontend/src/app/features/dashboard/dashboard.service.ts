import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { DashboardStats } from './dashboard-stats';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  getDashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.runtimeConfigService.apiBaseUrl}/api/dashboard`);
  }
}
