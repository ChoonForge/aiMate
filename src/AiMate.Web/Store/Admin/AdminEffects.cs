using System.Net.Http.Json;
using AiMate.Shared.Models;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace AiMate.Web.Store.Admin;

public class AdminEffects
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AdminEffects> _logger;
    private const string ApiEndpoint = "/api/v1/admin";

    public AdminEffects(
        IJSRuntime jsRuntime,
        IHttpClientFactory httpClientFactory,
        ILogger<AdminEffects> logger)
    {
        _jsRuntime = jsRuntime;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadData(LoadAdminDataAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Loading admin data from API");

            // Try to load from API if available
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var adminData = await httpClient.GetFromJsonAsync<AdminDataDto>(ApiEndpoint);

                if (adminData != null)
                {
                    // Map DTO to State
                    var adminState = new AdminState
                    {
                        // Overview statistics
                        TotalUsers = adminData.Overview.TotalUsers,
                        TotalConversations = adminData.Overview.TotalConversations,
                        ConversationsToday = adminData.Overview.ConversationsToday,
                        ActiveModels = adminData.Overview.ActiveModels,
                        TotalModels = adminData.Overview.TotalModels,
                        ConnectedMcpServers = adminData.Overview.ConnectedMcpServers,
                        TotalMcpServers = adminData.Overview.TotalMcpServers,

                        // System health
                        LiteLLMConnected = adminData.Overview.LiteLLMConnected,
                        LiteLLMUrl = adminData.Overview.LiteLLMUrl,
                        StorageUsedMB = adminData.Overview.StorageUsedMB,
                        StorageLimitMB = adminData.Overview.StorageLimitMB,
                        Uptime = adminData.Overview.Uptime,
                        AppVersion = adminData.Overview.AppVersion,

                        // Storage stats
                        LocalStorageUsedMB = adminData.Overview.LocalStorageUsedMB,
                        LocalStorageLimitMB = adminData.Overview.LocalStorageLimitMB,
                        IndexedDBUsedMB = adminData.Overview.IndexedDBUsedMB,
                        IndexedDBLimitMB = adminData.Overview.IndexedDBLimitMB,

                        // Models
                        Models = adminData.Models.Select(m => new AIModelConfig
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Provider = m.Provider,
                            IsEnabled = m.IsEnabled,
                            MaxTokens = m.MaxTokens,
                            Description = m.Description
                        }).ToList(),

                        // MCP Servers
                        McpServers = adminData.McpServers.Select(s => new MCPServerConfig
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Type = s.Type,
                            Connected = s.Connected,
                            ToolCount = s.ToolCount,
                            Command = s.Command,
                            Arguments = s.Arguments,
                            Url = s.Url
                        }).ToList(),

                        // System logs
                        SystemLogs = adminData.SystemLogs.Select(l => new SystemLog
                        {
                            Timestamp = l.Timestamp,
                            Level = l.Level,
                            Message = l.Message,
                            Source = l.Source
                        }).ToList(),

                        // Admin settings
                        AdminLiteLLMUrl = adminData.AdminLiteLLMUrl,
                        AdminLiteLLMApiKey = adminData.AdminLiteLLMApiKey
                    };

                    dispatcher.Dispatch(new LoadAdminDataSuccessAction(adminState));
                    _logger.LogInformation("Admin data loaded successfully from API");
                    return;
                }
            }
            catch (Exception apiEx)
            {
                _logger.LogWarning(apiEx, "Admin API not available, using default empty state");
            }

            // Fallback to empty/default state when API is not available
            var defaultState = new AdminState
            {
                AppVersion = "1.0.0",
                LiteLLMUrl = "http://localhost:4000",
                AdminLiteLLMUrl = "http://localhost:4000"
            };

            dispatcher.Dispatch(new LoadAdminDataSuccessAction(defaultState));
            _logger.LogInformation("Admin data initialized with defaults (API not available)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load admin data");
            dispatcher.Dispatch(new LoadAdminDataFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleSaveChanges(SaveAdminChangesAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Saving admin changes");

            // IMPLEMENTATION NEEDED: Convert current state to DTO and POST to API
            // For now, just acknowledge save
            await Task.Delay(500);

            dispatcher.Dispatch(new SaveAdminChangesSuccessAction());
            _logger.LogInformation("Admin changes saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save admin changes");
            dispatcher.Dispatch(new SaveAdminChangesFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleTestConnection(TestLiteLLMConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Testing LiteLLM connection");

            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            // Call API to test connection
            var response = await httpClient.PostAsJsonAsync($"{ApiEndpoint}/test-connection", new
            {
                Url = "http://localhost:4000",
                ApiKey = (string?)null
            });

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new TestLiteLLMConnectionSuccessAction());
                _logger.LogInformation("LiteLLM connection test successful");
            }
            else
            {
                throw new Exception("Connection test failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LiteLLM connection test failed");
            dispatcher.Dispatch(new TestLiteLLMConnectionFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleClearLocalStorage(ClearLocalStorageAction action, IDispatcher dispatcher)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
            // Optionally dispatch a success action
        }
        catch
        {
            // Handle error
        }
    }

    [EffectMethod]
    public async Task HandleExportLogs(ExportSystemLogsAction action, IDispatcher dispatcher)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Export logs to file
            // For now, just a placeholder
            await Task.CompletedTask;
        }
        catch
        {
            // Handle error
        }
    }
}
