import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';

import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { DiscogsRelease, DiscogsSearchResult } from '../discogs';
import { DiscogsService } from '../discogs.service';
import { DiscogsSearch } from './discogs-search';

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

const searchResults: DiscogsSearchResult[] = [
  { id: 1, title: 'Nevermind', year: 1991, label: 'DGC', thumbnailUrl: null },
];

const release: DiscogsRelease = {
  id: 1,
  title: 'Nevermind',
  year: 1991,
  artists: ['Nirvana'],
  labels: ['DGC'],
  genres: ['Rock'],
  styles: ['Grunge'],
  formats: [],
  coverImageUrl: null,
  tracklist: [],
};

describe('DiscogsSearch', () => {
  let discogsServiceMock: {
    search: ReturnType<typeof vi.fn>;
    getRelease: ReturnType<typeof vi.fn>;
  };
  let errorModalServiceMock: { showFromHttpError: ReturnType<typeof vi.fn> };

  function createFixture() {
    discogsServiceMock = { search: vi.fn(() => of([])), getRelease: vi.fn() };
    errorModalServiceMock = { showFromHttpError: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: DiscogsService, useValue: discogsServiceMock },
        { provide: ErrorModalService, useValue: errorModalServiceMock },
      ],
    });

    const fixture = TestBed.createComponent(DiscogsSearch);
    fixture.detectChanges();
    return fixture;
  }

  async function typeQuery(
    fixture: ReturnType<typeof createFixture>,
    value: string,
  ): Promise<void> {
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await wait(350);
    fixture.detectChanges();
  }

  it('löst unterhalb von 2 Zeichen keine Suche aus', async () => {
    // arrange
    const fixture = createFixture();

    // act
    await typeQuery(fixture, 'a');

    // assert
    expect(discogsServiceMock.search).not.toHaveBeenCalled();
  });

  it('sucht ab 2 Zeichen und zeigt Titel, Jahr und Label der Treffer', async () => {
    // arrange
    const fixture = createFixture();
    discogsServiceMock.search.mockReturnValue(of(searchResults));

    // act
    await typeQuery(fixture, 'Nevermind');

    // assert
    expect(discogsServiceMock.search).toHaveBeenCalledWith('Nevermind');
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Nevermind');
    expect(text).toContain('1991');
    expect(text).toContain('DGC');
  });

  it('zeigt eine Leermeldung, wenn die Suche keine Treffer liefert', async () => {
    // arrange
    const fixture = createFixture();
    discogsServiceMock.search.mockReturnValue(of([]));

    // act
    await typeQuery(fixture, 'Nevermind');

    // assert
    expect(discogsServiceMock.search).toHaveBeenCalledWith('Nevermind');
    expect((fixture.nativeElement as HTMLElement).querySelector('.empty')?.textContent).toContain(
      'Keine Daten vorhanden.',
    );
  });

  it('ruft bei Auswahl eines Treffers getRelease auf und emittiert applied', async () => {
    // arrange
    const fixture = createFixture();
    discogsServiceMock.search.mockReturnValue(of(searchResults));
    discogsServiceMock.getRelease.mockReturnValue(of(release));
    const appliedHandler = vi.fn();
    fixture.componentInstance.applied.subscribe(appliedHandler);
    await typeQuery(fixture, 'Nevermind');

    // act
    const resultButton = (fixture.nativeElement as HTMLElement).querySelector(
      'button[title]',
    ) as HTMLButtonElement;
    resultButton.click();
    await fixture.whenStable();

    // assert
    expect(discogsServiceMock.getRelease).toHaveBeenCalledWith(1);
    expect(appliedHandler).toHaveBeenCalledWith(release);
  });

  it('leitet einen Discogs-Fehler (502) beim Detailabruf an den ErrorModalService weiter', async () => {
    // arrange
    const error = new HttpErrorResponse({ status: 502 });
    const fixture = createFixture();
    discogsServiceMock.search.mockReturnValue(of(searchResults));
    discogsServiceMock.getRelease.mockReturnValue(throwError(() => error));
    const appliedHandler = vi.fn();
    fixture.componentInstance.applied.subscribe(appliedHandler);
    await typeQuery(fixture, 'Nevermind');

    // act
    const resultButton = (fixture.nativeElement as HTMLElement).querySelector(
      'button[title]',
    ) as HTMLButtonElement;
    resultButton.click();
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(error, 'Discogs');
    expect(appliedHandler).not.toHaveBeenCalled();
  });

  it('emittiert cancelled beim Abbrechen', () => {
    // arrange
    const fixture = createFixture();
    const cancelledHandler = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelledHandler);

    // act
    (
      (fixture.nativeElement as HTMLElement).querySelector('.btn-secondary') as HTMLButtonElement
    ).click();

    // assert
    expect(cancelledHandler).toHaveBeenCalledTimes(1);
  });
});
