// Jeder Testfall startet einen eigenen Aspire-AppHost mit eigenen Docker-Containern
// (Postgres, Keycloak). Parallele Ausführung mehrerer Testfälle konkurriert um
// Ressourcen/Ports und führt zu Timeouts - siehe Review-Notiz zu Block 0b.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
