namespace MyMusic.Application.Features.System.CurrentUser.Commands.ChangePassword;

public sealed class ChangeCurrentUserPasswordCommand : ICommand<bool>
{
    public string NewPassword { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}
