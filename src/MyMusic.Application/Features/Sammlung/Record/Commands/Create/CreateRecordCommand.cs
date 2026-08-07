namespace MyMusic.Application.Features.Sammlung.Record.Commands.Create;

public sealed class CreateRecordCommand : ICommand<RecordResponse>
{
    public int LabelId { get; set; }

    public int? ArtistId { get; set; }

    public RecordFormat Format { get; set; }

    public string AlbumName { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public RecordCondition Condition { get; set; } = RecordCondition.Vg;

    public string? Information { get; set; }

    public Guid UserId { get; set; }
}
