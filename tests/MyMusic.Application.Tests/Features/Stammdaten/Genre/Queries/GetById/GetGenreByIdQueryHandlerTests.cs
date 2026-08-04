namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.Queries.GetById;

public class GetGenreByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenesGenre_GibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var genre = GenreEntity.Create("Jazz", userId);

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);

        var handler = new GetGenreByIdQueryHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetGenreByIdQuery(genre.Id, userId), CancellationToken.None);

        // assert
        Assert.Equal("Jazz", response.Name);
    }

    [Fact]
    public async Task HandleAsync_UnbekanntesGenre_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((GenreEntity?)null);

        var handler = new GetGenreByIdQueryHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        // act
        var act = () => handler.HandleAsync(new GetGenreByIdQuery(1, Guid.NewGuid()), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremdesGenre_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremdesGenre = GenreEntity.Create("Jazz", Guid.NewGuid());

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(fremdesGenre.Id, Arg.Any<CancellationToken>()).Returns(fremdesGenre);

        var handler = new GetGenreByIdQueryHandler(repository, new ExceptionManager(), new GenreResponseBuilder());

        var query = new GetGenreByIdQuery(fremdesGenre.Id, Guid.NewGuid());

        // act
        var act = () => handler.HandleAsync(query, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
