import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { RecordTrack } from '../record';
import { TrackList } from './track-list';

function buildTrack(overrides: Partial<RecordTrack> = {}): RecordTrack {
  return {
    id: 1,
    recordId: 1,
    artistId: 1,
    artistName: 'Miles Davis',
    genreId: 1,
    genreName: 'Jazz',
    trackName: 'So What',
    recordSide: 'A',
    trackNumber: 1,
    information: null,
    ...overrides,
  };
}

function createFixture(tracks: RecordTrack[]) {
  const fixture = TestBed.createComponent(TrackList);
  fixture.componentRef.setInput('tracks', tracks);
  fixture.detectChanges();
  return fixture;
}

describe('TrackList', () => {
  it('zeigt einen Leerzustand ohne Tracks', () => {
    // arrange
    const fixture = createFixture([]);

    // act
    const element = fixture.nativeElement as HTMLElement;

    // assert
    expect(element.querySelector('.empty')?.textContent).toContain('Noch keine Tracks vorhanden');
  });

  it('gruppiert Tracks nach Seite mit Überschrift, wenn mehrere Seiten vorkommen', () => {
    // arrange
    const tracks = [
      buildTrack({ id: 1, recordSide: 'A', trackNumber: 1, trackName: 'So What' }),
      buildTrack({ id: 2, recordSide: 'B', trackNumber: 1, trackName: 'Blue in Green' }),
    ];

    // act
    const element = createFixture(tracks).nativeElement as HTMLElement;

    // assert
    expect(element.textContent).toContain('Seite A');
    expect(element.textContent).toContain('Seite B');
    expect(element.textContent).toContain('So What');
    expect(element.textContent).toContain('Blue in Green');
  });

  it('zeigt keine Seiten-Überschrift, wenn ausschließlich recordSide 0 vorkommt (CD)', () => {
    // arrange
    const tracks = [
      buildTrack({ id: 1, recordSide: '0', trackNumber: 1 }),
      buildTrack({ id: 2, recordSide: '0', trackNumber: 2 }),
    ];

    // act
    const element = createFixture(tracks).nativeElement as HTMLElement;

    // assert
    expect(element.textContent).not.toContain('Seite');
  });

  it('zeigt Künstler, Trackname und Information je Track', () => {
    // arrange
    const tracks = [
      buildTrack({
        trackName: 'So What',
        artistName: 'Miles Davis',
        information: 'Take 3',
      }),
    ];

    // act
    const element = createFixture(tracks).nativeElement as HTMLElement;

    // assert
    expect(element.textContent).toContain('Miles Davis · So What');
    expect(element.textContent).toContain('Take 3');
  });

  it('zeigt das Genre je Track rechts neben Künstler und Trackname', () => {
    // arrange
    const tracks = [buildTrack({ genreName: 'Jazz' })];

    // act
    const element = createFixture(tracks).nativeElement as HTMLElement;

    // assert
    expect(element.textContent).toContain('Jazz');
    expect(element.querySelector('.badge')?.getAttribute('title')).toBe('Genre des Tracks');
  });

  it('zeigt kein Informationsfeld, wenn keine Information hinterlegt ist', () => {
    // arrange
    const tracks = [buildTrack({ information: null })];

    // act
    const element = createFixture(tracks).nativeElement as HTMLElement;

    // assert
    expect(element.querySelector('.text-fg-subtle')).toBeNull();
  });

  it('emittiert editRequested mit dem angeklickten Track', () => {
    // arrange
    const track = buildTrack({ trackName: 'So What' });
    const fixture = createFixture([track]);
    let emitted: RecordTrack | undefined;
    fixture.componentInstance.editRequested.subscribe((value) => (emitted = value));

    // act
    (
      fixture.nativeElement.querySelector(
        '[aria-label="Track So What bearbeiten"]',
      ) as HTMLButtonElement
    ).click();

    // assert
    expect(emitted).toEqual(track);
  });

  it('hat Tooltips an Bearbeiten- und Löschen-Button', () => {
    // arrange
    const track = buildTrack({ trackName: 'So What' });
    const fixture = createFixture([track]);
    const compiled = fixture.nativeElement as HTMLElement;

    // act
    const editButton = compiled.querySelector(
      '[aria-label="Track So What bearbeiten"]',
    ) as HTMLButtonElement;
    const deleteButton = compiled.querySelector(
      '[aria-label="Track So What löschen"]',
    ) as HTMLButtonElement;

    // assert
    expect(editButton.title).toBe('Track So What bearbeiten');
    expect(deleteButton.title).toBe('Track So What löschen');
  });

  it('emittiert deleteRequested mit dem angeklickten Track', () => {
    // arrange
    const track = buildTrack({ trackName: 'So What' });
    const fixture = createFixture([track]);
    let emitted: RecordTrack | undefined;
    fixture.componentInstance.deleteRequested.subscribe((value) => (emitted = value));

    // act
    (
      fixture.nativeElement.querySelector(
        '[aria-label="Track So What löschen"]',
      ) as HTMLButtonElement
    ).click();

    // assert
    expect(emitted).toEqual(track);
  });
});
