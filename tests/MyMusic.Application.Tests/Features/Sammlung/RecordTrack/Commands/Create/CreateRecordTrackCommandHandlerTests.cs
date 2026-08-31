namespace MyMusic.Application.Tests.Features.Sammlung.RecordTrack.Commands.Create;

public class CreateRecordTrackCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerRecordOhneKonflikt_LegtTrackAnUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        const int recordId = 1;

        var record = RecordEntity.Create(
            1, 1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        var command = new CreateRecordTrackCommand
        {
            RecordId = recordId,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        StubExistingCount(repository, 0);

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetByIdAsync(recordId, Arg.Any<CancellationToken>()).Returns(record);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(ArtistEntity.Create("The Beatles", userId));

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        genreRepository.GetByIdAsync(3, Arg.Any<CancellationToken>())
            .Returns(GenreEntity.Create("Rock", userId));

        var handler = new CreateRecordTrackCommandHandler(
            repository,
            recordRepository,
            artistRepository,
            genreRepository,
            new ExceptionManager(),
            new RecordTrackResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Come Together", response.TrackName);
        Assert.Equal("The Beatles", response.ArtistName);
        Assert.Equal("Rock", response.GenreName);
        Assert.Equal("A", response.RecordSide);
        Assert.Equal(1, response.TrackNumber);

        await repository.Received(1).AddAsync(
            Arg.Is<RecordTrackEntity>(track => track != null && track.TrackName == "Come Together"),
            Arg.Any<CancellationToken>());

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterRecord_WirftNotFoundException()
    {
        // arrange
        var command = new CreateRecordTrackCommand
        {
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = Guid.NewGuid()
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((RecordEntity?)null);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var handler = new CreateRecordTrackCommandHandler(
            repository,
            recordRepository,
            artistRepository,
            genreRepository,
            new ExceptionManager(),
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
            1, 1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, Guid.NewGuid());

        var command = new CreateRecordTrackCommand
        {
            RecordId = fremderRecord.Id,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = Guid.NewGuid()
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetByIdAsync(fremderRecord.Id, Arg.Any<CancellationToken>()).Returns(fremderRecord);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var handler = new CreateRecordTrackCommandHandler(
            repository,
            recordRepository,
            artistRepository,
            genreRepository,
            new ExceptionManager(),
            new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_RecordSideUndTrackNumberBereitsVergeben_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, 1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        var command = new CreateRecordTrackCommand
        {
            RecordId = record.Id,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        StubExistingCount(repository, 1);

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var handler = new CreateRecordTrackCommandHandler(
            repository,
            recordRepository,
            artistRepository,
            genreRepository,
            new ExceptionManager(),
            new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        await repository.DidNotReceive().AddAsync(Arg.Any<RecordTrackEntity>(), Arg.Any<CancellationToken>());
    }

    private static void StubExistingCount(IRepository<RecordTrackEntity> repository, int totalCount)
    {
        var items = totalCount > 0
            ? new List<RecordTrackEntity>
            {
                RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, Guid.NewGuid())
            }
            : new List<RecordTrackEntity>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordTrackEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordTrackEntity>, IOrderedQueryable<RecordTrackEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordTrackEntity>)items, TotalCount: totalCount));
    }
}
