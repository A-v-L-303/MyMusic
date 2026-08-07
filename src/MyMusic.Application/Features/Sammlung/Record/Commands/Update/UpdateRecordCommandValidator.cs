namespace MyMusic.Application.Features.Sammlung.Record.Commands.Update;

public sealed class UpdateRecordCommandValidator : AbstractValidator<UpdateRecordCommand>
{
    public UpdateRecordCommandValidator(
        IRepository<LabelEntity> labelRepository,
        IRepository<ArtistEntity> artistRepository)
    {
        RuleFor(command => command.LabelId)
            .MustAsync((command, labelId, cancellationToken) =>
                BelongsToUserAsync(labelRepository, labelId, command.UserId, cancellationToken))
            .WithMessage("Das angegebene Label existiert nicht.");

        RuleFor(command => command.ArtistId)
            .MustAsync((command, artistId, cancellationToken) =>
                artistId is null
                    ? Task.FromResult(true)
                    : BelongsToUserAsync(artistRepository, artistId.Value, command.UserId, cancellationToken))
            .WithMessage("Der angegebene Artist existiert nicht.");

        RuleFor(command => command.AlbumName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Der Albumname ist erforderlich.")
            .MinimumLength(RecordEntity.MinAlbumNameLength)
            .WithMessage($"Der Albumname muss mindestens {RecordEntity.MinAlbumNameLength} Zeichen lang sein.")
            .MaximumLength(RecordEntity.MaxAlbumNameLength)
            .WithMessage($"Der Albumname darf höchstens {RecordEntity.MaxAlbumNameLength} Zeichen lang sein.")
            .Matches(RecordEntity.AlbumNamePattern)
            .WithMessage("Der Albumname darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / ( ) enthalten.");

        RuleFor(command => command.ReleaseYear)
            .InclusiveBetween(RecordEntity.MinReleaseYear, DateTime.UtcNow.Year)
            .WithMessage(
                $"Das Erscheinungsjahr muss zwischen {RecordEntity.MinReleaseYear} und {DateTime.UtcNow.Year} liegen.");

        RuleFor(command => command.Information)
            .MaximumLength(RecordEntity.MaxInformationLength)
            .WithMessage(
                $"Das Feld 'information' darf höchstens {RecordEntity.MaxInformationLength} Zeichen lang sein.");
    }

    private static async Task<bool> BelongsToUserAsync(
        IRepository<LabelEntity> repository,
        int labelId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var label = await repository.GetByIdAsync(labelId, cancellationToken);

        return label is not null && label.UserId == userId;
    }

    private static async Task<bool> BelongsToUserAsync(
        IRepository<ArtistEntity> repository,
        int artistId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var artist = await repository.GetByIdAsync(artistId, cancellationToken);

        return artist is not null && artist.UserId == userId;
    }
}
