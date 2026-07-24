using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyMusic.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<MyMusicDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("mymusicdb")));

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    using var scope = host.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<MyMusicDbContext>();

    logger.LogInformation("Datenbankmigration wird gestartet.");

    await context.Database.MigrateAsync();

    logger.LogInformation("Datenbankmigration erfolgreich abgeschlossen.");

    return 0;
}
catch (Exception exception)
{
    logger.LogError(exception, "Datenbankmigration fehlgeschlagen.");

    return 1;
}
