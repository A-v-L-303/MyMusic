namespace MyMusic.Application.Tests.Features.Sammlung.Record.Commands.Update;

public class UpdateRecordCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerRecord_AktualisiertWerteUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingRecord = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        var command = new UpdateRecordCommand
        {
            Id = existingRecord.Id,
            LabelId = 2,
            ArtistId = 3,
            Format = RecordFormat.CdAlbum,
            AlbumName = "Abbey Road (Remastered)",
            ReleaseYear = 2019,
            Condition = RecordCondition.Mint,
            Information = "Jubiläumsausgabe",
            UserId = userId
        };

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(existingRecord.Id, Arg.Any<CancellationToken>()).Returns(existingRecord);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Universal", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(3, Arg.Any<CancellationToken>())
            .Returns(ArtistEntity.Create("The Beatles", userId));

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        StubNoTracks(trackRepository);

        var handler = new UpdateRecordCommandHandler(
            repository,
            labelRepository,
            artistRepository,
            genreRepository,
            trackRepository,
            new ExceptionManager(),
            new RecordResponseBuilder(),
            new RecordTrackResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Abbey Road (Remastered)", response.AlbumName);
        Assert.Equal("Universal", response.LabelName);
        Assert.Equal("The Beatles", response.ArtistName);
        Assert.Equal(RecordCondition.Mint, response.Condition);
        Assert.Empty(response.Tracks);

        repository.Received(1).Update(
            Arg.Is<RecordEntity>(record => record != null && record.AlbumName == "Abbey Road (Remastered)"));

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterRecord_WirftNotFoundException()
    {
        // arrange
        var command = new UpdateRecordCommand
        {
            Id = 1,
            LabelId = 1,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = Guid.NewGuid()
        };

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((RecordEntity?)null);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var handler = new UpdateRecordCommandHandler(
            repository,
            labelRepository,
            artistRepository,
            genreRepository,
            trackRepository,
            new ExceptionManager(),
            new RecordResponseBuilder(),
            new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderRecord_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderRecord = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, Guid.NewGuid());

        var command = new UpdateRecordCommand
        {
            Id = fremderRecord.Id,
            LabelId = 1,
            AlbumName = "Irrelevant",
            ReleaseYear = 1969,
            UserId = Guid.NewGuid()
        };

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(fremderRecord);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var handler = new UpdateRecordCommandHandler(
            repository,
            labelRepository,
            artistRepository,
            genreRepository,
            trackRepository,
            new ExceptionManager(),
            new RecordResponseBuilder(),
            new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    private static void StubNoTracks(IRepository<RecordTrackEntity> trackRepository)
    {
        trackRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordTrackEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordTrackEntity>, IOrderedQueryable<RecordTrackEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordTrackEntity>)new List<RecordTrackEntity>(), TotalCount: 0));
    }
}
