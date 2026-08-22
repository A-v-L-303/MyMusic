import { describe, expect, it } from 'vitest';

import { parseDiscogsPosition } from './discogs-position';

describe('parseDiscogsPosition', () => {
  it('trennt Buchstaben-Seite und Tracknummer bei "A1"', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('A1', 0);

    // assert
    expect(result).toEqual({ recordSide: 'A', trackNumber: 1 });
  });

  it('trennt Buchstaben-Seite und Tracknummer bei "B2"', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('B2', 0);

    // assert
    expect(result).toEqual({ recordSide: 'B', trackNumber: 2 });
  });

  it('liefert Seite 0 bei rein numerischer Position (z. B. CD-Tracklist)', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('1', 0);

    // assert
    expect(result).toEqual({ recordSide: '0', trackNumber: 1 });
  });

  it('nimmt die letzte Ziffernfolge bei zusammengesetzten Positionen wie "2-3"', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('2-3', 0);

    // assert
    expect(result).toEqual({ recordSide: '2', trackNumber: 3 });
  });

  it('fällt bei leerer Position auf Seite 0 und den 1-basierten Index zurück', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('', 4);

    // assert
    expect(result).toEqual({ recordSide: '0', trackNumber: 5 });
  });

  it('erkennt eine reine Seitenangabe ohne Ziffer als Track 1 dieser Seite (Discogs lässt die Ziffer weg, wenn eine Seite nur einen Track hat)', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('A', 0);

    // assert
    expect(result).toEqual({ recordSide: 'A', trackNumber: 1 });
  });

  it('erkennt eine reine Seitenangabe ohne Ziffer auch bei einer späteren Seite (z. B. "C")', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('C', 3);

    // assert
    expect(result).toEqual({ recordSide: 'C', trackNumber: 1 });
  });

  it('fällt bei rein alphabetischer Position ohne Ziffer auf den 1-basierten Index zurück', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('Bonus', 2);

    // assert
    expect(result).toEqual({ recordSide: '0', trackNumber: 3 });
  });

  it('kürzt eine zu lange Buchstaben-Seite auf 3 Zeichen', () => {
    // arrange
    // act
    const result = parseDiscogsPosition('ABCD1', 0);

    // assert
    expect(result).toEqual({ recordSide: 'ABC', trackNumber: 1 });
  });
});
