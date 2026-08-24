import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { TopLabel } from '../dashboard-stats';
import { TopLabels } from './top-labels';

describe('TopLabels', () => {
  it('zeigt eine Textmeldung, wenn keine Labels vorhanden sind', () => {
    // arrange
    const fixture = TestBed.createComponent(TopLabels);
    fixture.componentRef.setInput('items', []);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Keine Daten vorhanden.');
  });

  it('zeigt Rang, Name und Anzahl je Label an', () => {
    // arrange
    const items: TopLabel[] = [
      { labelId: 1, labelName: 'Apple Records', count: 5 },
      { labelId: 2, labelName: 'Harvest', count: 3 },
    ];
    const fixture = TestBed.createComponent(TopLabels);
    fixture.componentRef.setInput('items', items);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('1');
    expect(compiled.textContent).toContain('Apple Records');
    expect(compiled.textContent).toContain('2');
    expect(compiled.textContent).toContain('Harvest');
  });
});
