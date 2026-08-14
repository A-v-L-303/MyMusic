namespace MyMusic.Application.Features.Stammdaten.Label.Queries.GetAll;

public sealed record GetAllLabelsQuery(Guid UserId) : IQuery<IEnumerable<LabelResponse>>;
