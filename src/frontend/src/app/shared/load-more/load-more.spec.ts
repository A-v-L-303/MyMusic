import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { LoadMore } from './load-more';

class FakeIntersectionObserver implements IntersectionObserver {
  static instances: FakeIntersectionObserver[] = [];

  readonly root: Element | Document | null = null;
  readonly rootMargin = '';
  readonly scrollMargin = '';
  readonly thresholds: ReadonlyArray<number> = [];

  readonly observe = vi.fn();
  readonly unobserve = vi.fn();
  readonly disconnect = vi.fn();
  readonly takeRecords = vi.fn((): IntersectionObserverEntry[] => []);

  constructor(private readonly callback: IntersectionObserverCallback) {
    FakeIntersectionObserver.instances.push(this);
  }

  trigger(isIntersecting: boolean): void {
    this.callback([{ isIntersecting } as IntersectionObserverEntry], this);
  }
}

describe('LoadMore', () => {
  beforeEach(() => {
    FakeIntersectionObserver.instances = [];
    vi.stubGlobal('IntersectionObserver', FakeIntersectionObserver);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  function createFixture(hasMore: boolean, loading: boolean) {
    const fixture = TestBed.createComponent(LoadMore);
    fixture.componentRef.setInput('hasMore', hasMore);
    fixture.componentRef.setInput('loading', loading);
    fixture.detectChanges();
    return fixture;
  }

  it('rendert nichts, wenn keine weiteren Seiten vorhanden sind', () => {
    // arrange
    const fixture = createFixture(false, false);

    // act
    const button = (fixture.nativeElement as HTMLElement).querySelector('button');

    // assert
    expect(button).toBeNull();
  });

  it('rendert Sentinel und Button, wenn weitere Seiten vorhanden sind', () => {
    // arrange
    const fixture = createFixture(true, false);

    // act
    const root = fixture.nativeElement as HTMLElement;

    // assert
    expect(root.querySelector('[aria-hidden="true"]')).not.toBeNull();
    expect(root.querySelector('button')).not.toBeNull();
  });

  it('hat einen Tooltip am "Mehr laden"-Button', () => {
    // arrange
    const fixture = createFixture(true, false);

    // act
    const button = (fixture.nativeElement as HTMLElement).querySelector('button') as HTMLButtonElement;

    // assert
    expect(button.title).toBe('Weitere Einträge laden');
  });

  it('emittiert loadMore bei Klick auf den Button', () => {
    // arrange
    const fixture = createFixture(true, false);
    const loadMoreHandler = vi.fn();
    fixture.componentInstance.loadMore.subscribe(loadMoreHandler);
    const button = (fixture.nativeElement as HTMLElement).querySelector('button') as HTMLButtonElement;

    // act
    button.click();

    // assert
    expect(loadMoreHandler).toHaveBeenCalledTimes(1);
  });

  it('emittiert loadMore, wenn der Sentinel in den sichtbaren Bereich scrollt', () => {
    // arrange
    const fixture = createFixture(true, false);
    const loadMoreHandler = vi.fn();
    fixture.componentInstance.loadMore.subscribe(loadMoreHandler);

    // act
    FakeIntersectionObserver.instances[0].trigger(true);

    // assert
    expect(loadMoreHandler).toHaveBeenCalledTimes(1);
  });

  it('emittiert kein loadMore per Scroll, während bereits nachgeladen wird', () => {
    // arrange
    const fixture = createFixture(true, true);
    const loadMoreHandler = vi.fn();
    fixture.componentInstance.loadMore.subscribe(loadMoreHandler);

    // act
    FakeIntersectionObserver.instances[0].trigger(true);

    // assert
    expect(loadMoreHandler).not.toHaveBeenCalled();
  });

  it('deaktiviert den Button, während nachgeladen wird', () => {
    // arrange
    const fixture = createFixture(true, true);

    // act
    const button = (fixture.nativeElement as HTMLElement).querySelector('button') as HTMLButtonElement;

    // assert
    expect(button.disabled).toBe(true);
  });

  it('trennt den Observer, wenn keine weiteren Seiten mehr vorhanden sind', () => {
    // arrange
    const fixture = createFixture(true, false);
    const observer = FakeIntersectionObserver.instances[0];

    // act
    fixture.componentRef.setInput('hasMore', false);
    fixture.detectChanges();

    // assert
    expect(observer.disconnect).toHaveBeenCalledTimes(1);
  });
});
