namespace MyMusic.Infrastructure.Persistence;

public class MyMusicDbContext(DbContextOptions<MyMusicDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<RecordFormat>(
            name: "record_format",
            nameTranslator: MyMusicNpgsqlOptionsConfigurator.RecordFormatTranslator);

        modelBuilder.HasPostgresEnum<RecordCondition>(
            name: "record_condition",
            nameTranslator: MyMusicNpgsqlOptionsConfigurator.RecordConditionTranslator);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyMusicDbContext).Assembly);
    }
}
