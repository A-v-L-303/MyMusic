import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideDisc3, LucideX } from '@lucide/angular';
import { map } from 'rxjs';

import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { Modal } from '../../../shared/modal/modal';
import {
  RECORD_CONDITION_GRADE_CLASS,
  RECORD_CONDITION_GRADE_TEXT,
  RECORD_FORMAT_LABELS,
  Record,
} from '../record';
import { RecordService } from '../record.service';
import { TrackList } from '../track-list/track-list';

@Component({
  selector: 'app-record-detail',
  imports: [Modal, TrackList, LucideDisc3, LucideX],
  templateUrl: './record-detail.html',
})
export class RecordDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly recordService = inject(RecordService);
  private readonly errorModalService = inject(ErrorModalService);

  private readonly idParam = toSignal(
    this.route.paramMap.pipe(map((params) => params.get('id'))),
    { initialValue: null },
  );
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
}
