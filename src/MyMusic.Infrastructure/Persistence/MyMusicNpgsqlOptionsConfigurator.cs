namespace MyMusic.Infrastructure.Persistence;

public static class MyMusicNpgsqlOptionsConfigurator
{
    // Wiederverwendete Singleton-Instanzen: NpgsqlDbContextOptionsBuilder.MapEnum fließt in die
    // DbContextOptions ein, anhand derer EF Core seinen internen ServiceProvider cacht. Eine neue
    // Translator-Instanz je Aufruf (pro DbContext-Konstruktion, also pro Request) lässt EF Core
    // die Konfiguration als "geändert" ansehen und einen neuen ServiceProvider aufbauen -
    // nach mehr als zwanzig Aufrufen bricht das mit ManyServiceProvidersCreatedWarning ab.
    internal static readonly RecordFormatPgNameTranslator RecordFormatTranslator = new();

    internal static readonly RecordConditionPgNameTranslator RecordConditionTranslator = new();

    public static void ConfigureEnums(NpgsqlDbContextOptionsBuilder npgsqlOptions)
    {
        npgsqlOptions.MapEnum<RecordFormat>(nameTranslator: RecordFormatTranslator);

        npgsqlOptions.MapEnum<RecordCondition>(nameTranslator: RecordConditionTranslator);
    }
}
