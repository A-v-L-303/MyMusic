namespace MyMusic.Application.Features.Stammdaten.Label.ResponseDtos.Builder;

public sealed class LabelResponseBuilder
{
    public LabelResponse Build(LabelEntity label, string countryName)
    {
        return new LabelResponse(label.Id, label.Name, label.CountryId, countryName, label.Information);
    }

    public LabelListResponse BuildPaged(
        IReadOnlyList<LabelEntity> labels,
        IReadOnlyDictionary<int, string> countryNamesById,
        int totalCount,
        int page,
        int pageSize)
    {
        var items = labels.Select(label => Build(label, countryNamesById[label.CountryId])).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new LabelListResponse(items, totalCount, page, pageSize, totalPages);
    }
}
