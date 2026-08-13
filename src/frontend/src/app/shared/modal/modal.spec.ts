import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Modal } from './modal';

describe('Modal', () => {
  it('emittiert closed bei Klick auf den Scrim', () => {
    // arrange
    const fixture = TestBed.createComponent(Modal);
    fixture.detectChanges();
    const closedHandler = vi.fn();
    fixture.componentInstance.closed.subscribe(closedHandler);
    const scrim = (fixture.nativeElement as HTMLElement).querySelector('.scrim') as HTMLElement;

    // act
    scrim.dispatchEvent(new MouseEvent('click', { bubbles: true }));

    // assert
    expect(closedHandler).toHaveBeenCalledTimes(1);
  });

  it('emittiert closed nicht bei Klick innerhalb des Panels', () => {
    // arrange
    const fixture = TestBed.createComponent(Modal);
    fixture.detectChanges();
    const closedHandler = vi.fn();
    fixture.componentInstance.closed.subscribe(closedHandler);
    const panel = (fixture.nativeElement as HTMLElement).querySelector('.modal') as HTMLElement;

    // act
    panel.dispatchEvent(new MouseEvent('click', { bubbles: true }));

    // assert
    // Klick-Ziel ist das Panel selbst, nicht der Scrim — onScrimClick() prüft target === currentTarget.
    expect(closedHandler).not.toHaveBeenCalled();
  });

  it('emittiert closed bei Escape-Taste', () => {
    // arrange
    const fixture = TestBed.createComponent(Modal);
    fixture.detectChanges();
    const closedHandler = vi.fn();
    fixture.componentInstance.closed.subscribe(closedHandler);

    // act
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));

    // assert
    expect(closedHandler).toHaveBeenCalledTimes(1);
  });
});
