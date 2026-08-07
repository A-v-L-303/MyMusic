namespace MyMusic.Application.Tests.Features.Sammlung.RecordTrack.Commands.Delete;

public class DeleteRecordTrackCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerTrack_EntferntUndGibtTrueZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var track = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, userId);

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(track.Id, Arg.Any<CancellationToken>()).Returns(track);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteRecordTrackCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var result = await handler.HandleAsync(new DeleteRecordTrackCommand(1, track.Id), CancellationToken.None);

        // assert
        Assert.True(result);

        repository.Received(1).Remove(track);

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterTrack_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((RecordTrackEntity?)null);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteRecordTrackCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteRecordTrackCommand(1, 1), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderTrack_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderTrack = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, Guid.NewGuid());

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(fremderTrack.Id, Arg.Any<CancellationToken>()).Returns(fremderTrack);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteRecordTrackCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(
            new DeleteRecordTrackCommand(1, fremderTrack.Id), CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_TrackGehoertAnderemRecord_WirftNotFoundException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var track = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, userId);

        var repository = Substitute.For<IRepository<RecordTrackEntity>>();

        repository.GetByIdAsync(track.Id, Arg.Any<CancellationToken>()).Returns(track);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteRecordTrackCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteRecordTrackCommand(999, track.Id), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
