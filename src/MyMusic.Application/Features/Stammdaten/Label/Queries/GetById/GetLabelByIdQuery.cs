namespace MyMusic.Application.Features.Stammdaten.Label.Queries.GetById;

public sealed record GetLabelByIdQuery(int Id, Guid UserId) : IQuery<LabelResponse>;
