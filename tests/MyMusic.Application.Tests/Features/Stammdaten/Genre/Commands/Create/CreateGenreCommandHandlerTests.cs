namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.Commands.Create;

public class CreateGenreCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NeuerName_LegtGenreAnUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateGenreCommand { Name = "Jazz", UserId = userId };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        StubExistingCount(repository, 0);

        var handler = new CreateGenreCommandHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Jazz", response.Name);

        await repository.Received(1).AddAsync(
            Arg.Is<GenreEntity>(genre => genre != null && genre.Name == "Jazz" && genre.UserId == userId),
            Arg.Any<CancellationToken>());

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NameBereitsVorhandenFuerBenutzer_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateGenreCommand { Name = "Jazz", UserId = userId };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        StubExistingCount(repository, 1);

        var handler = new CreateGenreCommandHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        await repository.DidNotReceive().AddAsync(Arg.Any<GenreEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PruefungBeschraenktSichAufEigeneGenresDesBenutzers()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateGenreCommand { Name = "Jazz", UserId = userId };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        Expression<Func<GenreEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<GenreEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)new List<GenreEntity>(), TotalCount: 0));

        var handler = new CreateGenreCommandHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesGenreGleicherName = GenreEntity.Create("Jazz", userId);

        var fremdesGenreGleicherName = GenreEntity.Create("Jazz", Guid.NewGuid());

        // gleicher Name eines anderen Benutzers darf keinen Konflikt auslösen (Mandantentrennung)
        Assert.True(predicate(eigenesGenreGleicherName));
        Assert.False(predicate(fremdesGenreGleicherName));
    }

    private static void StubExistingCount(IRepository<GenreEntity> repository, int totalCount)
    {
        var items = totalCount > 0
            ? new List<GenreEntity> { GenreEntity.Create("Jazz", Guid.NewGuid()) }
            : new List<GenreEntity>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<GenreEntity, bool>>>(),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)items, TotalCount: totalCount));
    }
}
