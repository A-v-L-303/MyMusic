import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { StatTile } from './stat-tile';

describe('StatTile', () => {
  it('zeigt Wert und Label an', () => {
    // arrange
    const fixture = TestBed.createComponent(StatTile);
    fixture.componentRef.setInput('label', 'Records');
    fixture.componentRef.setInput('value', 42);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('42');
    expect(compiled.textContent).toContain('Records');
  });
});
