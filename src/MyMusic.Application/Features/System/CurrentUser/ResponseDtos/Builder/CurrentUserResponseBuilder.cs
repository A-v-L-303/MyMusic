namespace MyMusic.Application.Features.System.CurrentUser.ResponseDtos.Builder;

public sealed class CurrentUserResponseBuilder
{
    public CurrentUserResponse Build(Guid userId)
    {
        return new CurrentUserResponse(userId);
    }
}
