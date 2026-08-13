import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { Labels } from './labels';

describe('Labels', () => {
  it('zeigt den Feature-Titel und den Platzhaltertext', () => {
    // arrange
    const fixture = TestBed.createComponent(Labels);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Labels');
    expect(compiled.textContent).toContain('folgt in einem späteren Block');
  });
});
