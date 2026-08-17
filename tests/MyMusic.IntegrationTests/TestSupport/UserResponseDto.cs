namespace MyMusic.IntegrationTests.TestSupport;

public sealed record UserResponseDto(Guid Id, string Username, string Email, string Role);
