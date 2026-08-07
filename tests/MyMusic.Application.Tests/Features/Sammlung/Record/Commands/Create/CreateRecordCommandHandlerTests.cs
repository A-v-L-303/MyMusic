namespace MyMusic.Application.Tests.Features.Sammlung.Record.Commands.Create;

public class CreateRecordCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_GueltigeWerteMitArtist_LegtRecordAnUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            ArtistId = 2,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            Condition = RecordCondition.Nm,
            Information = "Erste Pressung",
            UserId = userId
        };

        var repository = Substitute.For<IRepository<RecordEntity>>();

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Apple Records", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(ArtistEntity.Create("The Beatles", userId));

        var handler = new CreateRecordCommandHandler(
            repository, labelRepository, artistRepository, new RecordResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Abbey Road", response.AlbumName);
        Assert.Equal("Apple Records", response.LabelName);
        Assert.Equal("The Beatles", response.ArtistName);
        Assert.Equal(RecordCondition.Nm, response.Condition);

        await repository.Received(1).AddAsync(
            Arg.Is<RecordEntity>(record => record != null && record.AlbumName == "Abbey Road"
                && record.UserId == userId),
            Arg.Any<CancellationToken>());

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_OhneArtist_ArtistNameIstNull()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            ArtistId = null,
            Format = RecordFormat.Compilation,
            AlbumName = "Various Artists",
            ReleaseYear = 1999,
            Condition = RecordCondition.Vg,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<RecordEntity>>();

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Various Records", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new CreateRecordCommandHandler(
            repository, labelRepository, artistRepository, new RecordResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Null(response.ArtistId);
        Assert.Null(response.ArtistName);

        await artistRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
