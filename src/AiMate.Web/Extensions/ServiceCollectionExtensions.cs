using AiMate.Infrastructure.Data;
using AiMate.Infrastructure.Services;
using AiMate.Infrastructure.Services.ActionHandlers;
using AiMate.Core.Services;
using AiMate.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;
using Fluxor;
using Serilog;

namespace AiMate.Web.Extensions;

/// <summary>
/// Extension methods for registering services in the dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Razor components and Blazor Server support
    /// </summary>
    public static IServiceCollection AddBlazorComponents(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        services.AddControllers();
        return services;
    }

    /// <summary>
    /// Configure rate limiting policies
    /// </summary>
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Default policy for authenticated API calls
            options.AddFixedWindowLimiter("api", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.PermitLimit = 60;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
            });

            // Strict policy for anonymous error logging (prevent abuse)
            options.AddFixedWindowLimiter("error-logging", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.PermitLimit = 10;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // Developer tier policy (higher limits)
            options.AddFixedWindowLimiter("developer", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.PermitLimit = 120;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 20;
            });

            // Admin endpoints (generous limits)
            options.AddFixedWindowLimiter("admin", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.PermitLimit = 200;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 50;
            });

            // Global limiter (fallback)
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
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
        return services;
    }

    /// <summary>
    /// Configure response caching and compression
    /// </summary>
    public static IServiceCollection AddResponseCachingAndCompression(this IServiceCollection services)
    {
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 64 * 1024 * 1024;
            options.UseCaseSensitivePaths = false;
            options.SizeLimit = 100 * 1024 * 1024;
        });

        services.AddResponseCompression(options =>
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

        services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
            options.Level = System.IO.Compression.CompressionLevel.Fastest);

        services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
            options.Level = System.IO.Compression.CompressionLevel.Fastest);

        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder
                .Expire(TimeSpan.FromSeconds(60))
                .Tag("default"));

            options.AddPolicy("static", builder => builder
                .Expire(TimeSpan.FromMinutes(5))
                .Tag("static")
                .SetVaryByQuery("*"));

            options.AddPolicy("search", builder => builder
                .Expire(TimeSpan.FromMinutes(2))
                .Tag("search")
                .SetVaryByQuery("query", "limit", "threshold"));

            options.AddPolicy("knowledge", builder => builder
                .Expire(TimeSpan.FromMinutes(5))
                .Tag("knowledge")
                .SetVaryByQuery("*"));

            options.AddPolicy("public", builder => builder
                .Expire(TimeSpan.FromMinutes(30))
                .Tag("public")
                .SetVaryByQuery("*"));

            options.AddPolicy("analytics", builder => builder
                .Expire(TimeSpan.FromMinutes(1))
                .Tag("analytics")
                .SetVaryByQuery("*"));

            options.AddPolicy("no-cache", builder => builder
                .NoCache()
                .Tag("no-cache"));
        });

        Log.Information("Response caching and compression configured");
        return services;
    }

    /// <summary>
    /// Configure JWT authentication and authorization
    /// </summary>
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var jwtSecret = configuration["Jwt:Secret"]
            ?? "aiMate-super-secret-key-change-in-production-minimum-32-characters-long";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = !environment.IsDevelopment();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/_blazor"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanAddOwnKeys", policy =>
                policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
                    AiMate.Core.Enums.UserPermission.AddOwnKeys)));

            options.AddPolicy("CanManageMCP", policy =>
                policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
                    AiMate.Core.Enums.UserPermission.ManageMCP)));

            options.AddPolicy("CanManageModels", policy =>
                policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
                    AiMate.Core.Enums.UserPermission.ManageModels)));

            options.AddPolicy("CanShareConnections", policy =>
                policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
                    AiMate.Core.Enums.UserPermission.ShareConnections)));

            options.AddPolicy("CanAddCustomEndpoints", policy =>
                policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
                    AiMate.Core.Enums.UserPermission.AddCustomEndpoints)));

            options.AddPolicy("AdminOnly", policy =>
                policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
                    AiMate.Core.Enums.UserPermission.AdminAccess)));

            options.AddPolicy("CanViewAllAnalytics", policy =>
                policy.Requirements.Add(new AiMate.Web.Authorization.PermissionRequirement(
                    AiMate.Core.Enums.UserPermission.ViewAllAnalytics)));
        });

        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
            AiMate.Web.Authorization.PermissionHandler>();

        Log.Information("JWT authentication and authorization configured");
        return services;
    }

    /// <summary>
    /// Configure CORS policy
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("ApiCorsPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        return services;
    }

    /// <summary>
    /// Configure data protection for API key encryption
    /// </summary>
    public static IServiceCollection AddDataProtectionConfig(this IServiceCollection services, IHostEnvironment environment, string contentRootPath)
    {
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(contentRootPath, "keys")))
            .SetApplicationName("AiMate")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

        Log.Information("Data Protection configured for API key encryption");
        return services;
    }

    /// <summary>
    /// Add MudBlazor and Fluxor UI frameworks
    /// </summary>
    public static IServiceCollection AddUIFrameworks(this IServiceCollection services)
    {
        services.AddMudServices();
        services.AddFluxor(options =>
        {
            options.ScanAssemblies(typeof(Program).Assembly);
        });
        return services;
    }

    /// <summary>
    /// Configure database context
    /// </summary>
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider") ?? "InMemory";

        if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrEmpty(connectionString))
            {
                Log.Warning("PostgreSQL provider selected but no connection string found. Falling back to InMemory database.");
                services.AddDbContext<AiMateDbContext>(options =>
                    options.UseInMemoryDatabase("AiMateDb"));
            }
            else
            {
                Log.Information("Using PostgreSQL database provider");
                services.AddDbContext<AiMateDbContext>(options =>
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.UseVector();
                    }));
            }
        }
        else
        {
            Log.Information("Using InMemory database provider");
            services.AddDbContext<AiMateDbContext>(options =>
                options.UseInMemoryDatabase("AiMateDb"));
        }

        return services;
    }

    /// <summary>
    /// Register HTTP clients
    /// </summary>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddHttpClient();

        services.AddHttpClient("ApiClient", (serviceProvider, client) =>
        {
            var baseUrl = configuration["ApiBaseUrl"];

            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = environment.IsDevelopment()
                    ? "https://localhost:5001"
                    : "https://localhost:5001";
            }

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(120);

            Log.Information("Configured ApiClient with BaseAddress: {BaseUrl}", baseUrl);
        });

        services.AddHttpClient<LiteLLMService>();
        services.AddHttpClient<OpenAIEmbeddingService>();
        services.AddHttpClient<MCPToolService>();

        return services;
    }

    /// <summary>
    /// Register application services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<ILiteLLMService, LiteLLMService>();
        services.AddScoped<IPersonalityService, PersonalityService>();
        services.AddScoped<IKnowledgeGraphService, KnowledgeGraphService>();
        services.AddScoped<IKnowledgeService, KnowledgeService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<INotesService, NotesService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFileUploadService, Infrastructure.Services.FileUploadService>();
        services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
        services.AddScoped<IDatasetGeneratorService, DatasetGeneratorService>();
        services.AddScoped<IMCPToolService, MCPToolService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IPluginSettingsService, PluginSettingsService>();
        services.AddScoped<ICodeFileService, CodeFileService>();

        // Roslyn and IntelliSense
        services.AddScoped<IRoslynCompilationService, RoslynCompilationService>();
        services.AddScoped<IIntelliSenseService, IntelliSenseService>();

        // Structured Content
        services.AddScoped<IStructuredContentService, StructuredContentService>();
        services.AddScoped<IActionHandler, NavigationActionHandler>();
        services.AddScoped<IActionHandler, ApiCallActionHandler>();
        services.AddScoped<IActionHandler, ExportActionHandler>();

        // Singletons
        services.AddSingleton<IPluginManager, PluginManager>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        // Scoped services
        services.AddScoped<MarkdownService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IUserFeedbackService, UserFeedbackService>();
        services.AddScoped<IErrorLoggingService, ErrorLoggingService>();
        services.AddScoped<ISearchService, SearchService>();

        // Background jobs (with fallback)
        try
        {
            services.AddSingleton<IBackgroundJobService, HangfireBackgroundJobService>();
            services.AddScoped<IBackgroundJobs, BackgroundJobs>();
            Log.Information("Background job services registered");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Background job services not registered - Hangfire packages may not be installed");
        }

        // Localization
        services.AddLocalization();

        Log.Information("All application services registered");
        return services;
    }
}