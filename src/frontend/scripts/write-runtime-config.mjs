import { writeFile } from 'node:fs/promises';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const apiBaseUrl = process.env.MYMUSIC_API_BASE_URL ?? '';
const keycloakAuthority = process.env.MYMUSIC_KEYCLOAK_AUTHORITY ?? '';

const scriptDir = dirname(fileURLToPath(import.meta.url));
const target = join(scriptDir, '..', 'public', 'runtime-config.json');

await writeFile(target, `${JSON.stringify({ apiBaseUrl, keycloakAuthority }, null, 2)}\n`, 'utf-8');

console.log(
  `runtime-config.json geschrieben: apiBaseUrl=${apiBaseUrl || '(leer)'}, keycloakAuthority=${keycloakAuthority || '(leer)'}`,
);
