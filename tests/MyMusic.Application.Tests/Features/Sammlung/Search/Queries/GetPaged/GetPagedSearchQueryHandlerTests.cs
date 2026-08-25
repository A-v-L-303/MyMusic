namespace MyMusic.Application.Tests.Features.Sammlung.Search.Queries.GetPaged;

public class GetPagedSearchQueryHandlerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_LeererQuery_GibtLeeresErgebnisZurueckOhneRepositoryAufrufe(string? searchTerm)
    {
        // arrange
        var userId = Guid.NewGuid();

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var handler = new GetPagedSearchQueryHandler(
            recordRepository, artistRepository, labelRepository, genreRepository, countryRepository,
            recordTrackRepository, new SearchResponseBuilder());

        var query = new GetPagedSearchQuery(userId, 1, 20, searchTerm);

        // act
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(1, response.Page);
        Assert.Equal(20, response.PageSize);

        await recordRepository.DidNotReceive().GetPagedAsync(
            Arg.Any<Expression<Func<RecordEntity, bool>>>(),
            Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());

        await artistRepository.DidNotReceive().GetProjectedAsync(
            Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
            Arg.Any<Expression<Func<ArtistEntity, int>>>(),
            Arg.Any<CancellationToken>());

        await genreRepository.DidNotReceive().GetProjectedAsync(
            Arg.Any<Expression<Func<GenreEntity, bool>>>(),
            Arg.Any<Expression<Func<GenreEntity, int>>>(),
            Arg.Any<CancellationToken>());

        await countryRepository.DidNotReceive().GetProjectedAsync(
            Arg.Any<Expression<Func<CountryEntity, bool>>>(),
            Arg.Any<Expression<Func<CountryEntity, int>>>(),
            Arg.Any<CancellationToken>());

        await labelRepository.DidNotReceive().GetProjectedAsync(
            Arg.Any<Expression<Func<LabelEntity, bool>>>(),
            Arg.Any<Expression<Func<LabelEntity, int>>>(),
            Arg.Any<CancellationToken>());

        await recordTrackRepository.DidNotReceive().GetProjectedAsync(
            Arg.Any<Expression<Func<RecordTrackEntity, bool>>>(),
            Arg.Any<Expression<Func<RecordTrackEntity, int>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ArtistAufloesung_CaseInsensitivUndMandantengefiltert()
    {
        // arrange
        var userId = Guid.NewGuid();

        Expression<Func<ArtistEntity, bool>>? capturedFilter = null;

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetProjectedAsync(
                Arg.Do<Expression<Func<ArtistEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Expression<Func<ArtistEntity, int>>>(),
                Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<int>)[]);

        var handler = BuildHandler(userId, artistRepository: artistRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "BEATLES");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenerPassenderArtist = ArtistEntity.Create("The Beatles", userId);

        var eigenerAndererArtist = ArtistEntity.Create("Pink Floyd", userId);

        var fremderPassenderArtist = ArtistEntity.Create("The Beatles", Guid.NewGuid());

        Assert.True(predicate(eigenerPassenderArtist));
        Assert.False(predicate(eigenerAndererArtist));

        // Mandantentrennung
        Assert.False(predicate(fremderPassenderArtist));
    }

    [Fact]
    public async Task HandleAsync_GenreAufloesung_TrifftNurEigeneGenresMitPassendemNamen()
    {
        // arrange
        var userId = Guid.NewGuid();

        Expression<Func<GenreEntity, bool>>? capturedFilter = null;

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        genreRepository.GetProjectedAsync(
                Arg.Do<Expression<Func<GenreEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Expression<Func<GenreEntity, int>>>(),
                Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<int>)[]);

        var handler = BuildHandler(userId, genreRepository: genreRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "rock");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesPassendesGenre = GenreEntity.Create("Rock", userId);

        var eigenesAnderesGenre = GenreEntity.Create("Jazz", userId);

        var fremdesPassendesGenre = GenreEntity.Create("Rock", Guid.NewGuid());

        Assert.True(predicate(eigenesPassendesGenre));
        Assert.False(predicate(eigenesAnderesGenre));

        // Mandantentrennung
        Assert.False(predicate(fremdesPassendesGenre));
    }

    [Fact]
    public async Task HandleAsync_CountryAufloesung_TrifftUnabhaengigVomBenutzerNurNachNamen()
    {
        // arrange
        var userId = Guid.NewGuid();

        Expression<Func<CountryEntity, bool>>? capturedFilter = null;

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetProjectedAsync(
                Arg.Do<Expression<Func<CountryEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Expression<Func<CountryEntity, int>>>(),
                Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<int>)[]);

        var handler = BuildHandler(userId, countryRepository: countryRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "united");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var passendesLand = CountryEntity.Create("United Kingdom", "GB");

        var anderesLand = CountryEntity.Create("Germany", "DE");

        Assert.True(predicate(passendesLand));
        Assert.False(predicate(anderesLand));
    }

    [Fact]
    public async Task HandleAsync_LabelAufloesung_TrifftEigenesLabelNachNamenOderNachLand()
    {
        // arrange
        var userId = Guid.NewGuid();

        Expression<Func<LabelEntity, bool>>? capturedFilter = null;

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetProjectedAsync(
                Arg.Do<Expression<Func<LabelEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Expression<Func<LabelEntity, int>>>(),
                Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<int>)[]);

        var countryRepository = StubProjectedIds<CountryEntity>([5]);

        var handler = BuildHandler(userId, labelRepository: labelRepository, countryRepository: countryRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "apple");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesLabelPassenderName = LabelEntity.Create("Apple Records", 1, null, userId);

        var eigenesLabelPassendesLand = LabelEntity.Create("Some Records", 5, null, userId);

        var eigenesLabelOhneTreffer = LabelEntity.Create("Some Records", 1, null, userId);

        var fremdesLabelPassenderName = LabelEntity.Create("Apple Records", 1, null, Guid.NewGuid());

        Assert.True(predicate(eigenesLabelPassenderName));
        Assert.True(predicate(eigenesLabelPassendesLand));
        Assert.False(predicate(eigenesLabelOhneTreffer));

        // Mandantentrennung
        Assert.False(predicate(fremdesLabelPassenderName));
    }

    [Fact]
    public async Task HandleAsync_RecordTrackAufloesung_TrifftUeberTrackArtistOderGenreMandantengefiltert()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artistRepository = StubProjectedIds<ArtistEntity>([42]);

        var genreRepository = StubProjectedIds<GenreEntity>([7]);

        Expression<Func<RecordTrackEntity, bool>>? capturedFilter = null;

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        recordTrackRepository.GetProjectedAsync(
                Arg.Do<Expression<Func<RecordTrackEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Expression<Func<RecordTrackEntity, int>>>(),
                Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<int>)[]);

        var handler = BuildHandler(
            userId, artistRepository: artistRepository, genreRepository: genreRepository,
            recordTrackRepository: recordTrackRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "beliebig");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenerTrackPassenderArtist = RecordTrackEntity.Create(1, 42, 99, "Track", "A", 1, null, userId);

        var eigenerTrackPassendesGenre = RecordTrackEntity.Create(1, 99, 7, "Track", "A", 1, null, userId);

        var eigenerTrackOhneTreffer = RecordTrackEntity.Create(1, 99, 99, "Track", "A", 1, null, userId);

        var fremderTrackPassenderArtist = RecordTrackEntity.Create(1, 42, 99, "Track", "A", 1, null, Guid.NewGuid());

        Assert.True(predicate(eigenerTrackPassenderArtist));
        Assert.True(predicate(eigenerTrackPassendesGenre));
        Assert.False(predicate(eigenerTrackOhneTreffer));

        // Mandantentrennung
        Assert.False(predicate(fremderTrackPassenderArtist));
    }

    [Fact]
    public async Task HandleAsync_OhneArtistUndGenreTreffer_RuftRecordTrackRepositoryNichtAuf()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artistRepository = StubProjectedIds<ArtistEntity>([]);

        var genreRepository = StubProjectedIds<GenreEntity>([]);

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var handler = BuildHandler(
            userId, artistRepository: artistRepository, genreRepository: genreRepository,
            recordTrackRepository: recordTrackRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "kein-treffer");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        await recordTrackRepository.DidNotReceive().GetProjectedAsync(
            Arg.Any<Expression<Func<RecordTrackEntity, bool>>>(),
            Arg.Any<Expression<Func<RecordTrackEntity, int>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FinalerRecordFilter_KombiniertAlbumtitelArtistUndLabelTreffer()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artistRepository = StubProjectedIds<ArtistEntity>([42]);

        var labelRepository = StubProjectedIds<LabelEntity>([1]);

        // 999 kann von keinem über RecordEntity.Create() erzeugten Record erreicht werden (Id immer
        // 0, siehe Kommentar unten) - stellt sicher, dass dieser Test unabhängig vom
        // Track-Treffer-Pfad bleibt (siehe eigener Test
        // HandleAsync_FinalerRecordFilter_TrifftUeberRecordIdAusTrackAufloesung).
        var recordTrackRepository = StubProjectedIds<RecordTrackEntity>([999]);

        Expression<Func<RecordEntity, bool>>? capturedFilter = null;

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Do<Expression<Func<RecordEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var handler = BuildHandler(
            userId, recordRepository: recordRepository, artistRepository: artistRepository,
            labelRepository: labelRepository, recordTrackRepository: recordTrackRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "abbey");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var trefferAlbumtitel = CreateRecord(userId, labelId: 2, artistId: null, albumName: "Abbey Road");

        var trefferArtist = CreateRecord(userId, labelId: 2, artistId: 42, albumName: "Anderer Titel");

        var trefferLabel = CreateRecord(userId, labelId: 1, artistId: null, albumName: "Anderer Titel");

        var keinTreffer = CreateRecord(userId, labelId: 3, artistId: 99, albumName: "Anderer Titel");

        var fremderTreffer = RecordEntity.Create(
            2, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, Guid.NewGuid());

        Assert.True(predicate(trefferAlbumtitel));
        Assert.True(predicate(trefferArtist));
        Assert.True(predicate(trefferLabel));
        Assert.False(predicate(keinTreffer));

        // Mandantentrennung gilt auch für den finalen Record-Filter
        Assert.False(predicate(fremderTreffer));
    }

    [Fact]
    public async Task HandleAsync_FinalerRecordFilter_TrifftUeberRecordIdAusTrackAufloesung()
    {
        // arrange
        var userId = Guid.NewGuid();

        // RecordEntity.Create() liefert immer Id 0 (interner Konstruktor, kein InternalsVisibleTo
        // für Tests) - der Stub liefert bewusst genau diese Id, um den Contains(record.Id)-Zweig
        // isoliert zu testen, ohne mit anderen "kein Treffer"-Fällen zu kollidieren.
        var recordTrackRepository = StubProjectedIds<RecordTrackEntity>([0]);

        // ResolveMatchingRecordIdsViaTrackAsync ruft recordTrackRepository nur auf, wenn mindestens
        // eine Artist- oder Genre-Id gefunden wurde (Optimierung, siehe
        // HandleAsync_OhneArtistUndGenreTreffer_RuftRecordTrackRepositoryNichtAuf) - für diesen Test
        // muss die Artist-Auflösung daher einen Treffer liefern, sonst bleibt matchingRecordIdsViaTrack leer.
        var artistRepository = StubProjectedIds<ArtistEntity>([1]);

        Expression<Func<RecordEntity, bool>>? capturedFilter = null;

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Do<Expression<Func<RecordEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var handler = BuildHandler(
            userId, recordRepository: recordRepository, artistRepository: artistRepository,
            recordTrackRepository: recordTrackRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "kein-albumtitel-treffer");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var trefferUeberTrack = CreateRecord(userId, labelId: 1, artistId: null, albumName: "Anderer Titel");

        var fremderTrefferUeberTrack = RecordEntity.Create(
            1, null, RecordFormat.Album, "Anderer Titel", 1969, RecordCondition.Vg, null, Guid.NewGuid());

        Assert.True(predicate(trefferUeberTrack));

        // Mandantentrennung gilt auch für den Track-basierten Treffer
        Assert.False(predicate(fremderTrefferUeberTrack));
    }

    [Fact]
    public async Task HandleAsync_QueryMitLeerzeichenUndGrossschreibung_WirdGetrimmtUndKleingeschrieben()
    {
        // arrange
        var userId = Guid.NewGuid();

        Expression<Func<RecordEntity, bool>>? capturedFilter = null;

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Do<Expression<Func<RecordEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var handler = BuildHandler(userId, recordRepository: recordRepository);

        var query = new GetPagedSearchQuery(userId, 1, 20, "  ABBEY  ");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var treffer = CreateRecord(userId, labelId: 1, artistId: null, albumName: "Abbey Road");

        Assert.True(predicate(treffer));
    }

    [Fact]
    public async Task HandleAsync_LeitetSeitenparameterWeiter()
    {
        // arrange
        var userId = Guid.NewGuid();

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                2,
                10,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 12));

        var handler = BuildHandler(userId, recordRepository: recordRepository);

        var query = new GetPagedSearchQuery(userId, 2, 10, "abbey");

        // act
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.Equal(12, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
    }

    // Namensauflösung (labelNamesById/artistNamesById) über den Handler wird in
    // SearchResponseBuilderTests mit frei wählbaren Dictionaries geprüft (analog
    // GetPagedRecordsQueryHandlerTests/RecordResponseBuilderTests) - record.LabelId ist zwar frei
    // wählbar, aber der volle Pfad inklusive echter, über GetPagedAsync zurückgelieferter Records
    // ist hier nicht sinnvoll nachstellbar, ohne das Repository-Mocking der übrigen Tests zu
    // verdoppeln.

    private static GetPagedSearchQueryHandler BuildHandler(
        Guid userId,
        IRepository<RecordEntity>? recordRepository = null,
        IRepository<ArtistEntity>? artistRepository = null,
        IRepository<LabelEntity>? labelRepository = null,
        IRepository<GenreEntity>? genreRepository = null,
        IRepository<CountryEntity>? countryRepository = null,
        IRepository<RecordTrackEntity>? recordTrackRepository = null)
    {
        return new GetPagedSearchQueryHandler(
            recordRepository ?? StubEmptyRecordRepository(),
            artistRepository ?? StubProjectedIds<ArtistEntity>([]),
            labelRepository ?? StubProjectedIds<LabelEntity>([]),
            genreRepository ?? StubProjectedIds<GenreEntity>([]),
            countryRepository ?? StubProjectedIds<CountryEntity>([]),
            recordTrackRepository ?? StubProjectedIds<RecordTrackEntity>([]),
            new SearchResponseBuilder());
    }

    private static IRepository<RecordEntity> StubEmptyRecordRepository()
    {
        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        return repository;
    }

    private static IRepository<TEntity> StubProjectedIds<TEntity>(IReadOnlyList<int> ids)
        where TEntity : class
    {
        var repository = Substitute.For<IRepository<TEntity>>();

        repository.GetProjectedAsync(
                Arg.Any<Expression<Func<TEntity, bool>>>(),
                Arg.Any<Expression<Func<TEntity, int>>>(),
                Arg.Any<CancellationToken>())
            .Returns(ids);

        return repository;
    }

    private static RecordEntity CreateRecord(Guid userId, int labelId, int? artistId, string albumName)
    {
        return RecordEntity.Create(
            labelId, artistId, RecordFormat.Album, albumName, 1969, RecordCondition.Vg, null, userId);
    }
}
