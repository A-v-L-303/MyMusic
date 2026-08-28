namespace MyMusic.Application.Features.System.CurrentUser.Commands.UpdateEmail;

public sealed class UpdateCurrentUserEmailCommandValidator : AbstractValidator<UpdateCurrentUserEmailCommand>
{
    public UpdateCurrentUserEmailCommandValidator()
    {
        RuleFor(command => command.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Die E-Mail-Adresse ist erforderlich.")
            .MaximumLength(120).WithMessage("Die E-Mail-Adresse darf höchstens 120 Zeichen lang sein.")
            .EmailAddress().WithMessage("Die E-Mail-Adresse hat kein gültiges Format.");
    }
}
