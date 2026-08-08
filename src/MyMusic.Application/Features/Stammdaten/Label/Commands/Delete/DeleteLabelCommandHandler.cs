namespace MyMusic.Application.Features.Stammdaten.Label.Commands.Delete;

public sealed class DeleteLabelCommandHandler(
    IRepository<LabelEntity> repository,
    IRepository<RecordEntity> recordRepository,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteLabelCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteLabelCommand command, CancellationToken cancellationToken)
    {
        var label = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (label is null || label.UserId != currentUserService.UserId)
            throw exceptionManager.NotFound("Label", command.Id);

        var (_, referencingRecordCount) = await recordRepository.GetPagedAsync(
            record => record.LabelId == command.Id,
            query => query.OrderBy(record => record.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (referencingRecordCount > 0)
            throw exceptionManager.Conflict(
                $"Label '{label.Name}' kann nicht gelöscht werden, da es noch von mindestens einem Record " +
                "verwendet wird.");

        repository.Remove(label);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
