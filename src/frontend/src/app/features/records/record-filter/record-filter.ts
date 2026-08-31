import { Component, effect, input, output, signal } from '@angular/core';
import { FormField, debounce, form } from '@angular/forms/signals';
import { LucideArrowDown, LucideArrowUp, LucideSearch } from '@lucide/angular';

import { Autocomplete, AutocompleteOption } from '../../../shared/autocomplete/autocomplete';
import { Country } from '../../../shared/country/country';
import { RECORD_FORMAT_LABELS, RecordFormat } from '../record';

export type RecordSortBy = 'collectionNumber' | 'name' | 'releaseYear' | 'format';
export type RecordSortDirection = 'asc' | 'desc';

export interface RecordFilterValue {
  name: string;
  artistId: number | undefined;
  labelId: number | undefined;
  yearFrom: number | undefined;
  yearTo: number | undefined;
  countryId: number | undefined;
  format: RecordFormat | undefined;
  sortBy: RecordSortBy;
  sortDirection: RecordSortDirection;
}

interface RecordFilterFormModel {
  name: string;
  yearFrom: string;
  yearTo: string;
  countryId: string;
  format: string;
  sortBy: string;
}

@Component({
  selector: 'app-record-filter',
  imports: [FormField, LucideSearch, LucideArrowUp, LucideArrowDown, Autocomplete],
  templateUrl: './record-filter.html',
})
export class RecordFilter {
  readonly artistSuggestions = input<AutocompleteOption[]>([]);
  readonly labelSuggestions = input<AutocompleteOption[]>([]);
  readonly countries = input.required<Country[]>();

  readonly artistQueryChange = output<string>();
  readonly labelQueryChange = output<string>();
  readonly filterChange = output<RecordFilterValue>();

  protected readonly formatOptions = Object.entries(RECORD_FORMAT_LABELS) as [
    RecordFormat,
    string,
  ][];

  protected readonly filterModel = signal<RecordFilterFormModel>({
    name: '',
    yearFrom: '',
    yearTo: '',
    countryId: '',
    format: '',
    sortBy: 'collectionNumber',
  });
  protected readonly filterForm = form(this.filterModel, (path) => {
    debounce(path.name, 300);
  });

  protected readonly selectedArtistId = signal<number | undefined>(undefined);
  protected readonly selectedLabelId = signal<number | undefined>(undefined);
  protected readonly sortDirection = signal<RecordSortDirection>('asc');

  constructor() {
    effect(() => {
      const model = this.filterModel();
      const artistId = this.selectedArtistId();
      const labelId = this.selectedLabelId();
      const sortDirection = this.sortDirection();

      this.filterChange.emit({
        name: model.name.trim(),
        artistId,
        labelId,
        yearFrom: model.yearFrom ? Number(model.yearFrom) : undefined,
        yearTo: model.yearTo ? Number(model.yearTo) : undefined,
        countryId: model.countryId ? Number(model.countryId) : undefined,
        format: model.format ? (model.format as RecordFormat) : undefined,
        sortBy: model.sortBy as RecordSortBy,
        sortDirection,
      });
    });
  }

  protected toggleSortDirection(): void {
    this.sortDirection.update((direction) => (direction === 'asc' ? 'desc' : 'asc'));
  }

  protected onArtistSelected(option: AutocompleteOption | undefined): void {
    this.selectedArtistId.set(option?.id as number | undefined);
  }

  protected onLabelSelected(option: AutocompleteOption | undefined): void {
    this.selectedLabelId.set(option?.id as number | undefined);
  }
}
