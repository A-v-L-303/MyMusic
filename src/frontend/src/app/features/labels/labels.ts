import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { LucidePlus } from '@lucide/angular';

import { ConfirmModal } from '../../shared/confirm-modal/confirm-modal';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { CountryService } from './country.service';
import { Label } from './label';
import { LabelFilter, LabelFilterValue } from './label-filter/label-filter';
import { LabelForm } from './label-form/label-form';
import { LabelTable } from './label-table/label-table';
import { LabelService } from './label.service';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-labels',
  imports: [LabelFilter, LabelTable, LabelForm, ConfirmModal, LucidePlus],
  templateUrl: './labels.html',
})
export class Labels {
  private readonly labelService = inject(LabelService);
  private readonly countryService = inject(CountryService);
  private readonly errorModalService = inject(ErrorModalService);

  protected readonly filterName = signal('');
  protected readonly filterCountryId = signal<number | undefined>(undefined);
  protected readonly page = signal(1);

  protected readonly labelsResource = rxResource({
    params: () => ({
      page: this.page(),
      pageSize: PAGE_SIZE,
      name: this.filterName(),
      countryId: this.filterCountryId(),
    }),
    stream: ({ params }) =>
      this.labelService.getPaged(params.page, params.pageSize, params.name, params.countryId),
  });

  protected readonly countriesResource = rxResource({
    stream: () => this.countryService.getAll(),
  });

  protected readonly countries = computed(() =>
    this.countriesResource.hasValue() ? this.countriesResource.value() : [],
  );

  protected readonly labels = computed(() =>
    this.labelsResource.hasValue() ? this.labelsResource.value().items : [],
  );
  protected readonly totalPages = computed(() =>
    this.labelsResource.hasValue() ? this.labelsResource.value().totalPages : 1,
  );
  protected readonly totalCount = computed(() =>
    this.labelsResource.hasValue() ? this.labelsResource.value().totalCount : 0,
  );

  protected readonly formOpen = signal(false);
  protected readonly editingLabel = signal<Label | null>(null);

  protected readonly pendingDelete = signal<Label | null>(null);
  protected readonly pendingDeleteMessage = computed(() => {
    const label = this.pendingDelete();
    return label ? `Soll „${label.name}" wirklich gelöscht werden?` : '';
  });

  constructor() {
    effect(() => {
      const error = this.labelsResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Label', () =>
          this.labelsResource.reload(),
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

  protected onFilterChange(filter: LabelFilterValue): void {
    this.filterName.set(filter.name);
    this.filterCountryId.set(filter.countryId);
    this.page.set(1);
  }

  protected onPageChange(page: number): void {
    this.page.set(page);
  }

  protected openCreateForm(): void {
    this.editingLabel.set(null);
    this.formOpen.set(true);
  }

  protected openEditForm(label: Label): void {
    this.editingLabel.set(label);
    this.formOpen.set(true);
  }

  protected onFormCancelled(): void {
    this.formOpen.set(false);
  }

  protected onFormSaved(): void {
    this.formOpen.set(false);
    this.labelsResource.reload();
  }

  protected onDeleteRequested(label: Label): void {
    this.pendingDelete.set(label);
  }

  protected onDeleteCancelled(): void {
    this.pendingDelete.set(null);
  }

  protected onDeleteConfirmed(): void {
    const label = this.pendingDelete();

    if (!label) {
      return;
    }

    this.labelService.delete(label.id).subscribe({
      next: () => {
        this.pendingDelete.set(null);
        this.labelsResource.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.pendingDelete.set(null);
        this.errorModalService.showFromHttpError(error, 'Label');
      },
    });
  }
}
