namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Create;

public sealed class CreateArtistCommand : ICommand<ArtistResponse>
{
    public string Name { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}
