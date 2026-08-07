namespace MyMusic.Application.Features.Sammlung.Record.Queries.GetById;

public sealed record GetRecordByIdQuery(int Id, Guid UserId) : IQuery<RecordResponse>;
