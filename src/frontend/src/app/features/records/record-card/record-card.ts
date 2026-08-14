import { Component, computed, input, output } from '@angular/core';
import { LucideDisc3 } from '@lucide/angular';

import {
  CD_FORMATS,
  RECORD_CONDITION_GRADE_CLASS,
  RECORD_CONDITION_GRADE_TEXT,
  Record,
} from '../record';

@Component({
  selector: 'app-record-card',
  imports: [LucideDisc3],
  templateUrl: './record-card.html',
})
export class RecordCard {
  readonly record = input.required<Record>();

  readonly opened = output<void>();

  protected readonly formatPill = computed(() =>
    CD_FORMATS.includes(this.record().format) ? 'CD' : 'LP',
  );

  protected readonly gradeClass = computed(
    () => RECORD_CONDITION_GRADE_CLASS[this.record().condition],
  );

  protected readonly gradeText = computed(
    () => RECORD_CONDITION_GRADE_TEXT[this.record().condition],
  );
}
