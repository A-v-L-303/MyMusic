namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.Commands.Delete;

public class DeleteGenreCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenesGenre_EntferntGenreUndGibtTrueZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var genre = GenreEntity.Create("Rock", userId);

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteGenreCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var result = await handler.HandleAsync(new DeleteGenreCommand(genre.Id), CancellationToken.None);

        // assert
        Assert.True(result);

        repository.Received(1).Remove(genre);

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekanntesGenre_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((GenreEntity?)null);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteGenreCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteGenreCommand(1), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremdesGenre_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremdesGenre = GenreEntity.Create("Rock", Guid.NewGuid());

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(fremdesGenre.Id, Arg.Any<CancellationToken>()).Returns(fremdesGenre);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteGenreCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteGenreCommand(fremdesGenre.Id), CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);

        repository.DidNotReceive().Remove(Arg.Any<GenreEntity>());
    }
}
