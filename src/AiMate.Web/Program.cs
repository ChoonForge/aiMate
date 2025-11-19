using AiMate.Infrastructure.Data;
using AiMate.Web.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Fluxor;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/aiMate-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API Controllers for Developer tier
builder.Services.AddControllers();

// Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "aiMate API",
        Version = "v1",
        Description = "REST API for aiMate - Developer tier access",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "aiMate",
            Email = "support@aimate.co.nz"
        }
    });

    // Add API Key authentication
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key authentication. Use format: Bearer {your-api-key}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS for API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// MudBlazor
builder.Services.AddMudServices();

// Fluxor state management
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    // UseReduxDevTools() is not available in this version of Fluxor
    // For debugging, use browser developer tools with Fluxor DevTools extension
});

// Database
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AiMateDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.UseVector();
        }));
}

// Add HTTP client
builder.Services.AddHttpClient();

// Add localization
builder.Services.AddLocalization();

// Register our services
builder.Services.AddScoped<AiMate.Core.Services.ILiteLLMService, AiMate.Infrastructure.Services.LiteLLMService>();
builder.Services.AddScoped<AiMate.Core.Services.IPersonalityService, AiMate.Infrastructure.Services.PersonalityService>();
builder.Services.AddScoped<AiMate.Core.Services.IKnowledgeGraphService, AiMate.Infrastructure.Services.KnowledgeGraphService>();
builder.Services.AddScoped<AiMate.Core.Services.IWorkspaceService, AiMate.Infrastructure.Services.WorkspaceService>();
builder.Services.AddScoped<AiMate.Core.Services.IAuthService, AiMate.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<AiMate.Core.Services.IFileUploadService, AiMate.Infrastructure.Services.FileUploadService>();
builder.Services.AddScoped<AiMate.Core.Services.IEmbeddingService, AiMate.Infrastructure.Services.OpenAIEmbeddingService>();
builder.Services.AddScoped<AiMate.Core.Services.IDatasetGeneratorService, AiMate.Infrastructure.Services.DatasetGeneratorService>();
builder.Services.AddScoped<AiMate.Core.Services.IMCPToolService, AiMate.Infrastructure.Services.MCPToolService>();
builder.Services.AddScoped<AiMate.Core.Services.IApiKeyService, AiMate.Infrastructure.Services.ApiKeyService>();
builder.Services.AddScoped<AiMate.Core.Services.IFeedbackService, AiMate.Infrastructure.Services.FeedbackService>();

// Register Plugin System (Singleton for plugin lifecycle management)
builder.Services.AddSingleton<AiMate.Core.Services.IPluginManager, AiMate.Infrastructure.Services.PluginManager>();

// Register Permission Service (Singleton for access control)
builder.Services.AddSingleton<AiMate.Core.Services.IPermissionService, AiMate.Infrastructure.Services.PermissionService>();

// Register HttpClient for services that need it
builder.Services.AddHttpClient<AiMate.Infrastructure.Services.LiteLLMService>();
builder.Services.AddHttpClient<AiMate.Infrastructure.Services.OpenAIEmbeddingService>();
builder.Services.AddHttpClient<AiMate.Infrastructure.Services.MCPToolService>();

Log.Information("All services registered successfully (Phase 6 complete)");

var app = builder.Build();

// Initialize plugins on startup
using (var scope = app.Services.CreateScope())
{
    var pluginManager = scope.ServiceProvider.GetRequiredService<AiMate.Core.Services.IPluginManager>();
    await pluginManager.LoadPluginsAsync();
    Log.Information("Plugins initialized successfully");
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Enable Swagger in all environments (production needs API docs too)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "aiMate API v1");
    c.RoutePrefix = "api/docs"; // Access at /api/docs
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// CORS for API
app.UseCors("ApiCorsPolicy");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", async (AiMateDbContext? db) =>
{
    if (db == null)
    {
        return Results.Json(new { status = "degraded", message = "Database not configured" }, statusCode: 200);
    }

    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect)
        {
            return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        else
        {
            return Results.Json(new { status = "unhealthy", message = "Cannot connect to database" }, statusCode: 503);
        }
    }
    catch (Exception ex)
    {
        return Results.Json(new { status = "unhealthy", message = ex.Message }, statusCode: 503);
    }
});

try
{
    Log.Information("Starting aiMate application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
