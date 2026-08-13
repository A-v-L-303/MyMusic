import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { LucidePlus } from '@lucide/angular';

import { ConfirmModal } from '../../shared/confirm-modal/confirm-modal';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { Genre } from './genre';
import { GenreFilter } from './genre-filter/genre-filter';
import { GenreForm } from './genre-form/genre-form';
import { GenreTable } from './genre-table/genre-table';
import { GenreService } from './genre.service';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-genres',
  imports: [GenreFilter, GenreTable, GenreForm, ConfirmModal, LucidePlus],
  templateUrl: './genres.html',
})
export class Genres {
  private readonly genreService = inject(GenreService);
  private readonly errorModalService = inject(ErrorModalService);

  protected readonly filterName = signal('');
  protected readonly page = signal(1);

  protected readonly genresResource = rxResource({
    params: () => ({ page: this.page(), pageSize: PAGE_SIZE, name: this.filterName() }),
    stream: ({ params }) => this.genreService.getPaged(params.page, params.pageSize, params.name),
  });

  protected readonly genres = computed(() =>
    this.genresResource.hasValue() ? this.genresResource.value().items : [],
  );
  protected readonly totalPages = computed(() =>
    this.genresResource.hasValue() ? this.genresResource.value().totalPages : 1,
  );
  protected readonly totalCount = computed(() =>
    this.genresResource.hasValue() ? this.genresResource.value().totalCount : 0,
  );

  protected readonly formOpen = signal(false);
  protected readonly editingGenre = signal<Genre | null>(null);

  protected readonly pendingDelete = signal<Genre | null>(null);
  protected readonly pendingDeleteMessage = computed(() => {
    const genre = this.pendingDelete();
    return genre ? `Soll „${genre.name}" wirklich gelöscht werden?` : '';
  });

  constructor() {
    effect(() => {
      const error = this.genresResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Genre', () =>
          this.genresResource.reload(),
        );
      }
    });
  }

  protected onFilterChange(name: string): void {
    this.filterName.set(name);
    this.page.set(1);
  }

  protected onPageChange(page: number): void {
    this.page.set(page);
  }

  protected openCreateForm(): void {
    this.editingGenre.set(null);
    this.formOpen.set(true);
  }

  protected openEditForm(genre: Genre): void {
    this.editingGenre.set(genre);
    this.formOpen.set(true);
  }

  protected onFormCancelled(): void {
    this.formOpen.set(false);
  }

  protected onFormSaved(): void {
    this.formOpen.set(false);
    this.genresResource.reload();
  }

  protected onDeleteRequested(genre: Genre): void {
    this.pendingDelete.set(genre);
  }

  protected onDeleteCancelled(): void {
    this.pendingDelete.set(null);
  }

  protected onDeleteConfirmed(): void {
    const genre = this.pendingDelete();

    if (!genre) {
      return;
    }

    this.genreService.delete(genre.id).subscribe({
      next: () => {
        this.pendingDelete.set(null);
        this.genresResource.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.pendingDelete.set(null);
        this.errorModalService.showFromHttpError(error, 'Genre');
      },
    });
  }
}
