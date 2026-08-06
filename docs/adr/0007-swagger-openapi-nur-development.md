# ADR 0007 — Swagger/OpenAPI: Paketwahl und Aktivierung nur in Development

**Status**: Angenommen
**Datum**: 2026-08-05
**Betrifft**: `MyMusic.Api`

## Kontext

Der Tech-Stack schreibt Swagger/OpenAPI für die Endpunkte vor (CLAUDE.md §3), verlangt
in Production eine Einschränkung der Swagger-UI auf die Admin-Rolle (§5.3) und eine
XML-`<summary>`-Pflicht an jeder Swagger-sichtbaren Endpoint-Methode (§9). Die
Wiki-Seite `tech-stack/swagger.md` markiert die konkrete Konfiguration (u. a.
Security-Definitions für Keycloak) selbst als „Prüfungsbedürftig".

Bei der Umsetzung stellten sich zwei offene Punkte heraus:

1. Es gibt zwei konkurrierende Wege zur OpenAPI-Generierung: das klassische, aus
   Generator und UI bestehende Paket `Swashbuckle.AspNetCore`, oder eine Kombination
   aus dem im .NET-10-SDK eingebauten `Microsoft.AspNetCore.OpenApi` (nur Generierung)
   mit einem separaten UI-Paket wie `Swashbuckle.AspNetCore.SwaggerUI`.
2. §5.3 verlangt eine Admin-Einschränkung der Swagger-UI in Production. Das dafür
   nötige Rollenkonzept (`User`/`Admin`) existiert im Code noch nicht — es ist Teil
   des noch offenen Blocks 7 (Authentifizierung und Mandantentrennung, siehe
   `TASK.md`).

## Entscheidung

- Paket: `Swashbuckle.AspNetCore` (Version 10.2.3 zum Zeitpunkt der Umsetzung) für
  Generierung und UI zusammen.
- Globale Bearer-Security-Definition (JWT) in `AddSwaggerGen`, damit die bereits
  ausnahmslos `.RequireAuthorization()`-geschützten Endpunkte über den
  „Authorize"-Button der UI testbar sind.
- `UseSwagger()`/`UseSwaggerUI()` ausschließlich innerhalb
  `if (app.Environment.IsDevelopment())`.
- Keine Admin-Ausnahme für Production wird vorab improvisiert. Production bleibt bis
  zur Umsetzung von Block 7 vollständig ohne Swagger-UI.

## Begründung

- `Swashbuckle.AspNetCore` entspricht wörtlich dem im Tech-Stack genannten Begriff
  „Swagger" und bringt Generierung und UI über eine einzige, etablierte Abhängigkeit
  statt über zwei separate Pakete.
- Eine Admin-Ausnahme ohne reales Rollenkonzept würde eine isolierte,
  vorgezogene Rollenprüfung erzwingen, die dem in Block 7 geplanten
  Rollen-/Ownership-Konzept vorgreifen und später widersprechen könnte. Die Lücke
  offen zu benennen ist der sauberere Weg als eine Ad-hoc-Lösung, die absehbar wieder
  verworfen wird.
- Ohne Bearer-Security-Definition wäre die Swagger-UI für die bestehenden Endpunkte
  praktisch nutzlos — jeder Testaufruf aus der UI würde ohne hinterlegtes Token mit
  401 fehlschlagen, was dem in `tech-stack/swagger.md` genannten Zweck
  („Dokumentation und Erkundung der Minimal-API-Endpunkte") widerspräche.

## Konsequenzen

- Production hat bis Block 7 keine Swagger-Dokumentation. Das ist eine bewusste,
  befristete Lücke, kein Sicherheitsrisiko: Ohne Rollenkonzept lässt sich in
  Production ohnehin nicht zwischen Admin und regulärem Benutzer unterscheiden — die
  UI wäre also entweder für niemanden oder für alle erreichbar, „für niemanden" ist
  die sichere Wahl.
- `Swashbuckle.AspNetCore` 10.x bringt transitiv `Microsoft.OpenApi` 2.x
  (OpenAPI.NET v2) mit einem gegenüber v1 geänderten Objektmodell für
  Security-Referenzen: `OpenApiSecuritySchemeReference` statt der früheren
  `OpenApiReference`/`.Reference`-Eigenschaft, `AddSecurityRequirement` erwartet einen
  `Func<OpenApiDocument, OpenApiSecurityRequirement>` statt eines statischen Objekts.
  Wer künftig an der Security-Definition arbeitet, sollte das berücksichtigen.
- `GenerateDocumentationFile` ist projektweit in `MyMusic.Api.csproj` aktiv; die
  Warnung CS1591 (fehlender XML-Kommentar) ist dort projektweit unterdrückt, weil §9
  nur für die privaten, bereits dokumentierten Endpoint-Handler-Methoden eine
  Doku-Pflicht vorsieht, nicht für jedes öffentliche Mitglied (z. B. die
  `MapXEndpoints`-Erweiterungsmethoden).
