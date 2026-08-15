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

  it('zeigt Trackname, Artist, Genre und Information je Track', () => {
    // arrange
    const tracks = [
      buildTrack({
        trackName: 'So What',
        artistName: 'Miles Davis',
        genreName: 'Jazz',
        information: 'Take 3',
      }),
    ];

    // act
    const element = createFixture(tracks).nativeElement as HTMLElement;

    // assert
    expect(element.textContent).toContain('So What');
    expect(element.textContent).toContain('Miles Davis');
    expect(element.textContent).toContain('Jazz');
    expect(element.textContent).toContain('Take 3');
  });

  it('zeigt kein Informationsfeld, wenn keine Information hinterlegt ist', () => {
    // arrange
    const tracks = [buildTrack({ information: null })];

    // act
    const element = createFixture(tracks).nativeElement as HTMLElement;

    // assert
    expect(element.querySelector('.text-fg-subtle')).toBeNull();
  });
});
