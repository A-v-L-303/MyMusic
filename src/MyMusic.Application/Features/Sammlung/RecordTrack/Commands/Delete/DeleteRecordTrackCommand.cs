namespace MyMusic.Application.Features.Sammlung.RecordTrack.Commands.Delete;

public sealed record DeleteRecordTrackCommand(int RecordId, int Id) : ICommand<bool>;
