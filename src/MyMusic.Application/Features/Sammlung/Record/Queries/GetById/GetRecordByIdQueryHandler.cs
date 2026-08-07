namespace MyMusic.Application.Features.Sammlung.Record.Queries.GetById;

public sealed class GetRecordByIdQueryHandler(
    IRepository<RecordEntity> repository,
    IRepository<LabelEntity> labelRepository,
    IRepository<ArtistEntity> artistRepository,
    IRepository<GenreEntity> genreRepository,
    IRepository<RecordTrackEntity> trackRepository,
    ExceptionManager exceptionManager,
    RecordResponseBuilder responseBuilder,
    RecordTrackResponseBuilder trackResponseBuilder)
    : IQueryHandler<GetRecordByIdQuery, RecordResponse>
{
    public async Task<RecordResponse> HandleAsync(GetRecordByIdQuery query, CancellationToken cancellationToken)
    {
        var record = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (record is null || record.UserId != query.UserId)
            throw exceptionManager.NotFound("Record", query.Id);

        var label = await labelRepository.GetByIdAsync(record.LabelId, cancellationToken);

        var artist = record.ArtistId is null
            ? null
            : await artistRepository.GetByIdAsync(record.ArtistId.Value, cancellationToken);

        var (tracks, _) = await trackRepository.GetPagedAsync(
            track => track.RecordId == record.Id,
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

        return responseBuilder.Build(record, label!.Name, artist?.Name, trackResponses);
    }
}
