#!/bin/bash
# Legt die Datenbank und die eingeschraenkte Rolle fuer die API an.
#
# Berechtigungskonzept (Wiki architektur/aspire-orchestrierung.md):
#   Migrator = DDL + DML (nutzt den privilegierten POSTGRES_USER)
#   API      = nur DML   (nutzt mymusic_api)
#
# Achtung: Das Postgres-Image fuehrt dieses Skript ausschliesslich aus, wenn das
# Datenverzeichnis leer ist. Zusammen mit dem Datenvolume laeuft es also genau einmal.
# Nach Aenderungen muss das Volume verworfen werden, sonst greifen sie nicht.
set -e

if [ -z "${MYMUSIC_API_PASSWORD}" ]; then
  echo "MYMUSIC_API_PASSWORD ist nicht gesetzt - Abbruch." >&2
  exit 1
fi

psql -v ON_ERROR_STOP=1 --username "${POSTGRES_USER}" --dbname postgres <<-EOSQL
    SELECT 'CREATE DATABASE mymusicdb' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'mymusicdb')\gexec
    CREATE ROLE mymusic_api WITH LOGIN PASSWORD '${MYMUSIC_API_PASSWORD}';
EOSQL

psql -v ON_ERROR_STOP=1 --username "${POSTGRES_USER}" --dbname mymusicdb <<-EOSQL
    GRANT CONNECT ON DATABASE mymusicdb TO mymusic_api;

    -- USAGE erlaubt den Zugriff auf das Schema, CREATE wird bewusst nicht gewaehrt.
    GRANT USAGE ON SCHEMA public TO mymusic_api;

    -- Entscheidend: Ohne DEFAULT PRIVILEGES haette die API keinerlei Rechte an den
    -- Tabellen, die der Migrator erst spaeter anlegt.
    ALTER DEFAULT PRIVILEGES FOR ROLE ${POSTGRES_USER} IN SCHEMA public
        GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO mymusic_api;
    ALTER DEFAULT PRIVILEGES FOR ROLE ${POSTGRES_USER} IN SCHEMA public
        GRANT USAGE, SELECT ON SEQUENCES TO mymusic_api;
EOSQL

echo "Rolle mymusic_api und Datenbank mymusicdb eingerichtet."
