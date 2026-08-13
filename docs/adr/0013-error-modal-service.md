# ADR 0013 — Zentraler `ErrorModalService` für die Fehlerdarstellung

**Status**: Angenommen
**Datum**: 2026-08-13
**Betrifft**: `src/frontend`

## Kontext

`architektur/fehler-und-ausnahmekonzept.md` legt fest, *welche* Fehlerklasse
*wie* dargestellt wird (400 → Inline, 404/409/500/429/Netzwerk → Modal,
401/403 → bereits vom Interceptor behandelt), aber nicht *wie* das technisch
umgesetzt wird. Diese Lücke war als Punkt 1 in
`03 Ressourcen/offene-punkte-angular-feature-slices.md` vermerkt und musste
für den ersten echten CRUD-Slice (Genre) entschieden werden, da sie sonst pro
Feature-Slice unterschiedlich und inkonsistent gelöst würde.

Drei Alternativen standen zur Wahl:

1. **Globaler HTTP-Interceptor**, der jeden fehlgeschlagenen Request
   abfängt und zentral eine Fehlermeldung anzeigt.
2. **`catchError` je Service-Aufruf**, mit lokal dupliziertem
   Mapping-Code in jeder Komponente.
3. **Zentraler, injizierbarer Service** (`ErrorModalService`) mit
   Signal-State, den jede Komponente nach Bedarf explizit aufruft.

## Entscheidung

Zentraler `ErrorModalService` (`shared/error-modal/error-modal.service.ts`,
`providedIn: 'root'`):

- `showFromHttpError(error: HttpErrorResponse, entityName: string, onRetry?: () => void)`
  mappt den HTTP-Status auf einen von fünf Zuständen (`not-found`,
  `conflict`, `server`, `rate-limit`, `network`) und setzt ein Signal
  (`current`), das die begleitende `ErrorModal`-Komponente konsumiert.
- 401/403 werden explizit ignoriert (bereits vom bestehenden
  `unauthorizedRedirectInterceptor` behandelt, siehe ADR 0009/0010-Umfeld).
- Die Meldung für 409 wird, sofern vorhanden, aus dem `detail`-Feld des
  `ProblemDetails`-Bodys übernommen (siehe `GlobalExceptionHandler.cs`) —
  damit steuert das Backend den Wortlaut, nicht ein hartkodierter
  Frontend-Text.
- `ErrorModal` (die Komponente) wird **einmalig** in `app.html` neben
  `<app-nav />` gemountet und gilt ab sofort global für alle künftigen
  CRUD-Slices.
- Validierungsfehler (HTTP 400) laufen **nicht** über diesen Service — sie
  werden im jeweiligen Formular über `submit(form, action)` direkt inline
  ins betroffene Feld eingehängt (siehe `genre-form.ts`), da 400 laut
  Fehlerkonzept grundsätzlich Inline und nicht als Modal dargestellt wird.

## Begründung

- Ein globaler HTTP-Interceptor (Alternative 1) kann zwar jeden Fehler
  zentral abfangen, kennt aber den fachlichen Kontext nicht (welche Entität,
  welcher Retry-Callback bei Netzwerkfehlern) — das müsste er sich über
  Zusatzmetadaten am Request erst wieder beschaffen. Der explizite
  Service-Aufruf aus der jeweiligen Komponente heraus (Alternative 3) hat
  diesen Kontext ohnehin bereits vorliegen (`entityName`, `reload()` als
  `onRetry`).
- `catchError` je Service-Aufruf (Alternative 2) hätte das
  Status-Code-zu-Darstellung-Mapping in jeder Komponente dupliziert — bei
  sechs Fehlerklassen und künftig vier weiteren CRUD-Slices (Label, Artist,
  Record, Search) ein erhebliches Duplikations- und Inkonsistenzrisiko.
- Ein zentraler Service mit einmalig gemounteter Komponente bündelt das
  Mapping an einer Stelle, während der Aufruf aus der jeweiligen Komponente
  heraus den fachlichen Kontext (Entitätsname, Retry-Aktion) erhält, ohne
  ihn künstlich an den HTTP-Request anhängen zu müssen.

## Konsequenzen

- Jeder künftige CRUD-Slice injiziert `ErrorModalService` und ruft
  `showFromHttpError(error, '<Entität>', retryCallback?)` in den
  Fehlerpfaden von Service-Aufrufen und dem `error()`-Signal der jeweiligen
  `rxResource` auf — kein weiterer eigener Modal-Mechanismus nötig.
- Der Service kennt nur die fünf oben genannten Zustände; ein sechster Fall
  (z. B. eine Discogs-spezifische Fehlermeldung, siehe
  `fehler-und-ausnahmekonzept.md`) müsste bei Bedarf um einen weiteren
  `ErrorModalKind` erweitert werden.
- 400-Validierungsfehler bleiben bewusst außerhalb dieses Service — jedes
  Formular ist selbst für die Inline-Darstellung zuständig, da sie an ein
  konkretes Feld gebunden ist.
