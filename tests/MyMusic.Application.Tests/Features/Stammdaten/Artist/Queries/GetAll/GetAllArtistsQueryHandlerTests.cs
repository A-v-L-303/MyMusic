namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.Queries.GetAll;

public class GetAllArtistsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_RuftGetPagedAsyncMitSeite1UndMaximalerSeitengroesseAufUndMapptErgebnis()
    {
        // arrange
        var userId = Guid.NewGuid();

        // Die tatsächliche alphabetische Sortierung übernimmt das an GetPagedAsync übergebene
        // OrderBy-Delegate (in der echten Implementierung datenbankseitig übersetzt) - der
        // Substitute-Mock führt es nicht aus, daher liefert der Handler hier bewusst exakt die
        // vom Repository gelieferte Reihenfolge unverändert weiter.
        var artists = new List<ArtistEntity>
        {
            ArtistEntity.Create("Daft Punk", userId),
            ArtistEntity.Create("Miles Davis", userId),
            ArtistEntity.Create("Radiohead", userId),
        };

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)artists, TotalCount: artists.Count));

        var handler = new GetAllArtistsQueryHandler(repository, new ArtistResponseBuilder());

        // act
        var response = (await handler.HandleAsync(new GetAllArtistsQuery(userId), CancellationToken.None)).ToList();

        // assert
        Assert.Equal(3, response.Count);
        Assert.Equal("Daft Punk", response[0].Name);
        Assert.Equal("Miles Davis", response[1].Name);
        Assert.Equal("Radiohead", response[2].Name);
    }

    [Fact]
    public async Task HandleAsync_LeereListe_GibtLeereListeZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        var handler = new GetAllArtistsQueryHandler(repository, new ArtistResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetAllArtistsQuery(userId), CancellationToken.None);

        // assert
        Assert.Empty(response);
    }

    [Fact]
    public async Task HandleAsync_FilterBeschraenktAufEigeneArtists()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        Expression<Func<ArtistEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<ArtistEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        var handler = new GetAllArtistsQueryHandler(repository, new ArtistResponseBuilder());

        // act
        await handler.HandleAsync(new GetAllArtistsQuery(userId), CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenerArtist = ArtistEntity.Create("Daft Punk", userId);

        var fremderArtist = ArtistEntity.Create("Daft Punk", Guid.NewGuid());

        Assert.True(predicate(eigenerArtist));

        // Mandantentrennung: fremde Artists dürfen nicht mitgeliefert werden
        Assert.False(predicate(fremderArtist));
    }
}
