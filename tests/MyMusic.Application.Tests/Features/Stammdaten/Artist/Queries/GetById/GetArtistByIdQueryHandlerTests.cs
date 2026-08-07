namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.Queries.GetById;

public class GetArtistByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerArtist_GibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artist = ArtistEntity.Create("Pink Floyd", userId);

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(artist.Id, Arg.Any<CancellationToken>()).Returns(artist);

        var handler = new GetArtistByIdQueryHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetArtistByIdQuery(artist.Id, userId), CancellationToken.None);

        // assert
        Assert.Equal("Pink Floyd", response.Name);
    }

    [Fact]
    public async Task HandleAsync_UnbekannterArtist_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((ArtistEntity?)null);

        var handler = new GetArtistByIdQueryHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        var act = () => handler.HandleAsync(new GetArtistByIdQuery(1, Guid.NewGuid()), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderArtist_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderArtist = ArtistEntity.Create("Pink Floyd", Guid.NewGuid());

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(fremderArtist.Id, Arg.Any<CancellationToken>()).Returns(fremderArtist);

        var handler = new GetArtistByIdQueryHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        var query = new GetArtistByIdQuery(fremderArtist.Id, Guid.NewGuid());

        // act
        var act = () => handler.HandleAsync(query, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
