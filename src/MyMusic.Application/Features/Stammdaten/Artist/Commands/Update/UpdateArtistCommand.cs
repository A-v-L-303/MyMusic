namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Update;

public sealed class UpdateArtistCommand : ICommand<ArtistResponse>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}
