namespace MyMusic.Infrastructure.Persistence;

internal sealed class RecordConditionPgNameTranslator : INpgsqlNameTranslator
{
    public string TranslateMemberName(string clrName)
    {
        return clrName switch
        {
            nameof(RecordCondition.Mint) => "Mint",
            nameof(RecordCondition.Nm) => "NM",
            nameof(RecordCondition.VgPlus) => "VG+",
            nameof(RecordCondition.Vg) => "VG",
            nameof(RecordCondition.GPlus) => "G+",
            nameof(RecordCondition.G) => "G",
            nameof(RecordCondition.P) => "P",
            _ => clrName
        };
    }

    public string TranslateTypeName(string clrName)
    {
        return "record_condition";
    }
}
