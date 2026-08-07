namespace MyMusic.Application.Features.Sammlung.Record.Queries.GetById;

public sealed class GetRecordByIdQueryHandler(
    IRepository<RecordEntity> repository,
    IRepository<LabelEntity> labelRepository,
    IRepository<ArtistEntity> artistRepository,
    ExceptionManager exceptionManager,
    RecordResponseBuilder responseBuilder)
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

        return responseBuilder.Build(record, label!.Name, artist?.Name);
    }
}
