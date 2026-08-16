import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { Admin } from './admin';

describe('Admin', () => {
  it('zeigt den Feature-Titel und den Platzhaltertext', () => {
    // arrange
    const fixture = TestBed.createComponent(Admin);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Admin');
    expect(compiled.textContent).toContain('folgt in einem späteren Block');
  });
});
