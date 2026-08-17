namespace MyMusic.Application.Features.Verwaltung.Admin.Commands.Delete;

public sealed record DeleteUserCommand(Guid TargetUserId) : ICommand<bool>;
