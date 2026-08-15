import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { ArtistService } from '../../artists/artist.service';
import { GenreService } from '../../genres/genre.service';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { RecordTrack } from '../record';
import { RecordService } from '../record.service';
import { TrackForm } from './track-form';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

const existingTrack: RecordTrack = {
  id: 7,
  recordId: 5,
  artistId: 2,
  artistName: 'Miles Davis',
  genreId: 3,
  genreName: 'Jazz',
  trackName: 'So What',
  recordSide: 'A',
  trackNumber: 1,
  information: 'Take 3',
};

describe('TrackForm', () => {
  let recordServiceMock: {
    createTrack: ReturnType<typeof vi.fn>;
    updateTrack: ReturnType<typeof vi.fn>;
  };
  let artistServiceMock: { getPaged: ReturnType<typeof vi.fn> };
  let genreServiceMock: { getAll: ReturnType<typeof vi.fn> };
  let errorModalServiceMock: { showFromHttpError: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    recordServiceMock = { createTrack: vi.fn(), updateTrack: vi.fn() };
    artistServiceMock = {
      getPaged: vi.fn().mockReturnValue(
        of({
          items: [{ id: 2, name: 'Miles Davis' }],
          totalCount: 1,
          page: 1,
          pageSize: 10,
          totalPages: 1,
        }),
      ),
    };
    genreServiceMock = {
      getAll: vi.fn().mockReturnValue(
        of([
          { id: 3, name: 'Jazz' },
          { id: 4, name: 'Rock' },
        ]),
      ),
    };
    errorModalServiceMock = { showFromHttpError: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: RecordService, useValue: recordServiceMock },
        { provide: ArtistService, useValue: artistServiceMock },
        { provide: GenreService, useValue: genreServiceMock },
        { provide: ErrorModalService, useValue: errorModalServiceMock },
      ],
    });
  });

  function createFixture(track: RecordTrack | null = null, recordId = 5) {
    const fixture = TestBed.createComponent(TrackForm);
    fixture.componentRef.setInput('recordId', recordId);

    if (track) {
      fixture.componentRef.setInput('track', track);
    }

    fixture.detectChanges();
    return fixture;
  }

  function compiled(fixture: ReturnType<typeof createFixture>): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  async function selectArtistViaAutocomplete(
    fixture: ReturnType<typeof createFixture>,
    query: string,
  ): Promise<void> {
    const input = compiled(fixture).querySelector('app-autocomplete input') as HTMLInputElement;
    input.value = query;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    await fixture.whenStable();
    fixture.detectChanges();
    const option = compiled(fixture).querySelector('li[role="option"]') as HTMLLIElement;
    option.dispatchEvent(new MouseEvent('mousedown', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }

  function selectGenre(fixture: ReturnType<typeof createFixture>, value: string): void {
    const select = compiled(fixture).querySelector('#track-genre') as HTMLSelectElement;
    select.value = value;
    select.dispatchEvent(new Event('input'));
    select.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function typeTrackName(fixture: ReturnType<typeof createFixture>, value: string): void {
    const input = compiled(fixture).querySelector('#track-name') as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function typeRecordSide(fixture: ReturnType<typeof createFixture>, value: string): void {
    const input = compiled(fixture).querySelector('#track-side') as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function typeTrackNumber(fixture: ReturnType<typeof createFixture>, value: string): void {
    const input = compiled(fixture).querySelector('#track-number') as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function typeInformation(fixture: ReturnType<typeof createFixture>, value: string): void {
    const textarea = compiled(fixture).querySelector('#track-information') as HTMLTextAreaElement;
    textarea.value = value;
    textarea.dispatchEvent(new Event('input'));
    textarea.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function submitForm(fixture: ReturnType<typeof createFixture>): void {
    const form = compiled(fixture).querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit', { cancelable: true }));
  }

  function errorHints(fixture: ReturnType<typeof createFixture>): string[] {
    return Array.from(compiled(fixture).querySelectorAll('.hint.is-error')).map(
      (element) => element.textContent ?? '',
    );
  }

  async function fillValidTrack(fixture: ReturnType<typeof createFixture>): Promise<void> {
    await selectArtistViaAutocomplete(fixture, 'Miles');
    selectGenre(fixture, '3');
    typeTrackName(fixture, 'So What');
    typeTrackNumber(fixture, '1');
  }

  it('zeigt im Bearbeiten-Modus alle Felder vorbefüllt', async () => {
    // arrange & act
    const fixture = createFixture(existingTrack);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    const artistInput = compiled(fixture).querySelector(
      'app-autocomplete input',
    ) as HTMLInputElement;
    expect(artistInput.value).toBe('Miles Davis');
    expect((compiled(fixture).querySelector('#track-genre') as HTMLSelectElement).value).toBe('3');
    expect((compiled(fixture).querySelector('#track-name') as HTMLInputElement).value).toBe(
      'So What',
    );
    expect((compiled(fixture).querySelector('#track-side') as HTMLInputElement).value).toBe('A');
    expect((compiled(fixture).querySelector('#track-number') as HTMLInputElement).value).toBe('1');
    expect(
      (compiled(fixture).querySelector('#track-information') as HTMLTextAreaElement).value,
    ).toBe('Take 3');
  });

  it('setzt die Seite standardmäßig auf 0, wenn kein Track übergeben wird', () => {
    // arrange & act
    const fixture = createFixture();

    // assert
    expect((compiled(fixture).querySelector('#track-side') as HTMLInputElement).value).toBe('0');
  });

  it('zeigt einen Pflichtfeld-Fehler bei leerem Trackname nach Blur', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeTrackName(fixture, '');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('erforderlich'))).toBe(true);
  });

  it('zeigt einen Fehler bei zu langem Trackname', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeTrackName(fixture, 'A'.repeat(151));

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('höchstens 150'))).toBe(true);
  });

  it('zeigt einen Fehler bei einem nicht erlaubten Zeichen im Trackname', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeTrackName(fixture, 'So What #1');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('Buchstaben, Zahlen'))).toBe(true);
  });

  it('erlaubt Klammern im Trackname', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeTrackName(fixture, 'So What (Live)');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('Buchstaben, Zahlen'))).toBe(false);
  });

  it('zeigt einen Fehler bei einer Seite mit Sonderzeichen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeRecordSide(fixture, 'A#');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('Buchstaben oder Ziffern'))).toBe(true);
  });

  it('zeigt einen Fehler bei einer zu langen Seite', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeRecordSide(fixture, 'ABCD');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('höchstens 3'))).toBe(true);
  });

  it('zeigt einen Pflichtfeld-Fehler bei leerer Tracknummer nach Blur', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeTrackNumber(fixture, '');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('erforderlich'))).toBe(true);
  });

  it('zeigt einen Fehler bei einer Tracknummer kleiner als 1', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeTrackNumber(fixture, '0');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('mindestens 1'))).toBe(true);
  });

  it('zeigt nach einem Submit-Versuch ohne Künstlerauswahl einen Pflichtfeld-Fehler', () => {
    // arrange
    const fixture = createFixture();

    // act
    submitForm(fixture);
    fixture.detectChanges();

    // assert
    expect(recordServiceMock.createTrack).not.toHaveBeenCalled();
    expect(errorHints(fixture).some((hint) => hint.includes('Künstler'))).toBe(true);
  });

  it('zeigt bei leerer Genre-Auswahl nach Blur einen Pflichtfeld-Fehler', () => {
    // arrange
    const fixture = createFixture();

    // act
    selectGenre(fixture, '');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('Genre ist erforderlich'))).toBe(true);
  });

  it('sendet beim Anlegen alle Felder an RecordService.createTrack mit der Record-Id', async () => {
    // arrange
    recordServiceMock.createTrack.mockReturnValue(of({ ...existingTrack, id: 9 }));
    const fixture = createFixture(null, 5);
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidTrack(fixture);
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.createTrack).toHaveBeenCalledWith(5, {
      artistId: 2,
      genreId: 3,
      trackName: 'So What',
      recordSide: '0',
      trackNumber: 1,
      information: null,
    });
    expect(savedHandler).toHaveBeenCalledTimes(1);
  });

  it('ruft im Bearbeiten-Modus RecordService.updateTrack mit Record- und Track-Id auf', async () => {
    // arrange
    recordServiceMock.updateTrack.mockReturnValue(of(existingTrack));
    const fixture = createFixture(existingTrack, 5);
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.updateTrack).toHaveBeenCalledWith(5, 7, {
      artistId: 2,
      genreId: 3,
      trackName: 'So What',
      recordSide: 'A',
      trackNumber: 1,
      information: 'Take 3',
    });
    expect(savedHandler).toHaveBeenCalledTimes(1);
  });

  it('wandelt eine leere Information in null um', async () => {
    // arrange
    recordServiceMock.createTrack.mockReturnValue(of({ ...existingTrack, id: 9 }));
    const fixture = createFixture();

    // act
    await fillValidTrack(fixture);
    typeInformation(fixture, '   ');
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.createTrack).toHaveBeenCalledWith(
      5,
      expect.objectContaining({ information: null }),
    );
  });

  it('hängt eine 400-Serverantwort für TrackName inline ins Trackname-Feld ein, ohne das ErrorModal zu öffnen', async () => {
    // arrange
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        errors: { TrackName: ['Der Trackname ist bereits vergeben.'] },
        title: 'Validierungsfehler',
        status: 400,
      },
    });
    recordServiceMock.createTrack.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidTrack(fixture);
    submitForm(fixture);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('bereits vergeben'))).toBe(true);
    expect(errorModalServiceMock.showFromHttpError).not.toHaveBeenCalled();
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('leitet einen 409-Konflikt (doppelte Seite/Nummer) an den ErrorModalService weiter, ohne saved zu emittieren', async () => {
    // arrange
    const error = new HttpErrorResponse({ status: 409 });
    recordServiceMock.createTrack.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidTrack(fixture);
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(error, 'Track');
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('leitet einen 404-Fehler an den ErrorModalService weiter, ohne saved zu emittieren', async () => {
    // arrange
    const error = new HttpErrorResponse({ status: 404 });
    recordServiceMock.createTrack.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidTrack(fixture);
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(error, 'Track');
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('emittiert cancelled beim Abbrechen', () => {
    // arrange
    const fixture = createFixture();
    const cancelledHandler = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelledHandler);

    // act
    (compiled(fixture).querySelector('.btn-secondary') as HTMLButtonElement).click();

    // assert
    expect(cancelledHandler).toHaveBeenCalledTimes(1);
  });
});
