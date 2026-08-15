import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterOutlet } from '@angular/router';
import { LucidePlus } from '@lucide/angular';
import { of } from 'rxjs';

import { ArtistService } from '../artists/artist.service';
import { LabelService } from '../labels/label.service';
import { CountryService } from '../../shared/country/country.service';
import { AutocompleteOption } from '../../shared/autocomplete/autocomplete';
import { ConfirmModal } from '../../shared/confirm-modal/confirm-modal';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { Pagination } from '../../shared/pagination/pagination';
import { RecordCard } from './record-card/record-card';
import {
  RecordFilter,
  RecordFilterValue,
  RecordSortBy,
  RecordSortDirection,
} from './record-filter/record-filter';
import { RecordForm } from './record-form/record-form';
import { Record, RecordFormat } from './record';
import { RecordService } from './record.service';

const PAGE_SIZE = 20;
const SUGGESTION_PAGE_SIZE = 10;

@Component({
  selector: 'app-records',
  imports: [
    RecordFilter,
    RecordCard,
    Pagination,
    RecordForm,
    ConfirmModal,
    RouterOutlet,
    LucidePlus,
  ],
  templateUrl: './records.html',
})
export class Records {
  private readonly recordService = inject(RecordService);
  private readonly artistService = inject(ArtistService);
  private readonly labelService = inject(LabelService);
  private readonly countryService = inject(CountryService);
  private readonly errorModalService = inject(ErrorModalService);
  private readonly router = inject(Router);

  protected readonly filterName = signal('');
  protected readonly filterArtistId = signal<number | undefined>(undefined);
  protected readonly filterLabelId = signal<number | undefined>(undefined);
  protected readonly filterYearFrom = signal<number | undefined>(undefined);
  protected readonly filterYearTo = signal<number | undefined>(undefined);
  protected readonly filterCountryId = signal<number | undefined>(undefined);
  protected readonly filterFormat = signal<RecordFormat | undefined>(undefined);
  protected readonly filterSortBy = signal<RecordSortBy>('name');
  protected readonly filterSortDirection = signal<RecordSortDirection>('asc');
  protected readonly page = signal(1);

  protected readonly artistQuery = signal('');
  protected readonly labelQuery = signal('');

  protected readonly recordsResource = rxResource({
    params: () => ({
      page: this.page(),
      pageSize: PAGE_SIZE,
      name: this.filterName(),
      artistId: this.filterArtistId(),
      labelId: this.filterLabelId(),
      yearFrom: this.filterYearFrom(),
      yearTo: this.filterYearTo(),
      countryId: this.filterCountryId(),
      format: this.filterFormat(),
      sortBy: this.filterSortBy(),
      sortDirection: this.filterSortDirection(),
    }),
    stream: ({ params }) =>
      this.recordService.getPaged(
        params.page,
        params.pageSize,
        params.name,
        params.artistId,
        params.labelId,
        params.yearFrom,
        params.yearTo,
        params.countryId,
        params.format,
        params.sortBy,
        params.sortDirection,
      ),
  });

  protected readonly artistSuggestionsResource = rxResource({
    params: () => ({ query: this.artistQuery() }),
    stream: ({ params }) =>
      params.query
        ? this.artistService.getPaged(1, SUGGESTION_PAGE_SIZE, params.query)
        : of({ items: [], totalCount: 0, page: 1, pageSize: SUGGESTION_PAGE_SIZE, totalPages: 0 }),
  });

  protected readonly labelSuggestionsResource = rxResource({
    params: () => ({ query: this.labelQuery() }),
    stream: ({ params }) =>
      params.query
        ? this.labelService.getPaged(1, SUGGESTION_PAGE_SIZE, params.query)
        : of({ items: [], totalCount: 0, page: 1, pageSize: SUGGESTION_PAGE_SIZE, totalPages: 0 }),
  });

  protected readonly countriesResource = rxResource({
    stream: () => this.countryService.getAll(),
  });

  protected readonly artistSuggestions = computed<AutocompleteOption[]>(() =>
    this.artistSuggestionsResource.hasValue()
      ? this.artistSuggestionsResource.value().items.map((artist) => ({
          id: artist.id,
          label: artist.name,
        }))
      : [],
  );
  protected readonly labelSuggestions = computed<AutocompleteOption[]>(() =>
    this.labelSuggestionsResource.hasValue()
      ? this.labelSuggestionsResource.value().items.map((label) => ({
          id: label.id,
          label: label.name,
        }))
      : [],
  );
  protected readonly countries = computed(() =>
    this.countriesResource.hasValue() ? this.countriesResource.value() : [],
  );

  protected readonly records = computed(() =>
    this.recordsResource.hasValue() ? this.recordsResource.value().items : [],
  );
  protected readonly totalPages = computed(() =>
    this.recordsResource.hasValue() ? this.recordsResource.value().totalPages : 1,
  );
  protected readonly totalCount = computed(() =>
    this.recordsResource.hasValue() ? this.recordsResource.value().totalCount : 0,
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
      const error = this.recordsResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Record', () =>
          this.recordsResource.reload(),
        );
      }
    });

    effect(() => {
      const error = this.countriesResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Land', () =>
          this.countriesResource.reload(),
        );
      }
    });
  }

  protected onFilterChange(filter: RecordFilterValue): void {
    this.filterName.set(filter.name);
    this.filterArtistId.set(filter.artistId);
    this.filterLabelId.set(filter.labelId);
    this.filterYearFrom.set(filter.yearFrom);
    this.filterYearTo.set(filter.yearTo);
    this.filterCountryId.set(filter.countryId);
    this.filterFormat.set(filter.format);
    this.filterSortBy.set(filter.sortBy);
    this.filterSortDirection.set(filter.sortDirection);
    this.page.set(1);
  }

  protected onPageChange(page: number): void {
    this.page.set(page);
  }

  protected onArtistQueryChange(query: string): void {
    this.artistQuery.set(query);
  }

  protected onLabelQueryChange(query: string): void {
    this.labelQuery.set(query);
  }

  protected onRecordOpened(record: Record): void {
    this.router.navigate(['/records', record.id]);
  }

  protected openCreateForm(): void {
    this.editingRecord.set(null);
    this.formOpen.set(true);
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
    this.recordsResource.reload();
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
        this.recordsResource.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.pendingDelete.set(null);
        this.errorModalService.showFromHttpError(error, 'Record');
      },
    });
  }
}
