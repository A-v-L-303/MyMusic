import { Component, ElementRef, effect, input, output, viewChild } from '@angular/core';

@Component({
  selector: 'app-load-more',
  templateUrl: './load-more.html',
})
export class LoadMore {
  readonly hasMore = input.required<boolean>();
  readonly loading = input.required<boolean>();

  readonly loadMore = output<void>();

  private readonly sentinel = viewChild<ElementRef<HTMLElement>>('sentinel');

  constructor() {
    effect((onCleanup) => {
      const element = this.sentinel()?.nativeElement;

      if (!element) {
        return;
      }

      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0]?.isIntersecting) {
            this.triggerLoadMore();
          }
        },
        { rootMargin: '200px' },
      );

      observer.observe(element);
      onCleanup(() => observer.disconnect());
    });
  }

  protected triggerLoadMore(): void {
    if (this.loading() || !this.hasMore()) {
      return;
    }

    this.loadMore.emit();
  }
}
