namespace AiMate.Web.Store.Plugin;

// Load Plugins
public record LoadPluginsAction();
public record LoadPluginsSuccessAction(List<PluginInfo> Plugins);
public record LoadPluginsFailureAction(string Error);

// Enable/Disable Plugin
public record TogglePluginAction(string PluginId);
public record TogglePluginSuccessAction(string PluginId, bool IsEnabled);
public record TogglePluginFailureAction(string Error);

// Plugin Settings
public record OpenPluginSettingsAction(string PluginId);

// Clear Error
public record ClearPluginErrorAction();
