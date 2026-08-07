namespace MyMusic.Infrastructure.Persistence;

internal sealed class RecordFormatPgNameTranslator : INpgsqlNameTranslator
{
    public string TranslateMemberName(string clrName)
    {
        return clrName switch
        {
            nameof(RecordFormat.Album) => "Album",
            nameof(RecordFormat.MaxiSingle) => "MaxiSingle",
            nameof(RecordFormat.Single) => "Single",
            nameof(RecordFormat.Ep) => "EP",
            nameof(RecordFormat.Compilation) => "Compilation",
            nameof(RecordFormat.CdAlbum) => "CD-Album",
            nameof(RecordFormat.CdMaxiSingle) => "CD-MaxiSingle",
            nameof(RecordFormat.CdSingle) => "CD-Single",
            nameof(RecordFormat.CdEp) => "CD-EP",
            nameof(RecordFormat.CdCompilation) => "CD-Compilation",
            _ => clrName
        };
    }

    public string TranslateTypeName(string clrName)
    {
        return "record_format";
    }
}
