namespace MyMusic.Api.Tests;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_MitGueltigemSubClaim_GibtGuidZurueck()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        httpContextAccessor.HttpContext.Returns(CreateHttpContext(expectedUserId.ToString()));

        var currentUserService = new CurrentUserService(httpContextAccessor);

        // act
        // assert
        Assert.Equal(expectedUserId, currentUserService.UserId);
    }

    [Fact]
    public void UserId_OhneSubClaim_WirftInvalidOperationException()
    {
        // arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        httpContextAccessor.HttpContext.Returns(CreateHttpContext(subClaim: null));

        var currentUserService = new CurrentUserService(httpContextAccessor);

        // act
        // assert
        Assert.Throws<InvalidOperationException>(() => currentUserService.UserId);
    }

    [Fact]
    public void UserId_MitUngueltigemSubClaim_WirftInvalidOperationException()
    {
        // arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        httpContextAccessor.HttpContext.Returns(CreateHttpContext("kein-guid"));

        var currentUserService = new CurrentUserService(httpContextAccessor);

        // act
        // assert
        Assert.Throws<InvalidOperationException>(() => currentUserService.UserId);
    }

    private static DefaultHttpContext CreateHttpContext(string? subClaim)
    {
        var claims = subClaim is null ? [] : new[] { new Claim("sub", subClaim) };

        var identity = new ClaimsIdentity(claims, "TestAuthType");

        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }
}
