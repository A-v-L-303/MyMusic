namespace MyMusic.Application.Common.Services;

/// <summary>Stellt die Identität des aktuell angemeldeten Benutzers bereit.</summary>
public interface ICurrentUserService
{
    /// <summary>Die Benutzer-ID aus dem <c>sub</c>-Claim des validierten JWT.</summary>
    Guid UserId { get; }
}
