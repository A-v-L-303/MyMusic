# Block 0e: Swagger/OpenAPI nachrüsten

## Kontext

Swagger ist an drei verbindlichen Stellen als Tech-Stack-Entscheidung dokumentiert
(Projekt-`CLAUDE.md`, Repo-`CLAUDE.md` §3/§5.3/§9, Wiki `tech-stack/swagger.md`),
existierte im Code aber nicht — kein Paket, keine Verdrahtung in `Program.cs`.
`TASK.md` erwähnte Swagger an keiner Stelle, obwohl in den bereits abgeschlossenen
Blöcken 0b, 2 (Genre) und 3 (Country) reale, autorisierte Endpunkte entstanden
sind. Es gab keinen dokumentierten „Bewusst nicht Teil"-Vermerk — die Aufnahme war
schlicht bei der Aufgabenplanung durchgerutscht. Auf Hinweis des Benutzers wird das
hier als eigener Block nachgeholt.

## Entscheidungen

Siehe `docs/adr/0007-swagger-openapi-nur-development.md` für die vollständige
Begründung. Kurzfassung, mit dem Benutzer per Rückfrage geklärt:

1. Paket `Swashbuckle.AspNetCore` (Generierung und UI zusammen) statt der
   Kombination aus dem eingebauten `Microsoft.AspNetCore.OpenApi` und einem
   separaten `Swashbuckle.AspNetCore.SwaggerUI` — entspricht wörtlich dem im
   Tech-Stack genannten Begriff „Swagger", eine Abhängigkeit statt zwei.
2. Swagger-UI wird nur in Development aktiviert. Die von CLAUDE.md §5.3 verlangte
   Admin-Einschränkung in Production kann nicht sauber umgesetzt werden, weil das
   Rollenkonzept (`User`/`Admin`) im Code noch nicht existiert (TASK.md Abschnitt 7
   ist offen); die Freischaltung wird explizit auf Block 7 verschoben.

## Umgesetzte Schritte

1. Branch `block-0e-swagger-openapi` von `main`.
2. Paket `Swashbuckle.AspNetCore` (10.2.3) zu `MyMusic.Api.csproj` hinzugefügt.
3. `MyMusic.Api.csproj`: `GenerateDocumentationFile` aktiviert, `NoWarn` um `1591`
   ergänzt (§9 verlangt XML-Kommentare nur an den privaten, bereits dokumentierten
   Endpoint-Handler-Methoden, nicht an jedem öffentlichen Mitglied wie den
   `MapXEndpoints`-Erweiterungsmethoden).
4. `Program.cs`: `AddEndpointsApiExplorer()`, `AddSwaggerGen(...)` mit globaler
   Bearer-Security-Definition und `IncludeXmlComments(...)`; `UseSwagger()`/
   `UseSwaggerUI()` ausschließlich innerhalb `app.Environment.IsDevelopment()`.
5. `GlobalUsing.cs`: `global using Microsoft.OpenApi;` ergänzt.
6. `TASK.md`: neuer Unterabschnitt „0e. Swagger/OpenAPI-Dokumentation" unter
   „0. Fundament", Verweis in Abschnitt 7 auf die verschobene Admin-Freischaltung,
   veraltete Branch-Zeile korrigiert (`block-3-country` war bereits nach `main`
   gemerged).
7. `README.md`: neuer Unterabschnitt „Swagger/OpenAPI (Block 0e)" unter „Lokale
   Entwicklung".
8. `docs/adr/0007-swagger-openapi-nur-development.md`.

## Verifikation

1. `dotnet build MyMusic.slnx` — fehlerfrei.
2. `dotnet format MyMusic.slnx --verify-no-changes` — grün.
3. `dotnet test MyMusic.slnx` — bestehende Unit- und Integrationstests weiterhin
   grün (keine fachliche Änderung).
4. AppHost gestartet, `/swagger` in Development aufgerufen: UI zeigt `/api/me`,
   `/api/genres/*`, `/api/countries` mit den vorhandenen `<summary>`-Beschreibungen;
   Testaufruf ohne „Authorize" liefert 401, mit hinterlegtem Token 200.
5. `ASPNETCORE_ENVIRONMENT=Production` lokal simuliert: `/swagger` liefert 404.

## Hinweis zur Swashbuckle-v10-API

`Swashbuckle.AspNetCore` 10.x bringt transitiv `Microsoft.OpenApi` 2.x
(OpenAPI.NET v2) mit einem gegenüber v1 geänderten Objektmodell für
Security-Referenzen. Der `using`-Namespace ist `Microsoft.OpenApi` (nicht
`Microsoft.OpenApi.Models` wie in v1/Swashbuckle 6.x). Security-Referenzen laufen
über `OpenApiSecuritySchemeReference` statt über eine `OpenApiReference`/
`.Reference`-Eigenschaft; `AddSecurityRequirement` erwartet einen
`Func<OpenApiDocument, OpenApiSecurityRequirement>`:

```csharp
options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
});

options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
{
    [new OpenApiSecuritySchemeReference("bearer", document)] = []
});
```

Dieses Muster stammt aus der offiziellen Swashbuckle.AspNetCore-Dokumentation
(`docs/configure-and-customize-swaggergen.md`, Abschnitt „Add Security Definitions
and Requirements for Bearer authentication").

## Risiken und offene Punkte

- Production bleibt bis Block 7 vollständig ohne Swagger-UI (bewusste, befristete
  Lücke, siehe ADR 0007) — kein Sicherheitsrisiko, da ohne Rollenkonzept ohnehin
  nicht zwischen Admin und regulärem Benutzer unterschieden werden könnte.
- Der manuelle Test in Schritt 4 der Verifikation (Authorize-Button, 401/200)
  wurde versucht, aber nicht abgeschlossen: Der AppHost blieb beim Start nach
  „Application host directory is: ..." hängen (kein Fortschritt über mehrere
  Minuten); ein früherer Lauf in derselben Sitzung zeigte zusätzlich
  `AddressInUseException` beim Binden des Dashboard-Ports. Es handelt sich
  **nicht** um eine Aspire/DCP-Einschränkung — Ursache war, dass die Befehle in
  dieser Sitzung über Git Bash statt PowerShell ausgeführt wurden. Keine durch
  diesen Block verursachte Regression — Build,
  `dotnet format --verify-no-changes`, Zeilenlängen-Check und alle vier
  Unit-Test-Projekte liefen dagegen fehlerfrei. Schritt 4 und 5 der
  Verifikation sind vor dem nächsten produktiven Einsatz auf einer
  funktionsfähigen lokalen Aspire-Umgebung nachzuholen.

## Nachtrag: Dashboard-Link (2026-08-05)

Nach Rückmeldung des Benutzers ("ich sehe im Dashboard keinen Link zur Swagger
UI"): `AppHost.cs` zeigte für die `api`-Ressource nur den Basis-Endpoint, keinen
Shortcut auf `/swagger`. Ergänzt in `src/MyMusic.AppHost/AppHost.cs`:

```csharp
builder.AddProject<Projects.MyMusic_Api>("api")
    ...
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Swagger UI";
        url.Url += "/swagger";
    });
```

`WithUrlForEndpoint` und die `Action<ResourceUrlAnnotation>`-Überladung wurden
vorab über die installierte `Aspire.Hosting` 13.4.6-Assembly verifiziert (nicht
geraten); der Build von `MyMusic.AppHost.csproj` kompiliert fehlerfrei. Ein
Laufzeit-Nachweis, dass der Link im Dashboard tatsächlich erscheint, steht
weiterhin aus — dieselbe Aspire/DCP-Einschränkung wie oben verhinderte den
Live-Test.
