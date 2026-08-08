namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.Queries.GetPaged;

public class GetPagedArtistsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_LeitetSeitenparameterWeiterUndMapptErgebnis()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artists = new List<ArtistEntity>
        {
            ArtistEntity.Create("Pink Floyd", userId),
            ArtistEntity.Create("Genesis", userId)
        };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                2,
                10,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)artists, TotalCount: 12));

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        var handler = new GetPagedArtistsQueryHandler(repository, recordRepository, new ArtistResponseBuilder());

        var query = new GetPagedArtistsQuery(userId, Page: 2, PageSize: 10, Name: null, LabelId: null);

        // act
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(12, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
    }

    [Fact]
    public async Task HandleAsync_FilterBeschraenktAufEigeneArtistsUndOptionalenNamen()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        Expression<Func<ArtistEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<ArtistEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        var handler = new GetPagedArtistsQueryHandler(repository, recordRepository, new ArtistResponseBuilder());

        var query = new GetPagedArtistsQuery(userId, Page: 1, PageSize: 20, Name: "pink", LabelId: null);

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenerTreffer = ArtistEntity.Create("Pink Floyd", userId);

        var eigenerKeinTreffer = ArtistEntity.Create("Genesis", userId);

        var fremderTreffer = ArtistEntity.Create("Pink Floyd", Guid.NewGuid());

        Assert.True(predicate(eigenerTreffer));
        Assert.False(predicate(eigenerKeinTreffer));

        // Mandantentrennung gilt auch für die Namensfilterung der Liste
        Assert.False(predicate(fremderTreffer));
    }

    [Fact]
    public async Task HandleAsync_LabelFilterGesetzt_LoestArtistsDesLabelsMandantengefiltertAuf()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        Expression<Func<RecordEntity, bool>>? capturedFilter = null;

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Do<Expression<Func<RecordEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var handler = new GetPagedArtistsQueryHandler(repository, recordRepository, new ArtistResponseBuilder());

        var query = new GetPagedArtistsQuery(userId, Page: 1, PageSize: 20, Name: null, LabelId: 5);

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenerRecordRichtigesLabel = RecordEntity.Create(
            5, 1, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        var eigenerRecordFalschesLabel = RecordEntity.Create(
            6, 1, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        var fremderRecordRichtigesLabel = RecordEntity.Create(
            5, 1, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, Guid.NewGuid());

        Assert.True(predicate(eigenerRecordRichtigesLabel));
        Assert.False(predicate(eigenerRecordFalschesLabel));

        // Mandantentrennung gilt auch für die Artist-Auflösung nach Label
        Assert.False(predicate(fremderRecordRichtigesLabel));
    }

    [Fact]
    public async Task HandleAsync_OhneLabelFilter_RuftRecordRepositoryNichtAuf()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        var handler = new GetPagedArtistsQueryHandler(repository, recordRepository, new ArtistResponseBuilder());

        var query = new GetPagedArtistsQuery(userId, Page: 1, PageSize: 20, Name: null, LabelId: null);

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        await recordRepository.DidNotReceive().GetPagedAsync(
            Arg.Any<Expression<Func<RecordEntity, bool>>>(),
            Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }
}
