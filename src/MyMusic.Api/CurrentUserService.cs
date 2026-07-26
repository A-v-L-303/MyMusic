namespace MyMusic.Api;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var subjectClaim = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(subjectClaim) || !Guid.TryParse(subjectClaim, out var userId))
                throw new InvalidOperationException(
                    "Der Benutzer konnte nicht aus dem Zugriffstoken ermittelt werden.");

            return userId;
        }
    }
}
