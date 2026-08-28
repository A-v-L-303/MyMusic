import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { RuntimeConfigService } from '../../core/runtime-config/runtime-config.service';

@Injectable({ providedIn: 'root' })
export class UserProfileService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  private get baseUrl(): string {
    return `${this.runtimeConfigService.apiBaseUrl}/api/me`;
  }

  updateEmail(email: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/email`, { email });
  }

  changePassword(newPassword: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/password`, { newPassword });
  }
}
