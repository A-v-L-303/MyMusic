import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { CreateGenreRequest, Genre, GenreListResponse, UpdateGenreRequest } from './genre';

@Injectable({ providedIn: 'root' })
export class GenreService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  private get baseUrl(): string {
    return `${this.runtimeConfigService.apiBaseUrl}/api/genres`;
  }

  getPaged(page: number, pageSize: number, name?: string): Observable<GenreListResponse> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (name) {
      params = params.set('name', name);
    }

    return this.http.get<GenreListResponse>(this.baseUrl, { params });
  }

  create(request: CreateGenreRequest): Observable<Genre> {
    return this.http.post<Genre>(this.baseUrl, request);
  }

  update(id: number, request: UpdateGenreRequest): Observable<Genre> {
    return this.http.put<Genre>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
