namespace MyMusic.IntegrationTests;

public class LabelEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task LabelEndpoints_CrudFilterPaginierungUndMandantentrennung()
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
        var unauthorizedResponse = await apiClient.GetAsync("/api/labels", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

        // act: unpaginierte Liste ohne Token -> 401
        var unauthorizedAllResponse = await apiClient.GetAsync("/api/labels/all", cancellationToken);

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

            var (germanyId, otherCountryId) = await GetTwoCountryIdsAsync(apiClient, ownerToken, cancellationToken);

            var suffix = Guid.NewGuid().ToString("N")[..8];

            var roughTradeName = $"Rough Trade-{suffix}";

            var subPopName = $"Sub Pop-{suffix}";

            // act: Pflichtfeld verletzt -> 400
            var invalidNameResponse = await PostLabelAsync(
                apiClient, ownerToken, string.Empty, germanyId, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidNameResponse.StatusCode);

            // act: verbotenes Zeichen (Klammer) -> 400
            var invalidCharacterResponse = await PostLabelAsync(
                apiClient, ownerToken, "Parlophone (UK)", germanyId, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidCharacterResponse.StatusCode);

            // act: nicht existierendes Land -> 400
            var invalidCountryResponse = await PostLabelAsync(
                apiClient, ownerToken, roughTradeName, -1, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidCountryResponse.StatusCode);

            // act: gültiges Label anlegen
            var createRoughTradeResponse = await PostLabelAsync(
                apiClient, ownerToken, roughTradeName, germanyId, "Unabhängiges Label", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, createRoughTradeResponse.StatusCode);

            var roughTrade = await ReadLabelAsync(createRoughTradeResponse, cancellationToken);

            Assert.Equal(roughTradeName, roughTrade.Name);
            Assert.Equal(germanyId, roughTrade.CountryId);
            Assert.Equal("Unabhängiges Label", roughTrade.Information);

            // act: doppelter Name für denselben Benutzer -> 409
            var duplicateResponse = await PostLabelAsync(
                apiClient, ownerToken, roughTradeName, germanyId, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

            // act: derselbe Name für einen anderen Benutzer ist zulässig
            var otherUsersLabelResponse = await PostLabelAsync(
                apiClient, otherToken, roughTradeName, germanyId, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, otherUsersLabelResponse.StatusCode);

            // arrange
            var createSubPopResponse = await PostLabelAsync(
                apiClient, ownerToken, subPopName, otherCountryId, null, cancellationToken);

            var subPop = await ReadLabelAsync(createSubPopResponse, cancellationToken);

            // act: Liste mit Namensfilter, beschränkt auf die eigene Sammlung
            var listResponse = await GetLabelsAsync(apiClient, ownerToken, suffix, countryId: null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

            var list = await listResponse.Content
                .ReadFromJsonAsync<LabelListResponseDto>(_jsonOptions, cancellationToken);

            Assert.NotNull(list);
            Assert.Equal(2, list.TotalCount);
            Assert.Equal(2, list.Items.Count);

            // Sortierung nach Name: "Rough Trade-..." kommt alphabetisch vor "Sub Pop-...".
            Assert.Equal(roughTradeName, list.Items[0].Name);
            Assert.Equal(subPopName, list.Items[1].Name);

            // act: Liste zusätzlich nach Land gefiltert
            var listByCountryResponse = await GetLabelsAsync(
                apiClient, ownerToken, suffix, countryId: otherCountryId, cancellationToken);

            var listByCountry = await listByCountryResponse.Content
                .ReadFromJsonAsync<LabelListResponseDto>(_jsonOptions, cancellationToken);

            // assert
            Assert.NotNull(listByCountry);
            Assert.Equal(1, listByCountry.TotalCount);
            Assert.Equal(subPopName, listByCountry.Items[0].Name);

            // act: vollständige, unpaginierte Liste des Besitzers (GetAll statt GetPaged)
            var allOwnerResponse = await GetAllLabelsAsync(apiClient, ownerToken, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, allOwnerResponse.StatusCode);

            var allOwner = await allOwnerResponse.Content
                .ReadFromJsonAsync<List<LabelResponseDto>>(_jsonOptions, cancellationToken);

            Assert.NotNull(allOwner);
            Assert.Contains(allOwner, label => label.Name == roughTradeName);
            Assert.Contains(allOwner, label => label.Name == subPopName);

            // act: dieselbe vollständige Liste ist mandantengetrennt
            var allOtherResponse = await GetAllLabelsAsync(apiClient, otherToken, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, allOtherResponse.StatusCode);

            var allOther = await allOtherResponse.Content
                .ReadFromJsonAsync<List<LabelResponseDto>>(_jsonOptions, cancellationToken);

            Assert.NotNull(allOther);
            Assert.Single(allOther);
            Assert.Equal(roughTradeName, allOther[0].Name);

            // act: Get-by-Id des Besitzers -> 200
            var getOwnResponse = await GetLabelAsync(apiClient, ownerToken, roughTrade.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, getOwnResponse.StatusCode);

            // act: Get-by-Id eines fremden Labels -> 404 statt 403
            var getForeignResponse = await GetLabelAsync(apiClient, otherToken, roughTrade.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getForeignResponse.StatusCode);

            // act: unbekannte Id -> 404
            var getUnknownResponse = await GetLabelAsync(apiClient, ownerToken, -1, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getUnknownResponse.StatusCode);

            // act: Umbenennen auf den Namen eines anderen eigenen Labels -> 409
            var conflictingUpdateResponse = await PutLabelAsync(
                apiClient, ownerToken, subPop.Id, roughTradeName, otherCountryId, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, conflictingUpdateResponse.StatusCode);

            // act: gültiges Umbenennen mit Länderwechsel -> 200
            var nirvanaName = $"Nirvana Records-{suffix}";

            var updateResponse = await PutLabelAsync(
                apiClient, ownerToken, subPop.Id, nirvanaName, germanyId, "Neu", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updatedSubPop = await ReadLabelAsync(updateResponse, cancellationToken);

            Assert.Equal(nirvanaName, updatedSubPop.Name);
            Assert.Equal(germanyId, updatedSubPop.CountryId);
            Assert.Equal("Neu", updatedSubPop.Information);

            // act: fremdes Label bearbeiten -> 404
            var updateForeignResponse = await PutLabelAsync(
                apiClient, otherToken, roughTrade.Id, "Irrelevant", germanyId, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, updateForeignResponse.StatusCode);

            // act: eigenes Label löschen -> 204
            var deleteResponse = await DeleteLabelAsync(apiClient, ownerToken, subPop.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // act: gelöschtes Label erneut abrufen -> 404
            var getDeletedResponse = await GetLabelAsync(apiClient, ownerToken, subPop.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);

            // act: fremdes Label löschen -> 404
            var deleteForeignResponse = await DeleteLabelAsync(apiClient, otherToken, roughTrade.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteForeignResponse.StatusCode);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, owner, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, otherUser, cancellationToken);
        }
    }

    private static async Task<(int GermanyId, int OtherCountryId)> GetTwoCountryIdsAsync(
        HttpClient apiClient,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/countries");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await apiClient.SendAsync(request, cancellationToken);

        var countries = await response.Content
            .ReadFromJsonAsync<List<CountryResponseDto>>(_jsonOptions, cancellationToken);

        Assert.NotNull(countries);

        var germany = countries.Single(country => country.Code == "DE");

        var otherCountry = countries.First(country => country.Code != "DE");

        return (germany.Id, otherCountry.Id);
    }

    private static async Task<LabelResponseDto> ReadLabelAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var label = await response.Content.ReadFromJsonAsync<LabelResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(label);

        return label;
    }

    private static async Task<HttpResponseMessage> PostLabelAsync(
        HttpClient apiClient,
        string accessToken,
        string name,
        int countryId,
        string? information,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/labels", accessToken);

        request.Content = JsonContent.Create(new { name, countryId, information }, options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetLabelAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/labels/{id}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetLabelsAsync(
        HttpClient apiClient,
        string accessToken,
        string nameFilter,
        int? countryId,
        CancellationToken cancellationToken)
    {
        var url = countryId is null
            ? $"/api/labels?name={nameFilter}"
            : $"/api/labels?name={nameFilter}&countryId={countryId}";

        using var request = CreateAuthorizedRequest(HttpMethod.Get, url, accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetAllLabelsAsync(
        HttpClient apiClient,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/labels/all", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> PutLabelAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        string name,
        int countryId,
        string? information,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"/api/labels/{id}", accessToken);

        request.Content = JsonContent.Create(new { name, countryId, information }, options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> DeleteLabelAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/labels/{id}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }
}
