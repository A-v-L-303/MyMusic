namespace MyMusic.Application.Features.Stammdaten.Genre.Commands.Create;

public sealed class CreateGenreCommandValidator : AbstractValidator<CreateGenreCommand>
{
    public CreateGenreCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Der Name ist erforderlich.")
            .MinimumLength(GenreEntity.MinNameLength)
            .WithMessage($"Der Name muss mindestens {GenreEntity.MinNameLength} Zeichen lang sein.")
            .MaximumLength(GenreEntity.MaxNameLength)
            .WithMessage($"Der Name darf höchstens {GenreEntity.MaxNameLength} Zeichen lang sein.")
            .Matches(GenreEntity.NamePattern)
            .WithMessage("Der Name darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' enthalten.");
    }
}
