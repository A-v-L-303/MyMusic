import { Component, computed, input } from '@angular/core';

import { YearCount } from '../dashboard-stats';

const CHART_HEIGHT_PX = 180;
const MIN_BAR_HEIGHT_PX = 4;
const TARGET_LABEL_COUNT = 15;

interface YearBar {
  year: number;
  count: number;
  heightPx: number;
  showLabel: boolean;
}

@Component({
  selector: 'app-year-distribution',
  host: { class: 'contents' },
  templateUrl: './year-distribution.html',
})
export class YearDistribution {
  readonly items = input.required<YearCount[]>();

  protected readonly bars = computed<YearBar[]>(() => {
    const items = this.items();

    if (items.length === 0) {
      return [];
    }

    const countByYear = new Map(items.map((item) => [item.year, item.count]));
    const years = items.map((item) => item.year);
    const minYear = Math.min(...years);
    const maxYear = Math.max(...years);
    const maxCount = Math.max(...items.map((item) => item.count));
    const yearSpan = maxYear - minYear + 1;
    const labelInterval = Math.max(1, Math.ceil(yearSpan / TARGET_LABEL_COUNT));

    const bars: YearBar[] = [];

    for (let year = minYear; year <= maxYear; year++) {
      const count = countByYear.get(year) ?? 0;

      bars.push({
        year,
        count,
        heightPx:
          count === 0
            ? 0
            : Math.max(MIN_BAR_HEIGHT_PX, Math.round((count / maxCount) * CHART_HEIGHT_PX)),
        showLabel: year === minYear || year === maxYear || (year - minYear) % labelInterval === 0,
      });
    }

    return bars;
  });
}
