import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Record } from '../record';
import { RecordCard } from './record-card';

function buildRecord(overrides: Partial<Record> = {}): Record {
  return {
    id: 1,
    labelId: 1,
    labelName: 'Apple Records',
    artistId: 10,
    artistName: 'The Beatles',
    format: 'Album',
    albumName: 'Abbey Road',
    releaseYear: 1969,
    condition: 'Nm',
    information: null,
    albumCoverDataUrl: null,
    tracks: [],
    ...overrides,
  };
}

function createFixture(record: Record) {
  const fixture = TestBed.createComponent(RecordCard);
  fixture.componentRef.setInput('record', record);
  fixture.detectChanges();
  return fixture;
}

describe('RecordCard', () => {
  it('zeigt Albumname, Künstler, Jahr und Label', () => {
    // arrange
    const fixture = createFixture(buildRecord());

    // act
    const element = fixture.nativeElement as HTMLElement;

    // assert
    expect(element.querySelector('.record-title')?.textContent).toContain('Abbey Road');
    expect(element.querySelector('.record-artist')?.textContent).toContain('The Beatles');
    expect(element.querySelector('.record-sub')?.textContent).toContain('1969');
    expect(element.querySelector('.record-sub')?.textContent).toContain('Apple Records');
  });

  it('zeigt keinen Künstlerbereich, wenn kein Hauptkünstler vorhanden ist', () => {
    // arrange
    const fixture = createFixture(buildRecord({ artistId: null, artistName: null }));

    // act
    const element = fixture.nativeElement as HTMLElement;

    // assert
    expect(element.querySelector('.record-artist')).toBeNull();
  });

  it('zeigt das Platzhalter-Icon, wenn kein Cover vorhanden ist', () => {
    // arrange
    const fixture = createFixture(buildRecord({ albumCoverDataUrl: null }));

    // act
    const element = fixture.nativeElement as HTMLElement;

    // assert
    expect(element.querySelector('.record-cover img')).toBeNull();
    expect(element.querySelector('.record-cover svg')).not.toBeNull();
  });

  it('zeigt das Cover-Bild, wenn eine Data-URL vorhanden ist', () => {
    // arrange
    const fixture = createFixture(buildRecord({ albumCoverDataUrl: 'data:image/jpeg;base64,xyz' }));

    // act
    const image = (fixture.nativeElement as HTMLElement).querySelector(
      '.record-cover img',
    ) as HTMLImageElement;

    // assert
    expect(image).not.toBeNull();
    expect(image.getAttribute('src')).toBe('data:image/jpeg;base64,xyz');
  });

  it.each([
    ['Album', 'LP'],
    ['MaxiSingle', 'LP'],
    ['Single', 'LP'],
    ['Ep', 'LP'],
    ['Compilation', 'LP'],
    ['CdAlbum', 'CD'],
    ['CdMaxiSingle', 'CD'],
    ['CdSingle', 'CD'],
    ['CdEp', 'CD'],
    ['CdCompilation', 'CD'],
  ])('zeigt die Format-Pille "%s" -> "%s"', (format, expectedPill) => {
    // arrange
    const fixture = createFixture(buildRecord({ format: format as Record['format'] }));

    // act
    const pill = (fixture.nativeElement as HTMLElement).querySelector('.fmt');

    // assert
    expect(pill?.textContent?.trim()).toBe(expectedPill);
  });

  it('zeigt das Grade-Badge mit der zum Zustand passenden Klasse und Bezeichnung', () => {
    // arrange
    const fixture = createFixture(buildRecord({ condition: 'GPlus' }));

    // act
    const grade = (fixture.nativeElement as HTMLElement).querySelector('.grade') as HTMLElement;

    // assert
    expect(grade.classList.contains('grade-f')).toBe(true);
    expect(grade.textContent?.trim()).toBe('G+');
  });

  it('emittiert opened bei Klick auf die Karte', () => {
    // arrange
    const fixture = createFixture(buildRecord());
    const openedHandler = vi.fn();
    fixture.componentInstance.opened.subscribe(openedHandler);

    // act
    (fixture.nativeElement as HTMLElement)
      .querySelector('.record-card')
      ?.dispatchEvent(new Event('click', { bubbles: true }));

    // assert
    expect(openedHandler).toHaveBeenCalled();
  });
});
