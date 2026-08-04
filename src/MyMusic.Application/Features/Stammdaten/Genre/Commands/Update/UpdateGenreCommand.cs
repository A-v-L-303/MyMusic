namespace MyMusic.Application.Features.Stammdaten.Genre.Commands.Update;

public sealed class UpdateGenreCommand : ICommand<GenreResponse>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}
