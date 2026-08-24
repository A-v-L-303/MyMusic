# ADR 0021 — Repository-Projektion für Dashboard-Aggregation

**Status**: Angenommen
**Datum**: 2026-08-24
**Betrifft**: `src/MyMusic.Domain/Contracts/Repository`,
`src/MyMusic.Infrastructure/Persistence/Repositories`,
`src/MyMusic.Application/Features/Sammlung/Dashboard`

## Kontext

Block 9 führt mit `GET /api/dashboard` den ersten Endpunkt ein, der über
mehrere Records eines Benutzers aggregiert (Verteilung nach Format,
Top-10-Artists, Top-10-Labels, Verteilung nach Erscheinungsjahr — siehe
`wiki/user-stories/user-stories-dashboard.md`). `IRepository<T>` bot dafür
bisher nur `GetPagedAsync`, das immer die vollständige Entität lädt.

Der ursprünglich geplante Ansatz (Records des Benutzers vollständig über
`GetPagedAsync` mit `pageSize: int.MaxValue` laden, analog
`GetAllArtistsQueryHandler`) wurde bei der Planung mit dem Projektinhaber
verworfen: `record` trägt `album_cover BYTEA` (bis zu 5 MB, siehe
`wiki/datenbank/tabellenschema.md`), das bei einer vollständigen
Entitäts-Ladung immer mitgeladen würde — obwohl die Dashboard-Aggregation
ausschließlich `Format`, `ArtistId`, `LabelId` und `ReleaseYear` benötigt. Bei
einer größeren Sammlung eines einzelnen Benutzers wären das potenziell
Gigabytes an unnötigen Cover-Daten im Anwendungsspeicher für eine reine
Zähl-Query.

## Entscheidung

`IRepository<T>` bekommt eine neue, generische Methode:

```csharp
Task<IReadOnlyList<TProjection>> GetProjectedAsync<TProjection>(
    Expression<Func<TEntity, bool>> filter,
    Expression<Func<TEntity, TProjection>> selector,
    CancellationToken cancellationToken);
```

Implementiert in `Repository<TEntity>` als
`_dbSet.Where(filter).Select(selector).ToListAsync(cancellationToken)`. EF
Core übersetzt den `Select`-Ausdruck in ein SQL-`SELECT`, das nur die im
Projektions-Typ referenzierten Spalten liest — `album_cover` wird für die
Dashboard-Aggregation nie aus der Datenbank gelesen. Der Dashboard-Handler
projiziert `Record` auf `RecordAggregationProjection` (`Id`, `LabelId`,
`ArtistId`, `Format`, `ReleaseYear`, bewusst ohne Cover-Feld) und aggregiert
anschließend in C# (Gruppieren, Zählen, Sortieren, Top-10).

Nur eine bestehende Implementierung von `IRepository<T>` existiert
(`Repository<TEntity>`), die Erweiterung ist rein additiv und betrifft keine
andere Verwendungsstelle.

## Verworfene Alternative — echte SQL-Aggregation (GROUP BY/COUNT)

Alternative: `IRepository<T>` um eine Methode erweitern, die dem Application
Layer erlaubt, eine vollständige Aggregations-Pipeline (`GroupBy` +
`OrderBy` + `Take`) als LINQ-Ausdruck durchzureichen, sodass PostgreSQL die
Zählung/Sortierung selbst übernimmt und nur die wenigen aggregierten Zeilen
(eine je Format/Jahr/Top-10-Eintrag) zurückgibt.

Verworfen, weil:

- Das ursprüngliche Problem (Cover-Blobs) ist bereits mit der reinen
  Spalten-Projektion vollständig gelöst — ohne Cover-Spalte ist eine
  Record-Zeile nur noch wenige Bytes groß (ein paar Ints und ein Enum-Wert).
  Selbst bei einer für eine private Sammlung unrealistisch großen Anzahl
  Records bleibt die übertragene und im Speicher gehaltene Datenmenge im
  Kilobyte-Bereich.
- Eine Pipeline-Durchreiche-Methode (`Func<IQueryable<TEntity>,
  IQueryable<TResult>>` oder vergleichbar) würde die Aufgabe des
  Repository-Patterns — Persistenzdetails vor dem Application Layer zu
  kapseln — deutlich stärker aufweichen als eine einfache Projektion: der
  Application Layer würde de facto beliebige, EF-Core-übersetzbare
  Query-Formen definieren, statt nur Filter- und Sortier-Ausdrücke wie
  bisher.
- MyMusic ist laut Projektziel eine Anwendung für **private** Musiksammlungen
  (`CLAUDE.md`), kein Analytics-Werkzeug für große Datenmengen. Eine
  DB-seitige Aggregations-Fähigkeit für eine Skalierung zu bauen, die bei
  dieser Zielgröße nicht auftritt, wäre eine Abstraktion für einen
  hypothetischen Bedarf.

## Konsequenzen

- `IRepository<T>` hat jetzt zwei Lesepfade: `GetPagedAsync` (volle Entität,
  seitenweise) und `GetProjectedAsync` (beliebige Projektion, vollständig).
  Künftige Auswertungs-/Reporting-Features können `GetProjectedAsync`
  wiederverwenden, statt erneut volle Entitäten zu laden.
- Kein neuer Unit-Test für die SQL-Übersetzung von `GetProjectedAsync` in
  `RepositoryTests.cs` — dort wird bereits heute `GetPagedAsync` (dieselbe
  Art von LINQ-Query-Übersetzung) nicht gegen den dortigen
  NSubstitute-gemockten `DbSet<TestEntity>` getestet, sondern nur einfache
  Delegationen. Reale Query-Ausführung wird über die manuelle
  Live-Verifikation gegen die laufende Anwendung abgedeckt.
- Namensauflösung für Top-Artists/Top-Labels bleibt wie bisher über gezielte
  `GetPagedAsync`-Aufrufe mit `Contains`-Filter auf den tatsächlich
  vorkommenden Ids (Muster aus `GetPagedRecordsQueryHandler`) — Artist- und
  Label-Entitäten haben keine BYTEA-Spalte, eine Projektion ist dort nicht
  nötig.
