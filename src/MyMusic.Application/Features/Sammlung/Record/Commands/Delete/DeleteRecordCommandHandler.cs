namespace MyMusic.Application.Features.Sammlung.Record.Commands.Delete;

public sealed class DeleteRecordCommandHandler(
    IRepository<RecordEntity> repository,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteRecordCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteRecordCommand command, CancellationToken cancellationToken)
    {
        var record = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (record is null || record.UserId != currentUserService.UserId)
            throw exceptionManager.NotFound("Record", command.Id);

        repository.Remove(record);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
