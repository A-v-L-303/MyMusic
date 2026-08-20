# Block 7h: Admin-Benutzersuche

## Context

Die Admin-Benutzertabelle (`GET /api/admin/users`, Block 7c) zeigt aktuell nur
eine ungefilterte, paginierte Liste aller Keycloak-Benutzer
(`GetPagedUsersQuery(int Page, int PageSize)` →
`GetPagedUsersQueryHandler` → `IKeycloakAdminClient.GetUsersAsync()`, holt
alle Benutzer, sortiert und paginiert danach rein im Speicher). Der
Projektinhaber möchte eine Suche ergänzen: nach Benutzername oder E-Mail mit
Autocomplete-Vorschlägen, und zusätzlich nach der exakten Benutzer-ID (GUID).

Mit dem Projektinhaber geklärt:

- Ein einzelnes, gemeinsames Suchfeld für alle drei Kriterien.
- Erkennt das Feld eine vollständige gültige GUID, wird exakt nach dieser
  Benutzer-ID gesucht.
- Wird ein Autocomplete-Vorschlag (Name/E-Mail) ausgewählt, filtert die
  Tabelle auf genau diesen einen Benutzer.

`wiki/user-stories/user-stories-admin.md` und `wiki/architektur/api-endpunkte.md`
kennen diese Anforderung noch nicht — sie ist fachlich neu und wird mit
diesem Block ergänzt.

## Scope

**In diesem Block:**

- `GET /api/admin/users` bekommt einen optionalen `search`-Query-Parameter
  (Benutzername/E-Mail als Teilstring, vollständige Benutzer-ID exakt).
- Autocomplete im Angular-Admin-Bereich für Name/E-Mail, gespeist über
  denselben Endpunkt (etabliertes Projektmuster, kein neuer Endpunkt).
- Auswahl eines Vorschlags filtert die Tabelle auf genau einen Benutzer.
- Backend- und Frontend-Tests, Wiki-/TASK.md-Updates.

**Nicht Teil dieses Blocks:**

- Alles andere aus `TASK.md` Abschnitt 7 (Swagger-UI-Freischaltung Production,
  Rate Limiting, CORS-Production-Whitelist, CSP) — unverändert offen.
- Keine Änderung an `DELETE /api/admin/users/{id}` oder an der
  Keycloak-Rollenprüfung pro Benutzer (`IsAdmin`) — unverändert.

## Geklärt mit dir

- Ein gemeinsames Suchfeld statt getrennter Felder für Name/E-Mail und
  Benutzer-ID.
- Auswahl eines Autocomplete-Vorschlags filtert auf **genau** den gewählten
  Benutzer (nicht nur Text-Übernahme mit potenziell mehreren Treffern).
- Kein neuer ADR: reine Erweiterung bereits entschiedener, bestehender
  Muster (In-Memory-Filter analog zur bestehenden Sortierung/Paginierung in
  `GetPagedUsersQueryHandler`; Autocomplete über den bestehenden paginierten
  Endpunkt, wie bereits bei Artist/Label in `record-filter.ts` umgesetzt) —
  kein neuer Trade-off.

## Design — Backend

### 1. `GetPagedUsersQuery`

`src/MyMusic.Application/Features/Verwaltung/Admin/Queries/GetPaged/GetPagedUsersQuery.cs`
um `string? Search` erweitern:

```csharp
public sealed record GetPagedUsersQuery(int Page, int PageSize, string? Search) : IQuery<UserListResponse>;
```

### 2. `AdminEndpoints.GetPagedUsersAsync`

Neuen `string? search`-Parameter annehmen, wie `page`/`pageSize` normalisieren
(trimmen, leer/whitespace → `null`), an die Query durchreichen:

```csharp
var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
var query = new GetPagedUsersQuery(normalizedPage, normalizedPageSize, normalizedSearch);
```

`<summary>` der Methode um einen Hinweis auf die Suche ergänzen.

### 3. `GetPagedUsersQueryHandler`

Nach dem Laden aller Benutzer, vor Sortierung/Paginierung, eine private
`FilterBySearch`-Methode einfügen:

```csharp
private static IReadOnlyList<KeycloakUserSummary> FilterBySearch(
    IReadOnlyList<KeycloakUserSummary> users, string? search)
{
    if (search is null)
        return users;

    if (Guid.TryParse(search, out var userId))
        return users.Where(user => user.Id == userId).ToList();

    return users
        .Where(user =>
            user.Username.Contains(search, StringComparison.OrdinalIgnoreCase)
            || user.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
        .ToList();
}
```

Kein neuer Aufruf an `IKeycloakAdminClient` nötig — Filterung erfolgt auf der
bereits geladenen Liste, analog zur bestehenden In-Memory-Sortierung.

## Design — Frontend

### 4. `shared/autocomplete/autocomplete.ts`

`AutocompleteOption.id` von `number` auf `number | string` erweitern —
einzige Änderung an der gemeinsamen Komponente, reine Typ-Erweiterung ohne
Verhaltensänderung für die bestehende numerische Nutzung
(Artist/Label-Autocomplete in `record-filter.ts`, `record-form.ts`,
`track-form.ts`, `records.ts`).

### 5. `AdminService.getPaged`

`src/frontend/src/app/features/admin/admin.service.ts`: optionalen
`search`-Parameter ergänzen, analog zu `ArtistService.getPaged`:

```typescript
getPaged(page: number, pageSize: number, search?: string): Observable<AdminUserListResponse> {
  let params = new HttpParams().set('page', page).set('pageSize', pageSize);

  if (search) {
    params = params.set('search', search);
  }

  return this.http.get<AdminUserListResponse>(`${this.baseUrl}/users`, { params });
}
```

### 6. `Admin`-Komponente (`admin.ts`)

- Neues Signal `searchText` (aus `queryChange` des `Autocomplete`-Bausteins,
  bereits mit 300 ms debounced).
- Neues Signal `selectedUserId: string | undefined` (aus `selected`-Output).
- Neue `rxResource` `searchSuggestionsResource`, analog zu
  `artistSuggestionsResource` in `records.ts`: ruft bei nicht-leerem
  `searchText()` `adminService.getPaged(1, SUGGESTION_PAGE_SIZE, query)` auf,
  sonst leere Liste (`SUGGESTION_PAGE_SIZE = 10`, wie im übrigen Projekt).
  Ergebnis wird auf `AutocompleteOption[]` gemappt:
  `{ id: user.id, label: '${user.username} (${user.email})' }`.
- `usersResource` erhält zusätzlich
  `search: this.selectedUserId() ?? this.searchText()` als Param.
- `onSearchQueryChange(query: string)`: `searchText.set(query)`,
  `page.set(1)`.
- `onSearchSelected(option: AutocompleteOption | undefined)`:
  `selectedUserId.set(option?.id as string | undefined)`, `page.set(1)`.

Damit ist das Verhalten für alle drei Fälle korrekt, ohne dass das Frontend
selbst GUIDs erkennen muss: Freitext ohne Auswahl filtert serverseitig per
Teilstring (mehrere Treffer möglich), Auswahl eines Vorschlags filtert per ID
auf exakt einen Benutzer, eine vollständige Benutzer-ID im Freitext filtert
serverseitig ebenfalls exakt auf einen Benutzer (Backend-Logik oben).

### 7. `admin.html`

`<app-autocomplete>` oberhalb von `<app-admin-user-table>` einfügen,
Platzhalter „Name, E-Mail oder Benutzer-ID suchen", `ariaLabel` „Benutzer
suchen", `(queryChange)`/`(selected)` auf die neuen Handler verdrahtet.

## Tests

- `GetPagedUsersQueryHandlerTests.cs`: Teilstring-Treffer auf Username,
  Teilstring-Treffer auf E-Mail, Groß-/Kleinschreibung, exakte
  Benutzer-ID-Suche, keine Treffer (leeres Ergebnis, `TotalPages = 0`).
  Bestehende Tests (ohne `Search`) bleiben unverändert grün.
- `admin.service.spec.ts`: `search`-Parameter wird bei nicht-leerem Wert
  gesetzt, bei leerem Wert weggelassen.
- `admin.spec.ts`: Signal-Verdrahtung (`searchText`/`selectedUserId`),
  Page-Reset bei Sucheingabe/-auswahl, Mapping der Vorschläge.
- `autocomplete.spec.ts`: Regressionsfreiheit nach der Typ-Erweiterung
  prüfen (bestehende numerische Fälle weiterhin grün).

## Doku-Updates

- `wiki/user-stories/user-stories-admin.md`: neue User Story „US-AD5 —
  Benutzer suchen" mit den oben geklärten Akzeptanzkriterien.
- `wiki/architektur/api-endpunkte.md`: Zeile zu `GET /admin/users` um den
  optionalen `search`-Parameter ergänzen.
- `TASK.md`: neue Subsektion „### 7h. Admin-Benutzersuche" nach dem Muster
  der anderen Blöcke, Arbeits-Prompt-Referenz auf diese Datei.
- Dieses Dokument wird nach Freigabe unverändert als
  `docs/prompts/2026-08-20-block-7h-admin-benutzersuche.md` archiviert.

## Ablauf nach Freigabe

1. Branch geprüft (`git branch --show-current` → `main`, sauber), Feature-
   Branch `admin-benutzersuche` von `main` bereits angelegt.
2. Dieses Dokument als Arbeits-Prompt archiviert.
3. Umsetzung wie oben, Backend zuerst, danach Frontend.
4. `dotnet build`, `dotnet test` (Application-Tests), `dotnet format
   --verify-no-changes`; `ng test --watch=false`, `ng lint`.
5. TASK.md-/Wiki-Updates. Commit (Deutsch, echte Umlaute), Push und PR nur
   nach jeweils separater Freigabe.

## Verifikation

- Alle neuen und bestehenden Backend- und Frontend-Tests grün.
- Optional, falls gewünscht: manuelle Prüfung im laufenden Aspire-AppHost
  (Suche nach Teil-Username, Teil-E-Mail, vollständiger Benutzer-ID, Auswahl
  eines Autocomplete-Vorschlags).
