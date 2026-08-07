namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.Commands.Create;

public class CreateArtistCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NeuerName_LegtArtistAnUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateArtistCommand { Name = "Pink Floyd", UserId = userId };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        StubExistingCount(repository, 0);

        var handler = new CreateArtistCommandHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Pink Floyd", response.Name);

        await repository.Received(1).AddAsync(
            Arg.Is<ArtistEntity>(artist => artist != null && artist.Name == "Pink Floyd" && artist.UserId == userId),
            Arg.Any<CancellationToken>());

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NameBereitsVorhandenFuerBenutzer_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateArtistCommand { Name = "Pink Floyd", UserId = userId };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        StubExistingCount(repository, 1);

        var handler = new CreateArtistCommandHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        await repository.DidNotReceive().AddAsync(Arg.Any<ArtistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PruefungBeschraenktSichAufEigeneArtistsDesBenutzers()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateArtistCommand { Name = "Pink Floyd", UserId = userId };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        Expression<Func<ArtistEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<ArtistEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        var handler = new CreateArtistCommandHandler(repository, new ExceptionManager(), new ArtistResponseBuilder());

        // act
        await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenerArtistGleicherName = ArtistEntity.Create("Pink Floyd", userId);

        var fremderArtistGleicherName = ArtistEntity.Create("Pink Floyd", Guid.NewGuid());

        // gleicher Name eines anderen Benutzers darf keinen Konflikt auslösen (Mandantentrennung)
        Assert.True(predicate(eigenerArtistGleicherName));
        Assert.False(predicate(fremderArtistGleicherName));
    }

    private static void StubExistingCount(IRepository<ArtistEntity> repository, int totalCount)
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
