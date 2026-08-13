import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { Artists } from './artists';

describe('Artists', () => {
  it('zeigt den Feature-Titel und den Platzhaltertext', () => {
    // arrange
    const fixture = TestBed.createComponent(Artists);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Artists');
    expect(compiled.textContent).toContain('folgt in einem späteren Block');
  });
});
