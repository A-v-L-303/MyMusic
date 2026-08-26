import { randomBytes } from 'node:crypto';
import { readFile, writeFile } from 'node:fs/promises';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const apiBaseUrl = process.env.MYMUSIC_API_BASE_URL ?? '';
const keycloakAuthority = process.env.MYMUSIC_KEYCLOAK_AUTHORITY ?? '';

function originOf(url) {
  try {
    return new URL(url).origin;
  } catch {
    return null;
  }
}

const connectSrcOrigins = ["'self'", originOf(apiBaseUrl), originOf(keycloakAuthority)]
  .filter((origin) => origin !== null)
  .join(' ');

const nonce = randomBytes(16).toString('base64');

const directives =
  [
    "default-src 'self'",
    "script-src 'self'",
    `style-src 'self' 'nonce-${nonce}'`,
    `connect-src ${connectSrcOrigins}`,
    "img-src 'self' data:",
  ].join('; ') + ';';

const metaTag = `<meta http-equiv="Content-Security-Policy" content="${directives}" />`;

const scriptDir = dirname(fileURLToPath(import.meta.url));
const indexHtmlPath = join(scriptDir, '..', 'src', 'index.html');

const indexHtml = await readFile(indexHtmlPath, 'utf-8');

const updatedIndexHtml = indexHtml
  .replace(/<meta http-equiv="Content-Security-Policy"[^>]*\/>|<!-- CSP_META_PLACEHOLDER -->/, metaTag)
  .replace(/ngCspNonce="[^"]*"/, `ngCspNonce="${nonce}"`);

await writeFile(indexHtmlPath, updatedIndexHtml, 'utf-8');

console.log(`CSP-Meta-Tag in index.html geschrieben: connect-src=${connectSrcOrigins}`);
