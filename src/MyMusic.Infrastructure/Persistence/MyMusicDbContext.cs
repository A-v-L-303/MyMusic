using Microsoft.EntityFrameworkCore;

namespace MyMusic.Infrastructure.Persistence;

// Bewusst ohne DbSets: Block 0a weist nur die Migrationskette nach.
// Entitaeten und Konfigurationen folgen mit den fachlichen Slices.
public class MyMusicDbContext(DbContextOptions<MyMusicDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyMusicDbContext).Assembly);
    }
}
