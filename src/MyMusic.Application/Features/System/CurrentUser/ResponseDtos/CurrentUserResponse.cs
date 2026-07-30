namespace MyMusic.Application.Features.System.CurrentUser.ResponseDtos;

/// <param name="UserId">Die Benutzer-ID aus dem <c>sub</c>-Claim des JWT.</param>
public sealed record CurrentUserResponse(Guid UserId);
