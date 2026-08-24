import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { TopArtist } from '../dashboard-stats';
import { TopArtists } from './top-artists';

describe('TopArtists', () => {
  it('zeigt eine Textmeldung, wenn keine Artists vorhanden sind', () => {
    // arrange
    const fixture = TestBed.createComponent(TopArtists);
    fixture.componentRef.setInput('items', []);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Keine Daten vorhanden.');
  });

  it('zeigt Rang, Name und Anzahl je Artist an', () => {
    // arrange
    const items: TopArtist[] = [
      { artistId: 1, artistName: 'Pink Floyd', count: 5 },
      { artistId: 2, artistName: 'The Beatles', count: 3 },
    ];
    const fixture = TestBed.createComponent(TopArtists);
    fixture.componentRef.setInput('items', items);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('1');
    expect(compiled.textContent).toContain('Pink Floyd');
    expect(compiled.textContent).toContain('2');
    expect(compiled.textContent).toContain('The Beatles');
  });
});
