namespace MyMusic.Infrastructure.ExternalServices.Keycloak;

public sealed class KeycloakServiceAccountProvisioner(
    HttpClient httpClient,
    IConfiguration configuration,
    KeycloakServiceAccountSecretProvider secretProvider)
{
    private const string ServiceAccountClientId = "mymusic-admin-service";

    private static readonly JsonSerializerOptions _caseInsensitiveOptions =
        new() { PropertyNameCaseInsensitive = true };

    public async Task LoadSecretAsync(CancellationToken cancellationToken)
    {
        var adminAccessToken = await RequestBootstrapAdminAccessTokenAsync(cancellationToken);

        var internalClientId = await FindInternalClientIdAsync(adminAccessToken, cancellationToken);

        secretProvider.Secret = await FetchClientSecretAsync(internalClientId, adminAccessToken, cancellationToken);
    }

    private async Task<string> FindInternalClientIdAsync(string adminAccessToken, CancellationToken cancellationToken)
    {
        using var lookupRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/mymusic/clients?clientId={ServiceAccountClientId}");

        lookupRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        var lookupResponse = await httpClient.SendAsync(lookupRequest, cancellationToken);

        lookupResponse.EnsureSuccessStatusCode();

        var clients = await lookupResponse.Content.ReadFromJsonAsync<List<KeycloakClientRepresentation>>(
            _caseInsensitiveOptions,
            cancellationToken);

        return clients?.SingleOrDefault()?.Id
            ?? throw new InvalidOperationException(
                $"Keycloak-Client '{ServiceAccountClientId}' wurde nicht gefunden.");
    }

    private async Task<string> FetchClientSecretAsync(
        string internalClientId,
        string adminAccessToken,
        CancellationToken cancellationToken)
    {
        using var secretRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/mymusic/clients/{internalClientId}/client-secret");

        secretRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        var secretResponse = await httpClient.SendAsync(secretRequest, cancellationToken);

        secretResponse.EnsureSuccessStatusCode();

        var secret = await secretResponse.Content.ReadFromJsonAsync<KeycloakClientSecretRepresentation>(
            _caseInsensitiveOptions,
            cancellationToken);

        return secret?.Value
            ?? throw new InvalidOperationException(
                $"Keycloak-Client '{ServiceAccountClientId}' hat kein Secret geliefert.");
    }

    private async Task<string> RequestBootstrapAdminAccessTokenAsync(CancellationToken cancellationToken)
    {
        var username = configuration["Keycloak:BootstrapAdminUsername"];

        var password = configuration["Keycloak:BootstrapAdminPassword"];

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/realms/master/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = username ?? string.Empty,
                ["password"] = password ?? string.Empty
            })
        };

        var tokenResponse = await httpClient.SendAsync(tokenRequest, cancellationToken);

        tokenResponse.EnsureSuccessStatusCode();

        var token = await tokenResponse.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
            _caseInsensitiveOptions,
            cancellationToken);

        return token?.AccessToken
            ?? throw new InvalidOperationException("Kein Access Token von Keycloak erhalten.");
    }
}
