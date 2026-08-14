import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { RecordFormat, RecordListResponse } from './record';

@Injectable({ providedIn: 'root' })
export class RecordService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  private get baseUrl(): string {
    return `${this.runtimeConfigService.apiBaseUrl}/api/records`;
  }

  getPaged(
    page: number,
    pageSize: number,
    name?: string,
    artistId?: number,
    labelId?: number,
    yearFrom?: number,
    yearTo?: number,
    countryId?: number,
    format?: RecordFormat,
    sortBy?: string,
    sortDirection?: string,
  ): Observable<RecordListResponse> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (name) {
      params = params.set('name', name);
    }

    if (artistId) {
      params = params.set('artistId', artistId);
    }

    if (labelId) {
      params = params.set('labelId', labelId);
    }

    if (yearFrom) {
      params = params.set('yearFrom', yearFrom);
    }

    if (yearTo) {
      params = params.set('yearTo', yearTo);
    }

    if (countryId) {
      params = params.set('countryId', countryId);
    }

    if (format) {
      params = params.set('format', format);
    }

    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }

    if (sortDirection) {
      params = params.set('sortDirection', sortDirection);
    }

    return this.http.get<RecordListResponse>(this.baseUrl, { params });
  }
}
