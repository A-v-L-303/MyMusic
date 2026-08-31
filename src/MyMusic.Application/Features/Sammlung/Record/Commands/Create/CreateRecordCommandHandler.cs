namespace MyMusic.Application.Features.Sammlung.Record.Commands.Create;

public sealed class CreateRecordCommandHandler(
    IRepository<RecordEntity> repository,
    IRepository<LabelEntity> labelRepository,
    IRepository<ArtistEntity> artistRepository,
    RecordResponseBuilder responseBuilder)
    : ICommandHandler<CreateRecordCommand, RecordResponse>
{
    public async Task<RecordResponse> HandleAsync(CreateRecordCommand command, CancellationToken cancellationToken)
    {
        var existingCollectionNumbers = await repository.GetProjectedAsync(
            r => r.UserId == command.UserId,
            r => r.CollectionNumber,
            cancellationToken);

        var nextCollectionNumber = existingCollectionNumbers.Count == 0
            ? 1
            : existingCollectionNumbers.Max() + 1;

        var record = RecordEntity.Create(
            nextCollectionNumber,
            command.LabelId,
            command.ArtistId,
            command.Format,
            command.AlbumName,
            command.ReleaseYear,
            command.Condition,
            command.Information,
            command.UserId);

        await repository.AddAsync(record, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        var label = await labelRepository.GetByIdAsync(record.LabelId, cancellationToken);

        var artist = record.ArtistId is null
            ? null
            : await artistRepository.GetByIdAsync(record.ArtistId.Value, cancellationToken);

        return responseBuilder.Build(record, label!.Name, artist?.Name, []);
    }
}
