import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideDisc3, LucidePlus, LucideX } from '@lucide/angular';
import { map } from 'rxjs';

import { ConfirmModal } from '../../../shared/confirm-modal/confirm-modal';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { Modal } from '../../../shared/modal/modal';
import {
  RECORD_CONDITION_GRADE_CLASS,
  RECORD_CONDITION_GRADE_TEXT,
  RECORD_FORMAT_LABELS,
  Record,
  RecordTrack,
} from '../record';
import { RecordService } from '../record.service';
import { TrackForm } from '../track-form/track-form';
import { TrackList } from '../track-list/track-list';

@Component({
  selector: 'app-record-detail',
  imports: [Modal, TrackList, TrackForm, ConfirmModal, LucideDisc3, LucidePlus, LucideX],
  templateUrl: './record-detail.html',
})
export class RecordDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly recordService = inject(RecordService);
  private readonly errorModalService = inject(ErrorModalService);

  private readonly idParam = toSignal(this.route.paramMap.pipe(map((params) => params.get('id'))), {
    initialValue: null,
  });
  protected readonly id = computed(() => Number(this.idParam()));

  protected readonly recordResource = rxResource({
    params: () => ({ id: this.id() }),
    stream: ({ params }) => this.recordService.getById(params.id),
  });

  protected readonly record = computed(() =>
    this.recordResource.hasValue() ? this.recordResource.value() : null,
  );

  protected readonly gradeClass = computed(() => {
    const record = this.record();
    return record ? RECORD_CONDITION_GRADE_CLASS[record.condition] : '';
  });

  protected readonly gradeText = computed(() => {
    const record = this.record();
    return record ? RECORD_CONDITION_GRADE_TEXT[record.condition] : '';
  });

  protected readonly formatLabel = computed(() => {
    const record = this.record();
    return record ? RECORD_FORMAT_LABELS[record.format] : '';
  });

  protected readonly trackFormOpen = signal(false);
  protected readonly editingTrack = signal<RecordTrack | null>(null);
  protected readonly pendingDeleteTrack = signal<RecordTrack | null>(null);

  protected readonly pendingDeleteTrackMessage = computed(() => {
    const track = this.pendingDeleteTrack();
    return track ? `Soll der Track „${track.trackName}" wirklich gelöscht werden?` : '';
  });

  constructor() {
    effect(() => {
      const error = this.recordResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Record', () =>
          this.recordResource.reload(),
        );
      }
    });
  }

  protected onClose(): void {
    this.router.navigate(['/records']);
  }

  protected onAddTrackClicked(): void {
    this.editingTrack.set(null);
    this.trackFormOpen.set(true);
  }

  protected onEditTrackRequested(track: RecordTrack): void {
    this.editingTrack.set(track);
    this.trackFormOpen.set(true);
  }

  protected onTrackFormCancelled(): void {
    this.trackFormOpen.set(false);
    this.editingTrack.set(null);
  }

  protected onTrackFormSaved(): void {
    this.trackFormOpen.set(false);
    this.editingTrack.set(null);
    this.recordResource.reload();
  }

  protected onDeleteTrackRequested(track: RecordTrack): void {
    this.pendingDeleteTrack.set(track);
  }

  protected onDeleteTrackCancelled(): void {
    this.pendingDeleteTrack.set(null);
  }

  protected onDeleteTrackConfirmed(): void {
    const track = this.pendingDeleteTrack();

    if (!track) {
      return;
    }

    this.recordService.deleteTrack(this.id(), track.id).subscribe({
      next: () => {
        this.pendingDeleteTrack.set(null);
        this.recordResource.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.pendingDeleteTrack.set(null);
        this.errorModalService.showFromHttpError(error, 'Track');
      },
    });
  }
}
