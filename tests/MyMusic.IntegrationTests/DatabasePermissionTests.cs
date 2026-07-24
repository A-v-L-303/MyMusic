namespace MyMusic.IntegrationTests;

public class DatabasePermissionTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task ApiRolleDarfLesenUndSchreibenAberKeinSchemaAendern()
    {
        var cancellationToken = CancellationToken.None;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyMusic_AppHost>(cancellationToken);
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("migrator", KnownResourceStates.Finished, cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await using var connection = new NpgsqlConnection(
            await BuildApiConnectionStringAsync(app, appHost, cancellationToken));
        await connection.OpenAsync(cancellationToken);

        // Der Verbindungsaufbau selbst beweist bereits, dass die Rolle existiert und CONNECT hat.
        await using (var command = new NpgsqlCommand("SELECT current_user;", connection))
        {
            var currentUser = await command.ExecuteScalarAsync(cancellationToken);
            Assert.Equal("mymusic_api", currentUser);
        }

        // DML muss erlaubt sein: Die Tabelle gehoert dem Migrator, die API greift ueber
        // ALTER DEFAULT PRIVILEGES darauf zu.
        await using (var command = new NpgsqlCommand("""SELECT count(*) FROM "__EFMigrationsHistory";""", connection))
        {
            var count = await command.ExecuteScalarAsync(cancellationToken);
            Assert.NotNull(count);
        }

        // DDL muss verboten sein.
        await using (var command = new NpgsqlCommand("CREATE TABLE permission_probe (id int);", connection))
        {
            var exception = await Assert.ThrowsAsync<PostgresException>(
                () => command.ExecuteNonQueryAsync(cancellationToken));

            // 42501 = insufficient_privilege
            Assert.Equal("42501", exception.SqlState);
        }
    }

    // Aspire injiziert der API ihren Connection String als Umgebungsvariable, nicht als
    // abrufbare Ressource. Fuer den Test wird er deshalb aus dem privilegierten String
    // (Host und Port) und den Parametern der API-Rolle zusammengesetzt.
    private static async Task<string> BuildApiConnectionStringAsync(
        DistributedApplication app,
        IDistributedApplicationTestingBuilder appHost,
        CancellationToken cancellationToken)
    {
        var privilegedConnectionString = await app.GetConnectionStringAsync("mymusicdb", cancellationToken);
        Assert.NotNull(privilegedConnectionString);

        var apiPassword = appHost.Configuration["Parameters:api-database-password"];
        Assert.False(string.IsNullOrWhiteSpace(apiPassword),
            "Parameters:api-database-password fehlt - bitte per 'dotnet user-secrets' im AppHost setzen.");

        return new NpgsqlConnectionStringBuilder(privilegedConnectionString)
        {
            Username = "mymusic_api",
            Password = apiPassword
        }.ConnectionString;
    }
}
