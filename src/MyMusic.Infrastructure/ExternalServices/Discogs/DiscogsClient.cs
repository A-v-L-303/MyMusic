namespace MyMusic.Infrastructure.ExternalServices.Discogs;

public sealed class DiscogsClient(HttpClient httpClient) : IDiscogsClient
{
    private static readonly JsonSerializerOptions _caseInsensitiveOptions =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<DiscogsSearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var requestUri = $"/database/search?q={Uri.EscapeDataString(query)}&type=release";

        var response = await httpClient.GetAsync(requestUri, cancellationToken);

        response.EnsureSuccessStatusCode();

        var searchResponse = await response.Content.ReadFromJsonAsync<DiscogsSearchResponseRepresentation>(
            _caseInsensitiveOptions,
            cancellationToken);

        var results = searchResponse?.Results ?? [];

        return results
            .Where(result => result.Type == "release")
            .Select(MapSearchResult)
            .ToList();
    }

    public async Task<DiscogsRelease> GetReleaseAsync(int id, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"/releases/{id}", cancellationToken);

        response.EnsureSuccessStatusCode();

        var release = await response.Content.ReadFromJsonAsync<DiscogsReleaseRepresentation>(
            _caseInsensitiveOptions,
            cancellationToken);

        if (release is null)
            throw new HttpRequestException("Discogs hat keine Release-Daten geliefert.");

        return MapRelease(release);
    }

    private static DiscogsSearchResult MapSearchResult(DiscogsSearchResultRepresentation result)
    {
        var year = int.TryParse(result.Year, out var parsedYear) ? parsedYear : (int?)null;

        var label = result.Label is { Count: > 0 } ? string.Join(", ", result.Label) : null;

        return new DiscogsSearchResult(result.Id, result.Title ?? string.Empty, year, label, result.Thumb);
    }

    private static DiscogsRelease MapRelease(DiscogsReleaseRepresentation release)
    {
        var artists = (release.Artists ?? [])
            .Select(artist => artist.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();

        var labels = (release.Labels ?? [])
            .Select(label => label.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();

        var images = release.Images ?? [];

        var coverImageUrl = images.FirstOrDefault(image => image.Type == "primary")?.Uri
            ?? images.FirstOrDefault()?.Uri;

        var formats = (release.Formats ?? [])
            .Select(format => new DiscogsFormat(format.Name ?? string.Empty, format.Descriptions ?? []))
            .ToList();

        var tracklist = (release.Tracklist ?? [])
            .Select(track => new DiscogsTrack(
                track.Position ?? string.Empty,
                track.Title ?? string.Empty,
                track.Duration))
            .ToList();

        return new DiscogsRelease(
            release.Id,
            release.Title ?? string.Empty,
            release.Year,
            artists,
            labels,
            release.Genres ?? [],
            release.Styles ?? [],
            formats,
            coverImageUrl,
            tracklist);
    }
}
