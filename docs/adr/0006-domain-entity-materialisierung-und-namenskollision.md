# ADR 0006 — Domain-Entity-Materialisierung via EF Core und Namenskollision Feature-Namespace/Entität

**Status**: Angenommen
**Datum**: 2026-08-04
**Betrifft**: Alle Domain-Entitäten, alle Application-Feature-Namespaces (erstmals bei `Genre`, Block 2)

## Kontext

Genre ist die erste Domain-Entität der Anwendung und legt damit zwei bisher
offene, aber grundlegende und für jede künftige Entität (Country, Label,
Artist, Record, RecordTrack) wiederkehrende technische Fragen fest.

### 1. Wie materialisiert EF Core eine Entity ohne öffentlichen Konstruktor und ohne öffentliche Setter?

`domain-regeln.md` verlangt `private set`/`private init` auf allen
Properties und einen `internal`-Konstruktor, erreichbar nur über die
statische `Create(...)`-Factory. Offen war, wie `Repository<T>`
(`context.Set<TEntity>()`) eine Entity beim Lesen aus der Datenbank
überhaupt instanziieren kann, ohne dass die Domain-Regel durchbrochen wird.

### 2. Namenskollision zwischen Application-Feature-Namespace und Domain-Entität

`application-layer.md` legt `Application/Features/{Kategorie}/{Entität}/`
fest, `domain-regeln.md` entsprechend `Domain/DomainModels/{Kategorie}/{Entität}/`.
Für Genre bedeutet das den Namespace
`MyMusic.Application.Features.Stammdaten.Genre.Commands.Create` (u. a.), in
dem die Domain-Klasse `MyMusic.Domain.DomainModels.Stammdaten.Genre.Genre`
per `global using` referenziert werden muss (Repository-Injektion,
`Genre.Create(...)`, `entity.Update(...)`).

Empirisch verifiziert (Minimalbeispiel in einer Scratch-Solution): Importiert
man den Domain-Namespace per `global using`, während der eigene Namespace
selbst auf ein Segment `Genre` endet, löst der bare Bezeichner `Genre`
nicht auf die Domain-Klasse auf, sondern auf das gleichnamige
Namespace-Segment `...Features.Stammdaten.Genre` — Ergebnis ist
`CS0118: "Genre" ist "Namespace", wird aber wie "Typ" verwendet`. Der Grund:
die C#-Namensauflösung prüft zuerst Member des unmittelbar umschließenden
Namespace-Baums, bevor `using`-Importe herangezogen werden — und
`Application.Features.Stammdaten` hat ein Member namens `Genre` (den
eigenen Feature-Namespace). Diese Kollision tritt für **jede** künftige
Entität auf, deren Name als letztes Namespace-Segment ihres eigenen
Feature-Ordners wiederkehrt — also strukturell für alle.

## Entscheidung

**Zu 1 — Materialisierung:** Der volle, `internal` deklarierte Konstruktor
jeder Entity (`internal Genre(int id, string name, Guid userId)`) ist
zugleich das Ziel von EF Cores Constructor Binding. EF Core wählt beim
Materialisieren automatisch den Konstruktor, dessen Parameter (Name und Typ,
case-insensitiv) den Properties der Entity entsprechen — unabhängig von
dessen Zugriffsmodifizierer. Ein zusätzlicher parameterloser Konstruktor
oder öffentliche Setter sind nicht nötig.

**Zu 2 — Namenskollision:** Jede Entity erhält in der `GlobalUsing.cs` des
Application-Projekts (und der zugehörigen Testprojekte) einen
Namespace-Alias nach dem Schema `{Entität}Entity`:

```csharp
global using GenreEntity = MyMusic.Domain.DomainModels.Stammdaten.Genre.Genre;
```

Application-Code referenziert die Domain-Entity ausschließlich über diesen
Alias (`GenreEntity`), nie über den bloßen Klassennamen. Domain- und
Infrastructure-Code sind nicht betroffen (deren Namespace-Bäume enthalten
kein gleichnamiges Segment) und verwenden weiterhin den echten Klassennamen.

## Begründung

- **Materialisierung**: Alternative wäre ein zusätzlicher parameterloser
  `private`-Konstruktor plus Reflection-basiertes Setzen der Backing Fields
  gewesen — unnötige Komplexität gegenüber dem von EF Core nativ
  unterstützten Constructor Binding.
- **Alias statt Restrukturierung**: Geprüfte Alternativen für die
  Namenskollision:
  - *Vollständig qualifizierter Name* (`MyMusic.Domain.DomainModels.Stammdaten.Genre.Genre`
    an jeder Verwendungsstelle) — korrekt, aber bei jedem Repository-Zugriff,
    jeder `Create`/`Update`-Verwendung wiederholt und unleserlich.
  - *Entity-Ordner ohne eigenen Unterordner* (`DomainModels/Stammdaten/Genre.cs`
    statt `DomainModels/Stammdaten/Genre/Genre.cs`) — widerspricht der in
    `domain-regeln.md` explizit vorgegebenen Struktur (der Unterordner nimmt
    zusätzlich `ValueObjects/` und `Enums/` auf) und löst das Problem ohnehin
    nicht vollständig, da die identische Kollision unabhängig davon auch im
    Application-Layer-Namespace (`Features/{Kategorie}/{Entität}/`) entsteht.
  - *Alias* (gewählt): minimaler, lokal begrenzter Eingriff, ändert weder
    Domain- noch Application-Ordnerstruktur, generalisiert unverändert auf
    jede künftige Entität.

## Konsequenzen

- Jede künftige Entität (Country, Label, Artist, Record, RecordTrack)
  benötigt denselben `{Entität}Entity`-Alias in `MyMusic.Application/GlobalUsing.cs`
  sowie in den `GlobalUsing.cs`-Dateien der Testprojekte, die den Domain-Typ
  referenzieren (`MyMusic.Domain.Tests`, `MyMusic.Application.Tests`) —
  Checkliste für neue Features (`application-layer.md`) wird entsprechend
  ergänzt.
- Innerhalb der Domain- und Infrastructure-Schicht bleibt der echte
  Klassenname (`Genre`, künftig `Country`, `Label`, …) unverändert in
  Gebrauch; der Alias ist eine Application-Layer-lokale Konvention.
