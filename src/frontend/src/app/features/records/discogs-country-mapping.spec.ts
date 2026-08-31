import { describe, expect, it } from 'vitest';

import { Country } from '../../shared/country/country';
import { resolveDiscogsCountryId } from './discogs-country-mapping';

const countries: Country[] = [
  { id: 1, name: 'Deutschland', code: 'DE' },
  { id: 2, name: 'Vereinigtes Königreich', code: 'GB' },
  { id: 3, name: 'Vereinigte Staaten von Amerika', code: 'US' },
];

describe('resolveDiscogsCountryId', () => {
  it('löst einen bekannten Discogs-Ländertext auf die passende Country-Id auf', () => {
    expect(resolveDiscogsCountryId('Germany', countries)).toBe(1);
  });

  it('ist unabhängig von Groß-/Kleinschreibung und umgebenden Leerzeichen', () => {
    expect(resolveDiscogsCountryId('  GERMANY  ', countries)).toBe(1);
  });

  it('löst die Discogs-Kurzform "UK" auf Grossbritannien auf', () => {
    expect(resolveDiscogsCountryId('UK', countries)).toBe(2);
  });

  it('löst die Discogs-Kurzformen "US"/"USA" auf die Vereinigten Staaten auf', () => {
    expect(resolveDiscogsCountryId('US', countries)).toBe(3);
    expect(resolveDiscogsCountryId('USA', countries)).toBe(3);
  });

  it('liefert null bei einem regionalen Discogs-Sammelbegriff ohne eindeutiges Land', () => {
    expect(resolveDiscogsCountryId('Europe', countries)).toBeNull();
  });

  it('liefert null, wenn Discogs kein Land liefert', () => {
    expect(resolveDiscogsCountryId(null, countries)).toBeNull();
  });

  it('liefert null, wenn die Stammdaten kein Land mit dem ermittelten Code führen', () => {
    expect(resolveDiscogsCountryId('Japan', countries)).toBeNull();
  });
});
