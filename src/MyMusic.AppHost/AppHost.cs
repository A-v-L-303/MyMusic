var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var apiDatabasePassword = builder.AddParameter("api-database-password", secret: true);
var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume("mymusic-postgres-data")
    .WithEnvironment("MYMUSIC_API_PASSWORD", apiDatabasePassword)
    .WithInitFiles("initdb");

var database = postgres.AddDatabase("mymusicdb");

var seq = builder.AddSeq("seq")
    .WithDataVolume("mymusic-seq-data");

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.5")
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithHttpEndpoint(targetPort: 9000, name: "management")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithBindMount("../../keycloak", "/opt/keycloak/data/import", isReadOnly: true)
    .WithArgs("start-dev", "--import-realm")
    .WithHttpHealthCheck("/health/ready", endpointName: "management");

var migrator = builder.AddProject<Projects.MyMusic_Migrator>("migrator")
    .WithReference(database)
    .WaitFor(database);

var apiConnectionString = ReferenceExpression.Create(
    $"Host={postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)};" +
    $"Port={postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)};" +
    $"Database=mymusicdb;Username=mymusic_api;Password={apiDatabasePassword}");

builder.AddProject<Projects.MyMusic_Api>("api")
    .WithEnvironment("ConnectionStrings__mymusicdb", apiConnectionString)
    .WithReference(seq)
    .WaitFor(seq)
    .WaitForCompletion(migrator)
    .WaitFor(keycloak)
    .WithEnvironment("Keycloak__Authority", ReferenceExpression.Create(
        $"{keycloak.GetEndpoint("http")}/realms/mymusic"));

builder.Build().Run();
