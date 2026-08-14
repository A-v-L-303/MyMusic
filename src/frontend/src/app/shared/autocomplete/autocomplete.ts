import { Component, input, output, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { LucideSearch } from '@lucide/angular';
import { debounceTime, distinctUntilChanged } from 'rxjs';

export interface AutocompleteOption {
  id: number;
  label: string;
}

@Component({
  selector: 'app-autocomplete',
  imports: [LucideSearch],
  templateUrl: './autocomplete.html',
})
export class Autocomplete {
  readonly placeholder = input.required<string>();
  readonly ariaLabel = input.required<string>();
  readonly options = input<AutocompleteOption[]>([]);

  readonly queryChange = output<string>();
  readonly selected = output<AutocompleteOption | undefined>();

  protected readonly queryText = signal('');
  protected readonly isOpen = signal(false);
  protected readonly highlightedIndex = signal(-1);

  constructor() {
    toObservable(this.queryText)
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((query) => this.queryChange.emit(query.trim()));
  }

  protected onInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;

    this.queryText.set(value);
    this.highlightedIndex.set(-1);
    this.isOpen.set(value.trim().length > 0);

    if (value.trim().length === 0) {
      this.selected.emit(undefined);
    }
  }

  protected onFocus(): void {
    if (this.queryText().trim().length > 0 && this.options().length > 0) {
      this.isOpen.set(true);
    }
  }

  protected onKeydown(event: KeyboardEvent): void {
    const count = this.options().length;

    if (event.key === 'ArrowDown') {
      event.preventDefault();

      if (count > 0) {
        this.highlightedIndex.update((index) => (index + 1) % count);
        this.isOpen.set(true);
      }
      return;
    }

    if (event.key === 'ArrowUp') {
      event.preventDefault();

      if (count > 0) {
        this.highlightedIndex.update((index) => (index - 1 + count) % count);
        this.isOpen.set(true);
      }
      return;
    }

    if (event.key === 'Enter') {
      const option = this.options()[this.highlightedIndex()];

      if (option) {
        event.preventDefault();
        this.selectOption(option);
      }
      return;
    }

    if (event.key === 'Escape') {
      this.isOpen.set(false);
    }
  }

  protected onOptionMouseDown(event: MouseEvent, option: AutocompleteOption): void {
    event.preventDefault();
    this.selectOption(option);
  }

  private selectOption(option: AutocompleteOption): void {
    this.queryText.set(option.label);
    this.isOpen.set(false);
    this.highlightedIndex.set(-1);
    this.selected.emit(option);
  }
}
