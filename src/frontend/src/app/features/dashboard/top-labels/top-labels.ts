import { Component, computed, input } from '@angular/core';

import { TopLabel } from '../dashboard-stats';

@Component({
  selector: 'app-top-labels',
  host: { class: 'contents' },
  templateUrl: './top-labels.html',
})
export class TopLabels {
  readonly items = input.required<TopLabel[]>();

  protected readonly maxCount = computed(() =>
    Math.max(1, ...this.items().map((item) => item.count)),
  );
}
