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
    public string? ModelId { get; set; }
    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }
}

/// <summary>
/// Create conversation request
/// </summary>
public class CreateConversationRequest
{
    public required int WorkspaceId { get; set; }
    public string Title { get; set; } = "New Conversation";
}

// ============================================================================
// NOTES DTOs
// ============================================================================

/// <summary>
/// Note DTO - for personal notes and knowledge capture
/// </summary>
public class NoteDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "markdown"; // markdown, plain, html

    // Organization
    public List<string> Tags { get; set; } = new();
    public string? Collection { get; set; }
    public string? Category { get; set; }
    public string? Color { get; set; } // For visual organization

    // Metadata
    public bool IsPinned { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsArchived { get; set; }

    // Ownership & Access
    public string? OwnerId { get; set; }
    public string Visibility { get; set; } = "Private"; // Private, Shared, Public
    public List<string> SharedWith { get; set; } = new(); // User IDs or group IDs

    // Relations
    public string? LinkedConversationId { get; set; }
    public string? LinkedWorkspaceId { get; set; }
    public List<string> Attachments { get; set; } = new(); // File IDs

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastViewedAt { get; set; }
}

/// <summary>
/// Create note request
/// </summary>
public class CreateNoteRequest
{
    public required string Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "markdown";
    public List<string>? Tags { get; set; }
    public string? Collection { get; set; }
    public string? Category { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// Update note request
/// </summary>
public class UpdateNoteRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ContentType { get; set; }
    public List<string>? Tags { get; set; }
    public string? Collection { get; set; }
    public string? Category { get; set; }
    public string? Color { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsFavorite { get; set; }
    public bool? IsArchived { get; set; }
}

// ============================================================================
// KNOWLEDGE BASE DTOs
// ============================================================================

/// <summary>
/// Knowledge article DTO - for reference materials and documentation
/// </summary>
public class KnowledgeArticleDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "markdown"; // markdown, plain, html
    public string Summary { get; set; } = string.Empty;

    // Organization
    public string Type { get; set; } = "Article"; // Article, Guide, Reference, Tutorial, FAQ
    public List<string> Tags { get; set; } = new();
    public string? Collection { get; set; }
    public string? Category { get; set; }
    public string? Language { get; set; } = "en";

    // Analytics
    public int ViewCount { get; set; }
    public int ReferenceCount { get; set; } // How many times referenced in chats/notes
    public int UpvoteCount { get; set; }
    public int DownvoteCount { get; set; }
    public double AverageRating { get; set; }
    public DateTime? LastViewedAt { get; set; }

    // Metadata
    public string? Author { get; set; }
    public string? Source { get; set; } // URL or reference to original source
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; } = true;
    public bool IsVerified { get; set; } // Verified by admin/expert

    // Ownership & Access
    public string? OwnerId { get; set; }
    public string Visibility { get; set; } = "Public"; // Private, Team, Public
    public List<string> SharedWith { get; set; } = new();

    // Relations
    public List<string> RelatedArticles { get; set; } = new(); // Related article IDs
    public List<string> Attachments { get; set; } = new();

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// Create knowledge article request
/// </summary>
public class CreateKnowledgeArticleRequest
{
    public required string Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "markdown";
    public string Summary { get; set; } = string.Empty;
    public string Type { get; set; } = "Article";
    public List<string>? Tags { get; set; }
    public string? Collection { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// Update knowledge article request
/// </summary>
public class UpdateKnowledgeArticleRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ContentType { get; set; }
    public string? Summary { get; set; }
    public string? Type { get; set; }
    public List<string>? Tags { get; set; }
    public string? Collection { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsPublished { get; set; }
    public bool? IsVerified { get; set; }
}

/// <summary>
/// Knowledge base analytics DTO
/// </summary>
public class KnowledgeAnalyticsDto
{
    public int TotalArticles { get; set; }
    public int TotalViews { get; set; }
    public int TotalReferences { get; set; }
    public List<KnowledgeArticleDto> MostViewed { get; set; } = new();
    public List<KnowledgeArticleDto> MostReferenced { get; set; } = new();
    public List<KnowledgeArticleDto> RecentlyAdded { get; set; } = new();
    public Dictionary<string, int> TagCounts { get; set; } = new();
    public Dictionary<string, int> TypeCounts { get; set; } = new();
    public Dictionary<string, int> CategoryCounts { get; set; } = new();
}

// ============================================================================
// PROJECTS DTOs
// ============================================================================

public class ProjectDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerEmail { get; set; }
    public string? Owner { get; set; }
    public string Status { get; set; } = "Planning";
    public string Priority { get; set; } = "Medium";
    public decimal? Budget { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProgressPercent { get; set; } = 0;
    public List<string> Tags { get; set; } = new();
    public List<string> TeamMembers { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateProjectRequest
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? OwnerEmail { get; set; }
    public string? Owner { get; set; }
    public string Status { get; set; } = "Planning";
    public string Priority { get; set; } = "Medium";
    public decimal? Budget { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProgressPercent { get; set; } = 0;
    public List<string>? Tags { get; set; }
    public List<string>? TeamMembers { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public decimal? Budget { get; set; }
    public int? ProgressPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Owner { get; set; }
    public List<string>? TeamMembers { get; set; }
    public List<string>? Tags { get; set; }
    public string? Notes { get; set; }
}
