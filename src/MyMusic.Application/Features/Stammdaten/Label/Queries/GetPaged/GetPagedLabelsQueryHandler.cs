namespace MyMusic.Application.Features.Stammdaten.Label.Queries.GetPaged;

public sealed class GetPagedLabelsQueryHandler(
    IRepository<LabelEntity> repository,
    IRepository<CountryEntity> countryRepository,
    LabelResponseBuilder responseBuilder)
    : IQueryHandler<GetPagedLabelsQuery, LabelListResponse>
{
    public async Task<LabelListResponse> HandleAsync(GetPagedLabelsQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(
            label => label.UserId == query.UserId
                && (query.Name == null || label.Name.ToLower().Contains(query.Name.ToLower()))
                && (query.CountryId == null || label.CountryId == query.CountryId),
            queryable => queryable.OrderBy(label => label.Name),
            query.Page,
            query.PageSize,
            cancellationToken);

        var countries = await countryRepository.GetAllAsync(cancellationToken);

        var countryNamesById = countries.ToDictionary(country => country.Id, country => country.Name);

        return responseBuilder.BuildPaged(items, countryNamesById, totalCount, query.Page, query.PageSize);
    }
}
