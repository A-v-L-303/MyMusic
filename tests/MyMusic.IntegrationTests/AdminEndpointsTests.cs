namespace MyMusic.IntegrationTests;

public class AdminEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task AdminEndpoints_ZugriffsschutzListeUndLoeschungMitAppDaten()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyMusic_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("migrator", KnownResourceStates.Finished, cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("api", KnownResourceStates.Running, cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        using var apiClient = app.CreateHttpClient("api", "http");
        using var keycloakClient = app.CreateHttpClient("keycloak", "http");

        // act
        var unauthorizedResponse = await apiClient.GetAsync("/api/admin/users", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

        // arrange
        var admin = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        var plainUser = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            await KeycloakTestClient.AssignAdminRoleAsync(keycloakClient, appHost, admin, cancellationToken);

            var adminToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, admin, cancellationToken);

            var plainUserToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, plainUser, cancellationToken);

            // act: authentifiziert, aber ohne Admin-Rolle -> 403
            var forbiddenResponse = await GetUsersAsync(apiClient, plainUserToken, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

            // act: mit Admin-Rolle -> 200, beide Testbenutzer mit korrekter Rolle enthalten
            var listResponse = await GetUsersAsync(apiClient, adminToken, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

            var list = await listResponse.Content
                .ReadFromJsonAsync<UserListResponseDto>(_jsonOptions, cancellationToken);

            Assert.NotNull(list);

            Assert.Contains(list.Items, user => user.Id.ToString() == admin.Id && user.Role == "Admin");

            Assert.Contains(list.Items, user => user.Id.ToString() == plainUser.Id && user.Role == "User");

            // act: Selbstlöschung -> 409
            var selfDeleteResponse = await DeleteUserAsync(apiClient, adminToken, admin.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, selfDeleteResponse.StatusCode);

            // arrange: App-Daten für den zu löschenden Benutzer anlegen
            using var createGenreRequest = new HttpRequestMessage(HttpMethod.Post, "/api/genres");

            createGenreRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", plainUserToken);

            createGenreRequest.Content = JsonContent.Create(new { name = $"Testgenre-{Guid.NewGuid():N}" });

            var createGenreResponse = await apiClient.SendAsync(createGenreRequest, cancellationToken);

            Assert.Equal(HttpStatusCode.Created, createGenreResponse.StatusCode);

            // act: fremden Benutzer löschen -> 204
            var deleteResponse = await DeleteUserAsync(apiClient, adminToken, plainUser.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // act: der gelöschte Benutzer erscheint nicht mehr in der Keycloak-Liste
            var listAfterDeleteResponse = await GetUsersAsync(apiClient, adminToken, cancellationToken);

            var listAfterDelete = await listAfterDeleteResponse.Content
                .ReadFromJsonAsync<UserListResponseDto>(_jsonOptions, cancellationToken);

            // assert
            Assert.NotNull(listAfterDelete);

            Assert.DoesNotContain(listAfterDelete.Items, user => user.Id.ToString() == plainUser.Id);

            // act: die App-Daten (Genre) des gelöschten Benutzers sind aus der Datenbank entfernt
            await using var connection = new NpgsqlConnection(
                await BuildApiConnectionStringAsync(app, appHost, cancellationToken));

            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(
                "SELECT count(*) FROM genre WHERE user_id = @userId;", connection);

            command.Parameters.AddWithValue("userId", Guid.Parse(plainUser.Id));

            var remainingGenreCount = await command.ExecuteScalarAsync(cancellationToken);

            // assert
            Assert.Equal(0L, remainingGenreCount);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, admin, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, plainUser, cancellationToken);
        }
    }

    private static async Task<HttpResponseMessage> GetUsersAsync(
        HttpClient apiClient,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/users");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> DeleteUserAsync(
        HttpClient apiClient,
        string accessToken,
        string targetUserId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/users/{targetUserId}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
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

        Assert.False(
            string.IsNullOrWhiteSpace(apiPassword),
            "Parameters:api-database-password fehlt - bitte per 'dotnet user-secrets' im AppHost setzen.");

        return new NpgsqlConnectionStringBuilder(privilegedConnectionString)
        {
            Username = "mymusic_api",
            Password = apiPassword
        }.ConnectionString;
    }
}
