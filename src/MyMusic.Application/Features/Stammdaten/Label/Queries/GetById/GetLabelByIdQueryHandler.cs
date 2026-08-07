namespace MyMusic.Application.Features.Stammdaten.Label.Queries.GetById;

public sealed class GetLabelByIdQueryHandler(
    IRepository<LabelEntity> repository,
    IRepository<CountryEntity> countryRepository,
    ExceptionManager exceptionManager,
    LabelResponseBuilder responseBuilder)
    : IQueryHandler<GetLabelByIdQuery, LabelResponse>
{
    public async Task<LabelResponse> HandleAsync(GetLabelByIdQuery query, CancellationToken cancellationToken)
    {
        var label = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (label is null || label.UserId != query.UserId)
            throw exceptionManager.NotFound("Label", query.Id);

        var country = await countryRepository.GetByIdAsync(label.CountryId, cancellationToken);

        return responseBuilder.Build(label, country!.Name);
    }
}
