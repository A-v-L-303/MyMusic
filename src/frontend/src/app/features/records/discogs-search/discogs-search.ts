import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  ElementRef,
  afterNextRender,
  inject,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { rxResource, takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { LucideDisc3, LucideSearch } from '@lucide/angular';
import { debounceTime, distinctUntilChanged, firstValueFrom, of } from 'rxjs';

import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { Modal } from '../../../shared/modal/modal';
import { DiscogsRelease, DiscogsSearchResult } from '../discogs';
import { DiscogsService } from '../discogs.service';

const MIN_QUERY_LENGTH = 2;

@Component({
  selector: 'app-discogs-search',
  imports: [Modal, LucideDisc3, LucideSearch],
  templateUrl: './discogs-search.html',
})
export class DiscogsSearch {
  private readonly discogsService = inject(DiscogsService);
  private readonly errorModalService = inject(ErrorModalService);

  readonly cancelled = output<void>();
  readonly applied = output<DiscogsRelease>();

  protected readonly queryInput = viewChild<ElementRef<HTMLInputElement>>('queryInput');

  protected readonly queryText = signal('');
  protected readonly query = signal('');
  protected readonly loadingRelease = signal(false);

  constructor() {
    toObservable(this.queryText)
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((query) => this.query.set(query.trim()));

    afterNextRender(() => this.queryInput()?.nativeElement.focus());
  }

  protected readonly searchResource = rxResource({
    params: () => ({ query: this.query() }),
    stream: ({ params }) =>
      params.query.length >= MIN_QUERY_LENGTH
        ? this.discogsService.search(params.query)
        : of<DiscogsSearchResult[]>([]),
  });

  protected readonly results = () =>
    this.searchResource.hasValue() ? this.searchResource.value() : [];

  protected onQueryInput(event: Event): void {
    this.queryText.set((event.target as HTMLInputElement).value);
  }

  protected onCancel(): void {
    this.cancelled.emit();
  }

  protected async onResultSelected(result: DiscogsSearchResult): Promise<void> {
    this.loadingRelease.set(true);

    try {
      const release = await firstValueFrom(this.discogsService.getRelease(result.id));
      this.applied.emit(release);
    } catch (error) {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }
      this.errorModalService.showFromHttpError(error, 'Discogs');
    } finally {
      this.loadingRelease.set(false);
    }
  }
}
