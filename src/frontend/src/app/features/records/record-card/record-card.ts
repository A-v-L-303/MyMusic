import { Component, computed, input, output } from '@angular/core';
import { LucideDisc3, LucidePencil, LucideTrash2 } from '@lucide/angular';

import {
  CD_FORMATS,
  RECORD_CONDITION_GRADE_CLASS,
  RECORD_CONDITION_GRADE_TEXT,
  Record,
} from '../record';

@Component({
  selector: 'app-record-card',
  imports: [LucideDisc3, LucidePencil, LucideTrash2],
  templateUrl: './record-card.html',
})
export class RecordCard {
  readonly record = input.required<Record>();

  readonly opened = output<void>();
  readonly editRequested = output<Record>();
  readonly deleteRequested = output<Record>();

  protected readonly formatPill = computed(() =>
    CD_FORMATS.includes(this.record().format) ? 'CD' : 'LP',
  );

  protected readonly gradeClass = computed(
    () => RECORD_CONDITION_GRADE_CLASS[this.record().condition],
  );

  protected readonly gradeText = computed(
    () => RECORD_CONDITION_GRADE_TEXT[this.record().condition],
  );

  protected onEditClicked(event: MouseEvent): void {
    event.stopPropagation();
    this.editRequested.emit(this.record());
  }

  protected onDeleteClicked(event: MouseEvent): void {
    event.stopPropagation();
    this.deleteRequested.emit(this.record());
  }
}
