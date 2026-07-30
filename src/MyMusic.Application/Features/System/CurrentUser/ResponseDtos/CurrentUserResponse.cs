namespace MyMusic.Application.Features.System.CurrentUser.ResponseDtos;

/// <summary>Antwort auf die Abfrage der Daten des aktuell angemeldeten Benutzers.</summary>
/// <param name="UserId">Die Benutzer-ID aus dem <c>sub</c>-Claim des JWT.</param>
public sealed record CurrentUserResponse(Guid UserId);
