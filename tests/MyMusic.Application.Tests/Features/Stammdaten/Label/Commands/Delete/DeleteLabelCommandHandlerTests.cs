namespace MyMusic.Application.Tests.Features.Stammdaten.Label.Commands.Delete;

public class DeleteLabelCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenesLabel_EntferntLabelUndGibtTrueZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var label = LabelEntity.Create("Rough Trade", 1, null, userId);

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(label.Id, Arg.Any<CancellationToken>()).Returns(label);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(userId);

        var handler = new DeleteLabelCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var result = await handler.HandleAsync(new DeleteLabelCommand(label.Id), CancellationToken.None);

        // assert
        Assert.True(result);

        repository.Received(1).Remove(label);

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekanntesLabel_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((LabelEntity?)null);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteLabelCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteLabelCommand(1), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremdesLabel_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremdesLabel = LabelEntity.Create("Rough Trade", 1, null, Guid.NewGuid());

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(fremdesLabel.Id, Arg.Any<CancellationToken>()).Returns(fremdesLabel);

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteLabelCommandHandler(repository, currentUserService, new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteLabelCommand(fremdesLabel.Id), CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);

        repository.DidNotReceive().Remove(Arg.Any<LabelEntity>());
    }
}
