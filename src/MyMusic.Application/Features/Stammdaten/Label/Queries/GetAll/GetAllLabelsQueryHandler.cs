namespace MyMusic.Application.Features.Stammdaten.Label.Queries.GetAll;

public sealed class GetAllLabelsQueryHandler(
    IRepository<LabelEntity> repository,
    IRepository<CountryEntity> countryRepository,
    LabelResponseBuilder responseBuilder)
    : IQueryHandler<GetAllLabelsQuery, IEnumerable<LabelResponse>>
{
    public async Task<IEnumerable<LabelResponse>> HandleAsync(
        GetAllLabelsQuery query,
        CancellationToken cancellationToken)
    {
        var (items, _) = await repository.GetPagedAsync(
            label => label.UserId == query.UserId,
            queryable => queryable.OrderBy(label => label.Name),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        var countries = await countryRepository.GetAllAsync(cancellationToken);

        var countryNamesById = countries.ToDictionary(country => country.Id, country => country.Name);

        return items.Select(label => responseBuilder.Build(label, countryNamesById[label.CountryId]));
    }
}
