import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { Genres } from './genres';

describe('Genres', () => {
  it('zeigt den Feature-Titel und den Platzhaltertext', () => {
    // arrange
    const fixture = TestBed.createComponent(Genres);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Genres');
    expect(compiled.textContent).toContain('folgt in einem späteren Block');
  });
});
