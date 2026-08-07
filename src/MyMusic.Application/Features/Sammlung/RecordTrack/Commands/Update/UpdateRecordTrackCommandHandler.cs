namespace MyMusic.Application.Features.Sammlung.RecordTrack.Commands.Update;

public sealed class UpdateRecordTrackCommandHandler(
    IRepository<RecordTrackEntity> repository,
    IRepository<ArtistEntity> artistRepository,
    IRepository<GenreEntity> genreRepository,
    ExceptionManager exceptionManager,
    RecordTrackResponseBuilder responseBuilder)
    : ICommandHandler<UpdateRecordTrackCommand, RecordTrackResponse>
{
    public async Task<RecordTrackResponse> HandleAsync(
        UpdateRecordTrackCommand command, CancellationToken cancellationToken)
    {
        var existingTrack = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existingTrack is null
            || existingTrack.UserId != command.UserId
            || existingTrack.RecordId != command.RecordId)
            throw exceptionManager.NotFound("Track", command.Id);

        var (_, conflictingCount) = await repository.GetPagedAsync(
            track => track.RecordId == command.RecordId
                && track.RecordSide == command.RecordSide
                && track.TrackNumber == command.TrackNumber
                && track.Id != command.Id,
            query => query.OrderBy(track => track.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (conflictingCount > 0)
            throw exceptionManager.Conflict(
                $"Auf Seite '{command.RecordSide}' existiert bereits ein Track mit der Nummer {command.TrackNumber}.");

        var updatedTrack = existingTrack.Update(
            command.ArtistId,
            command.GenreId,
            command.TrackName,
            command.RecordSide,
            command.TrackNumber,
            command.Information);

        repository.Update(updatedTrack);

        await repository.SaveChangesAsync(cancellationToken);

        var artist = await artistRepository.GetByIdAsync(updatedTrack.ArtistId, cancellationToken);

        var genre = await genreRepository.GetByIdAsync(updatedTrack.GenreId, cancellationToken);

        return responseBuilder.Build(updatedTrack, artist!.Name, genre!.Name);
    }
}
