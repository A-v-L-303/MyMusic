namespace MyMusic.IntegrationTests;

public class RecordEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly byte[] _jpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    [Fact]
    public async Task RecordEndpoints_CrudFilterSortierungPaginierungUndMandantentrennung()
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
        var unauthorizedResponse = await apiClient.GetAsync("/api/records", cancellationToken);

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

            var (countryA, countryB) = await GetTwoCountryIdsAsync(apiClient, ownerToken, cancellationToken);

            var ownerLabel1 = await CreateLabelAsync(
                apiClient, ownerToken, $"Apple Records-{suffix}", countryA, cancellationToken);

            var ownerLabel2 = await CreateLabelAsync(
                apiClient, ownerToken, $"Universal-{suffix}", countryB, cancellationToken);

            var otherUsersLabel = await CreateLabelAsync(
                apiClient, otherToken, $"Fremdlabel-{suffix}", countryA, cancellationToken);

            var ownerArtist = await CreateArtistAsync(
                apiClient, ownerToken, $"The Beatles-{suffix}", cancellationToken);

            var otherUsersArtist = await CreateArtistAsync(
                apiClient, otherToken, $"Fremdartist-{suffix}", cancellationToken);

            var abbeyRoadName = $"Abbey Road-{suffix}";

            var letItBeName = $"Let It Be-{suffix}";

            // act: Pflichtfeld verletzt -> 400
            var invalidNameResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel1, ownerArtist, "Album", string.Empty, 1969, null, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidNameResponse.StatusCode);

            // act: verbotenes Zeichen -> 400
            var invalidCharacterResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel1, ownerArtist, "Album", "Album!", 1969, null, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidCharacterResponse.StatusCode);

            // act: Erscheinungsjahr vor 1860 -> 400
            var invalidYearTooEarlyResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel1, ownerArtist, "Album", abbeyRoadName, 1859, null, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidYearTooEarlyResponse.StatusCode);

            // act: Erscheinungsjahr in der Zukunft -> 400
            var invalidYearFutureResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel1, ownerArtist, "Album", abbeyRoadName, DateTime.UtcNow.Year + 1,
                null, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidYearFutureResponse.StatusCode);

            // act: nicht existierendes Label -> 400
            var invalidLabelResponse = await PostRecordAsync(
                apiClient, ownerToken, -1, ownerArtist, "Album", abbeyRoadName, 1969, null, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidLabelResponse.StatusCode);

            // act: fremdes Label -> 400 (Mandantentrennung, wie nicht existent behandelt)
            var foreignLabelResponse = await PostRecordAsync(
                apiClient, ownerToken, otherUsersLabel, ownerArtist, "Album", abbeyRoadName, 1969, null, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, foreignLabelResponse.StatusCode);

            // act: fremder Artist -> 400 (Mandantentrennung, wie nicht existent behandelt)
            var foreignArtistResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel1, otherUsersArtist, "Album", abbeyRoadName, 1969, null, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, foreignArtistResponse.StatusCode);

            // act: gültigen Record anlegen (mit Artist und Condition)
            var createAbbeyRoadResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel1, ownerArtist, "Album", abbeyRoadName, 1969, "Nm",
                "Erste Pressung", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, createAbbeyRoadResponse.StatusCode);

            var abbeyRoad = await ReadRecordAsync(createAbbeyRoadResponse, cancellationToken);

            Assert.Equal(abbeyRoadName, abbeyRoad.AlbumName);
            Assert.Equal(ownerLabel1, abbeyRoad.LabelId);
            Assert.Equal($"Apple Records-{suffix}", abbeyRoad.LabelName);
            Assert.Equal(ownerArtist, abbeyRoad.ArtistId);
            Assert.Equal($"The Beatles-{suffix}", abbeyRoad.ArtistName);
            Assert.Equal("Nm", abbeyRoad.Condition);
            Assert.Equal("Erste Pressung", abbeyRoad.Information);

            // act: zweiten Record ohne Artist und ohne Condition anlegen (Default VG erwartet)
            var createLetItBeResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel2, null, "CdAlbum", letItBeName, 1970, null, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, createLetItBeResponse.StatusCode);

            var letItBe = await ReadRecordAsync(createLetItBeResponse, cancellationToken);

            Assert.Null(letItBe.ArtistId);
            Assert.Null(letItBe.ArtistName);
            Assert.Equal("Vg", letItBe.Condition);

            // act: identischer Record erneut anlegen -> 201, Duplikate sind ausdrücklich erlaubt
            var duplicateResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel1, ownerArtist, "Album", abbeyRoadName, 1969, "Nm",
                "Erste Pressung", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, duplicateResponse.StatusCode);

            // act: Liste mit Namensfilter (Suffix), beschränkt auf die eigene Sammlung
            var listBySuffixResponse = await GetRecordsAsync(
                apiClient, ownerToken, suffix, cancellationToken: cancellationToken);

            var listBySuffix = await listBySuffixResponse.Content
                .ReadFromJsonAsync<RecordListResponseDto>(_jsonOptions, cancellationToken);

            // assert
            Assert.NotNull(listBySuffix);
            Assert.Equal(3, listBySuffix.TotalCount);

            // act: Liste nach labelId gefiltert -> nur Abbey-Road-Einträge (Original + Duplikat)
            var listByLabelResponse = await GetRecordsAsync(
                apiClient, ownerToken, suffix, labelId: ownerLabel1, cancellationToken: cancellationToken);

            var listByLabel = await listByLabelResponse.Content
                .ReadFromJsonAsync<RecordListResponseDto>(_jsonOptions, cancellationToken);

            // assert
            Assert.NotNull(listByLabel);
            Assert.Equal(2, listByLabel.TotalCount);

            // act: Liste nach artistId gefiltert -> nur Abbey-Road-Einträge
            var listByArtistResponse = await GetRecordsAsync(
                apiClient, ownerToken, suffix, artistId: ownerArtist, cancellationToken: cancellationToken);

            var listByArtist = await listByArtistResponse.Content
                .ReadFromJsonAsync<RecordListResponseDto>(_jsonOptions, cancellationToken);

            // assert
            Assert.NotNull(listByArtist);
            Assert.Equal(2, listByArtist.TotalCount);

            // act: Liste nach Erscheinungsjahr-Zeitraum gefiltert -> nur Let It Be
            var listByYearResponse = await GetRecordsAsync(
                apiClient, ownerToken, suffix, yearFrom: 1970, yearTo: 1970, cancellationToken: cancellationToken);

            var listByYear = await listByYearResponse.Content
                .ReadFromJsonAsync<RecordListResponseDto>(_jsonOptions, cancellationToken);

            // assert
            Assert.NotNull(listByYear);
            Assert.Equal(1, listByYear.TotalCount);
            Assert.Equal(letItBeName, listByYear.Items[0].AlbumName);

            // act: Liste nach Herkunftsland des Labels gefiltert (countryB -> nur Let It Be)
            var listByCountryResponse = await GetRecordsAsync(
                apiClient, ownerToken, suffix, countryId: countryB, cancellationToken: cancellationToken);

            var listByCountry = await listByCountryResponse.Content
                .ReadFromJsonAsync<RecordListResponseDto>(_jsonOptions, cancellationToken);

            // assert
            Assert.NotNull(listByCountry);
            Assert.Equal(1, listByCountry.TotalCount);
            Assert.Equal(letItBeName, listByCountry.Items[0].AlbumName);

            // act: Liste absteigend nach Erscheinungsjahr sortiert -> Let It Be (1970) zuerst
            var listSortedResponse = await GetRecordsAsync(
                apiClient, ownerToken, suffix, sortBy: "releaseYear", sortDirection: "desc",
                cancellationToken: cancellationToken);

            var listSorted = await listSortedResponse.Content
                .ReadFromJsonAsync<RecordListResponseDto>(_jsonOptions, cancellationToken);

            // assert
            Assert.NotNull(listSorted);
            Assert.Equal(letItBeName, listSorted.Items[0].AlbumName);

            // act: Get-by-Id des Besitzers -> 200
            var getOwnResponse = await GetRecordAsync(apiClient, ownerToken, abbeyRoad.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, getOwnResponse.StatusCode);

            // act: Get-by-Id eines fremden Records -> 404 statt 403
            var getForeignResponse = await GetRecordAsync(apiClient, otherToken, abbeyRoad.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getForeignResponse.StatusCode);

            // act: unbekannte Id -> 404
            var getUnknownResponse = await GetRecordAsync(apiClient, ownerToken, -1, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getUnknownResponse.StatusCode);

            // act: Bearbeiten mit fremdem Label -> 400
            var updateWithForeignLabelResponse = await PutRecordAsync(
                apiClient, ownerToken, letItBe.Id, otherUsersLabel, null, "CdAlbum", letItBeName, 1970, "Vg", null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, updateWithForeignLabelResponse.StatusCode);

            // act: Bearbeiten mit fremdem Artist -> 400
            var updateWithForeignArtistResponse = await PutRecordAsync(
                apiClient, ownerToken, letItBe.Id, ownerLabel2, otherUsersArtist, "CdAlbum", letItBeName, 1970,
                "Vg", null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, updateWithForeignArtistResponse.StatusCode);

            // act: gültiges Bearbeiten -> 200
            var remasteredName = $"Let It Be (Remastered)-{suffix}";

            var updateResponse = await PutRecordAsync(
                apiClient, ownerToken, letItBe.Id, ownerLabel1, ownerArtist, "Album", remasteredName, 1970,
                "Mint", "Remaster", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updatedLetItBe = await ReadRecordAsync(updateResponse, cancellationToken);

            Assert.Equal(remasteredName, updatedLetItBe.AlbumName);
            Assert.Equal(ownerLabel1, updatedLetItBe.LabelId);
            Assert.Equal(ownerArtist, updatedLetItBe.ArtistId);
            Assert.Equal("Mint", updatedLetItBe.Condition);
            Assert.Equal("Remaster", updatedLetItBe.Information);

            // act: fremden Record bearbeiten -> 404 (eigenes Label des Aufrufers, damit ausschliesslich
            // die Datensatz-Eigentuemerpruefung getestet wird, nicht die LabelId-Validierung)
            var updateForeignResponse = await PutRecordAsync(
                apiClient, otherToken, abbeyRoad.Id, otherUsersLabel, null, "Album", "Irrelevant", 1969, "Vg", null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, updateForeignResponse.StatusCode);

            // act: Cover ohne Token -> 401
            using var unauthorizedCoverRequest = new HttpRequestMessage(
                HttpMethod.Post, $"/api/records/{abbeyRoad.Id}/cover")
            {
                Content = BuildCoverContent(_jpegBytes, "cover.jpg")
            };

            var unauthorizedCoverResponse = await apiClient.SendAsync(unauthorizedCoverRequest, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedCoverResponse.StatusCode);

            // act: ungültiges Format (Textdatei) -> 400
            var invalidFormatCoverResponse = await PostRecordCoverAsync(
                apiClient, ownerToken, abbeyRoad.Id, "Kein Bild"u8.ToArray(), "cover.txt", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidFormatCoverResponse.StatusCode);

            // act: zu große Datei -> 400
            var oversizedCover = new byte[(5 * 1024 * 1024) + 1];

            Array.Copy(_jpegBytes, oversizedCover, _jpegBytes.Length);

            var oversizedCoverResponse = await PostRecordCoverAsync(
                apiClient, ownerToken, abbeyRoad.Id, oversizedCover, "cover.jpg", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, oversizedCoverResponse.StatusCode);

            // act: fremder Record -> 404 statt 403
            var foreignCoverResponse = await PostRecordCoverAsync(
                apiClient, otherToken, abbeyRoad.Id, _jpegBytes, "cover.jpg", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, foreignCoverResponse.StatusCode);

            // act: gültiger Upload -> 200 mit Data-URL
            var coverResponse = await PostRecordCoverAsync(
                apiClient, ownerToken, abbeyRoad.Id, _jpegBytes, "cover.jpg", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, coverResponse.StatusCode);

            var abbeyRoadWithCover = await ReadRecordAsync(coverResponse, cancellationToken);

            Assert.NotNull(abbeyRoadWithCover.AlbumCoverDataUrl);
            Assert.StartsWith("data:image/jpeg;base64,", abbeyRoadWithCover.AlbumCoverDataUrl);

            // act: Cover erscheint auch beim erneuten Abruf (Detailansicht)
            var getWithCoverResponse = await GetRecordAsync(apiClient, ownerToken, abbeyRoad.Id, cancellationToken);

            var abbeyRoadReloaded = await ReadRecordAsync(getWithCoverResponse, cancellationToken);

            // assert
            Assert.Equal(abbeyRoadWithCover.AlbumCoverDataUrl, abbeyRoadReloaded.AlbumCoverDataUrl);

            // act: eigenen Record löschen -> 204
            var deleteResponse = await DeleteRecordAsync(apiClient, ownerToken, updatedLetItBe.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // act: gelöschten Record erneut abrufen -> 404
            var getDeletedResponse = await GetRecordAsync(apiClient, ownerToken, updatedLetItBe.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);

            // act: fremden Record löschen -> 404
            var deleteForeignResponse = await DeleteRecordAsync(apiClient, otherToken, abbeyRoad.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteForeignResponse.StatusCode);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, owner, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, otherUser, cancellationToken);
        }
    }

    [Fact]
    public async Task RecordTrackEndpoints_CrudMandantentrennungUndEindeutigkeitspruefung()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyMusic_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("migrator", KnownResourceStates.Finished, cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("api", KnownResourceStates.Running, cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        using var apiClient = app.CreateHttpClient("api", "http");
        using var keycloakClient = app.CreateHttpClient("keycloak", "http");

        var owner = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        var otherUser = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            var ownerToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, owner, cancellationToken);

            var otherToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, otherUser, cancellationToken);

            var suffix = Guid.NewGuid().ToString("N")[..8];

            var (countryA, _) = await GetTwoCountryIdsAsync(apiClient, ownerToken, cancellationToken);

            var ownerLabel = await CreateLabelAsync(
                apiClient, ownerToken, $"Apple Records-{suffix}", countryA, cancellationToken);

            var ownerArtist = await CreateArtistAsync(
                apiClient, ownerToken, $"The Beatles-{suffix}", cancellationToken);

            var secondOwnerArtist = await CreateArtistAsync(
                apiClient, ownerToken, $"Billy Preston-{suffix}", cancellationToken);

            var ownerGenre = await CreateGenreAsync(apiClient, ownerToken, $"Rock-{suffix}", cancellationToken);

            var otherUsersArtist = await CreateArtistAsync(
                apiClient, otherToken, $"Fremdartist-{suffix}", cancellationToken);

            var otherUsersGenre = await CreateGenreAsync(
                apiClient, otherToken, $"Fremdgenre-{suffix}", cancellationToken);

            var createRecordResponse = await PostRecordAsync(
                apiClient, ownerToken, ownerLabel, ownerArtist, "Album", $"Abbey Road-{suffix}", 1969, null, null,
                cancellationToken);

            var record = await ReadRecordAsync(createRecordResponse, cancellationToken);

            Assert.Empty(record.Tracks);

            var otherUsersLabel = await CreateLabelAsync(
                apiClient, otherToken, $"Fremdlabel-{suffix}", countryA, cancellationToken);

            var otherUsersRecordResponse = await PostRecordAsync(
                apiClient, otherToken, otherUsersLabel, null, "Album", $"Fremdalbum-{suffix}", 1970, null, null,
                cancellationToken);

            var otherUsersRecord = await ReadRecordAsync(otherUsersRecordResponse, cancellationToken);

            // act: ohne Token -> 401
            using var unauthorizedRequest = new HttpRequestMessage(
                HttpMethod.Post, $"/api/records/{record.Id}/tracks")
            {
                Content = JsonContent.Create(
                    BuildTrackPayload(ownerArtist, ownerGenre, "Come Together", "A", 1, null), options: _jsonOptions)
            };

            var unauthorizedResponse = await apiClient.SendAsync(unauthorizedRequest, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

            // act: ungültiger Artist -> 400
            var invalidArtistResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, -1, ownerGenre, "Come Together", "A", 1, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidArtistResponse.StatusCode);

            // act: fremder Artist -> 400 (Mandantentrennung, wie nicht existent behandelt)
            var foreignArtistResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, otherUsersArtist, ownerGenre, "Come Together", "A", 1, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, foreignArtistResponse.StatusCode);

            // act: ungültiges Genre -> 400
            var invalidGenreResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, ownerArtist, -1, "Come Together", "A", 1, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidGenreResponse.StatusCode);

            // act: fremdes Genre -> 400 (Mandantentrennung, wie nicht existent behandelt)
            var foreignGenreResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, ownerArtist, otherUsersGenre, "Come Together", "A", 1, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, foreignGenreResponse.StatusCode);

            // act: leerer Trackname -> 400
            var invalidNameResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, ownerArtist, ownerGenre, string.Empty, "A", 1, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidNameResponse.StatusCode);

            // act: ungültige Seite -> 400
            var invalidSideResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, ownerArtist, ownerGenre, "Come Together", "A-", 1, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidSideResponse.StatusCode);

            // act: ungültige Tracknummer -> 400
            var invalidNumberResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, ownerArtist, ownerGenre, "Come Together", "A", 0, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidNumberResponse.StatusCode);

            // act: fremder Record -> 404 statt 403
            var foreignRecordResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, otherUsersRecord.Id, ownerArtist, ownerGenre, "Come Together", "A", 1, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, foreignRecordResponse.StatusCode);

            // act: nicht existierender Record -> 404
            var unknownRecordResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, -1, ownerArtist, ownerGenre, "Come Together", "A", 1, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, unknownRecordResponse.StatusCode);

            // act: gültigen Track anlegen -> 201
            var createFirstTrackResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, ownerArtist, ownerGenre, "Come Together", "A", 1, "Opener",
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, createFirstTrackResponse.StatusCode);

            var firstTrack = await ReadRecordTrackAsync(createFirstTrackResponse, cancellationToken);

            Assert.Equal("Come Together", firstTrack.TrackName);
            Assert.Equal(ownerArtist, firstTrack.ArtistId);
            Assert.Equal($"The Beatles-{suffix}", firstTrack.ArtistName);
            Assert.Equal(ownerGenre, firstTrack.GenreId);
            Assert.Equal($"Rock-{suffix}", firstTrack.GenreName);
            Assert.Equal("A", firstTrack.RecordSide);
            Assert.Equal(1, firstTrack.TrackNumber);
            Assert.Equal("Opener", firstTrack.Information);

            // act: zweiten Track mit derselben Seite/Nummer anlegen -> 409
            var duplicateTrackResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, ownerArtist, ownerGenre, "Duplicate", "A", 1, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateTrackResponse.StatusCode);

            // act: zweiten, gültigen Track anlegen -> 201
            var createSecondTrackResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, secondOwnerArtist, ownerGenre, "Something", "A", 2, null,
                cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Created, createSecondTrackResponse.StatusCode);

            var secondTrack = await ReadRecordTrackAsync(createSecondTrackResponse, cancellationToken);

            // act: Record-Detailansicht enthält beide Tracks, sortiert nach Seite/Nummer
            var getRecordWithTracksResponse = await GetRecordAsync(
                apiClient, ownerToken, record.Id, cancellationToken);

            var recordWithTracks = await ReadRecordAsync(getRecordWithTracksResponse, cancellationToken);

            // assert
            Assert.Equal(2, recordWithTracks.Tracks.Count);
            Assert.Equal("Come Together", recordWithTracks.Tracks[0].TrackName);
            Assert.Equal("Something", recordWithTracks.Tracks[1].TrackName);

            // act: Bearbeiten mit fremdem Artist -> 400
            var updateForeignArtistResponse = await PutRecordTrackAsync(
                apiClient, ownerToken, record.Id, firstTrack.Id, otherUsersArtist, ownerGenre, "Come Together", "A",
                1, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, updateForeignArtistResponse.StatusCode);

            // act: Bearbeiten auf eine bereits vom anderen Track belegte Seite/Nummer -> 409
            var updateConflictResponse = await PutRecordTrackAsync(
                apiClient, ownerToken, record.Id, secondTrack.Id, secondOwnerArtist, ownerGenre, "Something", "A",
                1, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, updateConflictResponse.StatusCode);

            // act: Bearbeiten über einen fremden Record -> 404 (eigene Artist/Genre-Ids des Aufrufers,
            // damit ausschliesslich die Datensatz-Eigentuemerpruefung getestet wird, nicht die
            // ArtistId/GenreId-Validierung)
            var updateForeignRecordResponse = await PutRecordTrackAsync(
                apiClient, otherToken, otherUsersRecord.Id, firstTrack.Id, otherUsersArtist, otherUsersGenre,
                "Irrelevant", "A", 1, null, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, updateForeignRecordResponse.StatusCode);

            // act: gültiges Bearbeiten -> 200
            var updateResponse = await PutRecordTrackAsync(
                apiClient, ownerToken, record.Id, firstTrack.Id, ownerArtist, ownerGenre, "Come Together (Remix)",
                "A", 1, "Geändert", cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updatedFirstTrack = await ReadRecordTrackAsync(updateResponse, cancellationToken);

            Assert.Equal("Come Together (Remix)", updatedFirstTrack.TrackName);
            Assert.Equal("Geändert", updatedFirstTrack.Information);

            // act: fremden Track löschen -> 404 statt 403
            var deleteForeignResponse = await DeleteRecordTrackAsync(
                apiClient, otherToken, record.Id, firstTrack.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteForeignResponse.StatusCode);

            // act: unbekannten Track löschen -> 404
            var deleteUnknownResponse = await DeleteRecordTrackAsync(
                apiClient, ownerToken, record.Id, -1, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteUnknownResponse.StatusCode);

            // act: eigenen Track löschen -> 204
            var deleteResponse = await DeleteRecordTrackAsync(
                apiClient, ownerToken, record.Id, secondTrack.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // act: gelöschten Track erneut löschen -> 404
            var deleteAgainResponse = await DeleteRecordTrackAsync(
                apiClient, ownerToken, record.Id, secondTrack.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteAgainResponse.StatusCode);

            // act: Record-Detailansicht enthält nur noch den verbliebenen Track
            var getAfterDeleteResponse = await GetRecordAsync(apiClient, ownerToken, record.Id, cancellationToken);

            var recordAfterDelete = await ReadRecordAsync(getAfterDeleteResponse, cancellationToken);

            // assert
            Assert.Single(recordAfterDelete.Tracks);
            Assert.Equal("Come Together (Remix)", recordAfterDelete.Tracks[0].TrackName);

            // act: Record mit verbliebenem Track löschen -> 204
            var deleteRecordResponse = await DeleteRecordAsync(apiClient, ownerToken, record.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteRecordResponse.StatusCode);

            // act: erneutes Löschen des zugehörigen Tracks -> 404, da per ON DELETE CASCADE bereits entfernt
            var deleteCascadedTrackResponse = await DeleteRecordTrackAsync(
                apiClient, ownerToken, record.Id, firstTrack.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteCascadedTrackResponse.StatusCode);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, owner, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, otherUser, cancellationToken);
        }
    }

    [Fact]
    public async Task ReferenzielleIntegritaet_VerhindertLoeschenVonGenreLabelUndArtistBeiVerwendung()
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

        var owner = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            var ownerToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, owner, cancellationToken);

            var suffix = Guid.NewGuid().ToString("N")[..8];

            var (countryA, _) = await GetTwoCountryIdsAsync(apiClient, ownerToken, cancellationToken);

            var label = await CreateLabelAsync(
                apiClient, ownerToken, $"Apple Records-{suffix}", countryA, cancellationToken);

            var artist = await CreateArtistAsync(apiClient, ownerToken, $"The Beatles-{suffix}", cancellationToken);

            var genre = await CreateGenreAsync(apiClient, ownerToken, $"Rock-{suffix}", cancellationToken);

            var createRecordResponse = await PostRecordAsync(
                apiClient, ownerToken, label, artist, "Album", $"Abbey Road-{suffix}", 1969, null, null,
                cancellationToken);

            var record = await ReadRecordAsync(createRecordResponse, cancellationToken);

            var createTrackResponse = await PostRecordTrackAsync(
                apiClient, ownerToken, record.Id, artist, genre, "Come Together", "A", 1, null, cancellationToken);

            var track = await ReadRecordTrackAsync(createTrackResponse, cancellationToken);

            // act: Genre löschen, solange ein Track es referenziert -> 409
            var deleteGenreWhileUsedResponse = await DeleteGenreAsync(apiClient, ownerToken, genre, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, deleteGenreWhileUsedResponse.StatusCode);

            // act: Label löschen, solange ein Record es referenziert -> 409
            var deleteLabelWhileUsedResponse = await DeleteLabelAsync(apiClient, ownerToken, label, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, deleteLabelWhileUsedResponse.StatusCode);

            // act: Artist löschen, solange ein Record ihn referenziert -> 409
            var deleteArtistWhileUsedByRecordResponse = await DeleteArtistAsync(
                apiClient, ownerToken, artist, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, deleteArtistWhileUsedByRecordResponse.StatusCode);

            // act: Track löschen
            var deleteTrackResponse = await DeleteRecordTrackAsync(
                apiClient, ownerToken, record.Id, track.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteTrackResponse.StatusCode);

            // act: Artist weiterhin löschen, solange der Record ihn direkt referenziert -> weiterhin 409
            var deleteArtistStillUsedByRecordResponse = await DeleteArtistAsync(
                apiClient, ownerToken, artist, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, deleteArtistStillUsedByRecordResponse.StatusCode);

            // act: Record löschen -> entfernt die letzte Referenz auf Label und Artist
            var deleteRecordResponse = await DeleteRecordAsync(apiClient, ownerToken, record.Id, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteRecordResponse.StatusCode);

            // act: Genre, Label und Artist sind jetzt unreferenziert und lassen sich löschen -> 204
            var deleteGenreResponse = await DeleteGenreAsync(apiClient, ownerToken, genre, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteGenreResponse.StatusCode);

            // act
            var deleteLabelResponse = await DeleteLabelAsync(apiClient, ownerToken, label, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteLabelResponse.StatusCode);

            // act
            var deleteArtistResponse = await DeleteArtistAsync(apiClient, ownerToken, artist, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteArtistResponse.StatusCode);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, owner, cancellationToken);
        }
    }

    private static async Task<(int CountryA, int CountryB)> GetTwoCountryIdsAsync(
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

    private static async Task<RecordResponseDto> ReadRecordAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var record = await response.Content.ReadFromJsonAsync<RecordResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(record);

        return record;
    }

    private static async Task<HttpResponseMessage> PostRecordAsync(
        HttpClient apiClient,
        string accessToken,
        int labelId,
        int? artistId,
        string format,
        string albumName,
        int releaseYear,
        string? condition,
        string? information,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/records", accessToken);

        request.Content = JsonContent.Create(
            BuildRecordPayload(labelId, artistId, format, albumName, releaseYear, condition, information),
            options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> PutRecordAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        int labelId,
        int? artistId,
        string format,
        string albumName,
        int releaseYear,
        string? condition,
        string? information,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"/api/records/{id}", accessToken);

        request.Content = JsonContent.Create(
            BuildRecordPayload(labelId, artistId, format, albumName, releaseYear, condition, information),
            options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> PostRecordCoverAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        byte[] fileContent,
        string fileName,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, $"/api/records/{id}/cover", accessToken);

        request.Content = BuildCoverContent(fileContent, fileName);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static MultipartFormDataContent BuildCoverContent(byte[] fileContent, string fileName)
    {
        var content = new MultipartFormDataContent();

        content.Add(new ByteArrayContent(fileContent), "file", fileName);

        return content;
    }

    /// <summary>
    /// Baut den JSON-Payload für Record-Create/Update. Ein <c>null</c>-Condition wird bewusst
    /// weggelassen statt als JSON-null gesendet: <c>CreateRecordCommand.Condition</c> ist ein
    /// nicht-nullable Enum, ein explizites JSON-null würde bei der Modellbindung eine nicht
    /// abgefangene Deserialisierungs-Exception (HTTP 500) statt eines Validierungsfehlers auslösen.
    /// </summary>
    private static Dictionary<string, object?> BuildRecordPayload(
        int labelId,
        int? artistId,
        string format,
        string albumName,
        int releaseYear,
        string? condition,
        string? information)
    {
        var payload = new Dictionary<string, object?>
        {
            ["labelId"] = labelId,
            ["artistId"] = artistId,
            ["format"] = format,
            ["albumName"] = albumName,
            ["releaseYear"] = releaseYear,
            ["information"] = information
        };

        if (condition is not null)
            payload["condition"] = condition;

        return payload;
    }

    private static async Task<HttpResponseMessage> GetRecordAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/records/{id}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetRecordsAsync(
        HttpClient apiClient,
        string accessToken,
        string nameFilter,
        int? artistId = null,
        int? labelId = null,
        int? yearFrom = null,
        int? yearTo = null,
        int? countryId = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"name={nameFilter}" };

        if (artistId is not null)
            query.Add($"artistId={artistId}");

        if (labelId is not null)
            query.Add($"labelId={labelId}");

        if (yearFrom is not null)
            query.Add($"yearFrom={yearFrom}");

        if (yearTo is not null)
            query.Add($"yearTo={yearTo}");

        if (countryId is not null)
            query.Add($"countryId={countryId}");

        if (sortBy is not null)
            query.Add($"sortBy={sortBy}");

        if (sortDirection is not null)
            query.Add($"sortDirection={sortDirection}");

        using var request = CreateAuthorizedRequest(
            HttpMethod.Get, $"/api/records?{string.Join('&', query)}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> DeleteRecordAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/records/{id}", accessToken);

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

    private static async Task<HttpResponseMessage> DeleteLabelAsync(
        HttpClient apiClient,
        string accessToken,
        int id,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/labels/{id}", accessToken);

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

    private static async Task<RecordTrackResponseDto> ReadRecordTrackAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var track = await response.Content
            .ReadFromJsonAsync<RecordTrackResponseDto>(_jsonOptions, cancellationToken);

        Assert.NotNull(track);

        return track;
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
        string? information,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post, $"/api/records/{recordId}/tracks", accessToken);

        request.Content = JsonContent.Create(
            BuildTrackPayload(artistId, genreId, trackName, recordSide, trackNumber, information),
            options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> PutRecordTrackAsync(
        HttpClient apiClient,
        string accessToken,
        int recordId,
        int trackId,
        int artistId,
        int genreId,
        string trackName,
        string recordSide,
        int trackNumber,
        string? information,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Put, $"/api/records/{recordId}/tracks/{trackId}", accessToken);

        request.Content = JsonContent.Create(
            BuildTrackPayload(artistId, genreId, trackName, recordSide, trackNumber, information),
            options: _jsonOptions);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> DeleteRecordTrackAsync(
        HttpClient apiClient,
        string accessToken,
        int recordId,
        int trackId,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Delete, $"/api/records/{recordId}/tracks/{trackId}", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }

    private static Dictionary<string, object?> BuildTrackPayload(
        int artistId,
        int genreId,
        string trackName,
        string recordSide,
        int trackNumber,
        string? information)
    {
        return new Dictionary<string, object?>
        {
            ["artistId"] = artistId,
            ["genreId"] = genreId,
            ["trackName"] = trackName,
            ["recordSide"] = recordSide,
            ["trackNumber"] = trackNumber,
            ["information"] = information
        };
    }
}
