import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { CreateLabelRequest, Label, LabelListResponse, UpdateLabelRequest } from './label';

@Injectable({ providedIn: 'root' })
export class LabelService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  private get baseUrl(): string {
    return `${this.runtimeConfigService.apiBaseUrl}/api/labels`;
  }

  getPaged(
    page: number,
    pageSize: number,
    name?: string,
    countryId?: number,
  ): Observable<LabelListResponse> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (name) {
      params = params.set('name', name);
    }

    if (countryId) {
      params = params.set('countryId', countryId);
    }

    return this.http.get<LabelListResponse>(this.baseUrl, { params });
  }

  create(request: CreateLabelRequest): Observable<Label> {
    return this.http.post<Label>(this.baseUrl, request);
  }

  update(id: number, request: UpdateLabelRequest): Observable<Label> {
    return this.http.put<Label>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
