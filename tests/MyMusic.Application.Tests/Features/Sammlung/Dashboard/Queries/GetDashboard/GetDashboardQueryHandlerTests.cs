namespace MyMusic.Application.Tests.Features.Sammlung.Dashboard.Queries.GetDashboard;

public class GetDashboardQueryHandlerTests
{
    // Namensauflösung (artistNamesById/labelNamesById) wird bewusst nicht mit einer nicht-leeren
    // Records-Liste über den Handler getestet: ArtistEntity.Create/LabelEntity.Create liefern stets
    // Id 0 (interner Konstruktor, kein InternalsVisibleTo für Tests), während eine echte
    // Id-basierte Zuordnung mehrere unterschiedliche Ids braucht - die Zuordnungslogik ist daher
    // hier nicht nachstellbar (bekannte Einschränkung, analog GetPagedRecordsQueryHandlerTests).
    // Die eigentliche Aggregations- und Zuordnungslogik wird stattdessen vollständig in
    // DashboardResponseBuilderTests mit frei wählbaren Projektionen und Dictionaries geprüft; der
    // volle Pfad inklusive echter Ids über die manuelle Live-Verifikation gegen Postgres.

    [Fact]
    public async Task HandleAsync_LeereSammlung_GibtUnabhaengigeKennzahlenUndLeereListenZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetProjectedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Expression<Func<RecordEntity, RecordAggregationProjection>>>(),
                Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<RecordAggregationProjection>)new List<RecordAggregationProjection>());

        var artistRepository = StubTotalCount<ArtistEntity>(3);

        var labelRepository = StubTotalCount<LabelEntity>(2);

        var genreRepository = StubTotalCount<GenreEntity>(4);

        var handler = new GetDashboardQueryHandler(
            recordRepository, artistRepository, labelRepository, genreRepository, new DashboardResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetDashboardQuery(userId), CancellationToken.None);

        // assert
        Assert.Equal(0, response.RecordsTotal);
        Assert.Equal(3, response.ArtistsTotal);
        Assert.Equal(2, response.LabelsTotal);
        Assert.Equal(4, response.GenresTotal);
        Assert.Empty(response.FormatDistribution);
        Assert.Empty(response.TopArtists);
        Assert.Empty(response.TopLabels);
        Assert.Empty(response.YearDistribution);
    }

    [Fact]
    public async Task HandleAsync_Mandantentrennung_FiltertAlleVierRepositoriesNachAngemeldetemBenutzer()
    {
        // arrange
        var userId = Guid.NewGuid();

        Expression<Func<RecordEntity, bool>>? recordFilter = null;

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetProjectedAsync(
                Arg.Do<Expression<Func<RecordEntity, bool>>>(filter => recordFilter = filter),
                Arg.Any<Expression<Func<RecordEntity, RecordAggregationProjection>>>(),
                Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<RecordAggregationProjection>)new List<RecordAggregationProjection>());

        Expression<Func<ArtistEntity, bool>>? artistFilter = null;

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetPagedAsync(
                Arg.Do<Expression<Func<ArtistEntity, bool>>>(filter => artistFilter = filter),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        Expression<Func<LabelEntity, bool>>? labelFilter = null;

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetPagedAsync(
                Arg.Do<Expression<Func<LabelEntity, bool>>>(filter => labelFilter = filter),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)new List<LabelEntity>(), TotalCount: 0));

        Expression<Func<GenreEntity, bool>>? genreFilter = null;

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        genreRepository.GetPagedAsync(
                Arg.Do<Expression<Func<GenreEntity, bool>>>(filter => genreFilter = filter),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)new List<GenreEntity>(), TotalCount: 0));

        var handler = new GetDashboardQueryHandler(
            recordRepository, artistRepository, labelRepository, genreRepository, new DashboardResponseBuilder());

        // act
        await handler.HandleAsync(new GetDashboardQuery(userId), CancellationToken.None);

        // assert
        Assert.NotNull(recordFilter);
        Assert.NotNull(artistFilter);
        Assert.NotNull(labelFilter);
        Assert.NotNull(genreFilter);

        var fremderUserId = Guid.NewGuid();

        var eigenerRecord = RecordEntity.Create(
            1, null, RecordFormat.Album, "Album", 1990, RecordCondition.Vg, null, userId);

        var fremderRecord = RecordEntity.Create(
            1, null, RecordFormat.Album, "Album", 1990, RecordCondition.Vg, null, fremderUserId);

        Assert.True(recordFilter!.Compile()(eigenerRecord));
        Assert.False(recordFilter.Compile()(fremderRecord));

        var eigenerArtist = ArtistEntity.Create("Pink Floyd", userId);
        var fremderArtist = ArtistEntity.Create("Pink Floyd", fremderUserId);

        Assert.True(artistFilter!.Compile()(eigenerArtist));
        Assert.False(artistFilter.Compile()(fremderArtist));

        var eigenesLabel = LabelEntity.Create("Apple Records", 1, null, userId);
        var fremdesLabel = LabelEntity.Create("Apple Records", 1, null, fremderUserId);

        Assert.True(labelFilter!.Compile()(eigenesLabel));
        Assert.False(labelFilter.Compile()(fremdesLabel));

        var eigenesGenre = GenreEntity.Create("Rock", userId);
        var fremdesGenre = GenreEntity.Create("Rock", fremderUserId);

        Assert.True(genreFilter!.Compile()(eigenesGenre));
        Assert.False(genreFilter.Compile()(fremdesGenre));
    }

    private static IRepository<TEntity> StubTotalCount<TEntity>(int totalCount)
        where TEntity : class
    {
        var repository = Substitute.For<IRepository<TEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<TEntity, bool>>>(),
                Arg.Any<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<TEntity>)new List<TEntity>(), TotalCount: totalCount));

        return repository;
    }
}
