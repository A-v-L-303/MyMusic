namespace MyMusic.Application.Features.Stammdaten.Label.Commands.Update;

public sealed class UpdateLabelCommand : ICommand<LabelResponse>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int CountryId { get; set; }

    public string? Information { get; set; }

    public Guid UserId { get; set; }
}
