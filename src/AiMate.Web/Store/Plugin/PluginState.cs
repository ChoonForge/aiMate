using AiMate.Core.Plugins;
using Fluxor;

namespace AiMate.Web.Store.Plugin;

/// <summary>
/// Plugin management state
/// </summary>
[FeatureState]
public record PluginState
{
    public List<PluginInfo> LoadedPlugins { get; init; } = new();
    public bool IsLoading { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// Plugin info for UI display
/// </summary>
public record PluginInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public PluginCategory Category { get; init; }
    public bool IsEnabled { get; init; }
    public bool HasSettings { get; init; }
    public int ActionCount { get; init; }
    public int ToolCount { get; init; }
}
