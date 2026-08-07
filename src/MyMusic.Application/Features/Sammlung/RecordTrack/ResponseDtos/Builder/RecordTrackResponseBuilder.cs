namespace MyMusic.Application.Features.Sammlung.RecordTrack.ResponseDtos.Builder;

public sealed class RecordTrackResponseBuilder
{
    public RecordTrackResponse Build(RecordTrackEntity track, string artistName, string genreName)
    {
        return new RecordTrackResponse(
            track.Id,
            track.RecordId,
            track.ArtistId,
            artistName,
            track.GenreId,
            genreName,
            track.TrackName,
            track.RecordSide,
            track.TrackNumber,
            track.Information);
    }
}
