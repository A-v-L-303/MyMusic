namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Delete;

public sealed record DeleteArtistCommand(int Id) : ICommand<bool>;
