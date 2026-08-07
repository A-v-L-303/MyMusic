namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Create;

public sealed class CreateArtistCommandValidator : AbstractValidator<CreateArtistCommand>
{
    public CreateArtistCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Der Name ist erforderlich.")
            .MinimumLength(ArtistEntity.MinNameLength)
            .WithMessage($"Der Name muss mindestens {ArtistEntity.MinNameLength} Zeichen lang sein.")
            .MaximumLength(ArtistEntity.MaxNameLength)
            .WithMessage($"Der Name darf höchstens {ArtistEntity.MaxNameLength} Zeichen lang sein.")
            .Matches(ArtistEntity.NamePattern)
            .WithMessage("Der Name darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / enthalten.");
    }
}
