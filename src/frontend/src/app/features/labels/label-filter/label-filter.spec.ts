import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Country } from '../country';
import { LabelFilter } from './label-filter';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

const countries: Country[] = [
  { id: 1, name: 'Deutschland', code: 'DE' },
  { id: 2, name: 'Vereinigtes Königreich', code: 'GB' },
];

describe('LabelFilter', () => {
  it('emittiert filterChange nicht vor Ablauf der Debounce-Zeit für den Namen', async () => {
    // arrange
    const fixture = TestBed.createComponent(LabelFilter);
    fixture.componentRef.setInput('countries', countries);
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    filterChangeHandler.mockClear();
    const input = (fixture.nativeElement as HTMLElement).querySelector('input') as HTMLInputElement;

    // act
    input.value = 'Rough';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // assert
    expect(filterChangeHandler).not.toHaveBeenCalled();
  });

  it('emittiert filterChange mit dem getrimmten Namen nach Ablauf der Debounce-Zeit', async () => {
    // arrange
    const fixture = TestBed.createComponent(LabelFilter);
    fixture.componentRef.setInput('countries', countries);
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    const input = (fixture.nativeElement as HTMLElement).querySelector('input') as HTMLInputElement;

    // act
    input.value = '  Rough Trade  ';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenCalledWith({ name: 'Rough Trade', countryId: undefined });
  });

  it('emittiert filterChange sofort bei Länderauswahl, ohne Debounce', async () => {
    // arrange
    const fixture = TestBed.createComponent(LabelFilter);
    fixture.componentRef.setInput('countries', countries);
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    filterChangeHandler.mockClear();
    const select = (fixture.nativeElement as HTMLElement).querySelector(
      'select',
    ) as HTMLSelectElement;

    // act
    select.value = '2';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenCalledWith({ name: '', countryId: 2 });
  });

  it('setzt countryId auf undefined zurück bei Auswahl von "Alle Länder"', async () => {
    // arrange
    const fixture = TestBed.createComponent(LabelFilter);
    fixture.componentRef.setInput('countries', countries);
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    const select = (fixture.nativeElement as HTMLElement).querySelector(
      'select',
    ) as HTMLSelectElement;
    select.value = '2';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();
    filterChangeHandler.mockClear();

    // act
    select.value = '';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenCalledWith({ name: '', countryId: undefined });
  });
});
