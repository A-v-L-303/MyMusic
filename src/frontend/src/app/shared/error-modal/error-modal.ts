import { Component, computed, inject } from '@angular/core';
import { LucideCircleAlert } from '@lucide/angular';

import { Modal } from '../modal/modal';
import { ErrorModalKind, ErrorModalService } from './error-modal.service';

const TITLES: Record<ErrorModalKind, string> = {
  'not-found': 'Nicht gefunden',
  conflict: 'Konflikt',
  server: 'Serverfehler',
  'rate-limit': 'Zu viele Anfragen',
  network: 'Verbindungsfehler',
};

@Component({
  selector: 'app-error-modal',
  imports: [Modal, LucideCircleAlert],
  templateUrl: './error-modal.html',
})
export class ErrorModal {
  private readonly errorModalService = inject(ErrorModalService);

  protected readonly state = this.errorModalService.current;
  protected readonly title = computed(() => {
    const state = this.state();
    return state ? TITLES[state.kind] : '';
  });

  protected onClosed(): void {
    this.errorModalService.dismiss();
  }

  protected onRetry(): void {
    const state = this.state();
    this.errorModalService.dismiss();
    state?.onRetry?.();
  }
}
