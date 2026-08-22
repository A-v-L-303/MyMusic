import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';
import { DiscogsRelease, DiscogsSearchResult } from './discogs';

@Injectable({ providedIn: 'root' })
export class DiscogsService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  private get baseUrl(): string {
    return `${this.runtimeConfigService.apiBaseUrl}/api/discogs`;
  }

  search(q: string): Observable<DiscogsSearchResult[]> {
    const params = new HttpParams().set('q', q);

    return this.http.get<DiscogsSearchResult[]>(`${this.baseUrl}/search`, { params });
  }

  getRelease(id: number): Observable<DiscogsRelease> {
    return this.http.get<DiscogsRelease>(`${this.baseUrl}/releases/${id}`);
  }
}
