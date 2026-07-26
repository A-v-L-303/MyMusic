namespace MyMusic.Application.Tests.Common.CQRS.Validation.TestDoubles;

public sealed class SampleValidatedCommandValidator : AbstractValidator<SampleValidatedCommand>
{
    public SampleValidatedCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().WithMessage("Der Name darf nicht leer sein.");
    }
}
