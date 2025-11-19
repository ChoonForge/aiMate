namespace AiMate.Core.Plugins;

/// <summary>
/// Base interface that all plugins must implement
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Unique plugin identifier (e.g., "message-rating", "code-copy")
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display name shown in UI
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Plugin description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Plugin version (semver)
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Plugin author
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Icon (Material Design icon name)
    /// </summary>
    string Icon { get; }

    /// <summary>
    /// Plugin category
    /// </summary>
    PluginCategory Category { get; }

    /// <summary>
    /// Is plugin enabled?
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Called when plugin is loaded
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Called when plugin is unloaded
    /// </summary>
    Task DisposeAsync();
}

/// <summary>
/// Plugin categories for organization
/// </summary>
public enum PluginCategory
{
    MessageActions,   // Buttons on messages (copy, share, etc.)
    InputExtensions,  // Add UI to chat input
    Interceptors,     // Modify messages before/after LLM
    Tools,            // Provide callable tools
    Analytics,        // Track & analyze usage
    Integration,      // External integrations
    UI,              // General UI enhancements
    Safety,          // Content filtering, safety
    Other
}
