using AiMate.Core.Entities;
using AiMate.Core.Plugins;

namespace AiMate.Core.Services;

/// <summary>
/// Manages plugin lifecycle and execution
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// Load plugins from assemblies
    /// </summary>
    Task LoadPluginsAsync();

    /// <summary>
    /// Register a plugin instance
    /// </summary>
    Task RegisterPluginAsync(IPlugin plugin);

    /// <summary>
    /// Unload a plugin
    /// </summary>
    Task UnloadPluginAsync(string pluginId);

    /// <summary>
    /// Enable/disable a plugin
    /// </summary>
    Task SetPluginEnabledAsync(string pluginId, bool enabled);

    /// <summary>
    /// Get all loaded plugins
    /// </summary>
    IEnumerable<IPlugin> GetLoadedPlugins();

    /// <summary>
    /// Get plugin by ID
    /// </summary>
    IPlugin? GetPlugin(string pluginId);

    // Message Interception
    Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context);
    Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context);

    // UI Extensions
    IEnumerable<MessageActionButton> GetMessageActions(Message message);
    IEnumerable<ChatInputExtension> GetInputExtensions();
    Dictionary<string, PluginSettings> GetAllPluginSettings();

    // Tool Execution
    IEnumerable<PluginTool> GetAllTools();
    Task<ToolResult> ExecuteToolAsync(string pluginId, string toolName, Dictionary<string, object> parameters);

    // Events
    event EventHandler<PluginEventArgs>? PluginLoaded;
    event EventHandler<PluginEventArgs>? PluginUnloaded;
    event EventHandler<PluginErrorEventArgs>? PluginError;
}

public class PluginEventArgs : EventArgs
{
    public IPlugin Plugin { get; }
    public PluginEventArgs(IPlugin plugin) => Plugin = plugin;
}

public class PluginErrorEventArgs : EventArgs
{
    public string PluginId { get; }
    public Exception Exception { get; }
    public PluginErrorEventArgs(string pluginId, Exception exception)
    {
        PluginId = pluginId;
        Exception = exception;
    }
}
