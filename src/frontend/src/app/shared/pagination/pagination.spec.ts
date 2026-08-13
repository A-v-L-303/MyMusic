import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Pagination } from './pagination';

describe('Pagination', () => {
  function createFixture(page: number, totalPages: number) {
    const fixture = TestBed.createComponent(Pagination);
    fixture.componentRef.setInput('page', page);
    fixture.componentRef.setInput('totalPages', totalPages);
    fixture.detectChanges();
    return fixture;
  }

  it('rendert nichts, wenn nur eine Seite existiert', () => {
    // arrange
    const fixture = createFixture(1, 1);

    // act
    const nav = (fixture.nativeElement as HTMLElement).querySelector('nav');

    // assert
    expect(nav).toBeNull();
  });

  it('rendert eine Schaltfläche je Seite', () => {
    // arrange
    const fixture = createFixture(1, 3);

    // act
    const pageButtons = (fixture.nativeElement as HTMLElement).querySelectorAll(
      'button.btn-sm:not(.btn-icon)',
    );

    // assert
    expect(pageButtons.length).toBe(3);
  });

  it('deaktiviert Zurück auf der ersten und Vor auf der letzten Seite', () => {
    // arrange
    const fixture = createFixture(1, 3);

    // act
    const iconButtons = (fixture.nativeElement as HTMLElement).querySelectorAll('button.btn-icon');

    // assert
    expect((iconButtons[0] as HTMLButtonElement).disabled).toBe(true);
    expect((iconButtons[1] as HTMLButtonElement).disabled).toBe(false);
  });

  it('emittiert pageChange mit der Zielseite bei Klick auf eine Seitenzahl', () => {
    // arrange
    const fixture = createFixture(1, 3);
    const pageChangeHandler = vi.fn();
    fixture.componentInstance.pageChange.subscribe(pageChangeHandler);
    const pageButtons = (fixture.nativeElement as HTMLElement).querySelectorAll(
      'button.btn-sm:not(.btn-icon)',
    );

    // act
    (pageButtons[2] as HTMLButtonElement).click();

    // assert
    expect(pageChangeHandler).toHaveBeenCalledExactlyOnceWith(3);
  });

  it('emittiert pageChange nicht für die bereits aktive Seite', () => {
    // arrange
    const fixture = createFixture(2, 3);
    const pageChangeHandler = vi.fn();
    fixture.componentInstance.pageChange.subscribe(pageChangeHandler);
    const pageButtons = (fixture.nativeElement as HTMLElement).querySelectorAll(
      'button.btn-sm:not(.btn-icon)',
    );

    // act
    (pageButtons[1] as HTMLButtonElement).click();

    // assert
    expect(pageChangeHandler).not.toHaveBeenCalled();
  });

  it('emittiert pageChange bei Klick auf Vor/Zurück', () => {
    // arrange
    const fixture = createFixture(2, 3);
    const pageChangeHandler = vi.fn();
    fixture.componentInstance.pageChange.subscribe(pageChangeHandler);
    const iconButtons = (fixture.nativeElement as HTMLElement).querySelectorAll('button.btn-icon');

    // act
    (iconButtons[1] as HTMLButtonElement).click();

    // assert
    expect(pageChangeHandler).toHaveBeenCalledExactlyOnceWith(3);
  });
});
