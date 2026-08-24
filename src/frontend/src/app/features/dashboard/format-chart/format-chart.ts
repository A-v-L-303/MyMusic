import { Component, computed, input } from '@angular/core';

import { RECORD_FORMAT_LABELS } from '../../records/record';
import { FormatCount } from '../dashboard-stats';

@Component({
  selector: 'app-format-chart',
  host: { class: 'contents' },
  templateUrl: './format-chart.html',
})
export class FormatChart {
  readonly items = input.required<FormatCount[]>();

  protected readonly formatLabels = RECORD_FORMAT_LABELS;

  protected readonly maxCount = computed(() =>
    Math.max(1, ...this.items().map((item) => item.count)),
  );
}
