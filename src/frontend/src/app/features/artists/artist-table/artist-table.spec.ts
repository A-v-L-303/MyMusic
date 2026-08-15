import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Artist } from '../artist';
import { ArtistTable } from './artist-table';

describe('ArtistTable', () => {
  function createFixture(artists: Artist[], loading: boolean, page = 1, totalPages = 1) {
    const fixture = TestBed.createComponent(ArtistTable);
    fixture.componentRef.setInput('artists', artists);
    fixture.componentRef.setInput('loading', loading);
    fixture.componentRef.setInput('page', page);
    fixture.componentRef.setInput('totalPages', totalPages);
    fixture.detectChanges();
    return fixture;
  }

  it('zeigt den Spinner während loading', () => {
    // arrange
    const fixture = createFixture([], true);

    // act
    const spinner = (fixture.nativeElement as HTMLElement).querySelector('.spinner');

    // assert
    expect(spinner).not.toBeNull();
  });

  it('zeigt den Empty State ohne Datensätze', () => {
    // arrange
    const fixture = createFixture([], false);

    // act
    const empty = (fixture.nativeElement as HTMLElement).querySelector('.empty');

    // assert
    expect(empty?.textContent).toContain('Keine Daten vorhanden');
  });

  it('rendert eine Tabellenzeile je Artist', () => {
    // arrange
    const artists: Artist[] = [
      { id: 1, name: 'Miles Davis' },
      { id: 2, name: 'AC/DC' },
    ];

    // act
    const fixture = createFixture(artists, false);

    // assert
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Miles Davis');
  });

  it('emittiert editRequested mit dem angeklickten Artist', () => {
    // arrange
    const artists: Artist[] = [{ id: 1, name: 'Miles Davis' }];
    const fixture = createFixture(artists, false);
    const editHandler = vi.fn();
    fixture.componentInstance.editRequested.subscribe(editHandler);

    // act
    const editButton = (fixture.nativeElement as HTMLElement).querySelector(
      '[aria-label="Artist Miles Davis bearbeiten"]',
    ) as HTMLButtonElement;
    editButton.click();

    // assert
    expect(editHandler).toHaveBeenCalledWith(artists[0]);
  });

  it('emittiert deleteRequested mit dem angeklickten Artist', () => {
    // arrange
    const artists: Artist[] = [{ id: 1, name: 'Miles Davis' }];
    const fixture = createFixture(artists, false);
    const deleteHandler = vi.fn();
    fixture.componentInstance.deleteRequested.subscribe(deleteHandler);

    // act
    const deleteButton = (fixture.nativeElement as HTMLElement).querySelector(
      '[aria-label="Artist Miles Davis löschen"]',
    ) as HTMLButtonElement;
    deleteButton.click();

    // assert
    expect(deleteHandler).toHaveBeenCalledWith(artists[0]);
  });

  it('hat Tooltips an Bearbeiten- und Löschen-Button', () => {
    // arrange
    const artists: Artist[] = [{ id: 1, name: 'Miles Davis' }];
    const fixture = createFixture(artists, false);
    const compiled = fixture.nativeElement as HTMLElement;

    // act
    const editButton = compiled.querySelector(
      '[aria-label="Artist Miles Davis bearbeiten"]',
    ) as HTMLButtonElement;
    const deleteButton = compiled.querySelector(
      '[aria-label="Artist Miles Davis löschen"]',
    ) as HTMLButtonElement;

    // assert
    expect(editButton.title).toBe('Artist Miles Davis bearbeiten');
    expect(deleteButton.title).toBe('Artist Miles Davis löschen');
  });

  it('reicht pageChange von der eingebetteten Paginierung durch', () => {
    // arrange
    const artists: Artist[] = [{ id: 1, name: 'Miles Davis' }];
    const fixture = createFixture(artists, false, 1, 3);
    const pageChangeHandler = vi.fn();
    fixture.componentInstance.pageChange.subscribe(pageChangeHandler);

    // act
    const pageButtons = (fixture.nativeElement as HTMLElement).querySelectorAll(
      'button.btn-sm:not(.btn-icon)',
    );
    (pageButtons[2] as HTMLButtonElement).click();

    // assert
    expect(pageChangeHandler).toHaveBeenCalledWith(3);
  });
});
