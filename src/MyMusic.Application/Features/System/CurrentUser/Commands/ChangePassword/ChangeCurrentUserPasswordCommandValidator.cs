namespace MyMusic.Application.Features.System.CurrentUser.Commands.ChangePassword;

public sealed class ChangeCurrentUserPasswordCommandValidator : AbstractValidator<ChangeCurrentUserPasswordCommand>
{
    public ChangeCurrentUserPasswordCommandValidator()
    {
        RuleFor(command => command.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Das Passwort ist erforderlich.")
            .MinimumLength(8).WithMessage("Das Passwort muss mindestens 8 Zeichen lang sein.")
            .MaximumLength(100).WithMessage("Das Passwort darf höchstens 100 Zeichen lang sein.");
    }
}
