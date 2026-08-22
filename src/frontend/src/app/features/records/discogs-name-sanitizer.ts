const DISCOGS_DISAMBIGUATION_SUFFIX_PATTERN = /\s*\(\d+\)\s*$/u;
const WHITESPACE_PATTERN = /\s+/gu;

// Spiegelt ARTIST_NAME_PATTERN (record-form.ts) und NAME_PATTERN (label-form.ts).
const ARTIST_OR_LABEL_DISALLOWED_CHARACTERS_PATTERN = /[^\p{L}\p{N} \-&'./]/gu;
// Spiegelt NAME_PATTERN (genre-form.ts) — anders als Artist/Label kein "." und kein "/".
const GENRE_DISALLOWED_CHARACTERS_PATTERN = /[^\p{L}\p{N} \-&']/gu;

const ARTIST_NAME_MAX_LENGTH = 120;
const LABEL_NAME_MAX_LENGTH = 60;
const GENRE_NAME_MAX_LENGTH = 50;

/**
 * Bereinigt einen von Discogs gelieferten Namen für die Übernahme als MyMusic-Stammdatum.
 * Discogs hängt bei mehrdeutigen Namen einen Disambiguierungs-Suffix wie „ (2)" an — der
 * gehört nicht zum eigentlichen Namen und wird komplett entfernt. Alle übrigen Zeichen, die
 * das jeweilige Formular nicht erlaubt (z. B. Komma, Klammern, Anführungszeichen), werden
 * durch ein Leerzeichen ersetzt, mehrfache Leerzeichen kollabiert, das Ergebnis auf die
 * jeweilige Maximallänge gekürzt.
 */
function sanitizeDiscogsName(
  rawName: string,
  disallowedCharactersPattern: RegExp,
  maxLength: number,
): string {
  const withoutDisambiguationSuffix = rawName.replace(DISCOGS_DISAMBIGUATION_SUFFIX_PATTERN, '');
  const withoutDisallowedCharacters = withoutDisambiguationSuffix.replace(
    disallowedCharactersPattern,
    ' ',
  );
  const normalized = withoutDisallowedCharacters.replace(WHITESPACE_PATTERN, ' ').trim();

  return normalized.slice(0, maxLength).trim();
}

/** Bereinigt einen Discogs-Artist-Namen, siehe {@link sanitizeDiscogsName}. */
export function sanitizeDiscogsArtistName(rawName: string): string {
  return sanitizeDiscogsName(
    rawName,
    ARTIST_OR_LABEL_DISALLOWED_CHARACTERS_PATTERN,
    ARTIST_NAME_MAX_LENGTH,
  );
}

/** Bereinigt einen Discogs-Label-Namen, siehe {@link sanitizeDiscogsName}. */
export function sanitizeDiscogsLabelName(rawName: string): string {
  return sanitizeDiscogsName(
    rawName,
    ARTIST_OR_LABEL_DISALLOWED_CHARACTERS_PATTERN,
    LABEL_NAME_MAX_LENGTH,
  );
}

/** Bereinigt einen Discogs-Genre-/Style-Namen, siehe {@link sanitizeDiscogsName}. */
export function sanitizeDiscogsGenreName(rawName: string): string {
  return sanitizeDiscogsName(rawName, GENRE_DISALLOWED_CHARACTERS_PATTERN, GENRE_NAME_MAX_LENGTH);
}
