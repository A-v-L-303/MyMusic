namespace MyMusic.Application.Tests.Features.Sammlung.Record.Queries.GetPaged;

public class GetPagedRecordsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_LeitetSeitenparameterWeiter()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = StubEmptyRecordRepository(page: 2, pageSize: 10, totalCount: 12);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new GetPagedRecordsQueryHandler(
            repository, labelRepository, artistRepository, new RecordResponseBuilder());

        var query = new GetPagedRecordsQuery(userId, 2, 10, null, null, null, null, null, null, null, null);

        // act
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.Equal(12, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
    }

    [Fact]
    public async Task HandleAsync_FilterNachNameArtistLabelUndJahresspanne_WirktMandantengefiltert()
    {
        // arrange
        var userId = Guid.NewGuid();

        Expression<Func<RecordEntity, bool>>? capturedFilter = null;

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<RecordEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new GetPagedRecordsQueryHandler(
            repository, labelRepository, artistRepository, new RecordResponseBuilder());

        var query = new GetPagedRecordsQuery(
            userId, 1, 20, Name: "abbey", ArtistId: 10, LabelId: 1, YearFrom: 1960, YearTo: 1969,
            CountryId: null, SortBy: null, SortDirection: null);

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var passendesRecord = CreateRecord(userId, labelId: 1, artistId: 10, albumName: "Abbey Road", year: 1969);

        var falscherName = CreateRecord(userId, labelId: 1, artistId: 10, albumName: "Let It Be", year: 1969);

        var falscherArtist = CreateRecord(userId, labelId: 1, artistId: 11, albumName: "Abbey Road", year: 1969);

        var falschesLabel = CreateRecord(userId, labelId: 2, artistId: 10, albumName: "Abbey Road", year: 1969);

        var falschesJahr = CreateRecord(userId, labelId: 1, artistId: 10, albumName: "Abbey Road", year: 1980);

        var fremdesRecord = CreateRecord(Guid.NewGuid(), labelId: 1, artistId: 10, albumName: "Abbey Road", year: 1969);

        Assert.True(predicate(passendesRecord));
        Assert.False(predicate(falscherName));
        Assert.False(predicate(falscherArtist));
        Assert.False(predicate(falschesLabel));
        Assert.False(predicate(falschesJahr));

        // Mandantentrennung gilt auch für die Filterung der Liste
        Assert.False(predicate(fremdesRecord));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("releaseyear", "asc")]
    [InlineData("releaseyear", "desc")]
    [InlineData("format", "asc")]
    [InlineData("format", "desc")]
    public async Task HandleAsync_Sortierung_WendetGewaehltesFeldUndRichtungAn(string? sortBy, string? sortDirection)
    {
        // arrange
        var userId = Guid.NewGuid();

        Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>? capturedOrderBy = null;

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Do<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(
                    orderBy => capturedOrderBy = orderBy),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new GetPagedRecordsQueryHandler(
            repository, labelRepository, artistRepository, new RecordResponseBuilder());

        var query = new GetPagedRecordsQuery(userId, 1, 20, null, null, null, null, null, null, sortBy, sortDirection);

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedOrderBy);

        var recordB = RecordEntity.Create(
            1, null, RecordFormat.CdAlbum, "B-Album", 2000, RecordCondition.Vg, null, userId);

        var recordA = RecordEntity.Create(
            1, null, RecordFormat.Album, "A-Album", 1990, RecordCondition.Vg, null, userId);

        var ordered = capturedOrderBy!(new List<RecordEntity> { recordB, recordA }.AsQueryable()).ToList();

        var expectedFirst = (sortBy?.ToLowerInvariant(), sortDirection?.ToLowerInvariant()) switch
        {
            ("releaseyear", "desc") => recordB,
            ("releaseyear", _) => recordA,
            ("format", "desc") => recordB,
            ("format", _) => recordA,
            (_, "desc") => recordB,
            _ => recordA
        };

        Assert.Same(expectedFirst, ordered[0]);
    }

    [Fact]
    public async Task HandleAsync_CountryFilterGesetzt_LoestLabelsDesLandesMandantengefiltertAuf()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = StubEmptyRecordRepository(page: 1, pageSize: 20, totalCount: 0);

        Expression<Func<LabelEntity, bool>>? capturedFilter = null;

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetPagedAsync(
                Arg.Do<Expression<Func<LabelEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)new List<LabelEntity>(), TotalCount: 0));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new GetPagedRecordsQueryHandler(
            repository, labelRepository, artistRepository, new RecordResponseBuilder());

        var query = new GetPagedRecordsQuery(userId, 1, 20, null, null, null, null, null, 5, null, null);

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesLabelRichtigesLand = LabelEntity.Create("Apple Records", 5, null, userId);

        var eigenesLabelFalschesLand = LabelEntity.Create("Apple Records", 6, null, userId);

        var fremdesLabelRichtigesLand = LabelEntity.Create("Apple Records", 5, null, Guid.NewGuid());

        Assert.True(predicate(eigenesLabelRichtigesLand));
        Assert.False(predicate(eigenesLabelFalschesLand));

        // Mandantentrennung gilt auch für die Label-Auflösung nach Land
        Assert.False(predicate(fremdesLabelRichtigesLand));
    }

    [Fact]
    public async Task HandleAsync_OhneCountryFilterUndLeereListe_RuftLabelUndArtistRepositoryNichtAuf()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = StubEmptyRecordRepository(page: 1, pageSize: 20, totalCount: 0);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new GetPagedRecordsQueryHandler(
            repository, labelRepository, artistRepository, new RecordResponseBuilder());

        var query = new GetPagedRecordsQuery(userId, 1, 20, null, null, null, null, null, null, null, null);

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        await labelRepository.DidNotReceive().GetPagedAsync(
            Arg.Any<Expression<Func<LabelEntity, bool>>>(),
            Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());

        await artistRepository.DidNotReceive().GetPagedAsync(
            Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
            Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    // Namensauflösung (labelNamesById/artistNamesById) wird bewusst nicht mit einer nicht-leeren
    // Items-Liste über den Handler getestet: LabelEntity.Create/ArtistEntity.Create liefern stets
    // Id 0 (interner Konstruktor, kein InternalsVisibleTo für Tests), während record.LabelId
    // zwingend > 0 sein muss - die Id-basierte Zuordnung ist daher hier nicht nachstellbar
    // (bekannte Einschränkung, analog GetPagedLabelsQueryHandlerTests). Die eigentliche
    // Zuordnungslogik wird stattdessen in RecordResponseBuilderTests mit frei wählbaren
    // Dictionaries geprüft; der volle Pfad inklusive echter Ids in RecordEndpointsTests.

    private static IRepository<RecordEntity> StubEmptyRecordRepository(int page, int pageSize, int totalCount)
    {
        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                page,
                pageSize,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: totalCount));

        return repository;
    }

    private static RecordEntity CreateRecord(Guid userId, int labelId, int? artistId, string albumName, int year)
    {
        return RecordEntity.Create(
            labelId, artistId, RecordFormat.Album, albumName, year, RecordCondition.Vg, null, userId);
    }
}
