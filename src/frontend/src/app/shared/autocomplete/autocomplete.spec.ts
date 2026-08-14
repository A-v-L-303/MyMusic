import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Autocomplete, AutocompleteOption } from './autocomplete';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

const options: AutocompleteOption[] = [
  { id: 1, label: 'The Beatles' },
  { id: 2, label: 'Miles Davis' },
];

function createFixture(optionsInput: AutocompleteOption[] = [], initialQuery = '') {
  const fixture = TestBed.createComponent(Autocomplete);
  fixture.componentRef.setInput('placeholder', 'Nach Künstler filtern');
  fixture.componentRef.setInput('ariaLabel', 'Nach Künstler filtern');
  fixture.componentRef.setInput('options', optionsInput);
  fixture.componentRef.setInput('initialQuery', initialQuery);
  fixture.detectChanges();
  return fixture;
}

describe('Autocomplete', () => {
  it('emittiert queryChange erst nach Ablauf der Debounce-Zeit, getrimmt', async () => {
    // arrange
    const fixture = createFixture();
    const queryChangeHandler = vi.fn();
    fixture.componentInstance.queryChange.subscribe(queryChangeHandler);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    // act
    input.value = '  Beatl  ';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // assert
    expect(queryChangeHandler).not.toHaveBeenCalled();
    await wait(350);
    expect(queryChangeHandler).toHaveBeenCalledWith('Beatl');
  });

  it('emittiert selected(undefined) sofort, wenn das Feld geleert wird', () => {
    // arrange
    const fixture = createFixture(options);
    const selectedHandler = vi.fn();
    fixture.componentInstance.selected.subscribe(selectedHandler);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    // act
    input.value = '';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // assert
    expect(selectedHandler).toHaveBeenCalledWith(undefined);
  });

  it('zeigt die Vorschlagsliste, sobald Text eingegeben und Optionen vorhanden sind', () => {
    // arrange
    const fixture = createFixture(options);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    // act
    input.value = 'Beatl';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // assert
    const items = fixture.nativeElement.querySelectorAll('li[role="option"]');
    expect(items.length).toBe(2);
    expect(items[0].textContent).toContain('The Beatles');
  });

  it('wählt eine Option per Klick aus, übernimmt den Text und schließt die Liste', () => {
    // arrange
    const fixture = createFixture(options);
    const selectedHandler = vi.fn();
    fixture.componentInstance.selected.subscribe(selectedHandler);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'Beatl';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // act
    const firstOption = fixture.nativeElement.querySelector('li[role="option"]') as HTMLLIElement;
    firstOption.dispatchEvent(new MouseEvent('mousedown', { bubbles: true, cancelable: true }));
    fixture.detectChanges();

    // assert
    expect(selectedHandler).toHaveBeenCalledWith(options[0]);
    expect(input.value).toBe('The Beatles');
    expect(fixture.nativeElement.querySelector('li[role="option"]')).toBeNull();
  });

  it('navigiert mit den Pfeiltasten und wählt mit Enter die hervorgehobene Option aus', () => {
    // arrange
    const fixture = createFixture(options);
    const selectedHandler = vi.fn();
    fixture.componentInstance.selected.subscribe(selectedHandler);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'a';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // act
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true }));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true }));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
    fixture.detectChanges();

    // assert: zweimal ArrowDown ab Index -1 landet auf Index 1 (Miles Davis)
    expect(selectedHandler).toHaveBeenCalledWith(options[1]);
  });

  it('schließt die Liste bei Escape', () => {
    // arrange
    const fixture = createFixture(options);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'Beatl';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('li[role="option"]')).not.toBeNull();

    // act
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    fixture.detectChanges();

    // assert
    expect(fixture.nativeElement.querySelector('li[role="option"]')).toBeNull();
  });

  it('zeigt initialQuery beim Rendern im Eingabefeld an', () => {
    // arrange & act
    const fixture = createFixture([], 'Columbia');
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    // assert
    expect(input.value).toBe('Columbia');
  });

  it('behält eigene Eingabe bei, solange sich initialQuery nicht ändert', () => {
    // arrange
    const fixture = createFixture([], 'Columbia');
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    // act
    input.value = 'Warner';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // assert: erneutes Setzen desselben initialQuery-Werts überschreibt die Eingabe nicht
    fixture.componentRef.setInput('initialQuery', 'Columbia');
    fixture.detectChanges();
    expect(input.value).toBe('Warner');
  });

  it('emittiert blur mit dem aktuellen Eingabetext', () => {
    // arrange
    const fixture = createFixture();
    const blurHandler = vi.fn();
    fixture.componentInstance.blur.subscribe(blurHandler);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'Neues Label';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // act
    input.dispatchEvent(new Event('blur'));

    // assert
    expect(blurHandler).toHaveBeenCalledWith('Neues Label');
  });

  it('setQuery() setzt den Anzeigetext programmatisch und schließt die Vorschlagsliste', () => {
    // arrange
    const fixture = createFixture(options);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'Beatl';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('li[role="option"]')).not.toBeNull();

    // act
    fixture.componentInstance.setQuery('Miles Davis');
    fixture.detectChanges();

    // assert
    expect(input.value).toBe('Miles Davis');
    expect(fixture.nativeElement.querySelector('li[role="option"]')).toBeNull();
  });

  it('setQuery() kann den Anzeigetext auch leeren', () => {
    // arrange
    const fixture = createFixture([], 'Columbia');
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    // act
    fixture.componentInstance.setQuery('');
    fixture.detectChanges();

    // assert
    expect(input.value).toBe('');
  });
});
