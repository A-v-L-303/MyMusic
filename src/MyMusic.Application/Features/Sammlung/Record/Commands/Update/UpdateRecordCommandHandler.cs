namespace MyMusic.Application.Features.Sammlung.Record.Commands.Update;

public sealed class UpdateRecordCommandHandler(
    IRepository<RecordEntity> repository,
    IRepository<LabelEntity> labelRepository,
    IRepository<ArtistEntity> artistRepository,
    ExceptionManager exceptionManager,
    RecordResponseBuilder responseBuilder)
    : ICommandHandler<UpdateRecordCommand, RecordResponse>
{
    public async Task<RecordResponse> HandleAsync(UpdateRecordCommand command, CancellationToken cancellationToken)
    {
        var existingRecord = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existingRecord is null || existingRecord.UserId != command.UserId)
            throw exceptionManager.NotFound("Record", command.Id);

        var updatedRecord = existingRecord.Update(
            command.LabelId,
            command.ArtistId,
            command.Format,
            command.AlbumName,
            command.ReleaseYear,
            command.Condition,
            command.Information);

        repository.Update(updatedRecord);

        await repository.SaveChangesAsync(cancellationToken);

        var label = await labelRepository.GetByIdAsync(updatedRecord.LabelId, cancellationToken);

        var artist = updatedRecord.ArtistId is null
            ? null
            : await artistRepository.GetByIdAsync(updatedRecord.ArtistId.Value, cancellationToken);

        return responseBuilder.Build(updatedRecord, label!.Name, artist?.Name);
    }
}
