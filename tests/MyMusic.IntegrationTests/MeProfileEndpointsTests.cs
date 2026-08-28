namespace MyMusic.IntegrationTests;

public class MeProfileEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task MeProfileEndpoints_ZugriffsschutzEmailKonfliktUndPasswortaenderung()
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

        // act: ohne Token -> 401 auf beiden neuen Routen
        var unauthorizedEmailResponse = await UpdateEmailAsync(
            apiClient, null, "irrelevant@example.com", cancellationToken);

        var unauthorizedPasswordResponse = await ChangePasswordAsync(
            apiClient, null, "irrelevantesPasswort1", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedEmailResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedPasswordResponse.StatusCode);

        // arrange
        var userA = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        var userB = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            var tokenA = await KeycloakTestClient.RequestUserAccessTokenAsync(keycloakClient, userA, cancellationToken);

            // act: E-Mail von userA auf die bereits von userB verwendete E-Mail aendern -> 409
            var conflictResponse = await UpdateEmailAsync(
                apiClient, tokenA, $"{userB.Username}@integrationtests.mymusic.invalid", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

            // act: E-Mail von userA auf einen neuen, freien Wert aendern -> 204
            var newEmail = $"{Guid.NewGuid():N}@integrationtests.mymusic.invalid";

            var updateEmailResponse = await UpdateEmailAsync(apiClient, tokenA, newEmail, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, updateEmailResponse.StatusCode);

            // act: Nachweis, dass die neue E-Mail wirklich im Keycloak-Account gespeichert wurde -
            // Keycloak erlaubt Login ueber die E-Mail als "username" (loginWithEmailAllowed)
            var tokenAfterEmailChange = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, userA with { Username = newEmail }, cancellationToken);

            // assert
            Assert.False(string.IsNullOrWhiteSpace(tokenAfterEmailChange));

            // act: Passwort von userA aendern (zu kurz) -> 400
            var invalidPasswordResponse = await ChangePasswordAsync(apiClient, tokenA, "kurz", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidPasswordResponse.StatusCode);

            // act: Passwort von userA gueltig aendern -> 204
            var newPassword = $"neuesPasswort-{Guid.NewGuid():N}";

            var changePasswordResponse = await ChangePasswordAsync(apiClient, tokenA, newPassword, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, changePasswordResponse.StatusCode);

            // act: Nachweis per Neu-Login mit dem neuen Passwort (Benutzername unveraendert seit
            // der Kontoerstellung, nur die E-Mail wurde oben getauscht)
            var tokenAfterPasswordChange = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, userA with { Password = newPassword }, cancellationToken);

            // assert
            Assert.False(string.IsNullOrWhiteSpace(tokenAfterPasswordChange));
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, userA, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, userB, cancellationToken);
        }
    }

    private static async Task<HttpResponseMessage> UpdateEmailAsync(
        HttpClient apiClient,
        string? accessToken,
        string newEmail,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/me/email")
        {
            Content = JsonContent.Create(new { email = newEmail })
        };

        if (accessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> ChangePasswordAsync(
        HttpClient apiClient,
        string? accessToken,
        string newPassword,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/me/password")
        {
            Content = JsonContent.Create(new { newPassword })
        };

        if (accessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }
}
