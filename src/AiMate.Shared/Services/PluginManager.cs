using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AiMate.Shared.Models;
using AiMate.Shared.Plugins;
using Microsoft.Extensions.Logging;

namespace AiMate.Shared.Services
{
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
        /// Load plugins from assemblies in the plugins directory
        /// </summary>
        public async Task LoadPluginsAsync(string pluginsDirectory)
        {
            _logger.LogInformation("Loading plugins from {Directory}", pluginsDirectory);

            try
            {
                // In Blazor WASM, we'll use reflection to find plugins in referenced assemblies
                var pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

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

            await plugin.InitializeAsync();
            _loadedPlugins[plugin.Id] = plugin;

            // Register specialized interfaces
            if (plugin is IMessageInterceptor interceptor)
                _messageInterceptors.Add(interceptor);

            if (plugin is IUIExtension uiExtension)
                _uiExtensions.Add(uiExtension);

            if (plugin is IToolProvider toolProvider)
                _toolProviders.Add(toolProvider);

            _logger.LogInformation("Registered plugin: {PluginName} v{Version}", plugin.Name, plugin.Version);
            PluginLoaded?.Invoke(this, new PluginEventArgs(plugin));
        }

        /// <summary>
        /// Unload a plugin
        /// </summary>
        public async Task UnloadPluginAsync(string pluginId)
        {
            if (!_loadedPlugins.TryGetValue(pluginId, out var plugin))
                return;

            await plugin.DisposeAsync();

            _messageInterceptors.RemoveAll(p => p.Id == pluginId);
            _uiExtensions.RemoveAll(p => p.Id == pluginId);
            _toolProviders.RemoveAll(p => p.Id == pluginId);
            _loadedPlugins.Remove(pluginId);

            _logger.LogInformation("Unloaded plugin: {PluginId}", pluginId);
            PluginUnloaded?.Invoke(this, new PluginEventArgs(plugin));
        }

        #region Message Interception

        public async Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context)
        {
            var result = new MessageInterceptResult { ModifiedMessage = message };

            foreach (var interceptor in _messageInterceptors.OrderBy(i => i.Name))
            {
                try
                {
                    var interceptResult = await interceptor.OnBeforeSendAsync(result.ModifiedMessage ?? message, context);
                    
                    if (!interceptResult.Continue)
                    {
                        _logger.LogInformation("Message intercepted by {Plugin}: {Reason}", 
                            interceptor.Name, interceptResult.CancelReason);
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

            foreach (var interceptor in _messageInterceptors.OrderBy(i => i.Name))
            {
                try
                {
                    var interceptResult = await interceptor.OnAfterReceiveAsync(result.ModifiedMessage ?? message, context);
                    
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

            foreach (var extension in _uiExtensions)
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

            return actions;
        }

        public IEnumerable<ChatInputExtension> GetInputExtensions()
        {
            var extensions = new List<ChatInputExtension>();

            foreach (var extension in _uiExtensions)
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

            foreach (var extension in _uiExtensions)
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

            foreach (var provider in _toolProviders)
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
            var provider = _toolProviders.FirstOrDefault(p => p.Id == pluginId);
            if (provider == null)
            {
                return new ToolResult
                {
                    Success = false,
                    Error = $"Plugin {pluginId} not found"
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

    public interface IPluginManager
    {
        Task LoadPluginsAsync(string pluginsDirectory);
        Task RegisterPluginAsync(IPlugin plugin);
        Task UnloadPluginAsync(string pluginId);
        
        Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context);
        Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context);
        
        IEnumerable<MessageActionButton> GetMessageActions(Message message);
        IEnumerable<ChatInputExtension> GetInputExtensions();
        Dictionary<string, PluginSettings> GetAllPluginSettings();
        
        IEnumerable<PluginTool> GetAllTools();
        Task<ToolResult> ExecuteToolAsync(string pluginId, string toolName, Dictionary<string, object> parameters);
        
        IEnumerable<IPlugin> GetLoadedPlugins();
        IPlugin? GetPlugin(string pluginId);

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
}
