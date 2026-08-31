import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { ArtistService } from '../../artists/artist.service';
import { GenreService } from '../../genres/genre.service';
import { Label } from '../../labels/label';
import { LabelService } from '../../labels/label.service';
import { CountryService } from '../../../shared/country/country.service';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { DiscogsRelease } from '../discogs';
import { DiscogsSearch } from '../discogs-search/discogs-search';
import { DiscogsService } from '../discogs.service';
import { Record } from '../record';
import { RecordService } from '../record.service';
import { RecordForm } from './record-form';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

const existingRecord: Record = {
  id: 5,
  collectionNumber: 5,
  labelId: 1,
  labelName: 'Columbia',
  artistId: 2,
  artistName: 'Miles Davis',
  format: 'Album',
  albumName: 'Kind of Blue',
  releaseYear: 1959,
  condition: 'Vg',
  information: 'Erstpressung',
  albumCoverDataUrl: null,
  tracks: [],
};

describe('RecordForm', () => {
  let recordServiceMock: {
    create: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    uploadCover: ReturnType<typeof vi.fn>;
    createTrack: ReturnType<typeof vi.fn>;
  };
  let labelServiceMock: {
    getPaged: ReturnType<typeof vi.fn>;
    getAll: ReturnType<typeof vi.fn>;
    create: ReturnType<typeof vi.fn>;
  };
  let artistServiceMock: {
    getPaged: ReturnType<typeof vi.fn>;
    getAll: ReturnType<typeof vi.fn>;
    create: ReturnType<typeof vi.fn>;
  };
  let genreServiceMock: { getAll: ReturnType<typeof vi.fn>; create: ReturnType<typeof vi.fn> };
  let countryServiceMock: { getAll: ReturnType<typeof vi.fn> };
  let discogsServiceMock: {
    search: ReturnType<typeof vi.fn>;
    getRelease: ReturnType<typeof vi.fn>;
  };
  let errorModalServiceMock: {
    showFromHttpError: ReturnType<typeof vi.fn>;
    showValidationMessage: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    recordServiceMock = {
      create: vi.fn(),
      update: vi.fn(),
      uploadCover: vi.fn(),
      createTrack: vi.fn(),
    };
    labelServiceMock = {
      getPaged: vi.fn().mockReturnValue(
        of({
          items: [{ id: 1, name: 'Columbia', countryId: 1, countryName: 'USA', information: null }],
          totalCount: 1,
          page: 1,
          pageSize: 10,
          totalPages: 1,
        }),
      ),
      getAll: vi
        .fn()
        .mockReturnValue(
          of([{ id: 1, name: 'Columbia', countryId: 1, countryName: 'USA', information: null }]),
        ),
      create: vi.fn(),
    };
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
      getAll: vi.fn().mockReturnValue(of([{ id: 2, name: 'Miles Davis' }])),
      create: vi.fn(),
    };
    genreServiceMock = { getAll: vi.fn().mockReturnValue(of([])), create: vi.fn() };
    countryServiceMock = { getAll: vi.fn().mockReturnValue(of([])) };
    discogsServiceMock = { search: vi.fn(() => of([])), getRelease: vi.fn() };
    errorModalServiceMock = { showFromHttpError: vi.fn(), showValidationMessage: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: RecordService, useValue: recordServiceMock },
        { provide: LabelService, useValue: labelServiceMock },
        { provide: ArtistService, useValue: artistServiceMock },
        { provide: GenreService, useValue: genreServiceMock },
        { provide: CountryService, useValue: countryServiceMock },
        { provide: DiscogsService, useValue: discogsServiceMock },
        { provide: ErrorModalService, useValue: errorModalServiceMock },
      ],
    });
  });

  function createFixture(record: Record | null = null) {
    const fixture = TestBed.createComponent(RecordForm);

    if (record) {
      fixture.componentRef.setInput('record', record);
    }

    fixture.detectChanges();
    return fixture;
  }

  function compiled(fixture: ReturnType<typeof createFixture>): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  async function selectViaAutocomplete(
    fixture: ReturnType<typeof createFixture>,
    index: number,
    query: string,
  ): Promise<void> {
    const input = compiled(fixture).querySelectorAll('app-autocomplete input')[
      index
    ] as HTMLInputElement;
    input.value = query;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    await fixture.whenStable();
    fixture.detectChanges();
    const option = compiled(fixture).querySelectorAll('li[role="option"]')[0] as HTMLLIElement;
    option.dispatchEvent(new MouseEvent('mousedown', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }

  function selectFormat(fixture: ReturnType<typeof createFixture>, value: string): void {
    const select = compiled(fixture).querySelector('#record-format') as HTMLSelectElement;
    select.value = value;
    select.dispatchEvent(new Event('input'));
    select.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function typeAlbumName(fixture: ReturnType<typeof createFixture>, value: string): void {
    const input = compiled(fixture).querySelector('#record-album-name') as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function typeReleaseYear(fixture: ReturnType<typeof createFixture>, value: string): void {
    const input = compiled(fixture).querySelector('#record-release-year') as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function selectCondition(fixture: ReturnType<typeof createFixture>, value: string): void {
    const select = compiled(fixture).querySelector('#record-condition') as HTMLSelectElement;
    select.value = value;
    select.dispatchEvent(new Event('input'));
    select.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function typeInformation(fixture: ReturnType<typeof createFixture>, value: string): void {
    const textarea = compiled(fixture).querySelector('#record-information') as HTMLTextAreaElement;
    textarea.value = value;
    textarea.dispatchEvent(new Event('input'));
    textarea.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function selectCoverFile(fixture: ReturnType<typeof createFixture>, file: File | null): void {
    const input = compiled(fixture).querySelector('input[type="file"]') as HTMLInputElement;
    Object.defineProperty(input, 'files', { value: file ? [file] : [], configurable: true });
    input.dispatchEvent(new Event('change'));
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

  async function fillValidRecord(fixture: ReturnType<typeof createFixture>): Promise<void> {
    await selectViaAutocomplete(fixture, 0, 'Columbia');
    selectFormat(fixture, 'Album');
    typeAlbumName(fixture, 'Kind of Blue');
    typeReleaseYear(fixture, '1959');
  }

  async function applyDiscogsRelease(
    fixture: ReturnType<typeof createFixture>,
    release: DiscogsRelease,
  ): Promise<void> {
    (
      compiled(fixture).querySelector(
        '[title="Nach Discogs-Metadaten suchen"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();
    const discogsSearch = fixture.debugElement.query(By.directive(DiscogsSearch))
      .componentInstance as DiscogsSearch;
    discogsSearch.applied.emit(release);
    await fixture.whenStable();
    fixture.detectChanges();
  }

  function confirmButtonWithLabel(
    fixture: ReturnType<typeof createFixture>,
    label: string,
  ): HTMLButtonElement | undefined {
    return Array.from(compiled(fixture).querySelectorAll('button')).find(
      (button) => button.textContent?.trim() === label,
    ) as HTMLButtonElement | undefined;
  }

  const nevermindRelease: DiscogsRelease = {
    id: 100,
    title: 'Nevermind',
    year: 1991,
    artists: ['Miles Davis'],
    labels: ['Columbia'],
    genres: ['Jazz'],
    styles: [],
    formats: [],
    coverImageUrl: null,
    tracklist: [
      { position: 'A1', title: 'So What', duration: '9:22', artist: null },
      { position: 'A2', title: 'Freddie Freeloader', duration: '9:46', artist: null },
    ],
    country: null,
  };

  it('zeigt im Bearbeiten-Modus Label/Künstler-Namen in den Autocomplete-Feldern vorbefüllt', () => {
    // arrange & act
    const fixture = createFixture(existingRecord);
    const inputs = compiled(fixture).querySelectorAll('app-autocomplete input');

    // assert
    expect((inputs[0] as HTMLInputElement).value).toBe('Columbia');
    expect((inputs[1] as HTMLInputElement).value).toBe('Miles Davis');
  });

  it('übernimmt im Bearbeiten-Modus die bestehenden Werte in Format, Albumname, Jahr, Zustand und Information', async () => {
    // arrange & act
    const fixture = createFixture(existingRecord);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect((compiled(fixture).querySelector('#record-format') as HTMLSelectElement).value).toBe(
      'Album',
    );
    expect((compiled(fixture).querySelector('#record-album-name') as HTMLInputElement).value).toBe(
      'Kind of Blue',
    );
    expect(
      (compiled(fixture).querySelector('#record-release-year') as HTMLInputElement).value,
    ).toBe('1959');
    expect((compiled(fixture).querySelector('#record-condition') as HTMLSelectElement).value).toBe(
      'Vg',
    );
    expect(
      (compiled(fixture).querySelector('#record-information') as HTMLTextAreaElement).value,
    ).toBe('Erstpressung');
  });

  it('zeigt einen Pflichtfeld-Fehler bei leerem Albumnamen nach Blur', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeAlbumName(fixture, '');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('erforderlich'))).toBe(true);
  });

  it('zeigt einen Fehler bei zu langem Albumnamen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeAlbumName(fixture, 'A'.repeat(151));

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('höchstens 150'))).toBe(true);
  });

  it('zeigt einen Fehler bei einem nicht erlaubten Zeichen im Albumnamen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeAlbumName(fixture, 'Kind of Blue #1');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('Buchstaben, Zahlen'))).toBe(true);
  });

  it('erlaubt Klammern im Albumnamen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeAlbumName(fixture, 'Kind of Blue (Deluxe Edition)');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('Buchstaben, Zahlen'))).toBe(false);
  });

  it('zeigt einen Fehler bei einem Erscheinungsjahr vor 1860', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeReleaseYear(fixture, '1859');

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('zwischen 1860'))).toBe(true);
  });

  it('zeigt einen Fehler bei einem Erscheinungsjahr in der Zukunft', () => {
    // arrange
    const fixture = createFixture();
    const nextYear = new Date().getFullYear() + 1;

    // act
    typeReleaseYear(fixture, String(nextYear));

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('zwischen 1860'))).toBe(true);
  });

  it('zeigt keinen Pflichtfeld-Fehler für den optionalen Künstler', () => {
    // arrange
    const fixture = createFixture();

    // act
    submitForm(fixture);
    fixture.detectChanges();

    // assert
    expect(recordServiceMock.create).not.toHaveBeenCalled();
    expect(errorHints(fixture).some((hint) => hint.includes('Künstler'))).toBe(false);
  });

  it('sendet beim Anlegen alle Felder inkl. gewähltem Label/Künstler an RecordService.create', async () => {
    // arrange
    recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidRecord(fixture);
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.create).toHaveBeenCalledWith({
      labelId: 1,
      artistId: null,
      format: 'Album',
      albumName: 'Kind of Blue',
      releaseYear: 1959,
      condition: 'Vg',
      information: null,
    });
    expect(savedHandler).toHaveBeenCalledTimes(1);
  });

  it('übernimmt eine Künstler-Auswahl in artistId', async () => {
    // arrange
    recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
    const fixture = createFixture();

    // act
    await fillValidRecord(fixture);
    await selectViaAutocomplete(fixture, 1, 'Miles');
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.create).toHaveBeenCalledWith(expect.objectContaining({ artistId: 2 }));
  });

  it('ruft im Bearbeiten-Modus RecordService.update mit der bestehenden Id auf', async () => {
    // arrange
    recordServiceMock.update.mockReturnValue(of({ ...existingRecord, albumName: 'Kind of Blue' }));
    const fixture = createFixture(existingRecord);
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.update).toHaveBeenCalledWith(5, {
      labelId: 1,
      artistId: 2,
      format: 'Album',
      albumName: 'Kind of Blue',
      releaseYear: 1959,
      condition: 'Vg',
      information: 'Erstpressung',
    });
    expect(savedHandler).toHaveBeenCalledTimes(1);
  });

  it('wandelt eine leere Information in null um', async () => {
    // arrange
    recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
    const fixture = createFixture();

    // act
    await fillValidRecord(fixture);
    typeInformation(fixture, '   ');
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.create).toHaveBeenCalledWith(
      expect.objectContaining({ information: null }),
    );
  });

  it('hängt eine 400-Serverantwort für AlbumName inline ins Albumname-Feld ein, ohne das ErrorModal zu öffnen', async () => {
    // arrange
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        errors: { AlbumName: ['Der Albumname ist bereits vergeben.'] },
        title: 'Validierungsfehler',
        status: 400,
      },
    });
    recordServiceMock.create.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidRecord(fixture);
    submitForm(fixture);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(errorHints(fixture).some((hint) => hint.includes('bereits vergeben'))).toBe(true);
    expect(errorModalServiceMock.showFromHttpError).not.toHaveBeenCalled();
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('leitet 409/404/500/Netzwerkfehler an den ErrorModalService weiter, ohne saved zu emittieren', async () => {
    // arrange
    const error = new HttpErrorResponse({ status: 409 });
    recordServiceMock.create.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidRecord(fixture);
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(error, 'Record');
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('zeigt im Bearbeiten-Modus das bestehende Cover als Vorschau', () => {
    // arrange & act
    const fixture = createFixture({
      ...existingRecord,
      albumCoverDataUrl: 'data:image/jpeg;base64,abc',
    });

    // assert
    const preview = compiled(fixture).querySelector(
      'img[alt="Cover-Vorschau"]',
    ) as HTMLImageElement;
    expect(preview.src).toBe('data:image/jpeg;base64,abc');
  });

  it('setzt nach Dateiauswahl eine Vorschau des gewählten Covers', () => {
    // arrange
    const fixture = createFixture();
    const file = new File(['cover-bytes'], 'cover.jpg', { type: 'image/jpeg' });

    // act
    selectCoverFile(fixture, file);

    // assert
    const preview = compiled(fixture).querySelector(
      'img[alt="Cover-Vorschau"]',
    ) as HTMLImageElement;
    expect(preview).not.toBeNull();
    expect(preview.src.startsWith('blob:')).toBe(true);
    expect(errorModalServiceMock.showValidationMessage).not.toHaveBeenCalled();
  });

  it('zeigt bei zu großer Cover-Datei ein Validierungsmodal, ohne eine Vorschau zu setzen', () => {
    // arrange
    const fixture = createFixture();
    const file = new File(['cover-bytes'], 'cover.jpg', { type: 'image/jpeg' });
    Object.defineProperty(file, 'size', { value: 5 * 1024 * 1024 + 1 });

    // act
    selectCoverFile(fixture, file);

    // assert
    expect(errorModalServiceMock.showValidationMessage).toHaveBeenCalledWith(
      'Es sind nur JPEG- oder PNG-Dateien bis 5 MB erlaubt.',
    );
    expect(compiled(fixture).querySelector('img[alt="Cover-Vorschau"]')).toBeNull();
  });

  it('zeigt bei falschem Dateiformat ein Validierungsmodal, ohne eine Vorschau zu setzen', () => {
    // arrange
    const fixture = createFixture();
    const file = new File(['cover-bytes'], 'cover.txt', { type: 'text/plain' });

    // act
    selectCoverFile(fixture, file);

    // assert
    expect(errorModalServiceMock.showValidationMessage).toHaveBeenCalledWith(
      'Es sind nur JPEG- oder PNG-Dateien bis 5 MB erlaubt.',
    );
    expect(compiled(fixture).querySelector('img[alt="Cover-Vorschau"]')).toBeNull();
  });

  it('ruft nach erfolgreichem Anlegen zusätzlich uploadCover mit der neuen Id auf, wenn ein Cover gewählt wurde', async () => {
    // arrange
    recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
    recordServiceMock.uploadCover.mockReturnValue(of({ ...existingRecord, id: 9 }));
    const fixture = createFixture();
    const file = new File(['cover-bytes'], 'cover.jpg', { type: 'image/jpeg' });
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidRecord(fixture);
    selectCoverFile(fixture, file);
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.uploadCover).toHaveBeenCalledWith(9, file);
    expect(savedHandler).toHaveBeenCalledTimes(1);
  });

  it('ruft uploadCover nicht auf, wenn kein Cover gewählt wurde', async () => {
    // arrange
    recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
    const fixture = createFixture();

    // act
    await fillValidRecord(fixture);
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(recordServiceMock.uploadCover).not.toHaveBeenCalled();
  });

  it('zeigt bei einem Cover-Upload-Fehler ein Modal, emittiert aber trotzdem saved', async () => {
    // arrange
    recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
    const coverError = new HttpErrorResponse({
      status: 400,
      error: {
        errors: { FileContent: ['Es sind nur JPEG- oder PNG-Dateien erlaubt.'] },
        title: 'Validierungsfehler',
        status: 400,
      },
    });
    recordServiceMock.uploadCover.mockReturnValue(throwError(() => coverError));
    const fixture = createFixture();
    const file = new File(['cover-bytes'], 'cover.jpg', { type: 'image/jpeg' });
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);

    // act
    await fillValidRecord(fixture);
    selectCoverFile(fixture, file);
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(coverError, 'Album-Cover');
    expect(savedHandler).toHaveBeenCalledTimes(1);
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

  it('hat Tooltips an Abbrechen- und Speichern-Button', () => {
    // arrange & act
    const fixture = createFixture();

    // assert
    expect((compiled(fixture).querySelector('.btn-secondary') as HTMLButtonElement).title).toBe(
      'Abbrechen',
    );
    expect((compiled(fixture).querySelector('.btn-primary') as HTMLButtonElement).title).toBe(
      'Speichern',
    );
  });

  it('hat einen Tooltip am Label-Anlegen-Button', () => {
    // arrange & act
    const fixture = createFixture();
    const button = compiled(fixture).querySelector(
      '[aria-label="Neues Label anlegen"]',
    ) as HTMLButtonElement;

    // assert
    expect(button.getAttribute('title')).toBe('Neues Label anlegen');
  });

  it('öffnet über den Label-Button das verschachtelte Label-Formular', () => {
    // arrange
    const fixture = createFixture();

    // act
    (
      compiled(fixture).querySelector('[aria-label="Neues Label anlegen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('app-label-form')).not.toBeNull();
  });

  it('legt über das verschachtelte Label-Formular ein neues Label an und übernimmt es', async () => {
    // arrange
    countryServiceMock.getAll.mockReturnValue(of([{ id: 1, name: 'USA', code: 'US' }]));
    const created: Label = {
      id: 9,
      name: 'Neues Label',
      countryId: 1,
      countryName: 'USA',
      information: null,
    };
    labelServiceMock.create.mockReturnValue(of(created));
    const fixture = createFixture();
    (
      compiled(fixture).querySelector('[aria-label="Neues Label anlegen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();
    const nameInput = compiled(fixture).querySelector('#label-name') as HTMLInputElement;
    nameInput.value = 'Neues Label';
    nameInput.dispatchEvent(new Event('input'));
    const countrySelect = compiled(fixture).querySelector('#label-country') as HTMLSelectElement;
    countrySelect.value = '1';
    countrySelect.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    const nestedForm = compiled(fixture).querySelector('app-label-form form') as HTMLFormElement;

    // act
    nestedForm.dispatchEvent(new Event('submit', { cancelable: true }));
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('app-label-form')).toBeNull();
    const labelInput = compiled(fixture).querySelectorAll(
      'app-autocomplete input',
    )[0] as HTMLInputElement;
    expect(labelInput.value).toBe('Neues Label');
  });

  it('Abbrechen im verschachtelten Label-Formular ändert nichts am Record-Formular', () => {
    // arrange
    const fixture = createFixture();
    (
      compiled(fixture).querySelector('[aria-label="Neues Label anlegen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    const cancelButtons = compiled(fixture).querySelectorAll('.btn-secondary');
    (cancelButtons[cancelButtons.length - 1] as HTMLButtonElement).click();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('app-label-form')).toBeNull();
    const labelInput = compiled(fixture).querySelectorAll(
      'app-autocomplete input',
    )[0] as HTMLInputElement;
    expect(labelInput.value).toBe('');
  });

  it('zeigt bei einem unbekannten, aber gültigen Künstlernamen nach Blur eine Rückfrage', () => {
    // arrange
    const fixture = createFixture();
    const artistInput = compiled(fixture).querySelectorAll(
      'app-autocomplete input',
    )[1] as HTMLInputElement;

    // act
    artistInput.value = 'Neuer Künstler';
    artistInput.dispatchEvent(new Event('input'));
    artistInput.dispatchEvent(new Event('blur'));
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).textContent).toContain('Künstler anlegen');
    expect(compiled(fixture).textContent).toContain('Neuer Künstler');
    expect(compiled(fixture).textContent).toContain('neu angelegt werden?');
  });

  it('legt bei Bestätigung einen neuen Künstler an und übernimmt ihn', async () => {
    // arrange
    artistServiceMock.create.mockReturnValue(of({ id: 11, name: 'Neuer Künstler' }));
    const fixture = createFixture();
    const artistInput = compiled(fixture).querySelectorAll(
      'app-autocomplete input',
    )[1] as HTMLInputElement;
    artistInput.value = 'Neuer Künstler';
    artistInput.dispatchEvent(new Event('input'));
    artistInput.dispatchEvent(new Event('blur'));
    fixture.detectChanges();

    // act
    const confirmButton = Array.from(compiled(fixture).querySelectorAll('button')).find(
      (button) => button.textContent?.trim() === 'Anlegen',
    );
    (confirmButton as HTMLButtonElement).click();
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(artistServiceMock.create).toHaveBeenCalledWith({ name: 'Neuer Künstler' });
    expect(compiled(fixture).textContent).not.toContain('neu angelegt werden?');
    expect(artistInput.value).toBe('Neuer Künstler');
  });

  it('leert das Künstlerfeld, wenn das Anlegen abgelehnt wird', async () => {
    // arrange
    const fixture = createFixture();
    const artistInput = compiled(fixture).querySelectorAll(
      'app-autocomplete input',
    )[1] as HTMLInputElement;
    artistInput.value = 'Neuer Künstler';
    artistInput.dispatchEvent(new Event('input'));
    artistInput.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
    const cancelButton = compiled(fixture).querySelector(
      'app-confirm-modal .btn-secondary',
    ) as HTMLButtonElement;

    // act
    cancelButton.click();
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).textContent).not.toContain('neu angelegt werden?');
    expect(artistInput.value).toBe('');
  });

  it('zeigt im Bearbeiten-Modus beim Blur ohne Änderung keine Rückfrage', () => {
    // arrange
    const fixture = createFixture(existingRecord);
    const artistInput = compiled(fixture).querySelectorAll(
      'app-autocomplete input',
    )[1] as HTMLInputElement;

    // act: Blur ohne die vorbefüllte Eingabe zu ändern
    artistInput.dispatchEvent(new Event('blur'));
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).textContent).not.toContain('neu angelegt werden?');
  });

  it('zeigt bei einem zu kurzen Künstlernamen keine Rückfrage', () => {
    // arrange
    const fixture = createFixture();
    const artistInput = compiled(fixture).querySelectorAll(
      'app-autocomplete input',
    )[1] as HTMLInputElement;

    // act
    artistInput.value = 'AB';
    artistInput.dispatchEvent(new Event('input'));
    artistInput.dispatchEvent(new Event('blur'));
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).textContent).not.toContain('neu angelegt werden?');
  });

  describe('Discogs-Integration', () => {
    afterEach(() => {
      vi.unstubAllGlobals();
    });

    it('öffnet über den Discogs-Button die Discogs-Suche', () => {
      // arrange
      const fixture = createFixture();

      // act
      (
        compiled(fixture).querySelector(
          '[title="Nach Discogs-Metadaten suchen"]',
        ) as HTMLButtonElement
      ).click();
      fixture.detectChanges();

      // assert
      expect(compiled(fixture).querySelector('app-discogs-search')).not.toBeNull();
    });

    it('übernimmt Albumname und Erscheinungsjahr aus einem Discogs-Treffer', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      const fixture = createFixture();

      // act
      await applyDiscogsRelease(fixture, nevermindRelease);

      // assert
      expect(
        (compiled(fixture).querySelector('#record-album-name') as HTMLInputElement).value,
      ).toBe('Nevermind');
      expect(
        (compiled(fixture).querySelector('#record-release-year') as HTMLInputElement).value,
      ).toBe('1991');
    });

    it('referenziert bestehende Label-, Artist- und Genre-Referenzen ohne Rückfrage', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      const fixture = createFixture();

      // act
      await applyDiscogsRelease(fixture, nevermindRelease);

      // assert
      const inputs = compiled(fixture).querySelectorAll('app-autocomplete input');
      expect((inputs[0] as HTMLInputElement).value).toBe('Columbia');
      expect((inputs[1] as HTMLInputElement).value).toBe('Miles Davis');
      expect(compiled(fixture).textContent).not.toContain('neu angelegt werden?');
      expect(compiled(fixture).querySelector('app-label-form')).toBeNull();
    });

    it('legt eine neue Artist-Referenz aus Discogs ohne Rückfrage automatisch an', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      artistServiceMock.create.mockReturnValue(of({ id: 21, name: 'Nirvana' }));
      const fixture = createFixture();
      const release: DiscogsRelease = { ...nevermindRelease, artists: ['Nirvana'] };

      // act
      await applyDiscogsRelease(fixture, release);

      // assert
      expect(artistServiceMock.create).toHaveBeenCalledWith({ name: 'Nirvana' });
      expect(compiled(fixture).textContent).not.toContain('Künstler anlegen');
      const inputs = compiled(fixture).querySelectorAll('app-autocomplete input');
      expect((inputs[1] as HTMLInputElement).value).toBe('Nirvana');
    });

    it('bereinigt einen Discogs-Artist-Namen mit unerlaubten Zeichen vor der automatischen Neuanlage', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      artistServiceMock.create.mockReturnValue(of({ id: 30, name: 'Prince' }));
      const fixture = createFixture();
      const release: DiscogsRelease = { ...nevermindRelease, artists: ['Prince (2)'] };

      // act
      await applyDiscogsRelease(fixture, release);

      // assert: Neuanlage verwendet bereits den bereinigten Namen, nicht den Discogs-Rohwert
      expect(artistServiceMock.create).toHaveBeenCalledWith({ name: 'Prince' });
      expect(compiled(fixture).textContent).not.toContain('Künstler anlegen');
      const inputs = compiled(fixture).querySelectorAll('app-autocomplete input');
      expect((inputs[1] as HTMLInputElement).value).toBe('Prince');
    });

    it('öffnet ohne ermittelbares Land das LabelForm zur manuellen Länderwahl und befüllt den Namen vor', async () => {
      // arrange: nevermindRelease liefert kein Discogs-Land (country: null)
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      const fixture = createFixture();
      const release: DiscogsRelease = { ...nevermindRelease, labels: ['Sub Pop'] };

      // act
      await applyDiscogsRelease(fixture, release);

      // assert
      const nameInput = compiled(fixture).querySelector('#label-name') as HTMLInputElement;
      expect(nameInput).not.toBeNull();
      expect(nameInput.value).toBe('Sub Pop');
    });

    it('bereinigt einen Discogs-Label-Namen mit unerlaubten Zeichen vor der Vorbefüllung im Fallback-Modal', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      const fixture = createFixture();
      const release: DiscogsRelease = { ...nevermindRelease, labels: ['Atlantic (2)'] };

      // act
      await applyDiscogsRelease(fixture, release);

      // assert: Vorbefülltes Namensfeld zeigt bereits den bereinigten Namen
      const nameInput = compiled(fixture).querySelector('#label-name') as HTMLInputElement;
      expect(nameInput).not.toBeNull();
      expect(nameInput.value).toBe('Atlantic');
    });

    it('legt eine neue Label-Referenz mit eindeutigem Discogs-Land ohne Rückfrage automatisch an', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      countryServiceMock.getAll.mockReturnValue(
        of([{ id: 5, name: 'Vereinigte Staaten von Amerika', code: 'US' }]),
      );
      labelServiceMock.create.mockReturnValue(
        of({
          id: 50,
          name: 'Sub Pop',
          countryId: 5,
          countryName: 'Vereinigte Staaten von Amerika',
          information: null,
        }),
      );
      const fixture = createFixture();
      const release: DiscogsRelease = { ...nevermindRelease, labels: ['Sub Pop'], country: 'US' };

      // act
      await applyDiscogsRelease(fixture, release);

      // assert
      expect(labelServiceMock.create).toHaveBeenCalledWith({
        name: 'Sub Pop',
        countryId: 5,
        information: null,
      });
      expect(compiled(fixture).querySelector('app-label-form')).toBeNull();
      const labelInput = compiled(fixture).querySelectorAll(
        'app-autocomplete input',
      )[0] as HTMLInputElement;
      expect(labelInput.value).toBe('Sub Pop');
    });

    it('legt eine neue Genre-Referenz aus Discogs ohne Rückfrage automatisch an', async () => {
      // arrange
      genreServiceMock.create.mockReturnValue(of({ id: 3, name: 'Jazz' }));
      const fixture = createFixture();

      // act
      await applyDiscogsRelease(fixture, nevermindRelease);

      // assert
      expect(genreServiceMock.create).toHaveBeenCalledWith({ name: 'Jazz' });
      expect(compiled(fixture).textContent).not.toContain('Genre anlegen');
    });

    it('bereinigt einen Discogs-Genre-Namen mit unerlaubten Zeichen vor der automatischen Neuanlage', async () => {
      // arrange
      genreServiceMock.create.mockReturnValue(of({ id: 40, name: 'Folk World Country' }));
      const fixture = createFixture();
      const release: DiscogsRelease = { ...nevermindRelease, genres: ['Folk, World, Country'] };

      // act
      await applyDiscogsRelease(fixture, release);

      // assert: Neuanlage verwendet bereits den bereinigten Namen
      expect(genreServiceMock.create).toHaveBeenCalledWith({ name: 'Folk World Country' });
      expect(compiled(fixture).textContent).not.toContain('Genre anlegen');
    });

    it('legt nach dem Speichern alle Tracks eines Discogs-Albums mit dem Record-Artist an', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
      recordServiceMock.createTrack.mockReturnValue(of({}));
      const fixture = createFixture();
      await applyDiscogsRelease(fixture, nevermindRelease);
      selectFormat(fixture, 'Album');

      // act
      submitForm(fixture);
      await fixture.whenStable();

      // assert
      expect(recordServiceMock.createTrack).toHaveBeenCalledTimes(2);
      expect(recordServiceMock.createTrack).toHaveBeenNthCalledWith(1, 9, {
        artistId: 2,
        genreId: 3,
        trackName: 'So What',
        recordSide: 'A',
        trackNumber: 1,
        information: 'Dauer: 9:22',
      });
      expect(recordServiceMock.createTrack).toHaveBeenNthCalledWith(2, 9, {
        artistId: 2,
        genreId: 3,
        trackName: 'Freddie Freeloader',
        recordSide: 'A',
        trackNumber: 2,
        information: 'Dauer: 9:46',
      });
    });

    it('ordnet Seiten mit nur einem Track (Discogs-Position ohne Ziffer, z. B. "A") korrekt zu, statt sie unter Seite 0 zu vermischen', async () => {
      // arrange: reale Discogs-Positionen von Release 91831 ("Atmos – Headcleaner") —
      // Seite A und C haben je nur einen Track, Discogs liefert dafür keine Ziffer.
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
      recordServiceMock.createTrack.mockReturnValue(of({}));
      const release: DiscogsRelease = {
        ...nevermindRelease,
        tracklist: [
          { position: 'A', title: 'Fill The Hat', duration: null, artist: null },
          { position: 'B1', title: 'Average Beverage', duration: null, artist: null },
          { position: 'B2', title: 'Sonic Kipper', duration: null, artist: null },
          { position: 'C', title: 'Cable Enable', duration: null, artist: null },
          { position: 'D1', title: 'Jimmy The Plate', duration: null, artist: null },
          { position: 'D2', title: 'Qualität Im Quadrat', duration: null, artist: null },
        ],
      };
      const fixture = createFixture();
      await applyDiscogsRelease(fixture, release);
      selectFormat(fixture, 'Album');

      // act
      submitForm(fixture);
      await fixture.whenStable();

      // assert: keine der sechs Seiten-/Nummer-Kombinationen ist "0" oder doppelt/übersprungen
      expect(recordServiceMock.createTrack).toHaveBeenCalledTimes(6);
      const sideAndNumberPairs = recordServiceMock.createTrack.mock.calls.map((call: unknown[]) => {
        const request = call[1] as { recordSide: string; trackNumber: number };
        return [request.recordSide, request.trackNumber];
      });
      expect(sideAndNumberPairs).toEqual([
        ['A', 1],
        ['B', 1],
        ['B', 2],
        ['C', 1],
        ['D', 1],
        ['D', 2],
      ]);
    });

    it('legt bei einem Various-Artists-Release für jeden Track den jeweils aufgelösten Track-Artist an', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      artistServiceMock.create.mockReturnValue(of({ id: 20, name: 'Cannonball Adderley' }));
      recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
      recordServiceMock.createTrack.mockReturnValue(of({}));
      const vaRelease: DiscogsRelease = {
        ...nevermindRelease,
        tracklist: [
          { position: 'A1', title: 'So What', duration: '9:22', artist: 'Miles Davis' },
          {
            position: 'A2',
            title: 'Autumn Leaves',
            duration: '10:59',
            artist: 'Cannonball Adderley',
          },
        ],
      };
      const fixture = createFixture();
      await applyDiscogsRelease(fixture, vaRelease);
      selectFormat(fixture, 'Album');

      // act
      submitForm(fixture);
      await fixture.whenStable();

      // assert
      expect(artistServiceMock.create).toHaveBeenCalledWith({ name: 'Cannonball Adderley' });
      expect(recordServiceMock.createTrack).toHaveBeenNthCalledWith(1, 9, {
        artistId: 2,
        genreId: 3,
        trackName: 'So What',
        recordSide: 'A',
        trackNumber: 1,
        information: 'Dauer: 9:22',
      });
      expect(recordServiceMock.createTrack).toHaveBeenNthCalledWith(2, 9, {
        artistId: 20,
        genreId: 3,
        trackName: 'Autumn Leaves',
        recordSide: 'A',
        trackNumber: 2,
        information: 'Dauer: 10:59',
      });
    });

    it('überspringt den Track-Import komplett, wenn die automatische Genre-Neuanlage fehlschlägt', async () => {
      // arrange: genreServiceMock.getAll bleibt beim Default [] – „Jazz" ist eine neue Referenz,
      // deren Anlage hier fehlschlägt (genreId ist Pflicht fuer den Track-Import)
      genreServiceMock.create.mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 400 })),
      );
      recordServiceMock.create.mockReturnValue(of({ ...existingRecord, id: 9 }));
      const fixture = createFixture();
      await applyDiscogsRelease(fixture, nevermindRelease);
      selectFormat(fixture, 'Album');

      // act
      submitForm(fixture);
      await fixture.whenStable();

      // assert
      expect(recordServiceMock.createTrack).not.toHaveBeenCalled();
    });

    it('übernimmt bei Übernahme automatisch das eingebettete Discogs-Cover als Vorschau', async () => {
      // arrange: coverImageUrl ist die vom Backend eingebettete Data-URL (ADR 0020) —
      // die Übernahme dekodiert sie direkt, ohne fetch() (ADR 0028)
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      const release: DiscogsRelease = {
        ...nevermindRelease,
        coverImageUrl: 'data:image/jpeg;base64,AQID',
      };
      const fixture = createFixture();

      // act
      await applyDiscogsRelease(fixture, release);

      // assert
      const preview = compiled(fixture).querySelector(
        'img[alt="Cover-Vorschau"]',
      ) as HTMLImageElement;
      expect(preview).not.toBeNull();
      expect(preview.src.startsWith('blob:')).toBe(true);
    });

    it('lässt das Cover leer, wenn die Discogs-Data-URL nicht dekodiert werden kann', async () => {
      // arrange
      genreServiceMock.getAll.mockReturnValue(of([{ id: 3, name: 'Jazz' }]));
      const release: DiscogsRelease = {
        ...nevermindRelease,
        coverImageUrl: 'data:image/jpeg;base64,not-valid-base64!!!',
      };
      const fixture = createFixture();

      // act
      await applyDiscogsRelease(fixture, release);

      // assert
      expect(compiled(fixture).querySelector('img[alt="Cover-Vorschau"]')).toBeNull();
    });
  });
});
