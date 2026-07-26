# Block 0b — CQRS-Framework, generisches Repository und Auth-Smoke-Test

## Kontext

Block 0a ist abgeschlossen: Solution- und Aspire-Fundament, leere API mit
Health-Check, leerer `MyMusicDbContext` ohne DbSets, Keycloak-Container mit
Realm-Import. `MyMusic.Application` und `MyMusic.Domain` enthalten bisher keine
einzige Fachklasse (nur generierte `obj/`-Artefakte) — verifiziert vor
Prompt-Erstellung.

Laut `TASK.md` ist Block 0b der nächste, hoch priorisierte Umsetzungsblock:
CQRS-Eigenframework, generisches Repository, `ExceptionManager` und die
JWT-Verdrahtung gegen Keycloak, nachgewiesen durch einen einzelnen geschützten
Smoke-Test-Endpunkt.

**Nicht Teil von 0b**: jede Fachlogik oder Entität (erster Slice ist Genre,
Block 2 der TASK.md); Angular-Workspace (0c); Rollenkonzept, Ownership-Prüfung,
Rate Limiting, CORS-Policy und CSP (vollständig Abschnitt 7 der TASK.md
vorbehalten — dafür fehlen in 0b die Entitäten, an denen Ownership überhaupt
geprüft werden könnte).

### Klarstellung zu TASK.md Abschnitt 7

TASK.md nennt unter Abschnitt 7 die Notiz: „Wie viel Authentifizierung bereits
mit dem Walking Skeleton kommt und wie viel in diesem Block — bei Planung von
Block 0 festlegen." Diese Entscheidung ist durch die bestehende
Aufgabenliste von Block 0b bereits getroffen: 0b liefert ausschließlich
`AddJwtBearer()` plus den einzelnen Smoke-Test-Endpunkt; alles andere aus
Abschnitt 7 bleibt dort. Die Notiz ist damit inhaltlich überholt — wird als
Wiki-/TASK.md-Korrektur vorgeschlagen, nicht stillschweigend aufgelöst.

## Vorgeschlagene Schritte

### 1. CQRS-Grundgerüst (`Application/Common/CQRS/`)

- `ICommand<TResponse>`, `IQuery<TResponse>` als Marker-Interfaces.
- `ICommandHandler<TCommand, TResponse>`, `IQueryHandler<TQuery, TResponse>`.
- `IMediator` + `Mediator`: löst Handler per Reflection aus dem
  DI-Container auf (Wiki `cqrs-framework.md`).
- DI-Registrierung: Assembly-Scan über die Application-Assembly, alle Handler
  automatisch registrieren.

### 2. Validierungs-Pipeline-Decorator (FluentValidation)

- Deckt den Command-Handling-Pfad ab: vor jedem `ICommandHandler` wird der
  passende `AbstractValidator<TCommand>` (falls registriert) ausgeführt.
- Bei Validierungsfehler eine eigene `ValidationException`, die der
  `IExceptionHandler` auf HTTP 400 mapped.
- Keine DataAnnotations auf Commands (Sicherheitskonzept, CLAUDE.md §5.3).

### 3. `ExceptionManager` und `IExceptionHandler` (`Application/Common/Exceptions/ExceptionManager/`)

- `ExceptionManager` als zentrale, in Handler injizierte Exception-Factory.
- In 0b nur die generischen, entitätsunabhängigen Fälle: `ValidationException`,
  `NotFoundException`, `ConflictException`. Entitätsspezifische
  Fabrikmethoden (z. B. `ArtistNotFound`) kommen erst mit dem jeweiligen Slice.
- Zentraler `IExceptionHandler` in `MyMusic.Api`, mapped Exception-Typen auf
  HTTP-Status gemäß der Tabelle in `fehler-und-ausnahmekonzept.md`
  (400/404/409/500). Kein `try-catch` in Endpoints (CLAUDE.md §7).

### 4. Generisches Repository

- `IRepository<T>`-Contract in `MyMusic.Domain/Contracts/Repository/`
  (CLAUDE.md §4.2).
- EF-Core-Implementierung `Repository<T>` in
  `MyMusic.Infrastructure/Persistence/Repositories/`, basierend auf
  `MyMusicDbContext`.
- Ohne konkrete Entität in 0b nur generisch implementiert und per Unit Test
  (gemockter `DbContext`/`DbSet`) abgesichert — der reale Nachweis gegen
  PostgreSQL folgt mit dem Genre-Slice (Integrationstest, Block 2).

### 5. JWT-Verdrahtung gegen Keycloak

- `AddAuthentication().AddJwtBearer()` in `MyMusic.Api` mit der Authority aus
  der bereits in `AppHost.cs:44-45` gesetzten Umgebungsvariable
  `Keycloak__Authority`.
- `ICurrentUserService` (kapselt `IHttpContextAccessor`), liest `userId` aus
  dem `sub`-Claim (Sicherheitskonzept).

### 6. Smoke-Test-Endpunkt

- Ein einzelner, minimaler Endpunkt, z. B. `GET /api/me`, durch die
  CQRS-Kette geführt (`GetCurrentUserQuery` → Handler nutzt
  `ICurrentUserService`), mit `.RequireAuthorization()`.
- Zweck ausschließlich Nachweis der Kette Keycloak → JWT → Mediator — keine
  Fachlogik, kein Repository-Zugriff nötig.
- Endpunkt-Methode `private static` gemäß Codierrichtlinien.

### 7. Tests

- Unit Tests: Mediator-Auflösung (Command/Query → korrekter Handler,
  Exception bei fehlendem Handler), Validierungs-Decorator,
  `ExceptionManager`-Mapping, generisches Repository (gemockter Kontext).
- Integrationstest in `MyMusic.IntegrationTests`: Aufruf von `/api/me` ohne
  Token → 401, mit gültigem Token (gegen laufenden Keycloak-Container per
  `Aspire.Hosting.Testing`) → 200.

### 8. Dokumentation

- `TASK.md` aktualisieren (0b abgeschlossen), inkl. Korrektur der
  überholten Notiz in Abschnitt 7.
- ADR nur falls während der Umsetzung eine grundlegende, bisher offene
  Architekturentscheidung fällt (z. B. Mocking-Framework, siehe unten).
- `README.md` bei Bedarf ergänzen (neuer geschützter Endpunkt, Token-Bezug für
  lokale Tests).

## Benötigte NuGet-Pakete (Freigabe erforderlich, CLAUDE.md §12)

| Paket | Projekt | Zweck | Alternative |
|---|---|---|---|
| `FluentValidation` | Application | Validatoren für Commands | keine (Tech-Stack-Vorgabe) |
| `FluentValidation.DependencyInjectionExtensions` | Application/Api | Registrierung der Validatoren im DI-Container | manuelle Registrierung je Validator |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Api | `AddJwtBearer()` gegen Keycloak-Authority | keine (ASP.NET-Core-Bordmittel, Tech-Stack-Vorgabe) |
| `NSubstitute` | Application.Tests | `IRepository<T>` und Handler-Abhängigkeiten in Unit Tests ersetzen (Glossar „Mock") | Moq (Sicherheitsbedenken seit SponsorLink-Vorfall 2023), FakeItEasy, Hand-geschriebene Test-Doubles |
| `Aspire.Hosting.Testing` | IntegrationTests | AppHost in Tests starten für den 401/200-Nachweis | manuelles Container-Setup |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | Application | `IServiceCollection`-Erweiterungsmethode (`AddApplication()`) für die Assembly-Scan-Registrierung der Handler aus Schritt 1 | keine (Application referenziert bewusst nur die Abstractions, nicht das volle DI-Container-Paket) |
| `Microsoft.Extensions.DependencyInjection` | Application.Tests | Konkreten `ServiceCollection`-Container in den DI-Registrierungstests von `AddApplication()` auflösen | manuelles Reflection-Assert ohne echte Auflösung — weniger aussagekräftig |

Das Wiki (Glossar, `unit-tests.md`) verlangt Mocks für `IRepository<T>`, legt
sich aber auf kein konkretes Framework fest. Entscheidung für `NSubstitute`:
klare, lesbare Syntax für reines Interface-Mocking, keine Sicherheitsvorfälle,
aktiv gepflegt.

## Verifikation

1. `dotnet build` über die Solution — fehlerfrei.
2. `dotnet test` für die Test-Projekte Domain/Application/Api — neue Tests
   grün.
3. Integrationstest: `/api/me` ohne Token → 401; mit Token (via
   Keycloak-Token-Endpoint im Testkontext geholt) → 200.
4. `dotnet format --verify-no-changes`, Zeilenlängen-Check — CI-Gate (Block
   0d) bleibt grün.
5. Manueller Nachweis wie in 0a: AppHost starten, `curl` ohne/mit Token gegen
   `/api/me`.

## Risiken und offene Punkte

- Generisches `IRepository<T>` bleibt in 0b ungetestet gegen echtes
  PostgreSQL — EF-Core-Besonderheiten (Tracking, Includes) fallen frühestens
  beim Genre-Slice auf.
- `Repository<T>.GetAllAsync` ist per Unit Test **nicht** absicherbar: `ToListAsync()`
  verlangt, dass das `IQueryable<T>` zugleich `IAsyncEnumerable<T>` ist; ein
  reiner NSubstitute-Mock von `DbSet<T>` implementiert das in der verwendeten
  EF-Core-Version nicht (empirisch geprüft — `InvalidCastException` beim
  Versuch). Ohne `Microsoft.EntityFrameworkCore.InMemory` (neues, nicht
  freigegebenes Paket) oder einen größeren selbstgebauten
  `IAsyncQueryProvider`-Test-Double bleibt diese Methode ungetestet; der reale
  Nachweis folgt mit dem Genre-Slice-Integrationstest.
- Integrationstest-Timeout bei gemeinsamer Ausführung beider
  `MyMusic.IntegrationTests`-Testklassen behoben: `AssemblyInfo.cs` schaltet
  Testparallelität für diese Assembly aus (`[CollectionBehavior(DisableTestParallelization = true)]`),
  da beide Tests einen eigenen Aspire-Stack samt Docker-Containern starten und
  sich bei Parallelausführung Ressourcen streitig machen.
- Keycloak-Token-Beschaffung im Integrationstest (Grant Type für Testzwecke,
  Test-User im Realm-JSON) ist im Wiki nicht beschrieben — wird während der
  Umsetzung geklärt und ggf. als Wiki-Ergänzung vorgeschlagen, nicht
  stillschweigend entschieden.
- Design des Smoke-Test-Endpunkts (`GET /api/me`) ist ein Vorschlag dieses
  Plans, im Wiki nicht vorgegeben — bei Bedarf anzupassen.
- TASK.md Abschnitt 7 enthält eine durch die 0b-Aufgabenliste überholte Notiz
  (siehe „Klarstellung" oben) — Korrektur ist Teil von Schritt 8.
