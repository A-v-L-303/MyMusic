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
    .WithVolume("mymusic-keycloak-data", "/opt/keycloak/data")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "management")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithBindMount("../../keycloak", "/opt/keycloak/data/import", isReadOnly: true)
    .WithBindMount("../../keycloak/themes/mymusic", "/opt/keycloak/themes/mymusic", isReadOnly: true)
    .WithArgs("start-dev", "--import-realm")
    .WithHttpHealthCheck("/health/ready", endpointName: "management");

var migrator = builder.AddProject<Projects.MyMusic_Migrator>("migrator")
    .WithReference(database)
    .WaitFor(database);

var apiConnectionString = ReferenceExpression.Create(
    $"Host={postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)};" +
    $"Port={postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)};" +
    $"Database=mymusicdb;Username=mymusic_api;Password={apiDatabasePassword}");

var api = builder.AddProject<Projects.MyMusic_Api>("api")
    .WithEnvironment("ConnectionStrings__mymusicdb", apiConnectionString)
    .WithReference(seq)
    .WaitFor(seq)
    .WaitForCompletion(migrator)
    .WaitFor(keycloak)
    .WithEnvironment("Keycloak__Authority", ReferenceExpression.Create(
        $"{keycloak.GetEndpoint("http")}/realms/mymusic"))
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Swagger UI";
        url.Url += "/swagger";
    });

builder.AddJavaScriptApp("frontend", "../frontend", runScriptName: "start")
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("MYMUSIC_API_BASE_URL", api.GetEndpoint("https"))
    .WithEnvironment("MYMUSIC_KEYCLOAK_AUTHORITY", ReferenceExpression.Create(
        $"{keycloak.GetEndpoint("http")}/realms/mymusic"))
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
