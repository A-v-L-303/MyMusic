import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { ThemeService } from './theme.service';

interface MediaQueryListStub {
  matches: boolean;
  media: string;
  addEventListener: (type: 'change', listener: (event: { matches: boolean }) => void) => void;
  triggerChange: (matches: boolean) => void;
}

function stubMatchMedia(initialMatches: boolean): MediaQueryListStub {
  let changeListener: ((event: { matches: boolean }) => void) | undefined;

  const mediaQueryList: MediaQueryListStub = {
    matches: initialMatches,
    media: '(prefers-color-scheme: dark)',
    addEventListener: (_type, listener) => {
      changeListener = listener;
    },
    triggerChange: (matches: boolean) => {
      changeListener?.({ matches });
    },
  };

  vi.stubGlobal('matchMedia', vi.fn().mockReturnValue(mediaQueryList));

  return mediaQueryList;
}

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  it('folgt der OS-Präferenz Light, wenn keine explizite Wahl gespeichert ist', () => {
    // arrange
    stubMatchMedia(false);
    TestBed.configureTestingModule({});

    // act
    const service = TestBed.inject(ThemeService);
    TestBed.tick();

    // assert: reiner CSS-Fallback bleibt aktiv, kein Attribut gesetzt
    expect(service.effectiveTheme()).toBe('light');
    expect(document.documentElement.getAttribute('data-theme')).toBeNull();
  });

  it('folgt der OS-Präferenz Dark, wenn keine explizite Wahl gespeichert ist', () => {
    // arrange
    stubMatchMedia(true);
    TestBed.configureTestingModule({});

    // act
    const service = TestBed.inject(ThemeService);
    TestBed.tick();

    // assert: reiner CSS-Fallback bleibt aktiv, kein Attribut gesetzt
    expect(service.effectiveTheme()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBeNull();
  });

  it('übernimmt eine gespeicherte explizite Präferenz unabhängig von der OS-Einstellung', () => {
    // arrange
    localStorage.setItem('mymusic-theme', 'dark');
    stubMatchMedia(false);
    TestBed.configureTestingModule({});

    // act
    const service = TestBed.inject(ThemeService);
    TestBed.tick();

    // assert
    expect(service.effectiveTheme()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('ignoriert einen ungültigen gespeicherten Wert und fällt auf die OS-Präferenz zurück', () => {
    // arrange
    localStorage.setItem('mymusic-theme', 'sepia');
    stubMatchMedia(true);
    TestBed.configureTestingModule({});

    // act
    const service = TestBed.inject(ThemeService);
    TestBed.tick();

    // assert
    expect(service.effectiveTheme()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBeNull();
  });

  it('reagiert live auf eine OS-Präferenzänderung, solange keine explizite Wahl vorliegt', () => {
    // arrange
    const mediaQueryList = stubMatchMedia(false);
    TestBed.configureTestingModule({});
    const service = TestBed.inject(ThemeService);

    // act
    mediaQueryList.triggerChange(true);
    TestBed.tick();

    // assert
    expect(service.effectiveTheme()).toBe('dark');
  });

  it('ignoriert eine OS-Präferenzänderung, sobald eine explizite Wahl getroffen wurde', () => {
    // arrange
    const mediaQueryList = stubMatchMedia(false);
    TestBed.configureTestingModule({});
    const service = TestBed.inject(ThemeService);
    service.toggle();
    TestBed.tick();

    // act
    mediaQueryList.triggerChange(true);
    TestBed.tick();

    // assert: explizite Wahl (jetzt "dark") bleibt von der OS-Änderung unberührt
    expect(service.effectiveTheme()).toBe('dark');
  });

  it('toggle() kehrt das effektive Theme um, persistiert es und setzt das DOM-Attribut', () => {
    // arrange
    stubMatchMedia(false);
    TestBed.configureTestingModule({});
    const service = TestBed.inject(ThemeService);

    // act
    service.toggle();
    TestBed.tick();

    // assert
    expect(service.effectiveTheme()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(localStorage.getItem('mymusic-theme')).toBe('dark');

    // act: zweiter Toggle schaltet zurück
    service.toggle();
    TestBed.tick();

    // assert
    expect(service.effectiveTheme()).toBe('light');
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
    expect(localStorage.getItem('mymusic-theme')).toBe('light');
  });
});
