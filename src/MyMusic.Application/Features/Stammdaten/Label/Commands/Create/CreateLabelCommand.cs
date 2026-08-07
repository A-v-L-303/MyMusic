namespace MyMusic.Application.Features.Stammdaten.Label.Commands.Create;

public sealed class CreateLabelCommand : ICommand<LabelResponse>
{
    public string Name { get; set; } = string.Empty;

    public int CountryId { get; set; }

    public string? Information { get; set; }

    public Guid UserId { get; set; }
}
