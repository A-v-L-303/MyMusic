using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();

    var seqUrl = builder.Configuration.GetConnectionString("seq");

    if (!string.IsNullOrWhiteSpace(seqUrl))
    {
        loggerConfiguration.WriteTo.Seq(seqUrl);
    }
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapDefaultEndpoints();

app.Run();
