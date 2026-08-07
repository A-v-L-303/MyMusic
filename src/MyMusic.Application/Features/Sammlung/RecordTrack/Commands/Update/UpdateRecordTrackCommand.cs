namespace MyMusic.Application.Features.Sammlung.RecordTrack.Commands.Update;

public sealed class UpdateRecordTrackCommand : ICommand<RecordTrackResponse>
{
    public int Id { get; set; }

    public int RecordId { get; set; }

    public int ArtistId { get; set; }

    public int GenreId { get; set; }

    public string TrackName { get; set; } = string.Empty;

    public string RecordSide { get; set; } = "0";

    public int TrackNumber { get; set; }

    public string? Information { get; set; }

    public Guid UserId { get; set; }
}
