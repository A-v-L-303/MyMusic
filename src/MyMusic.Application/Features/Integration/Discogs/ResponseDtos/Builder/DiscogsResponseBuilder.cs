namespace MyMusic.Application.Features.Integration.Discogs.ResponseDtos.Builder;

public sealed class DiscogsResponseBuilder
{
    public DiscogsSearchResultResponse BuildSearchResult(DiscogsSearchResult searchResult)
    {
        return new DiscogsSearchResultResponse(
            searchResult.Id,
            searchResult.Title,
            searchResult.Year,
            searchResult.Label,
            searchResult.ThumbnailUrl);
    }

    public DiscogsReleaseResponse BuildRelease(DiscogsRelease release)
    {
        return new DiscogsReleaseResponse(
            release.Id,
            release.Title,
            release.Year,
            release.Artists,
            release.Labels,
            release.Genres,
            release.Styles,
            release.Formats.Select(BuildFormat).ToList(),
            release.CoverImageUrl,
            release.Tracklist.Select(BuildTrack).ToList(),
            release.Country);
    }

    private static DiscogsFormatResponse BuildFormat(DiscogsFormat format)
    {
        return new DiscogsFormatResponse(format.Name, format.Descriptions);
    }

    private static DiscogsTrackResponse BuildTrack(DiscogsTrack track)
    {
        return new DiscogsTrackResponse(track.Position, track.Title, track.Duration, track.Artist);
    }
}
