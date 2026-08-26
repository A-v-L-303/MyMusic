import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Record, RecordListResponse } from '../records/record';

type SearchResult = Omit<Record, 'tracks'>;

interface SearchResultListResponse {
  items: SearchResult[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class SearchService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  private get baseUrl(): string {
    return `${this.runtimeConfigService.apiBaseUrl}/api/search`;
  }

  getPaged(page: number, pageSize: number, q: string): Observable<RecordListResponse> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize).set('q', q);

    return this.http.get<SearchResultListResponse>(this.baseUrl, { params }).pipe(
      map((response) => ({
        ...response,
        items: response.items.map((item) => ({ ...item, tracks: [] })),
      })),
    );
  }
}
