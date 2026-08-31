import { describe, expect, it } from 'vitest';

import { dataUrlToFile } from './discogs-cover';

describe('dataUrlToFile', () => {
  it('dekodiert eine Data-URL in ein File mit passendem MIME-Typ, Dateinamen und Inhalt', async () => {
    // arrange
    const dataUrl = 'data:image/jpeg;base64,AQID';

    // act
    const file = dataUrlToFile(dataUrl, 'discogs-cover');

    // assert
    expect(file.name).toBe('discogs-cover');
    expect(file.type).toBe('image/jpeg');
    const bytes = new Uint8Array(await file.arrayBuffer());
    expect(Array.from(bytes)).toEqual([1, 2, 3]);
  });

  it('fällt auf application/octet-stream zurück, wenn die Data-URL keinen MIME-Typ nennt', () => {
    // arrange
    const dataUrl = 'data:;base64,AQID';

    // act
    const file = dataUrlToFile(dataUrl, 'discogs-cover');

    // assert
    expect(file.type).toBe('application/octet-stream');
  });
});
