import { describe, expect, it } from 'vitest';

import {
  sanitizeDiscogsArtistName,
  sanitizeDiscogsGenreName,
  sanitizeDiscogsLabelName,
} from './discogs-name-sanitizer';

describe('sanitizeDiscogsArtistName', () => {
  it('lässt einen bereits gültigen Namen unverändert', () => {
    // arrange
    // act
    const result = sanitizeDiscogsArtistName('Bob Marley & The Wailers');

    // assert
    expect(result).toBe('Bob Marley & The Wailers');
  });

  it('entfernt einen Discogs-Disambiguierungs-Suffix wie „ (2)"', () => {
    // arrange
    // act
    const result = sanitizeDiscogsArtistName('Prince (2)');

    // assert
    expect(result).toBe('Prince');
  });

  it('entfernt ein Komma und normalisiert Leerzeichen', () => {
    // arrange
    // act
    const result = sanitizeDiscogsArtistName('Earth, Wind & Fire');

    // assert
    expect(result).toBe('Earth Wind & Fire');
  });

  it('entfernt Anführungszeichen', () => {
    // arrange
    // act
    const result = sanitizeDiscogsArtistName('Guns N\' Roses "Live"');

    // assert
    expect(result).toBe("Guns N' Roses Live");
  });

  it('behält Umlaute und andere Unicode-Buchstaben', () => {
    // arrange
    // act
    const result = sanitizeDiscogsArtistName('Sigur Rós');

    // assert
    expect(result).toBe('Sigur Rós');
  });

  it('behält Punkt und Schrägstrich (bei Artist erlaubt)', () => {
    // arrange
    // act
    const result = sanitizeDiscogsArtistName('R.E.M. / Ambient Works');

    // assert
    expect(result).toBe('R.E.M. / Ambient Works');
  });

  it('kürzt auf maximal 120 Zeichen', () => {
    // arrange
    const longName = 'A'.repeat(150);

    // act
    const result = sanitizeDiscogsArtistName(longName);

    // assert
    expect(result).toHaveLength(120);
  });

  it('liefert einen leeren String, wenn nur unerlaubte Symbole übrig bleiben', () => {
    // arrange
    // act
    const result = sanitizeDiscogsArtistName('★♫✦');

    // assert
    expect(result).toBe('');
  });
});

describe('sanitizeDiscogsLabelName', () => {
  it('entfernt einen Discogs-Disambiguierungs-Suffix wie „ (2)"', () => {
    // arrange
    // act
    const result = sanitizeDiscogsLabelName('Atlantic (2)');

    // assert
    expect(result).toBe('Atlantic');
  });

  it('entfernt ein Komma', () => {
    // arrange
    // act
    const result = sanitizeDiscogsLabelName('Rough Trade, Ltd.');

    // assert
    expect(result).toBe('Rough Trade Ltd.');
  });

  it('kürzt auf maximal 60 Zeichen', () => {
    // arrange
    const longName = 'B'.repeat(80);

    // act
    const result = sanitizeDiscogsLabelName(longName);

    // assert
    expect(result).toHaveLength(60);
  });
});

describe('sanitizeDiscogsGenreName', () => {
  it('lässt einen bereits gültigen Namen unverändert', () => {
    // arrange
    // act
    const result = sanitizeDiscogsGenreName('Non-Music');

    // assert
    expect(result).toBe('Non-Music');
  });

  it('entfernt ein Komma, wie es Discogs-Styles häufig enthalten', () => {
    // arrange
    // act
    const result = sanitizeDiscogsGenreName('Folk, World, & Country');

    // assert
    expect(result).toBe('Folk World & Country');
  });

  it('entfernt Punkt und Schrägstrich (bei Genre nicht erlaubt, anders als bei Artist/Label)', () => {
    // arrange
    // act
    const result = sanitizeDiscogsGenreName('Drum & Bass / Jungle feat. MC');

    // assert
    expect(result).toBe('Drum & Bass Jungle feat MC');
  });

  it('kürzt auf maximal 50 Zeichen', () => {
    // arrange
    const longName = 'C'.repeat(70);

    // act
    const result = sanitizeDiscogsGenreName(longName);

    // assert
    expect(result).toHaveLength(50);
  });
});
