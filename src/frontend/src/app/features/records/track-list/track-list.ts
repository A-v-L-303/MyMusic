import { Component, computed, input } from '@angular/core';

import { RecordTrack } from '../record';

interface TrackGroup {
  side: string;
  tracks: RecordTrack[];
}

@Component({
  selector: 'app-track-list',
  templateUrl: './track-list.html',
})
export class TrackList {
  readonly tracks = input.required<RecordTrack[]>();

  protected readonly groups = computed<TrackGroup[]>(() => {
    const bySide = new Map<string, RecordTrack[]>();

    for (const track of this.tracks()) {
      const tracksOfSide = bySide.get(track.recordSide) ?? [];
      tracksOfSide.push(track);
      bySide.set(track.recordSide, tracksOfSide);
    }

    return Array.from(bySide.entries()).map(([side, tracks]) => ({ side, tracks }));
  });

  protected readonly showSideHeading = computed(() => {
    const groups = this.groups();
    return !(groups.length === 1 && groups[0].side === '0');
  });
}
