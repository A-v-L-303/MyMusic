import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { Records } from './records';

describe('Records', () => {
  it('zeigt den Feature-Titel und den Platzhaltertext', () => {
    // arrange
    const fixture = TestBed.createComponent(Records);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Records');
    expect(compiled.textContent).toContain('folgt in einem späteren Block');
  });
});
