import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { RuntimeConfigService } from '../../runtime-config/runtime-config.service';

interface CurrentUserResponse {
  userId: string;
}

@Component({
  selector: 'app-home-placeholder',
  templateUrl: './home-placeholder.html',
})
export class HomePlaceholder {
  private readonly httpClient = inject(HttpClient);
  private readonly runtimeConfigService = inject(RuntimeConfigService);

  protected readonly currentUser = rxResource({
    stream: () =>
      this.httpClient.get<CurrentUserResponse>(`${this.runtimeConfigService.apiBaseUrl}/api/me`),
  });
}
