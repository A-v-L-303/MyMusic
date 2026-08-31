namespace MyMusic.Application.Tests.Features.Sammlung.Record.Queries.GetById;

public class GetRecordByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerRecordMitArtist_GibtResponseMitLabelUndArtistNamenZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, 1, 2, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Nm, null, userId);

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Apple Records", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(ArtistEntity.Create("The Beatles", userId));

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        StubTracks(trackRepository, []);

        var handler = new GetRecordByIdQueryHandler(
            repository,
            labelRepository,
            artistRepository,
            genreRepository,
            trackRepository,
            new ExceptionManager(),
            new RecordResponseBuilder(),
            new RecordTrackResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetRecordByIdQuery(record.Id, userId), CancellationToken.None);

        // assert
        Assert.Equal("Abbey Road", response.AlbumName);
        Assert.Equal("Apple Records", response.LabelName);
        Assert.Equal("The Beatles", response.ArtistName);
        Assert.Empty(response.Tracks);
    }

    [Fact]
    public async Task HandleAsync_EigenerRecordOhneArtist_ArtistNameIstNull()
    {
        // arrange
        var userId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, 1, null, RecordFormat.Compilation, "Various Artists", 1999, RecordCondition.Vg, null, userId);

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Various Records", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        StubTracks(trackRepository, []);

        var handler = new GetRecordByIdQueryHandler(
            repository,
            labelRepository,
            artistRepository,
            genreRepository,
            trackRepository,
            new ExceptionManager(),
            new RecordResponseBuilder(),
            new RecordTrackResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetRecordByIdQuery(record.Id, userId), CancellationToken.None);

        // assert
        Assert.Null(response.ArtistName);

        await artistRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_RecordMitTracks_GibtTracklisteMitAufgeloestenNamenZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, 1, 2, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Nm, null, userId);

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Apple Records", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(ArtistEntity.Create("The Beatles", userId));

        artistRepository.GetByIdAsync(3, Arg.Any<CancellationToken>())
            .Returns(ArtistEntity.Create("Billy Preston", userId));

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        genreRepository.GetByIdAsync(4, Arg.Any<CancellationToken>())
            .Returns(GenreEntity.Create("Rock", userId));

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var firstTrack = RecordTrackEntity.Create(1, 2, 4, "Come Together", "A", 1, null, userId);

        var secondTrack = RecordTrackEntity.Create(1, 3, 4, "Something", "A", 2, null, userId);

        StubTracks(trackRepository, [firstTrack, secondTrack]);

        var handler = new GetRecordByIdQueryHandler(
            repository,
            labelRepository,
            artistRepository,
            genreRepository,
            trackRepository,
            new ExceptionManager(),
            new RecordResponseBuilder(),
            new RecordTrackResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetRecordByIdQuery(record.Id, userId), CancellationToken.None);

        // assert
        Assert.Equal(2, response.Tracks.Count);
        Assert.Equal("Come Together", response.Tracks[0].TrackName);
        Assert.Equal("The Beatles", response.Tracks[0].ArtistName);
        Assert.Equal("Rock", response.Tracks[0].GenreName);
        Assert.Equal("Something", response.Tracks[1].TrackName);
        Assert.Equal("Billy Preston", response.Tracks[1].ArtistName);
    }

    [Fact]
    public async Task HandleAsync_UnbekannterRecord_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((RecordEntity?)null);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var handler = new GetRecordByIdQueryHandler(
            repository,
            labelRepository,
            artistRepository,
            genreRepository,
            trackRepository,
            new ExceptionManager(),
            new RecordResponseBuilder(),
            new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(new GetRecordByIdQuery(1, Guid.NewGuid()), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderRecord_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderRecord = RecordEntity.Create(
            1, 1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, Guid.NewGuid());

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(fremderRecord.Id, Arg.Any<CancellationToken>()).Returns(fremderRecord);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var trackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var handler = new GetRecordByIdQueryHandler(
            repository,
            labelRepository,
            artistRepository,
            genreRepository,
            trackRepository,
            new ExceptionManager(),
            new RecordResponseBuilder(),
            new RecordTrackResponseBuilder());

        var query = new GetRecordByIdQuery(fremderRecord.Id, Guid.NewGuid());

        // act
        var act = () => handler.HandleAsync(query, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    private static void StubTracks(IRepository<RecordTrackEntity> trackRepository, List<RecordTrackEntity> tracks)
    {
        trackRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordTrackEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordTrackEntity>, IOrderedQueryable<RecordTrackEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordTrackEntity>)tracks, TotalCount: tracks.Count));
    }
}
