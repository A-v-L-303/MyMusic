namespace MyMusic.IntegrationTests;

public class ArtistEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task ArtistEndpoints_CrudPaginierungUndMandantentrennung()
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
        var unauthorizedResponse = await apiClient.GetAsync("/api/artists", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

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

            var pinkFloydName = $"Pink Floyd-{suffix}";

            var genesisName = $"Genesis-{suffix}";

            // act: Pflichtfeld verletzt -> 400
            var invalidCreateResponse = await PostArtistAsync(apiClient, ownerToken, string.Empty, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidCreateResponse.StatusCode);

            // act: gültigen Artist anlegen
            var createPinkFloydResponse = await PostArtistAsync(
                apiClient, ownerToken, pinkFloydName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, createPinkFloydResponse.StatusCode);

            var pinkFloyd = await ReadArtistAsync(createPinkFloydResponse, cancellationToken);

            Assert.Equal(pinkFloydName, pinkFloyd.Name);

            // act: doppelter Name für denselben Benutzer -> 409
            var duplicateResponse = await PostArtistAsync(apiClient, ownerToken, pinkFloydName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

            // act: derselbe Name für einen anderen Benutzer ist zulässig
            var otherUsersArtistResponse = await PostArtistAsync(
                apiClient, otherToken, pinkFloydName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, otherUsersArtistResponse.StatusCode);

            // arrange
            var createGenesisResponse = await PostArtistAsync(apiClient, ownerToken, genesisName, cancellationToken);

            var genesis = await ReadArtistAsync(createGenesisResponse, cancellationToken);

            // act: Liste mit Namensfilter, beschränkt auf die eigene Sammlung
            var listResponse = await GetArtistsAsync(apiClient, ownerToken, suffix, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

            var list = await listResponse.Content
                .ReadFromJsonAsync<ArtistListResponseDto>(_jsonOptions, cancellationToken);

            Assert.NotNull(list);
            Assert.Equal(2, list.TotalCount);
            Assert.Equal(2, list.Items.Count);

            // Sortierung nach Name: "Genesis-..." kommt alphabetisch vor "Pink Floyd-...".
            Assert.Equal(genesisName, list.Items[0].Name);
            Assert.Equal(pinkFloydName, list.Items[1].Name);

            // arrange: Label anlegen und über einen Record mit Pink Floyd verknüpfen
            var countryId = await GetCountryIdAsync(apiClient, ownerToken, cancellationToken);

            var label = await CreateLabelAsync(
                apiClient, ownerToken, $"Harvest-{suffix}", countryId, cancellationToken);

            var createRecordResponse = await PostRecordAsync(
                apiClient, ownerToken, label, pinkFloyd.Id, $"The Wall-{suffix}", cancellationToken);

            Assert.Equal(HttpStatusCode.Created, createRecordResponse.StatusCode);

            // act: Liste nach labelId gefiltert -> nur Pink Floyd (indirekt über den Record verknüpft)
            var listByLabelResponse = await GetArtistsAsync(
                apiClient, ownerToken, suffix, label, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, listByLabelResponse.StatusCode);

            var listByLabel = await listByLabelResponse.Content
                .ReadFromJsonAsync<ArtistListResponseDto>(_jsonOptions, cancellationToken);

            Assert.NotNull(listByLabel);
            Assert.Equal(1, listByLabel.TotalCount);
            Assert.Equal(pinkFloydName, listByLabel.Items[0].Name);

            // act: Get-by-Id des Besitzers -> 200
            var getOwnResponse = await GetArtistAsync(apiClient, ownerToken, pinkFloyd.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, getOwnResponse.StatusCode);

            // act: Get-by-Id eines fremden Artists -> 404 statt 403
            var getForeignResponse = await GetArtistAsync(apiClient, otherToken, pinkFloyd.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getForeignResponse.StatusCode);

            // act: unbekannte Id -> 404
            var getUnknownResponse = await GetArtistAsync(apiClient, ownerToken, -1, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getUnknownResponse.StatusCode);

            // act: Umbenennen auf den Namen eines anderen eigenen Artists -> 409
            var conflictingUpdateResponse = await PutArtistAsync(
                apiClient, ownerToken, genesis.Id, pinkFloydName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, conflictingUpdateResponse.StatusCode);

            // act: gültiges Umbenennen -> 200
            var queenName = $"Queen-{suffix}";

            var updateResponse = await PutArtistAsync(apiClient, ownerToken, genesis.Id, queenName, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updatedGenesis = await ReadArtistAsync(updateResponse, cancellationToken);

            Assert.Equal(queenName, updatedGenesis.Name);

            // act: fremden Artist bearbeiten -> 404
            var updateForeignResponse = await PutArtistAsync(
                apiClient, otherToken, pinkFloyd.Id, "Irrelevant", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, updateForeignResponse.StatusCode);

            // act: eigenen Artist löschen -> 204
            var deleteResponse = await DeleteArtistAsync(apiClient, ownerToken, genesis.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // act: gelöschten Artist erneut abrufen -> 404
            var getDeletedResponse = await GetArtistAsync(apiClient, ownerToken, genesis.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);

            // act: fremden Artist löschen -> 404
            var deleteForeignResponse = await DeleteArtistAsync(apiClient, otherToken, pinkFloyd.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteForeignResponse.StatusCode);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, owner, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, otherUser, cancellationToken);
        }
    }

    private static async Task<ArtistResponseDto> ReadArtistAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var artist = await response.Content.ReadFromJsonAsync<ArtistResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(artist);

        return artist;
    }

    private static async Task<HttpResponseMessage> PostArtistAsync(
        HttpClient apiClient,
        string accessToken,
        string name,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/artists", accessToken);

        request.Content = JsonContent.Create(new { name }, options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetArtistAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/artists/{id}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetArtistsAsync(
        HttpClient apiClient,
        string accessToken,
        string nameFilter,
        int? labelId,
        CancellationToken cancellationToken)
    {
        var query = new List<string> { $"name={nameFilter}" };

        if (labelId is not null)
            query.Add($"labelId={labelId}");

        using var request = CreateAuthorizedRequest(
            HttpMethod.Get, $"/api/artists?{string.Join('&', query)}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<int> GetCountryIdAsync(
        HttpClient apiClient,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/countries", accessToken);

        var response = await apiClient.SendAsync(request, cancellationToken);

        var countries = await response.Content
            .ReadFromJsonAsync<List<CountryResponseDto>>(_jsonOptions, cancellationToken);

        Assert.NotNull(countries);

        return countries[0].Id;
    }

    private static async Task<int> CreateLabelAsync(
        HttpClient apiClient,
        string accessToken,
        string name,
        int countryId,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/labels", accessToken);

        request.Content = JsonContent.Create(new { name, countryId }, options: _jsonOptions);

        var response = await apiClient.SendAsync(request, cancellationToken);

        var label = await response.Content.ReadFromJsonAsync<LabelResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(label);

        return label.Id;
    }

    private static async Task<HttpResponseMessage> PostRecordAsync(
        HttpClient apiClient,
        string accessToken,
        int labelId,
        int? artistId,
        string albumName,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/records", accessToken);

        request.Content = JsonContent.Create(
            new { labelId, artistId, format = "Album", albumName, releaseYear = 1969 }, options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> PutArtistAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        string name,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"/api/artists/{id}", accessToken);

        request.Content = JsonContent.Create(new { name }, options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> DeleteArtistAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/artists/{id}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }
}
