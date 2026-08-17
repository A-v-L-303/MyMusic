namespace MyMusic.Application.Common.Services;

public sealed record KeycloakUserSummary(Guid Id, string Username, string Email, bool IsAdmin);
