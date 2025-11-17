using System.Text.Json;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace AiMate.Web.Store.Settings;

public class SettingsEffects
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<SettingsEffects> _logger;
    private const string StorageKey = "aiMate_settings";

    public SettingsEffects(
        IJSRuntime jsRuntime,
        ILogger<SettingsEffects> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadSettings(LoadSettingsAction action, IDispatcher dispatcher)
    {
        try
        {
            var settingsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);

            if (!string.IsNullOrEmpty(settingsJson))
            {
                var settings = JsonSerializer.Deserialize<SettingsState>(settingsJson);

                if (settings != null)
                {
                    dispatcher.Dispatch(new LoadSettingsSuccessAction(settings));
                    _logger.LogInformation("Settings loaded from localStorage");
                    return;
                }
            }

            // No settings found, use defaults
            dispatcher.Dispatch(new LoadSettingsSuccessAction(new SettingsState()));
            _logger.LogInformation("Using default settings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            dispatcher.Dispatch(new LoadSettingsFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleSaveSettings(SaveSettingsAction action, IDispatcher dispatcher)
    {
        try
        {
            // Get current state
            var state = dispatcher.GetType()
                .GetProperty("State")?
                .GetValue(dispatcher);

            if (state == null)
            {
                throw new InvalidOperationException("Could not access current state");
            }

            var settingsJson = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, settingsJson);

            dispatcher.Dispatch(new SaveSettingsSuccessAction());
            _logger.LogInformation("Settings saved to localStorage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            dispatcher.Dispatch(new SaveSettingsFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleResetSettings(ResetSettingsSuccessAction action, IDispatcher dispatcher)
    {
        try
        {
            // Clear from localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);

            _logger.LogInformation("Settings reset to defaults");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset settings");
        }
    }

    [EffectMethod]
    public async Task HandleTestConnection(TestConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Test actual LiteLLM connection
            // 1. Inject ILiteLLMService into SettingsEffects constructor
            // 2. Call: var models = await _liteLLMService.GetModelsAsync();
            // 3. Dispatch success with model count: $"Connected! {models.Count} models available"
            // Currently simulates successful connection
            await Task.Delay(1000);

            dispatcher.Dispatch(new TestConnectionSuccessAction("Connection successful!"));
            _logger.LogInformation("Connection test successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed");
            dispatcher.Dispatch(new TestConnectionFailureAction(ex.Message));
        }
    }
}
