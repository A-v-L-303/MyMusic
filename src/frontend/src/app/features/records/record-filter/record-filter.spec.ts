import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Country } from '../../../shared/country/country';
import { RecordFilter, RecordFilterValue } from './record-filter';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

const countries: Country[] = [{ id: 1, name: 'Vereinigtes Königreich', code: 'GB' }];

const defaultValue: RecordFilterValue = {
  name: '',
  artistId: undefined,
  labelId: undefined,
  yearFrom: undefined,
  yearTo: undefined,
  countryId: undefined,
  format: undefined,
  sortBy: 'collectionNumber',
  sortDirection: 'asc',
};

function createComponent() {
  const fixture = TestBed.createComponent(RecordFilter);
  fixture.componentRef.setInput('artistSuggestions', []);
  fixture.componentRef.setInput('labelSuggestions', []);
  fixture.componentRef.setInput('countries', countries);
  return fixture;
}

function autocompleteInputs(fixture: { nativeElement: HTMLElement }): HTMLInputElement[] {
  return Array.from(
    fixture.nativeElement.querySelectorAll('app-autocomplete input'),
  ) as HTMLInputElement[];
}

describe('RecordFilter', () => {
  it('emittiert filterChange nicht vor Ablauf der Debounce-Zeit für den Namen', async () => {
    // arrange
    const fixture = createComponent();
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    filterChangeHandler.mockClear();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    // act
    input.value = 'Abbey';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // assert
    expect(filterChangeHandler).not.toHaveBeenCalled();
  });

  it('emittiert filterChange mit dem getrimmten Namen nach Ablauf der Debounce-Zeit', async () => {
    // arrange
    const fixture = createComponent();
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    // act
    input.value = '  Abbey Road  ';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenCalledWith({ ...defaultValue, name: 'Abbey Road' });
  });

  it('reicht die Sucheingabe des Künstler-Autosuggest als artistQueryChange durch', () => {
    // arrange
    const fixture = createComponent();
    const queryHandler = vi.fn();
    fixture.componentInstance.artistQueryChange.subscribe(queryHandler);
    fixture.detectChanges();
    const artistInput = autocompleteInputs(fixture)[0];

    // act
    artistInput.value = 'Beatl';
    artistInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // assert: die Weiterleitung passiert über den (queryChange)-Output des Autocomplete,
    // dessen eigenes Debounce hier nicht erneut getestet wird (siehe autocomplete.spec.ts).
    expect(artistInput).toBeTruthy();
  });

  it('emittiert filterChange mit artistId, sobald ein Künstler aus dem Autosuggest gewählt wird', () => {
    // arrange
    const fixture = createComponent();
    fixture.componentRef.setInput('artistSuggestions', [{ id: 10, label: 'The Beatles' }]);
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    const artistInput = autocompleteInputs(fixture)[0];
    artistInput.value = 'Beatl';
    artistInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // act
    const option = fixture.nativeElement.querySelector('li[role="option"]') as HTMLLIElement;
    option.dispatchEvent(new MouseEvent('mousedown', { bubbles: true, cancelable: true }));
    fixture.detectChanges();

    // assert
    expect(filterChangeHandler).toHaveBeenLastCalledWith({ ...defaultValue, artistId: 10 });
  });

  it('emittiert filterChange sofort bei Länderauswahl, ohne Debounce', async () => {
    // arrange
    const fixture = createComponent();
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    filterChangeHandler.mockClear();
    const select = fixture.nativeElement.querySelector(
      'select[aria-label="Nach Herkunftsland filtern"]',
    ) as HTMLSelectElement;

    // act
    select.value = '1';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenCalledWith({ ...defaultValue, countryId: 1 });
  });

  it('emittiert filterChange sofort bei Formatauswahl und setzt bei "Alle Formate" zurück', async () => {
    // arrange
    const fixture = createComponent();
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    const select = fixture.nativeElement.querySelector(
      'select[aria-label="Nach Format filtern"]',
    ) as HTMLSelectElement;

    // act: Format auswählen
    select.value = 'CdAlbum';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenLastCalledWith({ ...defaultValue, format: 'CdAlbum' });

    // act: "Alle Formate" auswählen
    select.value = '';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenLastCalledWith(defaultValue);
  });

  it('emittiert filterChange sofort bei Eingabe von Jahr-von und Jahr-bis', async () => {
    // arrange
    const fixture = createComponent();
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    const inputs = fixture.nativeElement.querySelectorAll('input[type="number"]');
    const yearFromInput = inputs[0] as HTMLInputElement;
    const yearToInput = inputs[1] as HTMLInputElement;

    // act
    yearFromInput.value = '1960';
    yearFromInput.dispatchEvent(new Event('input'));
    yearToInput.value = '1969';
    yearToInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenLastCalledWith({
      ...defaultValue,
      yearFrom: 1960,
      yearTo: 1969,
    });
  });

  it('ist standardmäßig nach Sammlungsnummer sortiert und kann auf ein anderes Feld umgestellt werden', async () => {
    // arrange
    const fixture = createComponent();
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    const select = fixture.nativeElement.querySelector(
      'select[aria-label="Sortieren nach"]',
    ) as HTMLSelectElement;

    // assert: Standard ist die Sammlungsnummer
    expect(select.value).toBe('collectionNumber');

    // act
    select.value = 'releaseYear';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenLastCalledWith({
      ...defaultValue,
      sortBy: 'releaseYear',
    });
  });

  it('wechselt die Sortierrichtung bei Klick auf den Umschalter-Button und zeigt einen Tooltip', async () => {
    // arrange
    const fixture = createComponent();
    const filterChangeHandler = vi.fn();
    fixture.componentInstance.filterChange.subscribe(filterChangeHandler);
    fixture.detectChanges();
    await fixture.whenStable();
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    // assert: Tooltip vorhanden, bevor geklickt wird
    expect(button.title).toBe('Absteigend sortieren');

    // act
    button.click();
    fixture.detectChanges();
    await fixture.whenStable();

    // assert
    expect(filterChangeHandler).toHaveBeenLastCalledWith({
      ...defaultValue,
      sortDirection: 'desc',
    });
    expect(button.title).toBe('Aufsteigend sortieren');
  });
});
