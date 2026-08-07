namespace MyMusic.Application.Features.Sammlung.Record.ResponseDtos.Builder;

public sealed class RecordResponseBuilder
{
    public RecordResponse Build(RecordEntity record, string labelName, string? artistName)
    {
        return new RecordResponse(
            record.Id,
            record.LabelId,
            labelName,
            record.ArtistId,
            artistName,
            record.Format,
            record.AlbumName,
            record.ReleaseYear,
            record.Condition,
            record.Information);
    }

    public RecordListResponse BuildPaged(
        IReadOnlyList<RecordEntity> records,
        IReadOnlyDictionary<int, string> labelNamesById,
        IReadOnlyDictionary<int, string> artistNamesById,
        int totalCount,
        int page,
        int pageSize)
    {
        var items = records
            .Select(record => Build(
                record,
                labelNamesById[record.LabelId],
                record.ArtistId is null ? null : artistNamesById[record.ArtistId.Value]))
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new RecordListResponse(items, totalCount, page, pageSize, totalPages);
    }
}
