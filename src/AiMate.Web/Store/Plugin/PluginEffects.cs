using System.Net.Http.Json;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace AiMate.Web.Store.Plugin;

public class PluginEffects
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PluginEffects> _logger;

    public PluginEffects(HttpClient httpClient, ILogger<PluginEffects> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadPlugins(LoadPluginsAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Loading plugins from API");

            var plugins = await _httpClient.GetFromJsonAsync<List<PluginInfo>>("/api/v1/plugins");

            if (plugins != null)
            {
                dispatcher.Dispatch(new LoadPluginsSuccessAction(plugins));
                _logger.LogInformation("Loaded {Count} plugins", plugins.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load plugins");
            dispatcher.Dispatch(new LoadPluginsFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleTogglePlugin(TogglePluginAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Toggling plugin {PluginId}", action.PluginId);

            var response = await _httpClient.PostAsync($"/api/v1/plugins/{action.PluginId}/toggle", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TogglePluginResult>();
                if (result != null)
                {
                    dispatcher.Dispatch(new TogglePluginSuccessAction(action.PluginId, result.IsEnabled));
                }
            }
            else
            {
                throw new Exception("Failed to toggle plugin");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle plugin");
            dispatcher.Dispatch(new TogglePluginFailureAction(ex.Message));
        }
    }
}

public class TogglePluginResult
{
    public bool IsEnabled { get; set; }
}
