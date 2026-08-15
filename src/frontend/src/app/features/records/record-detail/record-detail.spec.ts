import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { RuntimeConfigService } from '../../../core/runtime-config/runtime-config.service';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { Record, RecordTrack } from '../record';
import { RecordDetail } from './record-detail';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function buildRecord(overrides: Partial<Record> = {}): Record {
  return {
    id: 1,
    labelId: 1,
    labelName: 'Columbia',
    artistId: 2,
    artistName: 'Miles Davis',
    format: 'Album',
    albumName: 'Kind of Blue',
    releaseYear: 1959,
    condition: 'Vg',
    information: null,
    albumCoverDataUrl: null,
    tracks: [],
    ...overrides,
  };
}

function buildTrack(overrides: Partial<RecordTrack> = {}): RecordTrack {
  return {
    id: 1,
    recordId: 1,
    artistId: 2,
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

describe('RecordDetail', () => {
  let httpTesting: HttpTestingController;
  let errorModalService: ErrorModalService;
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    router = { navigate: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RuntimeConfigService, useValue: { apiBaseUrl: 'https://api.test' } },
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ id: '1' })) } },
        { provide: Router, useValue: router },
      ],
    });

    httpTesting = TestBed.inject(HttpTestingController);
    errorModalService = TestBed.inject(ErrorModalService);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function compiled(fixture: { nativeElement: unknown }): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function findButton(root: HTMLElement, text: string): HTMLButtonElement {
    return Array.from(root.querySelectorAll('button')).find((button) =>
      button.textContent?.trim().includes(text),
    ) as HTMLButtonElement;
  }

  async function createLoadedFixture(record: Record = buildRecord()) {
    const fixture = TestBed.createComponent(RecordDetail);
    fixture.detectChanges();
    httpTesting.expectOne('https://api.test/api/records/1').flush(record);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  it('zeigt eine Ladeanzeige, während der Record geladen wird', () => {
    // arrange
    const fixture = TestBed.createComponent(RecordDetail);

    // act
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('.spinner')).not.toBeNull();
    httpTesting.expectOne('https://api.test/api/records/1').flush(buildRecord());
  });

  it('zeigt Albumname, Künstler, Jahr, Label und Format nach erfolgreichem Laden', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    expect(compiled(fixture).textContent).toContain('Kind of Blue');
    expect(compiled(fixture).textContent).toContain('Miles Davis');
    expect(compiled(fixture).textContent).toContain('1959');
    expect(compiled(fixture).textContent).toContain('Columbia');
    expect(compiled(fixture).textContent).toContain('Album');
  });

  it('zeigt das Platzhalter-Icon, wenn kein Cover vorhanden ist', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture(buildRecord({ albumCoverDataUrl: null }));

    // assert
    expect(compiled(fixture).querySelector('.record-cover img')).toBeNull();
    expect(compiled(fixture).querySelector('.record-cover svg')).not.toBeNull();
  });

  it('zeigt das Cover-Bild, wenn eine Data-URL vorhanden ist', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture(
      buildRecord({ albumCoverDataUrl: 'data:image/jpeg;base64,xyz' }),
    );

    // assert
    const image = compiled(fixture).querySelector('.record-cover img') as HTMLImageElement;
    expect(image.getAttribute('src')).toBe('data:image/jpeg;base64,xyz');
  });

  it('zeigt das Grade-Badge mit der zum Zustand passenden Klasse und Bezeichnung', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture(buildRecord({ condition: 'VgPlus' }));

    // assert
    const grade = compiled(fixture).querySelector('.grade') as HTMLElement;
    expect(grade.classList.contains('grade-vgp')).toBe(true);
    expect(grade.textContent?.trim()).toBe('VG+');
  });

  it('navigiert beim Schließen zurück zur Liste', async () => {
    // arrange
    const fixture = await createLoadedFixture();

    // act
    (compiled(fixture).querySelector('[aria-label="Schließen"]') as HTMLButtonElement).click();

    // assert
    expect(router.navigate).toHaveBeenCalledWith(['/records']);
  });

  it('bietet keine Bearbeiten- oder Löschen-Aktion an (reiner Lesemodus)', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture();

    // assert
    const buttonTexts = Array.from(compiled(fixture).querySelectorAll('button')).map((button) =>
      button.textContent?.trim(),
    );
    expect(buttonTexts).not.toContain('Bearbeiten');
    expect(buttonTexts).not.toContain('Löschen');
  });

  it('zeigt "Record nicht gefunden" über den ErrorModalService bei HTTP 404', async () => {
    // arrange
    const fixture = TestBed.createComponent(RecordDetail);

    // act
    fixture.detectChanges();
    httpTesting
      .expectOne('https://api.test/api/records/1')
      .flush({ title: 'Nicht gefunden', status: 404 }, { status: 404, statusText: 'Not Found' });
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'not-found' }));
  });

  it('zeigt einen Serverfehler über den ErrorModalService bei HTTP 500', async () => {
    // arrange
    const fixture = TestBed.createComponent(RecordDetail);

    // act
    fixture.detectChanges();
    httpTesting
      .expectOne('https://api.test/api/records/1')
      .flush(
        { title: 'Serverfehler', status: 500 },
        { status: 500, statusText: 'Internal Server Error' },
      );
    await fixture.whenStable();

    // assert
    expect(errorModalService.current()).toEqual(expect.objectContaining({ kind: 'server' }));
  });

  it('zeigt die Tracklist der geladenen Tracks', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture(
      buildRecord({
        tracks: [
          {
            id: 1,
            recordId: 1,
            artistId: 2,
            artistName: 'Miles Davis',
            genreId: 1,
            genreName: 'Jazz',
            trackName: 'So What',
            recordSide: 'A',
            trackNumber: 1,
            information: null,
          },
        ],
      }),
    );

    // assert
    expect(compiled(fixture).textContent).toContain('So What');
  });

  it('öffnet über den Track-hinzufügen-Button das Track-Formular im Anlegen-Modus', async () => {
    // arrange
    const fixture = await createLoadedFixture();

    // act
    findButton(compiled(fixture), 'Track hinzufügen').click();
    fixture.detectChanges();
    httpTesting.expectOne('https://api.test/api/genres/all').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('app-track-form')).not.toBeNull();
    expect((compiled(fixture).querySelector('#track-name') as HTMLInputElement).value).toBe('');
  });

  it('öffnet über das Bearbeiten-Icon eines Tracks das Track-Formular vorbefüllt', async () => {
    // arrange
    const track = buildTrack({ trackName: 'So What' });
    const fixture = await createLoadedFixture(buildRecord({ tracks: [track] }));

    // act
    (
      compiled(fixture).querySelector(
        '[aria-label="Track So What bearbeiten"]',
      ) as HTMLButtonElement
    ).click();
    fixture.detectChanges();
    httpTesting.expectOne('https://api.test/api/genres/all').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect((compiled(fixture).querySelector('#track-name') as HTMLInputElement).value).toBe(
      'So What',
    );
  });

  it('zeigt auch bei vorhandenen Tracks keine Bearbeiten-/Löschen-Buttons mit Text auf Record-Ebene', async () => {
    // arrange
    // act
    const fixture = await createLoadedFixture(
      buildRecord({ tracks: [buildTrack({ trackName: 'So What' })] }),
    );

    // assert
    const buttonTexts = Array.from(compiled(fixture).querySelectorAll('button')).map((button) =>
      button.textContent?.trim(),
    );
    expect(buttonTexts).not.toContain('Bearbeiten');
    expect(buttonTexts).not.toContain('Löschen');
  });

  it('löscht einen Track nach Bestätigung und lädt den Record danach neu', async () => {
    // arrange
    const track = buildTrack({ trackName: 'So What' });
    const fixture = await createLoadedFixture(buildRecord({ tracks: [track] }));
    (
      compiled(fixture).querySelector('[aria-label="Track So What löschen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Löschen').click();
    const deleteRequest = httpTesting.expectOne(
      (req) => req.method === 'DELETE' && req.url === 'https://api.test/api/records/1/tracks/1',
    );
    deleteRequest.flush(null);
    await wait(0);
    fixture.detectChanges();
    httpTesting.expectOne('https://api.test/api/records/1').flush(buildRecord({ tracks: [] }));
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(compiled(fixture).querySelector('app-confirm-modal')).toBeNull();
    expect(compiled(fixture).textContent).not.toContain('So What');
  });

  it('bricht das Löschen eines Tracks ohne HTTP-Aufruf ab', async () => {
    // arrange
    const track = buildTrack({ trackName: 'So What' });
    const fixture = await createLoadedFixture(buildRecord({ tracks: [track] }));
    (
      compiled(fixture).querySelector('[aria-label="Track So What löschen"]') as HTMLButtonElement
    ).click();
    fixture.detectChanges();

    // act
    findButton(compiled(fixture), 'Abbrechen').click();
    fixture.detectChanges();

    // assert
    // httpTesting.verify() im afterEach deckt einen unerwarteten DELETE-Request auf.
    expect(compiled(fixture).querySelector('app-confirm-modal')).toBeNull();
  });
});
