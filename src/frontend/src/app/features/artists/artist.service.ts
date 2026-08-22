import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { Artist, ArtistListResponse, CreateArtistRequest, UpdateArtistRequest } from './artist';

@Injectable({ providedIn: 'root' })
export class ArtistService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  private get baseUrl(): string {
    return `${this.runtimeConfigService.apiBaseUrl}/api/artists`;
  }

  getPaged(page: number, pageSize: number, name?: string): Observable<ArtistListResponse> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (name) {
      params = params.set('name', name);
    }

    return this.http.get<ArtistListResponse>(this.baseUrl, { params });
  }

  getAll(): Observable<Artist[]> {
    return this.http.get<Artist[]>(`${this.baseUrl}/all`);
  }

  create(request: CreateArtistRequest): Observable<Artist> {
    return this.http.post<Artist>(this.baseUrl, request);
  }

  update(id: number, request: UpdateArtistRequest): Observable<Artist> {
    return this.http.put<Artist>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
