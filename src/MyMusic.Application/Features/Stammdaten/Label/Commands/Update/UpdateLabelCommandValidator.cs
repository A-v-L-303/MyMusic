namespace MyMusic.Application.Features.Stammdaten.Label.Commands.Update;

public sealed class UpdateLabelCommandValidator : AbstractValidator<UpdateLabelCommand>
{
    public UpdateLabelCommandValidator(IRepository<CountryEntity> countryRepository)
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Der Name ist erforderlich.")
            .MinimumLength(LabelEntity.MinNameLength)
            .WithMessage($"Der Name muss mindestens {LabelEntity.MinNameLength} Zeichen lang sein.")
            .MaximumLength(LabelEntity.MaxNameLength)
            .WithMessage($"Der Name darf höchstens {LabelEntity.MaxNameLength} Zeichen lang sein.")
            .Matches(LabelEntity.NamePattern)
            .WithMessage("Der Name darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / enthalten.");

        RuleFor(command => command.CountryId)
            .MustAsync((countryId, cancellationToken) => ExistsAsync(countryRepository, countryId, cancellationToken))
            .WithMessage("Das angegebene Land existiert nicht.");

        RuleFor(command => command.Information)
            .MaximumLength(LabelEntity.MaxInformationLength)
            .WithMessage(
                $"Das Feld 'information' darf höchstens {LabelEntity.MaxInformationLength} Zeichen lang sein.");
    }

    private static async Task<bool> ExistsAsync(
        IRepository<CountryEntity> countryRepository,
        int countryId,
        CancellationToken cancellationToken)
    {
        return await countryRepository.GetByIdAsync(countryId, cancellationToken) is not null;
    }
}
