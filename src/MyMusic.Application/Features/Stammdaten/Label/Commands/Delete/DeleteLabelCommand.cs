namespace MyMusic.Application.Features.Stammdaten.Label.Commands.Delete;

public sealed record DeleteLabelCommand(int Id) : ICommand<bool>;
