namespace MyMusic.Application.Tests.Features.Sammlung.Record.Commands.UploadCover;

public class UploadRecordCoverCommandHandlerTests
{
    private static readonly byte[] _jpegBytes = [0xFF, 0xD8, 0xFF, 0xE0];

    [Fact]
    public async Task HandleAsync_EigenerRecord_SetztCoverUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingRecord = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        var command = new UploadRecordCoverCommand
        {
            Id = existingRecord.Id,
            UserId = userId,
            FileContent = _jpegBytes
        };

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(existingRecord.Id, Arg.Any<CancellationToken>()).Returns(existingRecord);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Universal", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        StubNoTracks(trackRepository);

        var handler = new UploadRecordCoverCommandHandler(
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
        Assert.Equal($"data:image/jpeg;base64,{Convert.ToBase64String(_jpegBytes)}", response.AlbumCoverDataUrl);
        Assert.Empty(response.Tracks);

        repository.Received(1).Update(
            Arg.Is<RecordEntity>(record => record != null && record.AlbumCover == _jpegBytes));

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterRecord_WirftNotFoundException()
    {
        // arrange
        var command = new UploadRecordCoverCommand
        {
            Id = 1,
            UserId = Guid.NewGuid(),
            FileContent = _jpegBytes
        };

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((RecordEntity?)null);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var handler = new UploadRecordCoverCommandHandler(
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

        var command = new UploadRecordCoverCommand
        {
            Id = fremderRecord.Id,
            UserId = Guid.NewGuid(),
            FileContent = _jpegBytes
        };

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(fremderRecord);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var handler = new UploadRecordCoverCommandHandler(
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
