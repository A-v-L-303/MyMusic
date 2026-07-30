# ADR 0006 — ID-Typ-Strategie: int/IDENTITY für Stammdaten, Guid nur für user_id

**Status**: Angenommen
**Datum**: 2026-07-30
**Betrifft**: `MyMusic.Domain/Contracts/Repository/IRepository.cs`,
`MyMusic.Infrastructure/Persistence/Repositories/Repository.cs`,
`MyMusic.Application/Common/Exceptions/ExceptionManager/ExceptionManager.cs`,
alle künftigen Stammdaten-Slices (Genre, Country, Label, Artist, Record, RecordTrack)

## Kontext

Wiki `datenbank/tabellenschema.md` legt für alle Stammdaten-Tabellen
`id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY` fest; `UUID` wird
ausschließlich für `user_id` verwendet, da Keycloak User-IDs als UUID liefert
und MyMusic keine eigene User-Tabelle führt. Das in Block 0b entstandene
generische `IRepository<TEntity>` (`GetByIdAsync(Guid id, ...)`) war jedoch
fest auf `Guid` fixiert — eine im Block-0b-Arbeits-Prompt bereits benannte,
bewusst offen gelassene Lücke ("folgt mit dem Genre-Slice").

Block 2 (Genre) wurde am 29.07.2026 vollständig implementiert, dabei aber
durchgängig mit `Guid`-IDs statt der vorgeschriebenen `int`-IDs — unter
anderem, weil das bestehende `IRepository<TEntity>` genau das nahelegt. Nach
Prüfung wurde die komplette Implementierung deshalb (zusammen mit weiteren
Wiki-Abweichungen) wieder gelöscht, ohne dass je committet wurde.

## Entscheidung

- `IRepository<TEntity, TKey>` mit `GetByIdAsync(TKey id, ...)`,
  `where TEntity : class, where TKey : notnull`.
- `Repository<TEntity, TKey>` als EF-Core-Implementierung entsprechend generisch.
- `ExceptionManager.NotFound<TId>(string entityName, TId id)` generisch.
- Stammdaten-Entitäten verwenden künftig `IRepository<TEntity, int>`; `Guid`
  bleibt reserviert für den Fall einer künftigen, an `user_id` gebundenen
  Entität (aktuell nicht geplant, da kein eigenes User-Aggregat existiert).

## Begründung

- Das Wiki ist laut CLAUDE.md §6 die verbindliche Quelle für das Datenmodell;
  `tabellenschema.md` begründet `INTEGER`/IDENTITY explizit mit Effizienz und
  einfacherer EF-Core-Konfigurierbarkeit gegenüber UUID für interne IDs.
  `UUID` ist gezielt auf `user_id` beschränkt, da nur dort ein extern
  (Keycloak) vorgegebener UUID-Wert vorliegt.
- Ein zweiter generischer Typparameter ist die einzige Lösung, die beide
  ID-Typen ohne Code-Duplikation unterstützt.
- Strukturerhaltend: Alle übrigen Methoden von `IRepository`/`Repository`
  bleiben unverändert; nur `GetByIdAsync` und der Typparameter ändern sich.

## Konsequenzen

- Jede künftige Repository-Injektion muss `TKey` explizit angeben (z. B.
  `IRepository<Genre, int>`) — mehr Schreibaufwand, aber compile-time-sicher.
- `CLAUDE.md` §4.2/§10.2 und `README.md` wurden an die neue Signatur angepasst.
- Bestehende Tests (`RepositoryTests`, `ExceptionManagerTests`) wurden an die
  neue Signatur angepasst.
- Keine Migration/Schemaänderung nötig — betrifft ausschließlich
  Code-Contracts, nicht die Datenbank.
- Offener Folgepunkt: `IRepository<T>` ist weiterhin nicht per DI in
  `MyMusic.Api`/`MyMusic.Infrastructure` verdrahtet — folgt mit dem
  Genre-Slice (unverändert gegenüber der 0b-Entscheidung, jetzt mit
  korrektem `TKey`).
