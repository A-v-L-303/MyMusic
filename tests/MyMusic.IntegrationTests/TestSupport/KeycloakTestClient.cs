namespace MyMusic.IntegrationTests.TestSupport;

public static class KeycloakTestClient
{
    public static async Task<KeycloakTestUser> CreateTestUserAsync(
        HttpClient keycloakClient,
        IDistributedApplicationTestingBuilder appHost,
        CancellationToken cancellationToken)
    {
        var adminToken = await RequestAdminAccessTokenAsync(keycloakClient, appHost, cancellationToken);

        var username = $"integrationtest-{Guid.NewGuid():N}";

        var password = Guid.NewGuid().ToString("N");

        using var createUserRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/realms/mymusic/users");

        createUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        createUserRequest.Content = JsonContent.Create(new
        {
            username,
            email = $"{username}@integrationtests.mymusic.invalid",
            firstName = "Integration",
            lastName = "Test",
            enabled = true,
            emailVerified = true,
            requiredActions = Array.Empty<string>(),
            credentials = new[]
            {
                new { type = "password", value = password, temporary = false }
            }
        });

        var createUserResponse = await keycloakClient.SendAsync(createUserRequest, cancellationToken);

        var createUserBody = await createUserResponse.Content.ReadAsStringAsync(cancellationToken);

        Assert.True(
            createUserResponse.StatusCode == HttpStatusCode.Created,
            $"Anlegen des Testbenutzers fehlgeschlagen: {createUserResponse.StatusCode} {createUserBody}");

        var location = createUserResponse.Headers.Location;

        Assert.NotNull(location);

        var userId = location.Segments[^1].TrimEnd('/');

        return new KeycloakTestUser(userId, username, password);
    }

    public static async Task DeleteTestUserAsync(
        HttpClient keycloakClient,
        IDistributedApplicationTestingBuilder appHost,
        KeycloakTestUser testUser,
        CancellationToken cancellationToken)
    {
        var adminToken = await RequestAdminAccessTokenAsync(keycloakClient, appHost, cancellationToken);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/admin/realms/mymusic/users/{testUser.Id}");

        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        await keycloakClient.SendAsync(deleteRequest, cancellationToken);
    }

    public static Task<string> RequestUserAccessTokenAsync(
        HttpClient keycloakClient,
        KeycloakTestUser testUser,
        CancellationToken cancellationToken)
    {
        return RequestAccessTokenAsync(
            keycloakClient,
            "mymusic",
            "mymusic-integration-tests",
            testUser.Username,
            testUser.Password,
            cancellationToken);
    }

    private static Task<string> RequestAdminAccessTokenAsync(
        HttpClient keycloakClient,
        IDistributedApplicationTestingBuilder appHost,
        CancellationToken cancellationToken)
    {
        var adminPassword = appHost.Configuration["Parameters:keycloak-admin-password"];

        Assert.False(
            string.IsNullOrWhiteSpace(adminPassword),
            "Parameters:keycloak-admin-password fehlt - bitte per 'dotnet user-secrets' im AppHost setzen.");

        return RequestAccessTokenAsync(
            keycloakClient,
            "master",
            "admin-cli",
            "admin",
            adminPassword!,
            cancellationToken);
    }

    private static async Task<string> RequestAccessTokenAsync(
        HttpClient keycloakClient,
        string realm,
        string clientId,
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var tokenEndpoint = $"/realms/{realm}/protocol/openid-connect/token";

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = clientId,
                ["username"] = username,
                ["password"] = password
            })
        };

        var tokenResponse = await keycloakClient.SendAsync(tokenRequest, cancellationToken);

        var responseBody = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);

        Assert.True(tokenResponse.IsSuccessStatusCode, $"Token-Anfrage fehlgeschlagen: {responseBody}");

        using var document = JsonDocument.Parse(responseBody);

        return document.RootElement.GetProperty("access_token").GetString()!;
    }
}
