import { Component, computed, input, output } from '@angular/core';
import { LucidePencil, LucideTrash2 } from '@lucide/angular';

import { RecordTrack } from '../record';

interface TrackGroup {
  side: string;
  tracks: RecordTrack[];
}

@Component({
  selector: 'app-track-list',
  imports: [LucidePencil, LucideTrash2],
  templateUrl: './track-list.html',
})
export class TrackList {
  readonly tracks = input.required<RecordTrack[]>();

  readonly editRequested = output<RecordTrack>();
  readonly deleteRequested = output<RecordTrack>();

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

  protected onEditClicked(track: RecordTrack): void {
    this.editRequested.emit(track);
  }

  protected onDeleteClicked(track: RecordTrack): void {
    this.deleteRequested.emit(track);
  }
}
