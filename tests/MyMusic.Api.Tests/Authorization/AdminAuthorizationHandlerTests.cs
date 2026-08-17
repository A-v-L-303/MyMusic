namespace MyMusic.Api.Tests.Authorization;

public class AdminAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_MitAdminRolleImRealmAccessClaim_GewaehrtZugriff()
    {
        // arrange
        var context = CreateContext("""{"roles":["User","Admin"]}""");

        var handler = new AdminAuthorizationHandler();

        // act
        await handler.HandleAsync(context);

        // assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_OhneRealmAccessClaim_VerweigertZugriff()
    {
        // arrange
        var context = CreateContext(realmAccessClaim: null);

        var handler = new AdminAuthorizationHandler();

        // act
        await handler.HandleAsync(context);

        // assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_MitLeeremRolesArray_VerweigertZugriff()
    {
        // arrange
        var context = CreateContext("""{"roles":[]}""");

        var handler = new AdminAuthorizationHandler();

        // act
        await handler.HandleAsync(context);

        // assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_MitUngueltigemJson_VerweigertZugriffOhneException()
    {
        // arrange
        var context = CreateContext("kein-json");

        var handler = new AdminAuthorizationHandler();

        // act
        await handler.HandleAsync(context);

        // assert: kein Absturz, nur Verweigerung - die Prüfung selbst ist die Assertion
        Assert.False(context.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateContext(string? realmAccessClaim)
    {
        var claims = realmAccessClaim is null
            ? []
            : new[] { new Claim("realm_access", realmAccessClaim) };

        var identity = new ClaimsIdentity(claims, "TestAuthType");

        var user = new ClaimsPrincipal(identity);

        return new AuthorizationHandlerContext([new AdminRequirement()], user, resource: null);
    }
}
