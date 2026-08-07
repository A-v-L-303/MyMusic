namespace MyMusic.Application.Features.Sammlung.RecordTrack.Commands.Create;

public sealed class CreateRecordTrackCommandValidator : AbstractValidator<CreateRecordTrackCommand>
{
    public CreateRecordTrackCommandValidator(
        IRepository<ArtistEntity> artistRepository,
        IRepository<GenreEntity> genreRepository)
    {
        RuleFor(command => command.ArtistId)
            .MustAsync((command, artistId, cancellationToken) =>
                BelongsToUserAsync(artistRepository, artistId, command.UserId, cancellationToken))
            .WithMessage("Der angegebene Artist existiert nicht.");

        RuleFor(command => command.GenreId)
            .MustAsync((command, genreId, cancellationToken) =>
                BelongsToUserAsync(genreRepository, genreId, command.UserId, cancellationToken))
            .WithMessage("Das angegebene Genre existiert nicht.");

        RuleFor(command => command.TrackName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Der Trackname ist erforderlich.")
            .MinimumLength(RecordTrackEntity.MinTrackNameLength)
            .WithMessage($"Der Trackname muss mindestens {RecordTrackEntity.MinTrackNameLength} Zeichen lang sein.")
            .MaximumLength(RecordTrackEntity.MaxTrackNameLength)
            .WithMessage($"Der Trackname darf höchstens {RecordTrackEntity.MaxTrackNameLength} Zeichen lang sein.")
            .Matches(RecordTrackEntity.TrackNamePattern)
            .WithMessage("Der Trackname darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / ( ) enthalten.");

        RuleFor(command => command.RecordSide)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Die Seite ist erforderlich.")
            .MaximumLength(RecordTrackEntity.MaxRecordSideLength)
            .WithMessage($"Die Seite darf höchstens {RecordTrackEntity.MaxRecordSideLength} Zeichen lang sein.")
            .Matches(RecordTrackEntity.RecordSidePattern)
            .WithMessage("Die Seite darf nur Buchstaben oder Ziffern enthalten.");

        RuleFor(command => command.TrackNumber)
            .GreaterThanOrEqualTo(RecordTrackEntity.MinTrackNumber)
            .WithMessage($"Die Tracknummer muss mindestens {RecordTrackEntity.MinTrackNumber} sein.");

        RuleFor(command => command.Information)
            .MaximumLength(RecordTrackEntity.MaxInformationLength)
            .WithMessage(
                $"Das Feld 'information' darf höchstens {RecordTrackEntity.MaxInformationLength} Zeichen lang sein.");
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

    private static async Task<bool> BelongsToUserAsync(
        IRepository<GenreEntity> repository,
        int genreId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var genre = await repository.GetByIdAsync(genreId, cancellationToken);

        return genre is not null && genre.UserId == userId;
    }
}
