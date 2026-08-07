namespace MyMusic.Application.Tests.Features.Sammlung.RecordTrack.Commands.Update;

public class UpdateRecordTrackCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerTrack_AktualisiertWerteUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingTrack = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = existingTrack.Id,
            RecordId = 1,
            ArtistId = 4,
            GenreId = 5,
            TrackName = "Something",
            RecordSide = "A",
            TrackNumber = 2,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(existingTrack.Id, Arg.Any<CancellationToken>()).Returns(existingTrack);

        StubExistingCount(repository, 0);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(4, Arg.Any<CancellationToken>())
            .Returns(ArtistEntity.Create("Billy Preston", userId));

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        genreRepository.GetByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns(GenreEntity.Create("Pop", userId));

        var handler = new UpdateRecordTrackCommandHandler(
            repository, artistRepository, genreRepository, new ExceptionManager(), new RecordTrackResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Something", response.TrackName);
        Assert.Equal("Billy Preston", response.ArtistName);
        Assert.Equal("Pop", response.GenreName);
        Assert.Equal(2, response.TrackNumber);

        repository.Received(1).Update(
            Arg.Is<RecordTrackEntity>(track => track != null && track.TrackName == "Something"));

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterTrack_WirftNotFoundException()
    {
        // arrange
        var command = new UpdateRecordTrackCommand
        {
            Id = 1,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = Guid.NewGuid()
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((RecordTrackEntity?)null);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var handler = new UpdateRecordTrackCommandHandler(
            repository, artistRepository, genreRepository, new ExceptionManager(), new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderTrack_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderTrack = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, Guid.NewGuid());

        var command = new UpdateRecordTrackCommand
        {
            Id = fremderTrack.Id,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = Guid.NewGuid()
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(fremderTrack.Id, Arg.Any<CancellationToken>()).Returns(fremderTrack);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var handler = new UpdateRecordTrackCommandHandler(
            repository, artistRepository, genreRepository, new ExceptionManager(), new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_TrackGehoertAnderemRecord_WirftNotFoundException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingTrack = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = existingTrack.Id,
            RecordId = 999,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(existingTrack.Id, Arg.Any<CancellationToken>()).Returns(existingTrack);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var handler = new UpdateRecordTrackCommandHandler(
            repository, artistRepository, genreRepository, new ExceptionManager(), new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_RecordSideUndTrackNumberBereitsVonAnderemTrackVergeben_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingTrack = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = existingTrack.Id,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 2,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(existingTrack.Id, Arg.Any<CancellationToken>()).Returns(existingTrack);

        StubExistingCount(repository, 1);

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var handler = new UpdateRecordTrackCommandHandler(
            repository, artistRepository, genreRepository, new ExceptionManager(), new RecordTrackResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        repository.DidNotReceive().Update(Arg.Any<RecordTrackEntity>());
    }

    private static void StubExistingCount(IRepository<RecordTrackEntity> repository, int totalCount)
    {
        var items = totalCount > 0
            ? new List<RecordTrackEntity>
            {
                RecordTrackEntity.Create(1, 2, 3, "Something Else", "A", 2, null, Guid.NewGuid())
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
