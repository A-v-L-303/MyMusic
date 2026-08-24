import { Component, computed, input } from '@angular/core';

import { TopArtist } from '../dashboard-stats';

@Component({
  selector: 'app-top-artists',
  host: { class: 'contents' },
  templateUrl: './top-artists.html',
})
export class TopArtists {
  readonly items = input.required<TopArtist[]>();

  protected readonly maxCount = computed(() =>
    Math.max(1, ...this.items().map((item) => item.count)),
  );
}
