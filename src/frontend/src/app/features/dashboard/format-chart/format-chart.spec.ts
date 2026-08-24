import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { FormatCount } from '../dashboard-stats';
import { FormatChart } from './format-chart';

describe('FormatChart', () => {
  it('zeigt eine Textmeldung, wenn keine Formate vorhanden sind', () => {
    // arrange
    const fixture = TestBed.createComponent(FormatChart);
    fixture.componentRef.setInput('items', []);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Keine Daten vorhanden.');
  });

  it('zeigt jedes Format mit sprechendem Label und Anzahl an', () => {
    // arrange
    const items: FormatCount[] = [
      { format: 'Album', count: 3 },
      { format: 'CdAlbum', count: 1 },
    ];
    const fixture = TestBed.createComponent(FormatChart);
    fixture.componentRef.setInput('items', items);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Album');
    expect(compiled.textContent).toContain('CD-Album');
    expect(compiled.textContent).toContain('3');
    expect(compiled.textContent).toContain('1');
  });
});
