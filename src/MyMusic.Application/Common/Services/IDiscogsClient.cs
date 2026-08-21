namespace MyMusic.Application.Common.Services;

public interface IDiscogsClient
{
    Task<IReadOnlyList<DiscogsSearchResult>> SearchAsync(string query, CancellationToken cancellationToken);

    Task<DiscogsRelease> GetReleaseAsync(int id, CancellationToken cancellationToken);
}
