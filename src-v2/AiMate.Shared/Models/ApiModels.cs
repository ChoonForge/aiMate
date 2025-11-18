namespace AiMate.Shared.Models;

// ============================================================================
// SETTINGS DTOs
// ============================================================================

/// <summary>
/// User settings request/response
/// </summary>
public class UserSettingsDto
{
    // General Settings
    public string Language { get; set; } = "en-NZ";
    public string TimeZone { get; set; } = "Pacific/Auckland";
    public bool EnableNotifications { get; set; } = true;
    public bool EnableSoundEffects { get; set; } = false;

    // Interface Settings
    public string Theme { get; set; } = "dark";
    public int FontSize { get; set; } = 14;
    public bool CompactMode { get; set; } = false;
    public bool ShowLineNumbers { get; set; } = true;
    public bool EnableMarkdownPreview { get; set; } = true;
    public string CodeTheme { get; set; } = "dracula";

    // Connection Settings
    public string LiteLLMUrl { get; set; } = "http://localhost:4000";
    public string? ApiKey { get; set; }
    public int RequestTimeout { get; set; } = 120;
    public int MaxRetries { get; set; } = 3;
    public bool UseStreamingByDefault { get; set; } = true;

    // Personalisation Settings
    public string DefaultPersonality { get; set; } = "KiwiMate";
    public string DefaultModel { get; set; } = "gpt-4";
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2000;
    public string SystemPromptOverride { get; set; } = string.Empty;

    // Account Settings
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string UserTier { get; set; } = "Free";

    // Usage Settings
    public bool TrackUsage { get; set; } = true;
    public bool ShowCostEstimates { get; set; } = true;
    public int MonthlyBudget { get; set; } = 0;
    public bool EnableUsageAlerts { get; set; } = true;
}

// ============================================================================
// ADMIN DTOs
// ============================================================================

/// <summary>
/// Admin overview statistics
/// </summary>
public class AdminOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalConversations { get; set; }
    public int ConversationsToday { get; set; }
    public int ActiveModels { get; set; }
    public int TotalModels { get; set; }
    public int ConnectedMcpServers { get; set; }
    public int TotalMcpServers { get; set; }
    public bool LiteLLMConnected { get; set; }
    public string LiteLLMUrl { get; set; } = string.Empty;
    public double StorageUsedMB { get; set; }
    public double StorageLimitMB { get; set; }
    public string Uptime { get; set; } = string.Empty;
    public string AppVersion { get; set; } = "v1.0.0";
    public double LocalStorageUsedMB { get; set; }
    public double LocalStorageLimitMB { get; set; }
    public double IndexedDBUsedMB { get; set; }
    public double IndexedDBLimitMB { get; set; }
}

/// <summary>
/// AI Model configuration
/// </summary>
public class AIModelDto
{
    // General
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Connection { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? CustomEndpoint { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Model Parameters
    public int ContextWindow { get; set; } = 8192;
    public int MaxTokens { get; set; } = 4096;
    public double Temperature { get; set; } = 0.7;
    public double TopP { get; set; } = 1.0;
    public double FrequencyPenalty { get; set; } = 0.0;
    public double PresencePenalty { get; set; } = 0.0;

    // Capabilities
    public bool SupportsVision { get; set; } = false;
    public bool SupportsWebSearch { get; set; } = false;
    public bool SupportsFileUpload { get; set; } = false;
    public bool SupportsImageGeneration { get; set; } = false;

    // Ownership & Access
    public string? OwnerId { get; set; }
    public string? Visibility { get; set; } = "Private";
    public List<string> AllowedGroups { get; set; } = new();
}

/// <summary>
/// LLM Provider Connection (OpenAI, Anthropic, etc.)
/// </summary>
public class ProviderConnectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Cloud"; // Cloud or Local
    public string Url { get; set; } = string.Empty;
    public string Auth { get; set; } = "None"; // None, Bearer, ApiKey, OAuth
    public string? AuthToken { get; set; }
    public string? Headers { get; set; } // JSON format
    public string? PrefixId { get; set; }
    public string ProviderType { get; set; } = "OpenAI"; // OpenAI, Anthropic, Local, etc.
    public List<string> ModelIds { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IsEnabled { get; set; } = true;

    // Ownership & Visibility
    public string? OwnerId { get; set; }
    public string Visibility { get; set; } = "Private";
    public List<string> AllowedGroups { get; set; } = new();
}

/// <summary>
/// MCP Server configuration
/// </summary>
public class MCPServerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "stdio";
    public bool Connected { get; set; }
    public int ToolCount { get; set; }
    public string? Command { get; set; }
    public string? Arguments { get; set; }
    public string? Url { get; set; }

    // Ownership & Visibility
    public string? OwnerId { get; set; }
    public string Visibility { get; set; } = "Private";
    public List<string> AllowedGroups { get; set; } = new();
}

/// <summary>
/// System log entry
/// </summary>
public class SystemLogDto
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "INFO";
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
}

/// <summary>
/// Admin data response (all admin data in one)
/// </summary>
public class AdminDataDto
{
    public AdminOverviewDto Overview { get; set; } = new();
    public List<AIModelDto> Models { get; set; } = new();
    public List<MCPServerDto> McpServers { get; set; } = new();
    public List<SystemLogDto> SystemLogs { get; set; } = new();
    public string AdminLiteLLMUrl { get; set; } = "http://localhost:4000";
    public string? AdminLiteLLMApiKey { get; set; }
}

// ============================================================================
// WORKSPACE DTOs
// ============================================================================

/// <summary>
/// Workspace DTO
/// </summary>
public class WorkspaceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "General";
    public string Personality { get; set; } = "KiwiMate";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Create workspace request
/// </summary>
public class CreateWorkspaceRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = "General";
    public string Personality { get; set; } = "KiwiMate";
}

/// <summary>
/// Update workspace request
/// </summary>
public class UpdateWorkspaceRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Personality { get; set; }
}

// ============================================================================
// CONVERSATION DTOs
// ============================================================================

/// <summary>
/// Conversation DTO
/// </summary>
public class ConversationDto
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
}

/// <summary>
/// Create conversation request
/// </summary>
public class CreateConversationRequest
{
    public required int WorkspaceId { get; set; }
    public string Title { get; set; } = "New Conversation";
}
