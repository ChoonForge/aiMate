using AiMate.Infrastructure.Data;
using AiMate.Web.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Fluxor;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using AiMate.Web;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard; // Add this using directive

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

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Default policy for authenticated API calls
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 60; // 60 requests per minute
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });

    // Strict policy for anonymous error logging (prevent abuse)
    options.AddFixedWindowLimiter("error-logging", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 10; // 10 errors per minute per IP
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // No queue for error logs
    });

    // Developer tier policy (higher limits)
    options.AddFixedWindowLimiter("developer", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 120; // 120 requests per minute
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 20;
    });

    // Admin endpoints (generous limits)
    options.AddFixedWindowLimiter("admin", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 200; // 200 requests per minute
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 50;
    });

    // Global limiter (fallback)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Get user identifier (IP address for anonymous, userId for authenticated)
        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown"
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 100,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10
        });
    });

    // Rejection response
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                message = "Too many requests. Please try again later.",
                type = "rate_limit_error",
                retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (int)retryAfter.TotalSeconds
                    : 60
            }
        }, cancellationToken);
    };
});

Log.Information("Rate limiting configured");

// Response Caching and Compression
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 64 * 1024 * 1024; // 64MB max cached response
    options.UseCaseSensitivePaths = false;
    options.SizeLimit = 100 * 1024 * 1024; // 100MB total cache size
});

// Configure response compression - exclude from Browser Link
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.ExcludedMimeTypes = new[]
    {
        "application/json",
        "application/xml",
        "text/plain",
        "text/css",
        "text/html",
        "application/javascript"
    }.Concat(Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes).ToArray();
});

// Configure Brotli compression (best compression)
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest; // Balance speed and size
});

// Configure Gzip compression (fallback for older browsers)
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// Configure output caching (ASP.NET Core 9 feature - better than ResponseCaching)
builder.Services.AddOutputCache(options =>
{
    // Default policy: cache for 60 seconds
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromSeconds(60))
        .Tag("default"));

    // Policy for static API responses (user profiles, settings, etc.)
    options.AddPolicy("static", builder => builder
        .Expire(TimeSpan.FromMinutes(5))
        .Tag("static")
        .SetVaryByQuery("*"));

    // Policy for search results (cache for 2 minutes)
    options.AddPolicy("search", builder => builder
        .Expire(TimeSpan.FromMinutes(2))
        .Tag("search")
        .SetVaryByQuery("query", "limit", "threshold"));

    // Policy for knowledge base items (cache for 5 minutes)
    options.AddPolicy("knowledge", builder => builder
        .Expire(TimeSpan.FromMinutes(5))
        .Tag("knowledge")
        .SetVaryByQuery("*"));

    // Policy for public content (cache for 30 minutes)
    options.AddPolicy("public", builder => builder
        .Expire(TimeSpan.FromMinutes(30))
        .Tag("public")
        .SetVaryByQuery("*"));

    // Policy for analytics (cache for 1 minute - frequently changing data)
    options.AddPolicy("analytics", builder => builder
        .Expire(TimeSpan.FromMinutes(1))
        .Tag("analytics")
        .SetVaryByQuery("*"));

    // No caching policy for dynamic content (chat completions, streaming, etc.)
    options.AddPolicy("no-cache", builder => builder
        .NoCache()
        .Tag("no-cache"));
});

Log.Information("Response caching and compression configured");

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

// Data Protection (for API key encryption)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
    .SetApplicationName("AiMate")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90)); // Rotate keys every 90 days

Log.Information("Data Protection configured for API key encryption");

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

// Hangfire Background Jobs
// Note: Requires Hangfire.AspNetCore and Hangfire.PostgreSql (or Hangfire.InMemory) NuGet packages
// Install with: dotnet add package Hangfire.AspNetCore
//              dotnet add package Hangfire.PostgreSql (for production)
//              dotnet add package Hangfire.InMemory (for development)
//try
//{
//    if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
//    {
//        var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
//        if (!string.IsNullOrEmpty(connectionString))
//        {
//            builder.Services.AddHangfire(config =>
//            {
//                config.UsePostgreSqlStorage(c =>
//                    c.UseNpgsqlConnection(connectionString));
//                config.UseSimpleAssemblyNameTypeSerializer();
//                config.UseRecommendedSerializerSettings();
//            });
//        }
//    }
//    else
//    {
//        // Use in-memory storage for development
//        builder.Services.AddHangfire(config =>
//        {
//            config.UseInMemoryStorage();
//            config.UseSimpleAssemblyNameTypeSerializer();
//            config.UseRecommendedSerializerSettings();
//        });
//    }

//    builder.Services.AddHangfireServer(options =>
//    {
//        options.WorkerCount = 2; // Number of background workers
//        options.Queues = new[] { "default", "high-priority" };
//    });

//    Log.Information("Hangfire background jobs configured");
//}
//catch (Exception ex)
//{
//    // Hangfire packages not installed yet - log warning but don't fail startup
//    Log.Warning(ex, "Hangfire not configured - background jobs disabled. Install Hangfire.AspNetCore package to enable.");
//}

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
// Use web-specific implementation that makes HTTP calls to backend API
builder.Services.AddScoped<AiMate.Core.Services.IFileUploadService, AiMate.Web.Services.FileUploadService>();
builder.Services.AddScoped<AiMate.Core.Services.IEmbeddingService, AiMate.Infrastructure.Services.OpenAIEmbeddingService>();
builder.Services.AddScoped<AiMate.Core.Services.IDatasetGeneratorService, AiMate.Infrastructure.Services.DatasetGeneratorService>();
builder.Services.AddScoped<AiMate.Core.Services.IMCPToolService, AiMate.Infrastructure.Services.MCPToolService>();
builder.Services.AddScoped<AiMate.Core.Services.IApiKeyService, AiMate.Infrastructure.Services.ApiKeyService>();
builder.Services.AddScoped<AiMate.Core.Services.IFeedbackService, AiMate.Infrastructure.Services.FeedbackService>();
builder.Services.AddScoped<AiMate.Core.Services.IConnectionService, AiMate.Infrastructure.Services.ConnectionService>();
builder.Services.AddScoped<AiMate.Core.Services.IPluginSettingsService, AiMate.Infrastructure.Services.PluginSettingsService>();
builder.Services.AddScoped<AiMate.Core.Services.ICodeFileService, AiMate.Infrastructure.Services.CodeFileService>();

// Register Roslyn and IntelliSense services
builder.Services.AddScoped<AiMate.Core.Services.IRoslynCompilationService, AiMate.Infrastructure.Services.RoslynCompilationService>();
builder.Services.AddScoped<AiMate.Core.Services.IIntelliSenseService, AiMate.Infrastructure.Services.IntelliSenseService>();

// Register Structured Content services
builder.Services.AddScoped<AiMate.Core.Services.IStructuredContentService, AiMate.Infrastructure.Services.StructuredContentService>();
builder.Services.AddScoped<AiMate.Core.Services.IActionHandler, AiMate.Infrastructure.Services.ActionHandlers.NavigationActionHandler>();
builder.Services.AddScoped<AiMate.Core.Services.IActionHandler, AiMate.Infrastructure.Services.ActionHandlers.ApiCallActionHandler>();
builder.Services.AddScoped<AiMate.Core.Services.IActionHandler, AiMate.Infrastructure.Services.ActionHandlers.ExportActionHandler>();

// Register Plugin System (Singleton for plugin lifecycle management)
builder.Services.AddSingleton<AiMate.Core.Services.IPluginManager, AiMate.Infrastructure.Services.PluginManager>();

// Register Permission Service (Singleton for access control)
builder.Services.AddSingleton<AiMate.Core.Services.IPermissionService, AiMate.Infrastructure.Services.PermissionService>();

// Register UI Services
builder.Services.AddScoped<AiMate.Web.Services.MarkdownService>();
// Register Organization and Group services
builder.Services.AddScoped<AiMate.Core.Services.IOrganizationService, AiMate.Infrastructure.Services.OrganizationService>();
builder.Services.AddScoped<AiMate.Core.Services.IGroupService, AiMate.Infrastructure.Services.GroupService>();

// Register User Feedback and Error Logging services (alpha testing)
builder.Services.AddScoped<AiMate.Core.Services.IUserFeedbackService, AiMate.Infrastructure.Services.UserFeedbackService>();
builder.Services.AddScoped<AiMate.Core.Services.IErrorLoggingService, AiMate.Infrastructure.Services.ErrorLoggingService>();

// Register Encryption Service (for API key protection)
builder.Services.AddSingleton<AiMate.Core.Services.IEncryptionService, AiMate.Infrastructure.Services.EncryptionService>();

// Register Search Service (full-text and semantic search)
builder.Services.AddScoped<AiMate.Core.Services.ISearchService, AiMate.Infrastructure.Services.SearchService>();

// Register File Storage Service (local filesystem, can be swapped for Azure/S3)
builder.Services.AddSingleton<AiMate.Core.Services.IFileStorageService, AiMate.Infrastructure.Services.LocalFileStorageService>();

// Register Background Job Services (requires Hangfire packages)
try
{
    builder.Services.AddSingleton<AiMate.Core.Services.IBackgroundJobService, AiMate.Infrastructure.Services.HangfireBackgroundJobService>();
    builder.Services.AddScoped<AiMate.Core.Services.IBackgroundJobs, AiMate.Infrastructure.Services.BackgroundJobs>();
    Log.Information("Background job services registered");
}
catch (Exception ex)
{
    Log.Warning(ex, "Background job services not registered - Hangfire packages may not be installed");
}

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
                DefaultPersonality = AiMate.Core.Enums.PersonalityMode.KiwiProfessional,
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

app.UseHttpsRedirection();

// Response compression (must be early in pipeline, before static files)
app.UseResponseCompression();

app.UseStaticFiles();
app.UseAntiforgery();

// CORS for API
app.UseCors("ApiCorsPolicy");

// Rate Limiting (must be before Authentication)
app.UseRateLimiter();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Output caching (after auth so we can cache per-user if needed)
app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();



