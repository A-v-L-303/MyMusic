namespace MyMusic.Application.Features.Stammdaten.Genre.Commands.Delete;

public sealed record DeleteGenreCommand(int Id) : ICommand<bool>;
