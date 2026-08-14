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

function createFixture(optionsInput: AutocompleteOption[] = []) {
  const fixture = TestBed.createComponent(Autocomplete);
  fixture.componentRef.setInput('placeholder', 'Nach Künstler filtern');
  fixture.componentRef.setInput('ariaLabel', 'Nach Künstler filtern');
  fixture.componentRef.setInput('options', optionsInput);
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
});
