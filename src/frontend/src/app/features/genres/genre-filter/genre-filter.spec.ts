import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { GenreFilter } from './genre-filter';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

describe('GenreFilter', () => {
  it('emittiert filterChange nicht vor Ablauf der Debounce-Zeit', async () => {
    // arrange
    const fixture = TestBed.createComponent(GenreFilter);
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    filterChangeHandler.mockClear();
    const input = (fixture.nativeElement as HTMLElement).querySelector('input') as HTMLInputElement;

    // act
    input.value = 'Rock';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // assert
    expect(filterChangeHandler).not.toHaveBeenCalled();
  });

  it('emittiert filterChange mit dem getrimmten Namen nach Ablauf der Debounce-Zeit', async () => {
    // arrange
    const fixture = TestBed.createComponent(GenreFilter);
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    const input = (fixture.nativeElement as HTMLElement).querySelector('input') as HTMLInputElement;

    // act
    input.value = '  Rock  ';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenCalledWith('Rock');
  });
});
