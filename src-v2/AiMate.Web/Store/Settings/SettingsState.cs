using Fluxor;

namespace AiMate.Web.Store.Settings;

/// <summary>
/// Settings state - all user preferences and configuration
/// </summary>
[FeatureState]
public record SettingsState
{
    // General Settings
    public string Language { get; init; } = "en-NZ";
    public string TimeZone { get; init; } = "Pacific/Auckland";
    public bool EnableNotifications { get; init; } = true;
    public bool EnableSoundEffects { get; init; } = false;

    // Interface Settings
    public string Theme { get; init; } = "dark";
    public int FontSize { get; init; } = 14;
    public bool CompactMode { get; init; } = false;
    public bool ShowLineNumbers { get; init; } = true;
    public bool EnableMarkdownPreview { get; init; } = true;
    public string CodeTheme { get; init; } = "dracula";

    // Connection Settings
    public string LiteLLMUrl { get; init; } = "http://localhost:4000";
    public string? ApiKey { get; init; }
    public int RequestTimeout { get; init; } = 120;
    public int MaxRetries { get; init; } = 3;
    public bool UseStreamingByDefault { get; init; } = true;

    // Personalisation Settings
    public string DefaultPersonality { get; init; } = "KiwiMate";
    public string DefaultModel { get; init; } = "gpt-4";
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 2000;
    public string SystemPromptOverride { get; init; } = string.Empty;

    // Account Settings
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? AvatarUrl { get; init; }
    public string UserTier { get; init; } = "Free";

    // Usage Settings
    public bool TrackUsage { get; init; } = true;
    public bool ShowCostEstimates { get; init; } = true;
    public int MonthlyBudget { get; init; } = 0; // 0 = unlimited
    public bool EnableUsageAlerts { get; init; } = true;

    // UI State
    public bool IsLoading { get; init; }
    public bool IsSaving { get; init; }
    public string? Error { get; init; }
    public int ActiveTab { get; init; } = 0;
}
