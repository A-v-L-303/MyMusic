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

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddApplication();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];

        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        // Ohne dies mappt ASP.NET Core den "sub"-Claim standardmäßig auf
        // ClaimTypes.NameIdentifier - der ICurrentUserService liest aber den rohen
        // "sub"-Claim (Sicherheitskonzept).
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Keycloak nimmt ohne eigenen Audience-Mapper standardmäßig "account" als
            // Audience in jedes Access Token auf.
            ValidAudience = "account"
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseAuthentication();

app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapMeEndpoints();

app.Run();
