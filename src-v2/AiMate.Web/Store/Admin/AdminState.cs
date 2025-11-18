using Fluxor;

namespace AiMate.Web.Store.Admin;

/// <summary>
/// Admin state - system administration and configuration
/// </summary>
[FeatureState]
public record AdminState
{
    // Overview Statistics
    public int TotalUsers { get; init; } = 1;
    public int TotalConversations { get; init; }
    public int ConversationsToday { get; init; }
    public int ActiveModels { get; init; }
    public int TotalModels { get; init; }
    public int ConnectedMcpServers { get; init; }
    public int TotalMcpServers { get; init; }

    // System Health
    public bool LiteLLMConnected { get; init; }
    public string LiteLLMUrl { get; init; } = "http://localhost:4000";
    public double StorageUsedMB { get; init; }
    public double StorageLimitMB { get; init; } = 50.0;
    public string Uptime { get; init; } = "0h 0m";
    public string AppVersion { get; init; } = "v1.0.0";

    // Models
    public List<AIModelConfig> Models { get; init; } = new();

    // MCP Servers
    public List<MCPServerConfig> McpServers { get; init; } = new();

    // Connection Settings (Admin-level)
    public string AdminLiteLLMUrl { get; init; } = "http://localhost:4000";
    public string? AdminLiteLLMApiKey { get; init; }

    // Storage Statistics
    public double LocalStorageUsedMB { get; init; }
    public double LocalStorageLimitMB { get; init; } = 50.0;
    public double IndexedDBUsedMB { get; init; }
    public double IndexedDBLimitMB { get; init; } = 500.0;

    // System Logs
    public List<SystemLog> SystemLogs { get; init; } = new();

    // UI State
    public bool IsLoading { get; init; }
    public bool IsSaving { get; init; }
    public string? Error { get; init; }
    public int ActiveTab { get; init; } = 0;
}

/// <summary>
/// AI Model configuration
/// </summary>
public record AIModelConfig
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public int MaxTokens { get; init; } = 4096;
    public string? Description { get; init; }
}

/// <summary>
/// MCP Server configuration
/// </summary>
public record MCPServerConfig
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "stdio"; // stdio, sse, http
    public bool Connected { get; init; }
    public int ToolCount { get; init; }
    public string? Command { get; init; }
    public string? Arguments { get; init; }
    public string? Url { get; init; }
}

/// <summary>
/// System log entry
/// </summary>
public record SystemLog
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string Level { get; init; } = "INFO"; // INFO, WARNING, ERROR
    public string Message { get; init; } = string.Empty;
    public string? Source { get; init; }
}
