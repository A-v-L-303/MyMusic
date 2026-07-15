using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Serilog schreibt nach Console (Aspire-Dashboard) und nach Seq.
// Die Seq-URL liefert Aspire als Verbindungsinformation; fehlt sie, bleibt nur die Console.
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

// Loggt nur Methode, Pfad, Statuscode und Dauer. Keine Header, keine Request-Bodies
// und keine DTOs — siehe Sicherheitskonzept. Wer hier Felder ergaenzt, muss pruefen,
// dass weder Authorization- noch Cookie-Header und keine personenbezogenen Daten anfallen.
app.UseSerilogRequestLogging();

app.MapDefaultEndpoints();

app.Run();
