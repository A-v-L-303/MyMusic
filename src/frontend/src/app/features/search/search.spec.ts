import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { describe, expect, it } from 'vitest';

import { Search } from './search';

describe('Search', () => {
  it('zeigt den generischen Platzhaltertext ohne Suchbegriff', () => {
    // arrange
    TestBed.configureTestingModule({
      imports: [Search],
      providers: [
        { provide: ActivatedRoute, useValue: { queryParamMap: of(convertToParamMap({})) } },
      ],
    });
    const fixture = TestBed.createComponent(Search);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Search');
    expect(compiled.textContent).toContain('folgt in einem späteren Block');
  });

  it('zeigt den Suchbegriff aus dem q-Query-Parameter an', () => {
    // arrange
    TestBed.configureTestingModule({
      imports: [Search],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { queryParamMap: of(convertToParamMap({ q: 'jazz' })) },
        },
      ],
    });
    const fixture = TestBed.createComponent(Search);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('jazz');
  });
});
