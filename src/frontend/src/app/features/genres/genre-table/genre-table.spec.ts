import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Genre } from '../genre';
import { GenreTable } from './genre-table';

describe('GenreTable', () => {
  function createFixture(genres: Genre[], loading: boolean, page = 1, totalPages = 1) {
    const fixture = TestBed.createComponent(GenreTable);
    fixture.componentRef.setInput('genres', genres);
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

  it('rendert eine Tabellenzeile je Genre', () => {
    // arrange
    const genres: Genre[] = [
      { id: 1, name: 'Rock' },
      { id: 2, name: 'Jazz' },
    ];

    // act
    const fixture = createFixture(genres, false);

    // assert
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Rock');
  });

  it('emittiert editRequested mit dem angeklickten Genre', () => {
    // arrange
    const genres: Genre[] = [{ id: 1, name: 'Rock' }];
    const fixture = createFixture(genres, false);
    const editHandler = vi.fn();
    fixture.componentInstance.editRequested.subscribe(editHandler);

    // act
    const editButton = (fixture.nativeElement as HTMLElement).querySelector(
      '[aria-label="Genre Rock bearbeiten"]',
    ) as HTMLButtonElement;
    editButton.click();

    // assert
    expect(editHandler).toHaveBeenCalledWith(genres[0]);
  });

  it('emittiert deleteRequested mit dem angeklickten Genre', () => {
    // arrange
    const genres: Genre[] = [{ id: 1, name: 'Rock' }];
    const fixture = createFixture(genres, false);
    const deleteHandler = vi.fn();
    fixture.componentInstance.deleteRequested.subscribe(deleteHandler);

    // act
    const deleteButton = (fixture.nativeElement as HTMLElement).querySelector(
      '[aria-label="Genre Rock löschen"]',
    ) as HTMLButtonElement;
    deleteButton.click();

    // assert
    expect(deleteHandler).toHaveBeenCalledWith(genres[0]);
  });

  it('hat Tooltips an Bearbeiten- und Löschen-Button', () => {
    // arrange
    const genres: Genre[] = [{ id: 1, name: 'Rock' }];
    const fixture = createFixture(genres, false);
    const compiled = fixture.nativeElement as HTMLElement;

    // act
    const editButton = compiled.querySelector(
      '[aria-label="Genre Rock bearbeiten"]',
    ) as HTMLButtonElement;
    const deleteButton = compiled.querySelector(
      '[aria-label="Genre Rock löschen"]',
    ) as HTMLButtonElement;

    // assert
    expect(editButton.title).toBe('Genre Rock bearbeiten');
    expect(deleteButton.title).toBe('Genre Rock löschen');
  });

  it('reicht pageChange von der eingebetteten Paginierung durch', () => {
    // arrange
    const genres: Genre[] = [{ id: 1, name: 'Rock' }];
    const fixture = createFixture(genres, false, 1, 3);
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
