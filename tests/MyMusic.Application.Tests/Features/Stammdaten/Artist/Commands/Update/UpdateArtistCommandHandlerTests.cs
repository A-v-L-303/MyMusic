namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.Commands.Update;

public class UpdateArtistCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerArtist_AktualisiertNamenUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingArtist = ArtistEntity.Create("Genesis", userId);

        var command = new UpdateArtistCommand { Id = existingArtist.Id, Name = "Pink Floyd", UserId = userId };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(existingArtist.Id, Arg.Any<CancellationToken>()).Returns(existingArtist);

        StubConflictingCount(repository, 0);

        var handler = new UpdateArtistCommandHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Pink Floyd", response.Name);

        repository.Received(1).Update(Arg.Is<ArtistEntity>(artist => artist != null && artist.Name == "Pink Floyd"));

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterArtist_WirftNotFoundException()
    {
        // arrange
        var command = new UpdateArtistCommand { Id = 1, Name = "Pink Floyd", UserId = Guid.NewGuid() };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((ArtistEntity?)null);

        var handler = new UpdateArtistCommandHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderArtist_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderArtist = ArtistEntity.Create("Genesis", Guid.NewGuid());

        var command = new UpdateArtistCommand { Id = fremderArtist.Id, Name = "Pink Floyd", UserId = Guid.NewGuid() };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(fremderArtist);

        var handler = new UpdateArtistCommandHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_NameBereitsBeiAnderemEigenenArtistVorhanden_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingArtist = ArtistEntity.Create("Genesis", userId);

        var command = new UpdateArtistCommand { Id = existingArtist.Id, Name = "Pink Floyd", UserId = userId };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(existingArtist.Id, Arg.Any<CancellationToken>()).Returns(existingArtist);

        StubConflictingCount(repository, 1);

        var handler = new UpdateArtistCommandHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

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

        var existingArtist = ArtistEntity.Create("Genesis", userId);

        var command = new UpdateArtistCommand { Id = existingArtist.Id, Name = "Genesis", UserId = userId };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(existingArtist.Id, Arg.Any<CancellationToken>()).Returns(existingArtist);

        Expression<Func<ArtistEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<ArtistEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        var handler = new UpdateArtistCommandHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        // der bearbeitete Datensatz selbst darf keinen Konflikt mit sich selbst auslösen
        Assert.False(predicate(existingArtist));
    }

    private static void StubConflictingCount(IRepository<ArtistEntity> repository, int totalCount)
    {
        var items = totalCount > 0
            ? new List<ArtistEntity> { ArtistEntity.Create("Pink Floyd", Guid.NewGuid()) }
            : new List<ArtistEntity>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)items, TotalCount: totalCount));
    }
}
