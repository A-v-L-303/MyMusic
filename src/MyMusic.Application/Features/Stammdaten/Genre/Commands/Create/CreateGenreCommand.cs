namespace MyMusic.Application.Features.Stammdaten.Genre.Commands.Create;

public sealed class CreateGenreCommand : ICommand<GenreResponse>
{
    public string Name { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}
