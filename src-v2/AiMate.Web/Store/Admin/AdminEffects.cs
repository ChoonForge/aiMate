using Fluxor;
using Microsoft.JSInterop;

namespace AiMate.Web.Store.Admin;

public class AdminEffects
{
    private readonly IJSRuntime _jsRuntime;

    public AdminEffects(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    [EffectMethod]
    public async Task HandleLoadData(LoadAdminDataAction action, IDispatcher dispatcher)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Load actual admin data from API or services
            // For now, using mock data

            var adminState = new AdminState
            {
                // Mock statistics
                TotalUsers = 1,
                TotalConversations = 42,
                ConversationsToday = 5,
                TotalModels = 3,
                ActiveModels = 3,
                TotalMcpServers = 2,
                ConnectedMcpServers = 1,

                // System health
                LiteLLMConnected = true,
                LiteLLMUrl = "http://localhost:4000",
                StorageUsedMB = 15.7,
                StorageLimitMB = 50.0,
                Uptime = "2h 34m",
                AppVersion = "v1.0.0",

                // Mock models
                Models = new List<AIModelConfig>
                {
                    new() { Id = "gpt-4", Name = "GPT-4", Provider = "OpenAI", IsEnabled = true, MaxTokens = 8192 },
                    new() { Id = "claude-3-5-sonnet-20241022", Name = "Claude 3.5 Sonnet", Provider = "Anthropic", IsEnabled = true, MaxTokens = 8192 },
                    new() { Id = "gpt-3.5-turbo", Name = "GPT-3.5 Turbo", Provider = "OpenAI", IsEnabled = true, MaxTokens = 4096 }
                },

                // Mock MCP servers
                McpServers = new List<MCPServerConfig>
                {
                    new() { Id = "fs-1", Name = "Filesystem", Type = "stdio", Connected = true, ToolCount = 8, Command = "npx", Arguments = "@modelcontextprotocol/server-filesystem /home/user/workspace" },
                    new() { Id = "web-1", Name = "Web Search", Type = "http", Connected = false, ToolCount = 3, Url = "http://localhost:8080/mcp" }
                },

                // Storage stats
                LocalStorageUsedMB = 12.3,
                LocalStorageLimitMB = 50.0,
                IndexedDBUsedMB = 3.4,
                IndexedDBLimitMB = 500.0,

                // System logs
                SystemLogs = new List<SystemLog>
                {
                    new() { Timestamp = DateTime.Now.AddMinutes(-5), Level = "INFO", Message = "Application started", Source = "System" },
                    new() { Timestamp = DateTime.Now.AddMinutes(-4), Level = "INFO", Message = "Connected to LiteLLM at http://localhost:4000", Source = "LiteLLM" },
                    new() { Timestamp = DateTime.Now.AddMinutes(-3), Level = "INFO", Message = "Loaded 3 models from configuration", Source = "Models" },
                    new() { Timestamp = DateTime.Now.AddMinutes(-2), Level = "WARNING", Message = "Model 'gpt-4' approaching rate limit", Source = "Models" },
                    new() { Timestamp = DateTime.Now.AddMinutes(-1), Level = "INFO", Message = "Admin panel accessed", Source = "Admin" }
                }
            };

            await Task.Delay(500); // Simulate network delay
            dispatcher.Dispatch(new LoadAdminDataSuccessAction(adminState));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadAdminDataFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleSaveChanges(SaveAdminChangesAction action, IDispatcher dispatcher)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Save admin changes to API or services
            // For now, just simulating save

            await Task.Delay(1000); // Simulate network delay
            dispatcher.Dispatch(new SaveAdminChangesSuccessAction());
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SaveAdminChangesFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleTestConnection(TestLiteLLMConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Actually test connection to LiteLLM
            // For now, simulating successful connection

            await Task.Delay(1000);
            dispatcher.Dispatch(new TestLiteLLMConnectionSuccessAction());
        }
        catch (Exception ex)
        {
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
