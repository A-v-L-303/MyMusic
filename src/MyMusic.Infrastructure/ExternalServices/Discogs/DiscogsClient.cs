namespace MyMusic.Infrastructure.ExternalServices.Discogs;

public sealed class DiscogsClient(HttpClient httpClient, ILogger<DiscogsClient> logger) : IDiscogsClient
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

        var searchResults = results
            .Where(result => result.Type == "release")
            .Select(MapSearchResult)
            .ToList();

        var thumbnailDataUrls = await Task.WhenAll(searchResults
            .Select(result => DownloadImageAsDataUrlAsync(result.ThumbnailUrl, cancellationToken)));

        return searchResults
            .Zip(thumbnailDataUrls, (result, thumbnailDataUrl) => result with { ThumbnailUrl = thumbnailDataUrl })
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

        var coverImageUrl = ResolveCoverImageUrl(release.Images);
        var coverImageDataUrl = await DownloadImageAsDataUrlAsync(coverImageUrl, cancellationToken);

        return MapRelease(release, coverImageDataUrl);
    }

    private static DiscogsSearchResult MapSearchResult(DiscogsSearchResultRepresentation result)
    {
        var year = int.TryParse(result.Year, out var parsedYear) ? parsedYear : (int?)null;

        var label = result.Label is { Count: > 0 } ? string.Join(", ", result.Label) : null;

        return new DiscogsSearchResult(result.Id, result.Title ?? string.Empty, year, label, result.Thumb);
    }

    private static DiscogsRelease MapRelease(DiscogsReleaseRepresentation release, string? coverImageDataUrl)
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

        var formats = (release.Formats ?? [])
            .Select(format => new DiscogsFormat(format.Name ?? string.Empty, format.Descriptions ?? []))
            .ToList();

        var tracklist = (release.Tracklist ?? [])
            .Select(track => new DiscogsTrack(
                track.Position ?? string.Empty,
                track.Title ?? string.Empty,
                track.Duration,
                MapTrackArtist(track.Artists)))
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
            coverImageDataUrl,
            tracklist,
            release.Country);
    }

    private static string? ResolveCoverImageUrl(List<DiscogsImageRepresentation>? images)
    {
        var nonNullImages = images ?? [];

        return nonNullImages.FirstOrDefault(image => image.Type == "primary")?.Uri
            ?? nonNullImages.FirstOrDefault()?.Uri;
    }

    /// <summary>
    /// Lädt ein Discogs-Bild (Release-Cover oder Such-Thumbnail) serverseitig über den
    /// authentifizierten Discogs-Client herunter und bettet es als Data-URL ein, damit der
    /// Browser nicht direkt gegen Discogs' Bild-CDN zugreifen muss (Discogs blockiert
    /// Hotlinking ohne passenden User-Agent/Referer).
    /// </summary>
    private async Task<string?> DownloadImageAsDataUrlAsync(string? imageUrl, CancellationToken cancellationToken)
    {
        if (imageUrl is null)
            return null;

        try
        {
            var imageResponse = await httpClient.GetAsync(imageUrl, cancellationToken);

            imageResponse.EnsureSuccessStatusCode();

            var bytes = await imageResponse.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = imageResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

            return $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception,
                "Discogs-Bild konnte nicht heruntergeladen werden: {ImageUrl}",
                imageUrl);

            return null;
        }
    }

    private static string? MapTrackArtist(List<DiscogsArtistRepresentation>? artists)
    {
        var names = (artists ?? [])
            .Select(artist => artist.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();

        return names.Count > 0 ? string.Join(", ", names) : null;
    }
}
