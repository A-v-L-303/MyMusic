import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Label } from '../label';
import { LabelTable } from './label-table';

describe('LabelTable', () => {
  function createFixture(labels: Label[], loading: boolean, page = 1, totalPages = 1) {
    const fixture = TestBed.createComponent(LabelTable);
    fixture.componentRef.setInput('labels', labels);
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

  it('rendert eine Tabellenzeile je Label inkl. Ländername und Information', () => {
    // arrange
    const labels: Label[] = [
      {
        id: 1,
        name: 'Rough Trade',
        countryId: 1,
        countryName: 'Vereinigtes Königreich',
        information: 'Unabhängiges Label',
      },
      {
        id: 2,
        name: '4AD',
        countryId: 1,
        countryName: 'Vereinigtes Königreich',
        information: null,
      },
    ];

    // act
    const fixture = createFixture(labels, false);

    // assert
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Rough Trade');
    expect(rows[0].textContent).toContain('Vereinigtes Königreich');
    expect(rows[0].textContent).toContain('Unabhängiges Label');
  });

  it('zeigt lange Information gekürzt mit vollem Text als Tooltip', () => {
    // arrange
    const longInformation = 'x'.repeat(200);
    const labels: Label[] = [
      {
        id: 1,
        name: 'Rough Trade',
        countryId: 1,
        countryName: 'Vereinigtes Königreich',
        information: longInformation,
      },
    ];

    // act
    const fixture = createFixture(labels, false);
    const informationCell = (fixture.nativeElement as HTMLElement).querySelector(
      'tbody tr td:nth-child(3) span',
    ) as HTMLElement;

    // assert
    // truncate schneidet visuell per CSS, nicht per Textinhalt — deshalb steht der volle Text im
    // title-Attribut, damit er bei Bedarf per Tooltip vollständig einsehbar bleibt.
    expect(informationCell.getAttribute('title')).toBe(longInformation);
  });

  it('setzt kein title-Attribut, wenn keine Information vorhanden ist', () => {
    // arrange
    const labels: Label[] = [
      {
        id: 1,
        name: 'Rough Trade',
        countryId: 1,
        countryName: 'Vereinigtes Königreich',
        information: null,
      },
    ];

    // act
    const fixture = createFixture(labels, false);
    const informationCell = (fixture.nativeElement as HTMLElement).querySelector(
      'tbody tr td:nth-child(3) span',
    ) as HTMLElement;

    // assert
    expect(informationCell.hasAttribute('title')).toBe(false);
  });

  it('emittiert editRequested mit dem angeklickten Label', () => {
    // arrange
    const labels: Label[] = [
      {
        id: 1,
        name: 'Rough Trade',
        countryId: 1,
        countryName: 'Vereinigtes Königreich',
        information: null,
      },
    ];
    const fixture = createFixture(labels, false);
    const editHandler = vi.fn();
    fixture.componentInstance.editRequested.subscribe(editHandler);

    // act
    const editButton = (fixture.nativeElement as HTMLElement).querySelector(
      '[aria-label="Label Rough Trade bearbeiten"]',
    ) as HTMLButtonElement;
    editButton.click();

    // assert
    expect(editHandler).toHaveBeenCalledWith(labels[0]);
  });

  it('emittiert deleteRequested mit dem angeklickten Label', () => {
    // arrange
    const labels: Label[] = [
      {
        id: 1,
        name: 'Rough Trade',
        countryId: 1,
        countryName: 'Vereinigtes Königreich',
        information: null,
      },
    ];
    const fixture = createFixture(labels, false);
    const deleteHandler = vi.fn();
    fixture.componentInstance.deleteRequested.subscribe(deleteHandler);

    // act
    const deleteButton = (fixture.nativeElement as HTMLElement).querySelector(
      '[aria-label="Label Rough Trade löschen"]',
    ) as HTMLButtonElement;
    deleteButton.click();

    // assert
    expect(deleteHandler).toHaveBeenCalledWith(labels[0]);
  });

  it('reicht pageChange von der eingebetteten Paginierung durch', () => {
    // arrange
    const labels: Label[] = [
      {
        id: 1,
        name: 'Rough Trade',
        countryId: 1,
        countryName: 'Vereinigtes Königreich',
        information: null,
      },
    ];
    const fixture = createFixture(labels, false, 1, 3);
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
