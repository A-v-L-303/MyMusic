import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { map, of } from 'rxjs';

import { ConfirmModal } from '../../shared/confirm-modal/confirm-modal';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { Pagination } from '../../shared/pagination/pagination';
import { RecordCard } from '../records/record-card/record-card';
import { Record, RecordListResponse } from '../records/record';
import { RecordForm } from '../records/record-form/record-form';
import { RecordService } from '../records/record.service';
import { SearchService } from './search.service';

const PAGE_SIZE = 20;

const EMPTY_RESULT: RecordListResponse = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: PAGE_SIZE,
  totalPages: 0,
};

@Component({
  selector: 'app-search',
  imports: [RecordCard, Pagination, RecordForm, ConfirmModal],
  templateUrl: './search.html',
})
export class Search {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly searchService = inject(SearchService);
  private readonly recordService = inject(RecordService);
  private readonly errorModalService = inject(ErrorModalService);

  protected readonly query = toSignal(
    this.route.queryParamMap.pipe(map((params) => params.get('q'))),
    { initialValue: null },
  );

  protected readonly page = signal(1);

  protected readonly searchResource = rxResource({
    params: () => ({ query: this.query(), page: this.page() }),
    stream: ({ params }) =>
      params.query
        ? this.searchService.getPaged(params.page, PAGE_SIZE, params.query)
        : of(EMPTY_RESULT),
  });

  protected readonly results = computed(() =>
    this.searchResource.hasValue() ? this.searchResource.value().items : [],
  );
  protected readonly totalPages = computed(() =>
    this.searchResource.hasValue() ? this.searchResource.value().totalPages : 1,
  );
  protected readonly totalCount = computed(() =>
    this.searchResource.hasValue() ? this.searchResource.value().totalCount : 0,
  );

  protected readonly formOpen = signal(false);
  protected readonly editingRecord = signal<Record | null>(null);

  protected readonly pendingDelete = signal<Record | null>(null);
  protected readonly pendingDeleteMessage = computed(() => {
    const record = this.pendingDelete();
    return record ? `Soll „${record.albumName}" wirklich gelöscht werden?` : '';
  });

  constructor() {
    effect(() => {
      this.query();
      this.page.set(1);
    });

    effect(() => {
      const error = this.searchResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Record', () =>
          this.searchResource.reload(),
        );
      }
    });
  }

  protected onPageChange(page: number): void {
    this.page.set(page);
  }

  protected onRecordOpened(record: Record): void {
    this.router.navigate(['/records', record.id]);
  }

  protected openEditForm(record: Record): void {
    this.editingRecord.set(record);
    this.formOpen.set(true);
  }

  protected onFormCancelled(): void {
    this.formOpen.set(false);
  }

  protected onFormSaved(): void {
    this.formOpen.set(false);
    this.searchResource.reload();
  }

  protected onDeleteRequested(record: Record): void {
    this.pendingDelete.set(record);
  }

  protected onDeleteCancelled(): void {
    this.pendingDelete.set(null);
  }

  protected onDeleteConfirmed(): void {
    const record = this.pendingDelete();

    if (!record) {
      return;
    }

    this.recordService.delete(record.id).subscribe({
      next: () => {
        this.pendingDelete.set(null);
        this.searchResource.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.pendingDelete.set(null);
        this.errorModalService.showFromHttpError(error, 'Record');
      },
    });
  }
}
