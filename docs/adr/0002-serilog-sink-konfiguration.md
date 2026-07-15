# ADR 0002 — Serilog-Sink-Konfiguration und Abgrenzung zu OpenTelemetry

**Status**: Angenommen
**Datum**: 2026-07-15
**Betrifft**: `MyMusic.Api`, Logging-Stack

## Kontext

Der Tech-Stack schreibt Serilog mit Seq als strukturiertes Logging vor (CLAUDE.md §3). Die
Wiki-Seite `tech-stack/serilog-seq.md` markiert die konkrete Sink-Konfiguration jedoch selbst
als offen: „Konkrete Sink- und Sink-Konfiguration ist in den Quellen bisher nicht beschrieben.
Prüfungsbedürftig."

Bei der Umsetzung von Block 0a stellte sich heraus, dass es zwei konkurrierende Wege gibt,
Logs nach Seq zu bringen:

1. Die offizielle Aspire-Client-Integration (`Aspire.Seq` mit `AddSeqEndpoint()`) exportiert
   Logs und Traces per **OpenTelemetry-OTLP** an Seq — sie nutzt gerade keine Serilog-Sinks.
2. Der klassische Serilog-Weg über `Serilog.Sinks.Seq` schickt Logs direkt an die native
   Ingestion-API von Seq.

Beide Wege gleichzeitig zu betreiben, würde jedes Ereignis doppelt in Seq ablegen.

## Entscheidung

Es wird der Serilog-Weg gewählt:

- `Serilog.AspNetCore` als Logging-Provider.
- Sinks: `Serilog.Sinks.Console` (Startup-Logs im Aspire-Dashboard) und `Serilog.Sinks.Seq`.
- Die Seq-URL stammt aus der von Aspire injizierten Verbindungsinformation
  (`ConnectionStrings:seq`). Fehlt sie, bleibt nur die Console — dadurch laufen Tests und
  Szenarien ohne Seq ohne Sonderfall.
- `UseSerilogRequestLogging()` erzeugt das Request-Log.
- In `appsettings.json` wird `Serilog:MinimumLevel:Override:Microsoft.AspNetCore` auf
  `Warning` gesetzt.
- Traces und Metriken laufen unverändert über OpenTelemetry aus den ServiceDefaults an das
  Aspire-Dashboard. `Aspire.Seq` wird **nicht** verwendet.

## Begründung

- Entspricht der Wiki-Vorgabe wörtlich („Serilog mit Seq").
- Das Sicherheitskonzept verlangt, `Authorization`- und `Cookie`-Header aus dem Request-Log
  auszuschließen. Das Request-Logging von `Serilog.AspNetCore` wird dafür ohnehin benötigt.
- Ein Ingest-Pfad statt zweier — kein Risiko doppelter Events.

Der `MinimumLevel.Override` ist nicht optional: Ohne ihn protokollieren das eingebaute
Request-Logging von ASP.NET Core (`Microsoft.AspNetCore.Hosting.Diagnostics`) **und**
`Serilog.AspNetCore.RequestLoggingMiddleware` denselben Request. Im ersten Testlauf von
Block 0a waren dadurch 8 Ereignisse aus dem eingebauten Logging neben 4 Serilog-Ereignissen
in Seq sichtbar.

## Konsequenzen

- **Seq sieht keine OpenTelemetry-Traces.** Eine Log-Trace-Korrelation innerhalb von Seq ist
  damit nicht möglich; Traces stehen nur im Aspire-Dashboard. Wird die Korrelation später
  gewünscht, ist der Wechsel auf `Serilog.Sinks.OpenTelemetry` oder `Aspire.Seq` eine eigene
  Entscheidung samt Wiki-Aktualisierung.
- Informationsmeldungen aus dem `Microsoft.AspNetCore`-Namespace erscheinen nicht mehr in
  Seq. Im Aspire-Dashboard bleiben sie über den OpenTelemetry-Pfad sichtbar.
- Die Anforderung „keine sensiblen Header im Log" ist erfüllt, aber nicht durch einen aktiven
  Ausschluss: `UseSerilogRequestLogging()` protokolliert von sich aus nur Methode, Pfad,
  Statuscode und Dauer — es gibt keine Header, die auszuschließen wären. Wer das Request-Log
  später über `EnrichDiagnosticContext` erweitert, muss die Anforderung erneut prüfen.
