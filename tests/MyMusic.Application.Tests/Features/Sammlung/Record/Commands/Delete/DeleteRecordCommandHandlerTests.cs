namespace MyMusic.Application.Tests.Features.Sammlung.Record.Commands.Delete;

public class DeleteRecordCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerRecord_EntferntRecordUndGibtTrueZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteRecordCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var result = await handler.HandleAsync(new DeleteRecordCommand(record.Id), CancellationToken.None);

        // assert
        Assert.True(result);

        repository.Received(1).Remove(record);

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterRecord_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((RecordEntity?)null);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteRecordCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteRecordCommand(1), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderRecord_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderRecord = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, Guid.NewGuid());

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(fremderRecord.Id, Arg.Any<CancellationToken>()).Returns(fremderRecord);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteRecordCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteRecordCommand(fremderRecord.Id), CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);

        repository.DidNotReceive().Remove(Arg.Any<RecordEntity>());
    }
}
