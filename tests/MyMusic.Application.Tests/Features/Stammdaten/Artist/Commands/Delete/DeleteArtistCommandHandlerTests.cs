namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.Commands.Delete;

public class DeleteArtistCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerArtist_EntferntArtistUndGibtTrueZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artist = ArtistEntity.Create("Genesis", userId);

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(artist.Id, Arg.Any<CancellationToken>()).Returns(artist);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteArtistCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var result = await handler.HandleAsync(new DeleteArtistCommand(artist.Id), CancellationToken.None);

        // assert
        Assert.True(result);

        repository.Received(1).Remove(artist);

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterArtist_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((ArtistEntity?)null);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteArtistCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteArtistCommand(1), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderArtist_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderArtist = ArtistEntity.Create("Genesis", Guid.NewGuid());

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(fremderArtist.Id, Arg.Any<CancellationToken>()).Returns(fremderArtist);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteArtistCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteArtistCommand(fremderArtist.Id), CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);

        repository.DidNotReceive().Remove(Arg.Any<ArtistEntity>());
    }
}
