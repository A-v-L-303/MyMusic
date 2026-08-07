namespace MyMusic.Application.Features.Sammlung.RecordTrack.Commands.Create;

public sealed class CreateRecordTrackCommandHandler(
    IRepository<RecordTrackEntity> repository,
    IRepository<RecordEntity> recordRepository,
    IRepository<ArtistEntity> artistRepository,
    IRepository<GenreEntity> genreRepository,
    ExceptionManager exceptionManager,
    RecordTrackResponseBuilder responseBuilder)
    : ICommandHandler<CreateRecordTrackCommand, RecordTrackResponse>
{
    public async Task<RecordTrackResponse> HandleAsync(
        CreateRecordTrackCommand command, CancellationToken cancellationToken)
    {
        var record = await recordRepository.GetByIdAsync(command.RecordId, cancellationToken);

        if (record is null || record.UserId != command.UserId)
            throw exceptionManager.NotFound("Record", command.RecordId);

        var (_, conflictingCount) = await repository.GetPagedAsync(
            track => track.RecordId == command.RecordId
                && track.RecordSide == command.RecordSide
                && track.TrackNumber == command.TrackNumber,
            query => query.OrderBy(track => track.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (conflictingCount > 0)
            throw exceptionManager.Conflict(
                $"Auf Seite '{command.RecordSide}' existiert bereits ein Track mit der Nummer {command.TrackNumber}.");

        var track = RecordTrackEntity.Create(
            command.RecordId,
            command.ArtistId,
            command.GenreId,
            command.TrackName,
            command.RecordSide,
            command.TrackNumber,
            command.Information,
            command.UserId);

        await repository.AddAsync(track, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        var artist = await artistRepository.GetByIdAsync(track.ArtistId, cancellationToken);

        var genre = await genreRepository.GetByIdAsync(track.GenreId, cancellationToken);

        return responseBuilder.Build(track, artist!.Name, genre!.Name);
    }
}
