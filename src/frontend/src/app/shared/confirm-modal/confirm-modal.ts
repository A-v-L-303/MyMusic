import { Component, input, output } from '@angular/core';
import { LucideTriangleAlert } from '@lucide/angular';

import { Modal } from '../modal/modal';

@Component({
  selector: 'app-confirm-modal',
  imports: [Modal, LucideTriangleAlert],
  templateUrl: './confirm-modal.html',
})
export class ConfirmModal {
  readonly title = input.required<string>();
  readonly message = input.required<string>();
  readonly confirmLabel = input('Löschen');
  readonly confirmVariant = input<'danger' | 'primary'>('danger');

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  protected onCancel(): void {
    this.cancelled.emit();
  }

  protected onConfirm(): void {
    this.confirmed.emit();
  }
}
