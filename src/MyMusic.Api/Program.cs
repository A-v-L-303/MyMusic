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

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddDbContext<MyMusicDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("mymusicdb"),
        MyMusicNpgsqlOptionsConfigurator.ConfigureEnums));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddApplication();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];

        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = "account"
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new AdminRequirement()));

builder.Services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();

builder.Services.AddSingleton<KeycloakServiceAccountSecretProvider>();

builder.Services.AddHttpClient<KeycloakServiceAccountProvisioner>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Keycloak:AdminApiBaseUrl"]!));

builder.Services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Keycloak:AdminApiBaseUrl"]!));

builder.Services.AddHttpClient<IDiscogsClient, DiscogsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Discogs:BaseUrl"]!);

    client.DefaultRequestHeaders.UserAgent.ParseAdd("MyMusic/1.0");

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Discogs", $"token={builder.Configuration["Discogs:Token"]}");
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCors", policy => policy
            .SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Authorization", "Content-Type"));
    });
}

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT-Access-Token aus Keycloak, z. B. \"Bearer eyJhbGci...\""
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });

    var xmlFilename = "MyMusic.Api.xml";

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

await using (var startupScope = app.Services.CreateAsyncScope())
{
    var keycloakServiceAccountProvisioner =
        startupScope.ServiceProvider.GetRequiredService<KeycloakServiceAccountProvisioner>();

    try
    {
        await keycloakServiceAccountProvisioner.LoadSecretAsync(CancellationToken.None);
    }
    catch (Exception exception)
    {
        // Ein Fehlschlag hier darf nicht die gesamte API zum Absturz bringen - er betrifft
        // ausschliesslich die Admin-Endpunkte, die dann mit 500 antworten (kein Secret geladen).
        app.Logger.LogWarning(
            exception,
            "Das Keycloak-Service-Account-Secret konnte beim Start nicht geladen werden. " +
            "Die Admin-Endpunkte sind dadurch nicht funktionsfähig.");
    }
}

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
}

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.MapDefaultEndpoints();

app.MapMeEndpoints();

app.MapGenreEndpoints();

app.MapCountryEndpoints();

app.MapLabelEndpoints();

app.MapArtistEndpoints();

app.MapRecordEndpoints();

app.MapAdminEndpoints();

app.MapDiscogsEndpoints();

app.Run();
