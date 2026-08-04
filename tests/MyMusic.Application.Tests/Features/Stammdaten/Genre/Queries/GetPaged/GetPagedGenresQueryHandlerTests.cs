namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.Queries.GetPaged;

public class GetPagedGenresQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_LeitetSeitenparameterWeiterUndMapptErgebnis()
    {
        // arrange
        var userId = Guid.NewGuid();

        var genres = new List<GenreEntity> { GenreEntity.Create("Jazz", userId), GenreEntity.Create("Rock", userId) };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<GenreEntity, bool>>>(),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                2,
                10,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)genres, TotalCount: 12));

        var handler = new GetPagedGenresQueryHandler(repository, new GenreResponseBuilder());

        var query = new GetPagedGenresQuery(userId, Page: 2, PageSize: 10, Name: null);

        // act
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(12, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
    }

    [Fact]
    public async Task HandleAsync_FilterBeschraenktAufEigeneGenresUndOptionalenNamen()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<GenreEntity>>();

        Expression<Func<GenreEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<GenreEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)new List<GenreEntity>(), TotalCount: 0));

        var handler = new GetPagedGenresQueryHandler(repository, new GenreResponseBuilder());

        var query = new GetPagedGenresQuery(userId, Page: 1, PageSize: 20, Name: "ja");

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesTreffer = GenreEntity.Create("Jazz", userId);

        var eigenesKeinTreffer = GenreEntity.Create("Rock", userId);

        var fremdesTreffer = GenreEntity.Create("Jazz", Guid.NewGuid());

        Assert.True(predicate(eigenesTreffer));
        Assert.False(predicate(eigenesKeinTreffer));

        // Mandantentrennung gilt auch für die Namensfilterung der Liste
        Assert.False(predicate(fremdesTreffer));
    }
}
