namespace MyMusic.Application.Features.Stammdaten.Label.Commands.Delete;

public sealed class DeleteLabelCommandHandler(
    IRepository<LabelEntity> repository,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteLabelCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteLabelCommand command, CancellationToken cancellationToken)
    {
        var label = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (label is null || label.UserId != currentUserService.UserId)
            throw exceptionManager.NotFound("Label", command.Id);

        // Eine Referenzprüfung gegen record entfällt hier bewusst: Die Tabelle
        // entsteht erst mit Slice 6, bis dahin kann kein Record ein Label referenzieren.
        repository.Remove(label);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
