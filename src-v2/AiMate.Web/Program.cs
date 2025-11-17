using AiMate.Infrastructure.Data;
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

// MudBlazor
builder.Services.AddMudServices();

// Fluxor state management
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
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

// TODO: Add custom services
// builder.Services.AddScoped<ILocalizationService, LocalizationService>();
// builder.Services.AddScoped<IChatService, ChatService>();
// builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
// builder.Services.AddScoped<IPersonalityService, PersonalityService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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
