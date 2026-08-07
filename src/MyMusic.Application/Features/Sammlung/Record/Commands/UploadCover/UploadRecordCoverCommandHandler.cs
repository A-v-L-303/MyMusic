namespace MyMusic.Application.Features.Sammlung.Record.Commands.UploadCover;

public sealed class UploadRecordCoverCommandHandler(
    IRepository<RecordEntity> repository,
    IRepository<LabelEntity> labelRepository,
    IRepository<ArtistEntity> artistRepository,
    IRepository<GenreEntity> genreRepository,
    IRepository<RecordTrackEntity> trackRepository,
    ExceptionManager exceptionManager,
    RecordResponseBuilder responseBuilder,
    RecordTrackResponseBuilder trackResponseBuilder)
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

        var (tracks, _) = await trackRepository.GetPagedAsync(
            track => track.RecordId == updatedRecord.Id,
            trackQuery => trackQuery.OrderBy(track => track.RecordSide).ThenBy(track => track.TrackNumber),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        var trackResponses = new List<RecordTrackResponse>();

        foreach (var track in tracks)
        {
            var trackArtist = await artistRepository.GetByIdAsync(track.ArtistId, cancellationToken);

            var trackGenre = await genreRepository.GetByIdAsync(track.GenreId, cancellationToken);

            trackResponses.Add(trackResponseBuilder.Build(track, trackArtist!.Name, trackGenre!.Name));
        }

        return responseBuilder.Build(updatedRecord, label!.Name, artist?.Name, trackResponses);
    }
}
