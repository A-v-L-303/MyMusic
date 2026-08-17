namespace MyMusic.Api.Authorization;

public sealed class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
{
    private const string AdminRoleName = "Admin";

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        var realmAccessClaim = context.User.FindFirst("realm_access")?.Value;

        if (!string.IsNullOrWhiteSpace(realmAccessClaim) && HasAdminRole(realmAccessClaim))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }

    private static bool HasAdminRole(string realmAccessClaim)
    {
        try
        {
            using var document = JsonDocument.Parse(realmAccessClaim);

            return document.RootElement.TryGetProperty("roles", out var roles)
                && roles.EnumerateArray().Any(role => role.GetString() == AdminRoleName);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
