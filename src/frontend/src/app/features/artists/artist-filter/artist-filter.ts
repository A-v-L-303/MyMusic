import { Component, effect, output, signal } from '@angular/core';
import { FormField, debounce, form } from '@angular/forms/signals';
import { LucideSearch } from '@lucide/angular';

@Component({
  selector: 'app-artist-filter',
  imports: [FormField, LucideSearch],
  templateUrl: './artist-filter.html',
})
export class ArtistFilter {
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
