namespace MyMusic.IntegrationTests;

public class SearchEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task SearchEndpoint_FindetRecordsUeberAlleKriterienUndTrenntMandanten()
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
        var unauthorizedResponse = await apiClient.GetAsync("/api/search?q=abbey", cancellationToken);

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

            var (countryId, countryName) = await GetGermanyAsync(apiClient, ownerToken, cancellationToken);

            var ownerLabel = await CreateLabelAsync(
                apiClient, ownerToken, $"Apple Records-{suffix}", countryId, cancellationToken);

            var ownerRecordArtist = await CreateArtistAsync(
                apiClient, ownerToken, $"The Beatles-{suffix}", cancellationToken);

            var ownerTrackArtist = await CreateArtistAsync(
                apiClient, ownerToken, $"Billy Preston-{suffix}", cancellationToken);

            var ownerGenre = await CreateGenreAsync(apiClient, ownerToken, $"Rock-{suffix}", cancellationToken);

            var otherUsersLabel = await CreateLabelAsync(
                apiClient, otherToken, $"Fremdlabel-{suffix}", countryId, cancellationToken);

            var otherUsersArtist = await CreateArtistAsync(
                apiClient, otherToken, $"Fremdartist-{suffix}", cancellationToken);

            var otherUsersGenre = await CreateGenreAsync(
                apiClient, otherToken, $"Fremdgenre-{suffix}", cancellationToken);

            var abbeyRoadName = $"Abbey Road-{suffix}";

            var createAbbeyRoadResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel, ownerRecordArtist, "Album", abbeyRoadName, 1969,
                cancellationToken);

            var abbeyRoad = await ReadRecordAsync(createAbbeyRoadResponse, cancellationToken);

            await PostRecordTrackAsync(
                apiClient, ownerToken, abbeyRoad.Id, ownerTrackArtist, ownerGenre, "Get Back", "A", 1,
                cancellationToken);

            var otherRecordResponse = await PostRecordAsync(
                apiClient, otherToken, otherUsersLabel, otherUsersArtist, "Album", $"Fremdalbum-{suffix}", 1970,
                cancellationToken);

            var otherRecord = await ReadRecordAsync(otherRecordResponse, cancellationToken);

            await PostRecordTrackAsync(
                apiClient, otherToken, otherRecord.Id, otherUsersArtist, otherUsersGenre, "Fremdtrack", "A", 1,
                cancellationToken);

            // act: leerer Suchbegriff -> 0 Treffer, kein Fehler
            var emptyQueryResponse = await GetSearchAsync(apiClient, ownerToken, string.Empty, cancellationToken);

            var emptyQueryResult = await ReadSearchResultAsync(emptyQueryResponse, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, emptyQueryResponse.StatusCode);
            Assert.Empty(emptyQueryResult.Items);
            Assert.Equal(0, emptyQueryResult.TotalCount);

            // act: Treffer über den Albumtitel
            var byTitleResult = await ReadSearchResultAsync(
                await GetSearchAsync(apiClient, ownerToken, suffix, cancellationToken), cancellationToken);

            // assert: nur die eigene Sammlung, unabhängig vom Kriterium, taucht auf
            Assert.Equal(1, byTitleResult.TotalCount);
            Assert.Equal(abbeyRoadName, byTitleResult.Items[0].AlbumName);

            // act: Treffer über den Record-Artist, case-insensitive
            var byRecordArtistResult = await ReadSearchResultAsync(
                await GetSearchAsync(apiClient, ownerToken, $"the beatles-{suffix}", cancellationToken),
                cancellationToken);

            // assert
            Assert.Equal(1, byRecordArtistResult.TotalCount);
            Assert.Equal(abbeyRoadName, byRecordArtistResult.Items[0].AlbumName);

            // act: Treffer über den Track-Artist (weicht vom Record-Artist ab)
            var byTrackArtistResult = await ReadSearchResultAsync(
                await GetSearchAsync(apiClient, ownerToken, $"billy preston-{suffix}", cancellationToken),
                cancellationToken);

            // assert
            Assert.Equal(1, byTrackArtistResult.TotalCount);
            Assert.Equal(abbeyRoadName, byTrackArtistResult.Items[0].AlbumName);

            // act: Treffer über das Label
            var byLabelResult = await ReadSearchResultAsync(
                await GetSearchAsync(apiClient, ownerToken, $"apple records-{suffix}", cancellationToken),
                cancellationToken);

            // assert
            Assert.Equal(1, byLabelResult.TotalCount);
            Assert.Equal(abbeyRoadName, byLabelResult.Items[0].AlbumName);

            // act: Treffer über das Genre (nur über den Track vorhanden)
            var byGenreResult = await ReadSearchResultAsync(
                await GetSearchAsync(apiClient, ownerToken, $"rock-{suffix}", cancellationToken),
                cancellationToken);

            // assert
            Assert.Equal(1, byGenreResult.TotalCount);
            Assert.Equal(abbeyRoadName, byGenreResult.Items[0].AlbumName);

            // act: Treffer über das Herkunftsland des Labels
            var byCountryResult = await ReadSearchResultAsync(
                await GetSearchAsync(apiClient, ownerToken, countryName, cancellationToken), cancellationToken);

            // assert: mindestens die eigene Sammlung ist enthalten (Länder sind nicht mandantengetrennt,
            // andere Benutzer mit demselben Land wären daher theoretisch ebenfalls enthalten)
            Assert.Contains(byCountryResult.Items, item => item.AlbumName == abbeyRoadName);

            // act: Suchbegriff, der nur beim fremden Benutzer vorkommt -> keine Treffer (Mandantentrennung)
            var foreignSuffixResult = await ReadSearchResultAsync(
                await GetSearchAsync(apiClient, ownerToken, $"fremdartist-{suffix}", cancellationToken),
                cancellationToken);

            // assert
            Assert.Empty(foreignSuffixResult.Items);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, owner, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, otherUser, cancellationToken);
        }
    }

    private static async Task<(int Id, string Name)> GetGermanyAsync(
        HttpClient apiClient,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/countries", accessToken);

        var response = await apiClient.SendAsync(request, cancellationToken);

        var countries = await response.Content
            .ReadFromJsonAsync<List<CountryResponseDto>>(_jsonOptions, cancellationToken);

        Assert.NotNull(countries);

        var germany = countries.Single(country => country.Code == "DE");

        return (germany.Id, germany.Name);
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

    private static async Task<int> CreateArtistAsync(
        HttpClient apiClient,
        string accessToken,
        string name,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/artists", accessToken);

        request.Content = JsonContent.Create(new { name }, options: _jsonOptions);

        var response = await apiClient.SendAsync(request, cancellationToken);

        var artist = await response.Content.ReadFromJsonAsync<ArtistResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(artist);

        return artist.Id;
    }

    private static async Task<int> CreateGenreAsync(
        HttpClient apiClient,
        string accessToken,
        string name,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/genres", accessToken);

        request.Content = JsonContent.Create(new { name }, options: _jsonOptions);

        var response = await apiClient.SendAsync(request, cancellationToken);

        var genre = await response.Content.ReadFromJsonAsync<GenreResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(genre);

        return genre.Id;
    }

    private static async Task<HttpResponseMessage> PostRecordAsync(
        HttpClient apiClient,
        string accessToken,
        int labelId,
        int? artistId,
        string format,
        string albumName,
        int releaseYear,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/records", accessToken);

        request.Content = JsonContent.Create(
            new
            {
                labelId,
                artistId,
                format,
                albumName,
                releaseYear,
                information = (string?)null
            },
            options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<RecordResponseDto> ReadRecordAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var record = await response.Content.ReadFromJsonAsync<RecordResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(record);

        return record;
    }

    private static async Task<HttpResponseMessage> PostRecordTrackAsync(
        HttpClient apiClient,
        string accessToken,
        int recordId,
        int artistId,
        int genreId,
        string trackName,
        string recordSide,
        int trackNumber,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post, $"/api/records/{recordId}/tracks", accessToken);

        request.Content = JsonContent.Create(
            new
            {
                artistId,
                genreId,
                trackName,
                recordSide,
                trackNumber,
                information = (string?)null
            },
            options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetSearchAsync(
        HttpClient apiClient,
        string accessToken,
        string query,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get, $"/api/search?q={Uri.EscapeDataString(query)}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<SearchResultListResponseDto> ReadSearchResultAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var result = await response.Content
            .ReadFromJsonAsync<SearchResultListResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(result);

        return result;
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }
}
