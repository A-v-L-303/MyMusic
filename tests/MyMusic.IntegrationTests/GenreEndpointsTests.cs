namespace MyMusic.IntegrationTests;

public class GenreEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GenreEndpoints_CrudPaginierungUndMandantentrennung()
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
        var unauthorizedResponse = await apiClient.GetAsync("/api/genres", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

        // act: unpaginierte Liste ohne Token -> 401
        var unauthorizedAllResponse = await apiClient.GetAsync("/api/genres/all", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAllResponse.StatusCode);

        // arrange
        var owner = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        var otherUser = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            var ownerToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, owner, cancellationToken);

            var otherToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, otherUser, cancellationToken);

            var suffix = Guid.NewGuid().ToString("N")[..8];

            var jazzName = $"Jazz-{suffix}";

            var metalName = $"Metal-{suffix}";

            // act: Pflichtfeld verletzt -> 400
            var invalidCreateResponse = await PostGenreAsync(apiClient, ownerToken, string.Empty, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidCreateResponse.StatusCode);

            // act: gültiges Genre anlegen
            var createJazzResponse = await PostGenreAsync(apiClient, ownerToken, jazzName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, createJazzResponse.StatusCode);

            var jazz = await ReadGenreAsync(createJazzResponse, cancellationToken);

            Assert.Equal(jazzName, jazz.Name);

            // act: doppelter Name für denselben Benutzer -> 409
            var duplicateResponse = await PostGenreAsync(apiClient, ownerToken, jazzName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

            // act: derselbe Name für einen anderen Benutzer ist zulässig
            var otherUsersGenreResponse = await PostGenreAsync(apiClient, otherToken, jazzName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, otherUsersGenreResponse.StatusCode);

            // arrange
            var createMetalResponse = await PostGenreAsync(apiClient, ownerToken, metalName, cancellationToken);

            var metal = await ReadGenreAsync(createMetalResponse, cancellationToken);

            // act: Liste mit Namensfilter, beschränkt auf die eigene Sammlung
            var listResponse = await GetGenresAsync(apiClient, ownerToken, suffix, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

            var list = await listResponse.Content
                .ReadFromJsonAsync<GenreListResponseDto>(_jsonOptions, cancellationToken);

            Assert.NotNull(list);
            Assert.Equal(2, list.TotalCount);
            Assert.Equal(2, list.Items.Count);

            // Sortierung nach Name: "Jazz-..." kommt alphabetisch vor "Metal-...".
            Assert.Equal(jazzName, list.Items[0].Name);
            Assert.Equal(metalName, list.Items[1].Name);

            // act: vollständige, unpaginierte Liste des Besitzers (GetAll statt GetPaged)
            var allOwnerResponse = await GetAllGenresAsync(apiClient, ownerToken, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, allOwnerResponse.StatusCode);

            var allOwner = await allOwnerResponse.Content
                .ReadFromJsonAsync<List<GenreResponseDto>>(_jsonOptions, cancellationToken);

            Assert.NotNull(allOwner);
            Assert.Contains(allOwner, genre => genre.Name == jazzName);
            Assert.Contains(allOwner, genre => genre.Name == metalName);

            // act: dieselbe vollständige Liste ist mandantengetrennt
            var allOtherResponse = await GetAllGenresAsync(apiClient, otherToken, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, allOtherResponse.StatusCode);

            var allOther = await allOtherResponse.Content
                .ReadFromJsonAsync<List<GenreResponseDto>>(_jsonOptions, cancellationToken);

            Assert.NotNull(allOther);
            Assert.Single(allOther);
            Assert.Equal(jazzName, allOther[0].Name);

            // act: Get-by-Id des Besitzers -> 200
            var getOwnResponse = await GetGenreAsync(apiClient, ownerToken, jazz.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, getOwnResponse.StatusCode);

            // act: Get-by-Id eines fremden Genres -> 404 statt 403
            var getForeignResponse = await GetGenreAsync(apiClient, otherToken, jazz.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getForeignResponse.StatusCode);

            // act: unbekannte Id -> 404
            var getUnknownResponse = await GetGenreAsync(apiClient, ownerToken, -1, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getUnknownResponse.StatusCode);

            // act: Umbenennen auf den Namen eines anderen eigenen Genres -> 409
            var conflictingUpdateResponse = await PutGenreAsync(
                apiClient, ownerToken, metal.Id, jazzName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, conflictingUpdateResponse.StatusCode);

            // act: gültiges Umbenennen -> 200
            var bluesName = $"Blues-{suffix}";

            var updateResponse = await PutGenreAsync(apiClient, ownerToken, metal.Id, bluesName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updatedMetal = await ReadGenreAsync(updateResponse, cancellationToken);

            Assert.Equal(bluesName, updatedMetal.Name);

            // act: fremdes Genre bearbeiten -> 404
            var updateForeignResponse = await PutGenreAsync(
                apiClient, otherToken, jazz.Id, "Irrelevant", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, updateForeignResponse.StatusCode);

            // act: eigenes Genre löschen -> 204
            var deleteResponse = await DeleteGenreAsync(apiClient, ownerToken, metal.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // act: gelöschtes Genre erneut abrufen -> 404
            var getDeletedResponse = await GetGenreAsync(apiClient, ownerToken, metal.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);

            // act: fremdes Genre löschen -> 404
            var deleteForeignResponse = await DeleteGenreAsync(apiClient, otherToken, jazz.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteForeignResponse.StatusCode);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, owner, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, otherUser, cancellationToken);
        }
    }

    private static async Task<GenreResponseDto> ReadGenreAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var genre = await response.Content.ReadFromJsonAsync<GenreResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(genre);

        return genre;
    }

    private static async Task<HttpResponseMessage> PostGenreAsync(
        HttpClient apiClient,
        string accessToken,
        string name,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/genres", accessToken);

        request.Content = JsonContent.Create(new { name }, options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetGenreAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/genres/{id}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetGenresAsync(
        HttpClient apiClient,
        string accessToken,
        string nameFilter,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/genres?name={nameFilter}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetAllGenresAsync(
        HttpClient apiClient,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/genres/all", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> PutGenreAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        string name,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"/api/genres/{id}", accessToken);

        request.Content = JsonContent.Create(new { name }, options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> DeleteGenreAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/genres/{id}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }
}
