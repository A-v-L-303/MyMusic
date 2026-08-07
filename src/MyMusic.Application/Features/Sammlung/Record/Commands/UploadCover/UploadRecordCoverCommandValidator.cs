namespace MyMusic.Application.Features.Sammlung.Record.Commands.UploadCover;

public sealed class UploadRecordCoverCommandValidator : AbstractValidator<UploadRecordCoverCommand>
{
    public UploadRecordCoverCommandValidator()
    {
        RuleFor(command => command.FileContent)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Es wurde keine Datei übermittelt.")
            .Must(content => content.Length <= RecordEntity.MaxAlbumCoverSizeBytes)
            .WithMessage(
                $"Die Datei darf höchstens {RecordEntity.MaxAlbumCoverSizeBytes / (1024 * 1024)} MB groß sein.")
            .Must(content => RecordEntity.DetectAlbumCoverContentType(content) is not null)
            .WithMessage("Es sind nur JPEG- oder PNG-Dateien erlaubt.");
    }
}
