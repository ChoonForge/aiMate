using AiMate.Infrastructure.Data;
using AiMate.Web.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Fluxor;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;

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

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? "aiMate-super-secret-key-change-in-production-minimum-32-characters-long";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Allow HTTP in dev
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = false,  // Not using issuer validation for now
        ValidateAudience = false, // Not using audience validation for now
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };

    // Enable JWT authentication for SignalR (Blazor Server uses SignalR)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // If the request is for a SignalR hub and contains an access token
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/_blazor"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Authorization with custom policies
builder.Services.AddAuthorization(options =>
{
    // Policy for users who can add their own API keys (BYOK)
    options.AddPolicy("CanAddOwnKeys", policy =>
        policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
            AiMate.Core.Enums.UserPermission.AddOwnKeys)));

    // Policy for users who can manage MCP servers
    options.AddPolicy("CanManageMCP", policy =>
        policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
            AiMate.Core.Enums.UserPermission.ManageMCP)));

    // Policy for users who can manage custom models
    options.AddPolicy("CanManageModels", policy =>
        policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
            AiMate.Core.Enums.UserPermission.ManageModels)));

    // Policy for users who can share connections
    options.AddPolicy("CanShareConnections", policy =>
        policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
            AiMate.Core.Enums.UserPermission.ShareConnections)));

    // Policy for users who can add custom endpoints
    options.AddPolicy("CanAddCustomEndpoints", policy =>
        policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
            AiMate.Core.Enums.UserPermission.AddCustomEndpoints)));

    // Policy for admins only
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
            AiMate.Core.Enums.UserPermission.AdminAccess)));

    // Policy for viewing all analytics (admin only)
    options.AddPolicy("CanViewAllAnalytics", policy =>
        policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
            AiMate.Core.Enums.UserPermission.ViewAllAnalytics)));
});

// Register authorization handler
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    AiMate.Web.Authorization.PermissionHandler>();

Log.Information("JWT authentication and authorization configured");

// Swagger/OpenAPI for API documentation (requires Swashbuckle.AspNetCore package)
// Commented out until package is added to avoid build errors

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
//    {
//        Title = "aiMate API",
//        Version = "v1",
//        Description = "REST API for aiMate - Developer tier access",
//        Contact = new Microsoft.OpenApi.Models.OpenApiContact
//        {
//            Name = "aiMate",
//            Email = "support@aimate.co.nz"
//        }
//    });

//    // Add API Key authentication
//    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//    {
//        Description = "API Key authentication. Use format: Bearer {your-api-key}",
//        Name = "Authorization",
//        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });

//    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//    {
//        {
//            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//            {
//                Reference = new Microsoft.OpenApi.Models.OpenApiReference
//                {
//                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
//                    Id = "ApiKey"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});


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

// Database configuration
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "InMemory";

if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    if (string.IsNullOrEmpty(connectionString))
    {
        Log.Warning("PostgreSQL provider selected but no connection string found. Falling back to InMemory database.");
        builder.Services.AddDbContext<AiMateDbContext>(options =>
            options.UseInMemoryDatabase("AiMateDb"));
    }
    else
    {
        Log.Information("Using PostgreSQL database provider");
        builder.Services.AddDbContext<AiMateDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            }));
    }
}
else
{
    Log.Information("Using InMemory database provider");
    builder.Services.AddDbContext<AiMateDbContext>(options =>
        options.UseInMemoryDatabase("AiMateDb"));
}

// Add HTTP client for general use
builder.Services.AddHttpClient();

// Add named HttpClient for API calls (Blazor frontend calling its own API)
builder.Services.AddHttpClient("ApiClient", (serviceProvider, client) =>
{
    // Get the current HTTP context to determine the base URL
    // In development, we know it's localhost:5001 (HTTPS) or localhost:5000 (HTTP)
    // In production, this would be the deployed URL
    var baseUrl = builder.Configuration["ApiBaseUrl"];

    if (string.IsNullOrEmpty(baseUrl))
    {
        // Default to localhost in development
        baseUrl = builder.Environment.IsDevelopment()
            ? "https://localhost:5001"
            : "https://localhost:5001"; // Should be configured in production
    }

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(120); // Longer timeout for streaming

    Log.Information("Configured ApiClient with BaseAddress: {BaseUrl}", baseUrl);
});

// Add localization
builder.Services.AddLocalization();

// Register our services
builder.Services.AddScoped<AiMate.Core.Services.ILiteLLMService, AiMate.Infrastructure.Services.LiteLLMService>();
builder.Services.AddScoped<AiMate.Core.Services.IPersonalityService, AiMate.Infrastructure.Services.PersonalityService>();
builder.Services.AddScoped<AiMate.Core.Services.IKnowledgeGraphService, AiMate.Infrastructure.Services.KnowledgeGraphService>();
builder.Services.AddScoped<AiMate.Core.Services.IKnowledgeService, AiMate.Infrastructure.Services.KnowledgeService>();
builder.Services.AddScoped<AiMate.Core.Services.IWorkspaceService, AiMate.Infrastructure.Services.WorkspaceService>();
builder.Services.AddScoped<AiMate.Core.Services.IConversationService, AiMate.Infrastructure.Services.ConversationService>();
builder.Services.AddScoped<AiMate.Core.Services.IProjectService, AiMate.Infrastructure.Services.ProjectService>();
builder.Services.AddScoped<AiMate.Core.Services.INotesService, AiMate.Infrastructure.Services.NotesService>();
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

// Seed database with default admin user for development
if (app.Environment.IsDevelopment() && databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AiMateDbContext>();

        // Check if admin user already exists
        if (!dbContext.Users.Any(u => u.Username == "admin"))
        {
            var adminUser = new AiMate.Core.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@localhost",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Tier = AiMate.Core.Enums.UserTier.Developer,
                DefaultPersonality = AiMate.Core.Enums.PersonalityMode.Professional,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();

            Log.Information("Created default admin user (username: admin, password: admin)");
        }
    }
}

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
// Commented out until Swashbuckle.AspNetCore package is added
/*
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "aiMate API v1");
    c.RoutePrefix = "api/docs"; // Access at /api/docs
});
*/

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// CORS for API
app.UseCors("ApiCorsPolicy");

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<AiMate.Web.App>()
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

// Dev-only: Database inspection endpoint
if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev/db", async (AiMateDbContext db) =>
    {
        var users = await db.Users.ToListAsync();
        var workspaces = await db.Workspaces.ToListAsync();
        var conversations = await db.Conversations.ToListAsync();
        var projects = await db.Projects.ToListAsync();
        var notes = await db.Notes.ToListAsync();
        var knowledgeItems = await db.KnowledgeItems.ToListAsync();

        return Results.Ok(new
        {
            DatabaseProvider = databaseProvider,
            Counts = new
            {
                Users = users.Count,
                Workspaces = workspaces.Count,
                Conversations = conversations.Count,
                Projects = projects.Count,
                Notes = notes.Count,
                KnowledgeItems = knowledgeItems.Count
            },
            Users = users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Tier,
                u.DefaultPersonality,
                u.CreatedAt,
                HasPassword = !string.IsNullOrEmpty(u.PasswordHash)
            }),
            Workspaces = workspaces.Select(w => new { w.Id, w.Name, w.UserId }),
            Conversations = conversations.Select(c => new { c.Id, c.Title, c.WorkspaceId, c.CreatedAt }),
            Projects = projects.Select(p => new { p.Id, p.Name, p.Key, p.Status, p.UserId }),
            Notes = notes.Select(n => new { n.Id, n.Title, n.WorkspaceId }),
            KnowledgeItems = knowledgeItems.Select(k => new { k.Id, k.Title, k.Type, k.UserId })
        });
    });

    Log.Information("Dev endpoint enabled: GET /dev/db");
}

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
