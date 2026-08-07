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

        var handler = new GetPagedArtistsQueryHandler(repository, new ArtistResponseBuilder());

        var query = new GetPagedArtistsQuery(userId, Page: 2, PageSize: 10, Name: null);

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

        var handler = new GetPagedArtistsQueryHandler(repository, new ArtistResponseBuilder());

        var query = new GetPagedArtistsQuery(userId, Page: 1, PageSize: 20, Name: "pink");

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
}
