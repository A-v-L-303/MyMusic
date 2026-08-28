namespace MyMusic.Application.Features.System.CurrentUser.Commands.UpdateEmail;

public sealed class UpdateCurrentUserEmailCommand : ICommand<bool>
{
    public string Email { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}
