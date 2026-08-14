import { Component, HostListener, OnDestroy, OnInit, output } from '@angular/core';

let openModalStack: Modal[] = [];

@Component({
  selector: 'app-modal',
  templateUrl: './modal.html',
})
export class Modal implements OnInit, OnDestroy {
  readonly closed = output<void>();

  ngOnInit(): void {
    openModalStack.push(this);
  }

  ngOnDestroy(): void {
    openModalStack = openModalStack.filter((modal) => modal !== this);
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    if (openModalStack[openModalStack.length - 1] === this) {
      this.closed.emit();
    }
  }

  protected onScrimClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closed.emit();
    }
  }
}
