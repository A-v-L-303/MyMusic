namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.Commands.Delete;

public class DeleteArtistCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerArtist_EntferntArtistUndGibtTrueZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artist = ArtistEntity.Create("Genesis", userId);

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(artist.Id, Arg.Any<CancellationToken>()).Returns(artist);

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                1,
                1,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        recordTrackRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordTrackEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordTrackEntity>, IOrderedQueryable<RecordTrackEntity>>>(),
                1,
                1,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordTrackEntity>)new List<RecordTrackEntity>(), TotalCount: 0));

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteArtistCommandHandler(
            repository, recordRepository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var result = await handler.HandleAsync(new DeleteArtistCommand(artist.Id), CancellationToken.None);

        // assert
        Assert.True(result);

        repository.Received(1).Remove(artist);

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterArtist_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((ArtistEntity?)null);

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteArtistCommandHandler(
            repository, recordRepository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteArtistCommand(1), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderArtist_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderArtist = ArtistEntity.Create("Genesis", Guid.NewGuid());

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(fremderArtist.Id, Arg.Any<CancellationToken>()).Returns(fremderArtist);

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteArtistCommandHandler(
            repository, recordRepository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteArtistCommand(fremderArtist.Id), CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);

        repository.DidNotReceive().Remove(Arg.Any<ArtistEntity>());
    }

    [Fact]
    public async Task HandleAsync_ArtistReferenziertVonRecord_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artist = ArtistEntity.Create("Genesis", userId);

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(artist.Id, Arg.Any<CancellationToken>()).Returns(artist);

        var referencingRecord = RecordEntity.Create(
            1, 1, artist.Id, RecordFormat.Album, "Abacab", 1981, RecordCondition.Vg, null, userId);

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                1,
                1,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity> { referencingRecord },
                TotalCount: 1));

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteArtistCommandHandler(
            repository, recordRepository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteArtistCommand(artist.Id), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        repository.DidNotReceive().Remove(Arg.Any<ArtistEntity>());

        // Kurzschluss: Der Record-Treffer verhindert die zweite (Track-)Abfrage
        await recordTrackRepository.DidNotReceive().GetPagedAsync(
            Arg.Any<Expression<Func<RecordTrackEntity, bool>>>(),
            Arg.Any<Func<IQueryable<RecordTrackEntity>, IOrderedQueryable<RecordTrackEntity>>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ArtistReferenziertVonTrack_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artist = ArtistEntity.Create("Genesis", userId);

        var repository = Substitute.For<IRepository<ArtistEntity>>();

        repository.GetByIdAsync(artist.Id, Arg.Any<CancellationToken>()).Returns(artist);

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                1,
                1,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var referencingTrack = RecordTrackEntity.Create(
            1, 1, 1, "Track", "A", 1, null, userId);

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        recordTrackRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordTrackEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordTrackEntity>, IOrderedQueryable<RecordTrackEntity>>>(),
                1,
                1,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordTrackEntity>)new List<RecordTrackEntity> { referencingTrack },
                TotalCount: 1));

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteArtistCommandHandler(
            repository, recordRepository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteArtistCommand(artist.Id), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        repository.DidNotReceive().Remove(Arg.Any<ArtistEntity>());
    }
}
