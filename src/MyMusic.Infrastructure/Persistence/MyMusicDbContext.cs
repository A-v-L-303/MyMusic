namespace MyMusic.Infrastructure.Persistence;

public class MyMusicDbContext(DbContextOptions<MyMusicDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyMusicDbContext).Assembly);
    }
}
