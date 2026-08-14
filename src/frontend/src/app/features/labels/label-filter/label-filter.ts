import { Component, effect, input, output, signal } from '@angular/core';
import { FormField, debounce, form } from '@angular/forms/signals';
import { LucideSearch } from '@lucide/angular';

import { Country } from '../../../shared/country/country';

export interface LabelFilterValue {
  name: string;
  countryId: number | undefined;
}

@Component({
  selector: 'app-label-filter',
  imports: [FormField, LucideSearch],
  templateUrl: './label-filter.html',
})
export class LabelFilter {
  readonly countries = input.required<Country[]>();

  protected readonly filterModel = signal({ name: '', countryId: '' });
  protected readonly filterForm = form(this.filterModel, (path) => {
    debounce(path.name, 300);
  });

  readonly filterChange = output<LabelFilterValue>();

  constructor() {
    effect(() => {
      const model = this.filterModel();

      this.filterChange.emit({
        name: model.name.trim(),
        countryId: model.countryId ? Number(model.countryId) : undefined,
      });
    });
  }
}
