import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { Dashboard } from './dashboard';

describe('Dashboard', () => {
  it('zeigt den Feature-Titel und den Platzhaltertext', () => {
    // arrange
    const fixture = TestBed.createComponent(Dashboard);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Dashboard');
    expect(compiled.textContent).toContain('folgt in einem späteren Block');
  });
});
