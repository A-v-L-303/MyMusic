namespace MyMusic.Application.Features.Sammlung.Record.Commands.Update;

public sealed class UpdateRecordCommand : ICommand<RecordResponse>
{
    public int Id { get; set; }

    public int LabelId { get; set; }

    public int? ArtistId { get; set; }

    public RecordFormat Format { get; set; }

    public string AlbumName { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public RecordCondition Condition { get; set; }

    public string? Information { get; set; }

    public Guid UserId { get; set; }
}
