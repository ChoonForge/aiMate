using Fluxor;

namespace AiMate.Web.Store.Plugin;

public static class PluginReducers
{
    [ReducerMethod]
    public static PluginState LoadPlugins(PluginState state, LoadPluginsAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static PluginState LoadPluginsSuccess(PluginState state, LoadPluginsSuccessAction action)
        => state with { LoadedPlugins = action.Plugins, IsLoading = false };

    [ReducerMethod]
    public static PluginState LoadPluginsFailure(PluginState state, LoadPluginsFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static PluginState TogglePluginSuccess(PluginState state, TogglePluginSuccessAction action)
    {
        var plugins = state.LoadedPlugins.Select(p =>
            p.Id == action.PluginId ? p with { IsEnabled = action.IsEnabled } : p
        ).ToList();

        return state with { LoadedPlugins = plugins };
    }

    [ReducerMethod]
    public static PluginState ClearError(PluginState state, ClearPluginErrorAction action)
        => state with { Error = null };
}
