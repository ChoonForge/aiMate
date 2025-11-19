using Fluxor;

namespace AiMate.Web.Store.Admin;

public static class AdminReducers
{
    // UI Reducers
    [ReducerMethod]
    public static AdminState SetActiveTab(AdminState state, SetActiveAdminTabAction action)
        => state with { ActiveTab = action.TabIndex };

    [ReducerMethod]
    public static AdminState ClearError(AdminState state, ClearAdminErrorAction action)
        => state with { Error = null };

    // Data Loading Reducers
    [ReducerMethod]
    public static AdminState LoadData(AdminState state, LoadAdminDataAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static AdminState LoadDataSuccess(AdminState state, LoadAdminDataSuccessAction action)
        => action.State with { IsLoading = false };

    [ReducerMethod]
    public static AdminState LoadDataFailure(AdminState state, LoadAdminDataFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    // Save Reducers
    [ReducerMethod]
    public static AdminState SaveChanges(AdminState state, SaveAdminChangesAction action)
        => state with { IsSaving = true, Error = null };

    [ReducerMethod]
    public static AdminState SaveChangesSuccess(AdminState state, SaveAdminChangesSuccessAction action)
        => state with { IsSaving = false };

    [ReducerMethod]
    public static AdminState SaveChangesFailure(AdminState state, SaveAdminChangesFailureAction action)
        => state with { IsSaving = false, Error = action.Error };

    // Model Management Reducers
    [ReducerMethod]
    public static AdminState AddModel(AdminState state, AddModelAction action)
    {
        var models = state.Models.ToList();
        models.Add(action.Model);
        return state with
        {
            Models = models,
            TotalModels = models.Count,
            ActiveModels = models.Count(m => m.IsEnabled)
        };
    }

    [ReducerMethod]
    public static AdminState UpdateModel(AdminState state, UpdateModelAction action)
    {
        var models = state.Models.Select(m =>
            m.Id == action.Model.Id ? action.Model : m
        ).ToList();

        return state with
        {
            Models = models,
            ActiveModels = models.Count(m => m.IsEnabled)
        };
    }

    [ReducerMethod]
    public static AdminState DeleteModel(AdminState state, DeleteModelAction action)
    {
        var models = state.Models.Where(m => m.Id != action.ModelId).ToList();
        return state with
        {
            Models = models,
            TotalModels = models.Count,
            ActiveModels = models.Count(m => m.IsEnabled)
        };
    }

    [ReducerMethod]
    public static AdminState ToggleModel(AdminState state, ToggleModelAction action)
    {
        var models = state.Models.Select(m =>
            m.Id == action.ModelId ? m with { IsEnabled = !m.IsEnabled } : m
        ).ToList();

        return state with
        {
            Models = models,
            ActiveModels = models.Count(m => m.IsEnabled)
        };
    }

    // MCP Server Reducers
    [ReducerMethod]
    public static AdminState AddMcpServer(AdminState state, AddMcpServerAction action)
    {
        var servers = state.McpServers.ToList();
        servers.Add(action.Server);
        return state with
        {
            McpServers = servers,
            TotalMcpServers = servers.Count,
            ConnectedMcpServers = servers.Count(s => s.Connected)
        };
    }

    [ReducerMethod]
    public static AdminState UpdateMcpServer(AdminState state, UpdateMcpServerAction action)
    {
        var servers = state.McpServers.Select(s =>
            s.Id == action.Server.Id ? action.Server : s
        ).ToList();

        return state with
        {
            McpServers = servers,
            ConnectedMcpServers = servers.Count(s => s.Connected)
        };
    }

    [ReducerMethod]
    public static AdminState DeleteMcpServer(AdminState state, DeleteMcpServerAction action)
    {
        var servers = state.McpServers.Where(s => s.Id != action.ServerId).ToList();
        return state with
        {
            McpServers = servers,
            TotalMcpServers = servers.Count,
            ConnectedMcpServers = servers.Count(s => s.Connected)
        };
    }

    // Connection Reducers
    [ReducerMethod]
    public static AdminState UpdateLiteLLMUrl(AdminState state, UpdateAdminLiteLLMUrlAction action)
        => state with { AdminLiteLLMUrl = action.Url };

    [ReducerMethod]
    public static AdminState UpdateLiteLLMApiKey(AdminState state, UpdateAdminLiteLLMApiKeyAction action)
        => state with { AdminLiteLLMApiKey = action.ApiKey };

    [ReducerMethod]
    public static AdminState TestConnectionSuccess(AdminState state, TestLiteLLMConnectionSuccessAction action)
        => state with { LiteLLMConnected = true };

    [ReducerMethod]
    public static AdminState TestConnectionFailure(AdminState state, TestLiteLLMConnectionFailureAction action)
        => state with { LiteLLMConnected = false, Error = action.Error };

    // System Log Reducers
    [ReducerMethod]
    public static AdminState RefreshLogs(AdminState state, RefreshSystemLogsAction action)
    {
        // In a real app, this would fetch fresh logs
        var newLog = new SystemLog
        {
            Timestamp = DateTime.Now,
            Level = "INFO",
            Message = "System logs refreshed",
            Source = "Admin Panel"
        };

        var logs = state.SystemLogs.ToList();
        logs.Insert(0, newLog);

        return state with { SystemLogs = logs.Take(100).ToList() }; // Keep last 100 logs
    }

    [ReducerMethod]
    public static AdminState ClearLogs(AdminState state, ClearSystemLogsAction action)
        => state with { SystemLogs = new List<SystemLog>() };
}
