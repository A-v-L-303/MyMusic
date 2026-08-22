export interface ParsedDiscogsPosition {
  recordSide: string;
  trackNumber: number;
}

const SEPARATED_POSITION_PATTERN = /^([\p{L}\p{N}]{1,3})[\s\-.:]+(\d+)$/u;
const COMPACT_POSITION_PATTERN = /^(\p{L}*)(\d+)$/u;
const LETTERS_ONLY_PATTERN = /^(\p{L}{1,3})$/u;

/**
 * Discogs lässt bei einer Seite mit nur einem Track die Tracknummer weg (Position ist dann
 * nur der Seitenbuchstabe, z. B. "A" statt "A1") — das ist kein Sonderfall, sondern eine
 * reguläre, häufige Discogs-Konvention (verifiziert an einer echten Discogs-Release-Antwort,
 * z. B. Release 91831 „Atmos – Headcleaner": Seite A und C haben je genau einen Track ohne
 * Ziffer). Eine solche Position bedeutet dann implizit Tracknummer 1 auf der jeweiligen Seite.
 */
export function parseDiscogsPosition(
  position: string,
  fallbackIndex: number,
): ParsedDiscogsPosition {
  const trimmed = position.trim();

  const separatedMatch = trimmed.match(SEPARATED_POSITION_PATTERN);
  if (separatedMatch) {
    return { recordSide: separatedMatch[1].toUpperCase(), trackNumber: Number(separatedMatch[2]) };
  }

  const compactMatch = trimmed.match(COMPACT_POSITION_PATTERN);
  if (compactMatch) {
    return {
      recordSide: compactMatch[1].slice(0, 3).toUpperCase() || '0',
      trackNumber: Number(compactMatch[2]),
    };
  }

  const lettersOnlyMatch = trimmed.match(LETTERS_ONLY_PATTERN);
  if (lettersOnlyMatch) {
    return { recordSide: lettersOnlyMatch[1].toUpperCase(), trackNumber: 1 };
  }

  return { recordSide: '0', trackNumber: fallbackIndex + 1 };
}
