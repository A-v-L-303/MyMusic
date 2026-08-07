namespace MyMusic.Application.Features.Sammlung.Record.Commands.UploadCover;

public sealed class UploadRecordCoverCommandHandler(
    IRepository<RecordEntity> repository,
    IRepository<LabelEntity> labelRepository,
    IRepository<ArtistEntity> artistRepository,
    ExceptionManager exceptionManager,
    RecordResponseBuilder responseBuilder)
    : ICommandHandler<UploadRecordCoverCommand, RecordResponse>
{
    public async Task<RecordResponse> HandleAsync(
        UploadRecordCoverCommand command,
        CancellationToken cancellationToken)
    {
        var existingRecord = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existingRecord is null || existingRecord.UserId != command.UserId)
            throw exceptionManager.NotFound("Record", command.Id);

        var updatedRecord = existingRecord.SetAlbumCover(command.FileContent);

        repository.Update(updatedRecord);

        await repository.SaveChangesAsync(cancellationToken);

        var label = await labelRepository.GetByIdAsync(updatedRecord.LabelId, cancellationToken);

        var artist = updatedRecord.ArtistId is null
            ? null
            : await artistRepository.GetByIdAsync(updatedRecord.ArtistId.Value, cancellationToken);

        return responseBuilder.Build(updatedRecord, label!.Name, artist?.Name);
    }
}
