namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.Queries.GetAll;

public class GetAllGenresQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_RuftGetPagedAsyncMitSeite1UndMaximalerSeitengroesseAufUndMapptErgebnis()
    {
        // arrange
        var userId = Guid.NewGuid();

        // Die tatsächliche alphabetische Sortierung übernimmt das an GetPagedAsync übergebene
        // OrderBy-Delegate (in der echten Implementierung datenbankseitig übersetzt) - der
        // Substitute-Mock führt es nicht aus, daher liefert der Handler hier bewusst exakt die
        // vom Repository gelieferte Reihenfolge unverändert weiter (kein eigenes C#-Sortieren,
        // anders als bei GetAllCountriesQueryHandler, dessen GetAllAsync kein OrderBy kennt).
        var genres = new List<GenreEntity>
        {
            GenreEntity.Create("Ambient", userId),
            GenreEntity.Create("Jazz", userId),
            GenreEntity.Create("Rock", userId),
        };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<GenreEntity, bool>>>(),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)genres, TotalCount: genres.Count));

        var handler = new GetAllGenresQueryHandler(repository, new GenreResponseBuilder());

        // act
        var response = (await handler.HandleAsync(new GetAllGenresQuery(userId), CancellationToken.None)).ToList();

        // assert
        Assert.Equal(3, response.Count);
        Assert.Equal("Ambient", response[0].Name);
        Assert.Equal("Jazz", response[1].Name);
        Assert.Equal("Rock", response[2].Name);
    }

    [Fact]
    public async Task HandleAsync_LeereListe_GibtLeereListeZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<GenreEntity, bool>>>(),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)new List<GenreEntity>(), TotalCount: 0));

        var handler = new GetAllGenresQueryHandler(repository, new GenreResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetAllGenresQuery(userId), CancellationToken.None);

        // assert
        Assert.Empty(response);
    }

    [Fact]
    public async Task HandleAsync_FilterBeschraenktAufEigeneGenres()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<GenreEntity>>();

        Expression<Func<GenreEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<GenreEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)new List<GenreEntity>(), TotalCount: 0));

        var handler = new GetAllGenresQueryHandler(repository, new GenreResponseBuilder());

        // act
        await handler.HandleAsync(new GetAllGenresQuery(userId), CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesGenre = GenreEntity.Create("Jazz", userId);

        var fremdesGenre = GenreEntity.Create("Jazz", Guid.NewGuid());

        Assert.True(predicate(eigenesGenre));

        // Mandantentrennung: fremde Genres dürfen nicht mitgeliefert werden
        Assert.False(predicate(fremdesGenre));
    }
}
