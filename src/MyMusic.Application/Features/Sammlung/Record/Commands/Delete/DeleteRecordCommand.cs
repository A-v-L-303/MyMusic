namespace MyMusic.Application.Features.Sammlung.Record.Commands.Delete;

public sealed record DeleteRecordCommand(int Id) : ICommand<bool>;
