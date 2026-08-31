/**
 * Wandelt eine Data-URL (das serverseitig eingebettete Discogs-Cover, siehe ADR 0020) direkt
 * in ein File-Objekt um, ohne fetch() zu verwenden. Ein fetch() auf eine data:-URL wird von
 * der Content Security Policy als connect-src-Ziel geprüft und dort blockiert (siehe ADR 0028),
 * obwohl für eine bereits vollständig inline vorliegende Data-URL keine Netzwerkanfrage nötig ist.
 */
export function dataUrlToFile(dataUrl: string, filename: string): File {
  const [header, base64] = dataUrl.split(',');
  const mimeMatch = header.match(/^data:(.*);base64$/);
  const mimeType = mimeMatch?.[1] || 'application/octet-stream';

  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);

  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }

  return new File([bytes], filename, { type: mimeType });
}
