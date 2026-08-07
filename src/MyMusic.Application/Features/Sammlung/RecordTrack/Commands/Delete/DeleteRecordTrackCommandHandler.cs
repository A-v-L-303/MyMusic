namespace MyMusic.Application.Features.Sammlung.RecordTrack.Commands.Delete;

public sealed class DeleteRecordTrackCommandHandler(
    IRepository<RecordTrackEntity> repository,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteRecordTrackCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteRecordTrackCommand command, CancellationToken cancellationToken)
    {
        var track = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (track is null || track.UserId != currentUserService.UserId || track.RecordId != command.RecordId)
            throw exceptionManager.NotFound("Track", command.Id);

        repository.Remove(track);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
