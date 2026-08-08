namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.Commands.Delete;

public class DeleteGenreCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenesGenre_EntferntGenreUndGibtTrueZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var genre = GenreEntity.Create("Rock", userId);

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);

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

        var handler = new DeleteGenreCommandHandler(
            repository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var result = await handler.HandleAsync(new DeleteGenreCommand(genre.Id), CancellationToken.None);

        // assert
        Assert.True(result);

        repository.Received(1).Remove(genre);

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekanntesGenre_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((GenreEntity?)null);

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteGenreCommandHandler(
            repository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteGenreCommand(1), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremdesGenre_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremdesGenre = GenreEntity.Create("Rock", Guid.NewGuid());

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(fremdesGenre.Id, Arg.Any<CancellationToken>()).Returns(fremdesGenre);

        var recordTrackRepository = Substitute.For<IRepository<RecordTrackEntity>>();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteGenreCommandHandler(
            repository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteGenreCommand(fremdesGenre.Id), CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);

        repository.DidNotReceive().Remove(Arg.Any<GenreEntity>());
    }

    [Fact]
    public async Task HandleAsync_GenreReferenziertVonTrack_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var genre = GenreEntity.Create("Rock", userId);

        var repository = Substitute.For<IRepository<GenreEntity>>();

        repository.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);

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

        var handler = new DeleteGenreCommandHandler(
            repository, recordTrackRepository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteGenreCommand(genre.Id), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        repository.DidNotReceive().Remove(Arg.Any<GenreEntity>());
    }
}
