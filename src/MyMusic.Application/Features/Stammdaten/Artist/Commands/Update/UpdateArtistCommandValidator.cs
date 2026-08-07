namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Update;

public sealed class UpdateArtistCommandValidator : AbstractValidator<UpdateArtistCommand>
{
    public UpdateArtistCommandValidator()
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
