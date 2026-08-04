namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.Commands.Update;

public class UpdateGenreCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenesGenre_AktualisiertNamenUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingGenre = GenreEntity.Create("Rock", userId);

        var command = new UpdateGenreCommand { Id = existingGenre.Id, Name = "Pop", UserId = userId };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(existingGenre.Id, Arg.Any<CancellationToken>()).Returns(existingGenre);

        StubConflictingCount(repository, 0);

        var handler = new UpdateGenreCommandHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Pop", response.Name);

        repository.Received(1).Update(Arg.Is<GenreEntity>(genre => genre != null && genre.Name == "Pop"));

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekanntesGenre_WirftNotFoundException()
    {
        // arrange
        var command = new UpdateGenreCommand { Id = 1, Name = "Pop", UserId = Guid.NewGuid() };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((GenreEntity?)null);

        var handler = new UpdateGenreCommandHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremdesGenre_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderGenre = GenreEntity.Create("Rock", Guid.NewGuid());

        var command = new UpdateGenreCommand { Id = fremderGenre.Id, Name = "Pop", UserId = Guid.NewGuid() };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(fremderGenre);

        var handler = new UpdateGenreCommandHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_NameBereitsBeiAnderemEigenenGenreVorhanden_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingGenre = GenreEntity.Create("Rock", userId);

        var command = new UpdateGenreCommand { Id = existingGenre.Id, Name = "Pop", UserId = userId };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(existingGenre.Id, Arg.Any<CancellationToken>()).Returns(existingGenre);

        StubConflictingCount(repository, 1);

        var handler = new UpdateGenreCommandHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task HandleAsync_PruefungSchliesstDenEigenenDatensatzAus()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingGenre = GenreEntity.Create("Rock", userId);

        var command = new UpdateGenreCommand { Id = existingGenre.Id, Name = "Rock", UserId = userId };

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(existingGenre.Id, Arg.Any<CancellationToken>()).Returns(existingGenre);

        Expression<Func<GenreEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<GenreEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)new List<GenreEntity>(), TotalCount: 0));

        var handler = new UpdateGenreCommandHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        // der bearbeitete Datensatz selbst darf keinen Konflikt mit sich selbst auslösen
        Assert.False(predicate(existingGenre));
    }

    private static void StubConflictingCount(IRepository<GenreEntity> repository, int totalCount)
    {
        var items = totalCount > 0
            ? new List<GenreEntity> { GenreEntity.Create("Pop", Guid.NewGuid()) }
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
