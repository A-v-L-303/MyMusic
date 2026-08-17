import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { AdminUserListResponse } from './admin-user';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  private get baseUrl(): string {
    return `${this.runtimeConfigService.apiBaseUrl}/api/admin`;
  }

  getPaged(page: number, pageSize: number): Observable<AdminUserListResponse> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);

    return this.http.get<AdminUserListResponse>(`${this.baseUrl}/users`, { params });
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${id}`);
  }
}
