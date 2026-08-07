namespace MyMusic.Application.Features.Sammlung.Record.Commands.UploadCover;

public sealed class UploadRecordCoverCommand : ICommand<RecordResponse>
{
    public int Id { get; set; }

    public byte[] FileContent { get; set; } = [];

    public Guid UserId { get; set; }
}
