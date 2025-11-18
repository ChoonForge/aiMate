using System.Reflection;
using AiMate.Core.Entities;
using AiMate.Core.Plugins;
using AiMate.Core.Services;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Manages plugin lifecycle and execution
/// </summary>
public class PluginManager : IPluginManager
{
    private readonly ILogger<PluginManager> _logger;
    private readonly Dictionary<string, IPlugin> _loadedPlugins = new();
    private readonly List<IMessageInterceptor> _messageInterceptors = new();
    private readonly List<IUIExtension> _uiExtensions = new();
    private readonly List<IToolProvider> _toolProviders = new();

    public event EventHandler<PluginEventArgs>? PluginLoaded;
    public event EventHandler<PluginEventArgs>? PluginUnloaded;
    public event EventHandler<PluginErrorEventArgs>? PluginError;

    public PluginManager(ILogger<PluginManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Load plugins from assemblies
    /// </summary>
    public async Task LoadPluginsAsync()
    {
        _logger.LogInformation("Loading plugins from assemblies");

        try
        {
            // Find all plugin types in loaded assemblies
            var pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return Array.Empty<Type>();
                    }
                })
                .Where(t => typeof(IPlugin).IsAssignableFrom(t)
                         && !t.IsInterface
                         && !t.IsAbstract
                         && t.GetConstructor(Type.EmptyTypes) != null); // Must have parameterless constructor

            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
                    await RegisterPluginAsync(plugin);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load plugin {PluginType}", pluginType.Name);
                    PluginError?.Invoke(this, new PluginErrorEventArgs(pluginType.Name, ex));
                }
            }

            _logger.LogInformation("Loaded {Count} plugins", _loadedPlugins.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load plugins");
        }
    }

    /// <summary>
    /// Register a plugin instance
    /// </summary>
    public async Task RegisterPluginAsync(IPlugin plugin)
    {
        if (_loadedPlugins.ContainsKey(plugin.Id))
        {
            _logger.LogWarning("Plugin {PluginId} already loaded", plugin.Id);
            return;
        }

        try
        {
            await plugin.InitializeAsync();
            _loadedPlugins[plugin.Id] = plugin;

            // Register specialized interfaces
            if (plugin is IMessageInterceptor interceptor)
                _messageInterceptors.Add(interceptor);

            if (plugin is IUIExtension uiExtension)
                _uiExtensions.Add(uiExtension);

            if (plugin is IToolProvider toolProvider)
                _toolProviders.Add(toolProvider);

            _logger.LogInformation(
                "Registered plugin: {PluginName} v{Version} by {Author} (Category: {Category})",
                plugin.Name, plugin.Version, plugin.Author, plugin.Category
            );

            PluginLoaded?.Invoke(this, new PluginEventArgs(plugin));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register plugin {PluginId}", plugin.Id);
            PluginError?.Invoke(this, new PluginErrorEventArgs(plugin.Id, ex));
            throw;
        }
    }

    /// <summary>
    /// Unload a plugin
    /// </summary>
    public async Task UnloadPluginAsync(string pluginId)
    {
        if (!_loadedPlugins.TryGetValue(pluginId, out var plugin))
        {
            _logger.LogWarning("Plugin {PluginId} not found", pluginId);
            return;
        }

        try
        {
            await plugin.DisposeAsync();

            _messageInterceptors.RemoveAll(p => p.Id == pluginId);
            _uiExtensions.RemoveAll(p => p.Id == pluginId);
            _toolProviders.RemoveAll(p => p.Id == pluginId);
            _loadedPlugins.Remove(pluginId);

            _logger.LogInformation("Unloaded plugin: {PluginId}", pluginId);
            PluginUnloaded?.Invoke(this, new PluginEventArgs(plugin));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unload plugin {PluginId}", pluginId);
            PluginError?.Invoke(this, new PluginErrorEventArgs(pluginId, ex));
            throw;
        }
    }

    /// <summary>
    /// Enable/disable a plugin
    /// </summary>
    public async Task SetPluginEnabledAsync(string pluginId, bool enabled)
    {
        if (!_loadedPlugins.TryGetValue(pluginId, out var plugin))
        {
            _logger.LogWarning("Plugin {PluginId} not found", pluginId);
            return;
        }

        plugin.IsEnabled = enabled;
        _logger.LogInformation("Plugin {PluginId} {Status}", pluginId, enabled ? "enabled" : "disabled");

        await Task.CompletedTask;
    }

    #region Message Interception

    public async Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context)
    {
        var result = new MessageInterceptResult { ModifiedMessage = message };

        foreach (var interceptor in _messageInterceptors.Where(i => i.IsEnabled).OrderBy(i => i.Name))
        {
            try
            {
                var interceptResult = await interceptor.OnBeforeSendAsync(
                    result.ModifiedMessage ?? message,
                    context
                );

                if (!interceptResult.Continue)
                {
                    _logger.LogInformation(
                        "Message intercepted by {Plugin}: {Reason}",
                        interceptor.Name, interceptResult.CancelReason
                    );
                    return interceptResult;
                }

                if (interceptResult.ModifiedMessage != null)
                    result.ModifiedMessage = interceptResult.ModifiedMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plugin {Plugin} failed in OnBeforeSend", interceptor.Name);
                PluginError?.Invoke(this, new PluginErrorEventArgs(interceptor.Id, ex));
            }
        }

        return result;
    }

    public async Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context)
    {
        var result = new MessageInterceptResult { ModifiedMessage = message };

        foreach (var interceptor in _messageInterceptors.Where(i => i.IsEnabled).OrderBy(i => i.Name))
        {
            try
            {
                var interceptResult = await interceptor.OnAfterReceiveAsync(
                    result.ModifiedMessage ?? message,
                    context
                );

                if (interceptResult.ModifiedMessage != null)
                    result.ModifiedMessage = interceptResult.ModifiedMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plugin {Plugin} failed in OnAfterReceive", interceptor.Name);
                PluginError?.Invoke(this, new PluginErrorEventArgs(interceptor.Id, ex));
            }
        }

        return result;
    }

    #endregion

    #region UI Extensions

    public IEnumerable<MessageActionButton> GetMessageActions(Message message)
    {
        var actions = new List<MessageActionButton>();

        foreach (var extension in _uiExtensions.Where(e => e.IsEnabled))
        {
            try
            {
                actions.AddRange(extension.GetMessageActions(message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plugin {Plugin} failed to get message actions", extension.Name);
            }
        }

        return actions.OrderBy(a => a.Order);
    }

    public IEnumerable<ChatInputExtension> GetInputExtensions()
    {
        var extensions = new List<ChatInputExtension>();

        foreach (var extension in _uiExtensions.Where(e => e.IsEnabled))
        {
            try
            {
                extensions.AddRange(extension.GetInputExtensions());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plugin {Plugin} failed to get input extensions", extension.Name);
            }
        }

        return extensions.OrderBy(e => e.Order);
    }

    public Dictionary<string, PluginSettings> GetAllPluginSettings()
    {
        var settings = new Dictionary<string, PluginSettings>();

        foreach (var extension in _uiExtensions.Where(e => e.IsEnabled))
        {
            try
            {
                var pluginSettings = extension.GetSettingsUI();
                if (pluginSettings != null)
                    settings[extension.Id] = pluginSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plugin {Plugin} failed to get settings", extension.Name);
            }
        }

        return settings;
    }

    #endregion

    #region Tool Execution

    public IEnumerable<PluginTool> GetAllTools()
    {
        var tools = new List<PluginTool>();

        foreach (var provider in _toolProviders.Where(p => p.IsEnabled))
        {
            try
            {
                tools.AddRange(provider.GetTools());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plugin {Plugin} failed to get tools", provider.Name);
            }
        }

        return tools;
    }

    public async Task<ToolResult> ExecuteToolAsync(string pluginId, string toolName, Dictionary<string, object> parameters)
    {
        var provider = _toolProviders.FirstOrDefault(p => p.Id == pluginId && p.IsEnabled);
        if (provider == null)
        {
            return new ToolResult
            {
                Success = false,
                Error = $"Plugin {pluginId} not found or not enabled"
            };
        }

        try
        {
            _logger.LogInformation("Executing tool {Tool} from plugin {Plugin}", toolName, pluginId);
            return await provider.ExecuteToolAsync(toolName, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool execution failed: {Tool} in {Plugin}", toolName, pluginId);
            return new ToolResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    #endregion

    public IEnumerable<IPlugin> GetLoadedPlugins() => _loadedPlugins.Values;

    public IPlugin? GetPlugin(string pluginId) =>
        _loadedPlugins.TryGetValue(pluginId, out var plugin) ? plugin : null;
}
