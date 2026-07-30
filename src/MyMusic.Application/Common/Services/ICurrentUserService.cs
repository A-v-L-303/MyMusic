namespace MyMusic.Application.Common.Services;

public interface ICurrentUserService
{
    /// <summary>Die Benutzer-ID aus dem <c>sub</c>-Claim des validierten JWT.</summary>
    Guid UserId { get; }
}
