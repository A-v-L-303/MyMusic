namespace MyMusic.Application.Common.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
}
