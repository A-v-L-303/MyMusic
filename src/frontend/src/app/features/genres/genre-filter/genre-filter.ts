import { Component, effect, output, signal } from '@angular/core';
import { FormField, debounce, form } from '@angular/forms/signals';
import { LucideSearch } from '@lucide/angular';

@Component({
  selector: 'app-genre-filter',
  imports: [FormField, LucideSearch],
  templateUrl: './genre-filter.html',
})
export class GenreFilter {
  protected readonly filterModel = signal({ name: '' });
  protected readonly filterForm = form(this.filterModel, (path) => {
    debounce(path.name, 300);
  });

  readonly filterChange = output<string>();

  constructor() {
    effect(() => {
      this.filterChange.emit(this.filterModel().name.trim());
    });
  }
}
