import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { LucidePlus } from '@lucide/angular';

import { ConfirmModal } from '../../shared/confirm-modal/confirm-modal';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { Artist } from './artist';
import { ArtistFilter } from './artist-filter/artist-filter';
import { ArtistForm } from './artist-form/artist-form';
import { ArtistTable } from './artist-table/artist-table';
import { ArtistService } from './artist.service';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-artists',
  imports: [ArtistFilter, ArtistTable, ArtistForm, ConfirmModal, LucidePlus],
  templateUrl: './artists.html',
})
export class Artists {
  private readonly artistService = inject(ArtistService);
  private readonly errorModalService = inject(ErrorModalService);

  protected readonly filterName = signal('');
  protected readonly page = signal(1);

  protected readonly artistsResource = rxResource({
    params: () => ({ page: this.page(), pageSize: PAGE_SIZE, name: this.filterName() }),
    stream: ({ params }) => this.artistService.getPaged(params.page, params.pageSize, params.name),
  });

  protected readonly artists = computed(() =>
    this.artistsResource.hasValue() ? this.artistsResource.value().items : [],
  );
  protected readonly totalPages = computed(() =>
    this.artistsResource.hasValue() ? this.artistsResource.value().totalPages : 1,
  );
  protected readonly totalCount = computed(() =>
    this.artistsResource.hasValue() ? this.artistsResource.value().totalCount : 0,
  );

  protected readonly formOpen = signal(false);
  protected readonly editingArtist = signal<Artist | null>(null);

  protected readonly pendingDelete = signal<Artist | null>(null);
  protected readonly pendingDeleteMessage = computed(() => {
    const artist = this.pendingDelete();
    return artist ? `Soll „${artist.name}" wirklich gelöscht werden?` : '';
  });

  constructor() {
    effect(() => {
      const error = this.artistsResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Artist', () =>
          this.artistsResource.reload(),
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
    this.editingArtist.set(null);
    this.formOpen.set(true);
  }

  protected openEditForm(artist: Artist): void {
    this.editingArtist.set(artist);
    this.formOpen.set(true);
  }

  protected onFormCancelled(): void {
    this.formOpen.set(false);
  }

  protected onFormSaved(): void {
    this.formOpen.set(false);
    this.artistsResource.reload();
  }

  protected onDeleteRequested(artist: Artist): void {
    this.pendingDelete.set(artist);
  }

  protected onDeleteCancelled(): void {
    this.pendingDelete.set(null);
  }

  protected onDeleteConfirmed(): void {
    const artist = this.pendingDelete();

    if (!artist) {
      return;
    }

    this.artistService.delete(artist.id).subscribe({
      next: () => {
        this.pendingDelete.set(null);
        this.artistsResource.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.pendingDelete.set(null);
        this.errorModalService.showFromHttpError(error, 'Artist');
      },
    });
  }
}
