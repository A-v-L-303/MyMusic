namespace MyMusic.Infrastructure.ExternalServices.Keycloak;

public sealed class KeycloakAdminClient(HttpClient httpClient, KeycloakServiceAccountSecretProvider secretProvider)
    : IKeycloakAdminClient
{
    private const string ServiceAccountClientId = "mymusic-admin-service";

    private const string AdminRoleName = "Admin";

    private static readonly JsonSerializerOptions _caseInsensitiveOptions =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<KeycloakUserSummary>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var accessToken = await RequestAccessTokenAsync(cancellationToken);

        var users = await FetchUsersAsync(accessToken, "/admin/realms/mymusic/users?max=1000", cancellationToken);

        var summaries = new List<KeycloakUserSummary>();

        foreach (var user in users)
        {
            var isAdmin = await HasAdminRealmRoleAsync(accessToken, user.Id, cancellationToken);

            summaries.Add(new KeycloakUserSummary(
                Guid.Parse(user.Id), user.Username, user.Email ?? string.Empty, isAdmin));
        }

        return summaries;
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var accessToken = await RequestAccessTokenAsync(cancellationToken);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/admin/realms/mymusic/users/{userId}");

        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var deleteResponse = await httpClient.SendAsync(deleteRequest, cancellationToken);

        deleteResponse.EnsureSuccessStatusCode();
    }

    private async Task<List<KeycloakUserRepresentation>> FetchUsersAsync(
        string accessToken,
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var usersRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

        usersRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var usersResponse = await httpClient.SendAsync(usersRequest, cancellationToken);

        usersResponse.EnsureSuccessStatusCode();

        var users = await usersResponse.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>(
            _caseInsensitiveOptions,
            cancellationToken);

        return users ?? [];
    }

    private async Task<bool> HasAdminRealmRoleAsync(
        string accessToken,
        string userId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/realms/mymusic/users/{userId}/role-mappings/realm");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<KeycloakRoleRepresentation>>(
            _caseInsensitiveOptions,
            cancellationToken);

        return roles?.Any(role => role.Name == AdminRoleName) ?? false;
    }

    private async Task<string> RequestAccessTokenAsync(CancellationToken cancellationToken)
    {
        var secret = secretProvider.Secret
            ?? throw new InvalidOperationException("Das Keycloak-Service-Account-Secret wurde noch nicht geladen.");

        using var tokenRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/realms/mymusic/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = ServiceAccountClientId,
                ["client_secret"] = secret
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
