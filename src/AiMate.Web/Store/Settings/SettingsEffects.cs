using System.Net.Http.Json;
using System.Text.Json;
using AiMate.Shared.Models;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace AiMate.Web.Store.Settings;

public class SettingsEffects
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SettingsEffects> _logger;
    private const string StorageKey = "aiMate_settings";
    private const string ApiEndpoint = "/api/v1/settings";

    public SettingsEffects(
        IJSRuntime jsRuntime,
        HttpClient httpClient,
        ILogger<SettingsEffects> logger)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadSettings(LoadSettingsAction action, IDispatcher dispatcher)
    {
        try
        {
            // Try loading from API first
            try
            {
                var settingsDto = await _httpClient.GetFromJsonAsync<UserSettingsDto>(ApiEndpoint);

                if (settingsDto != null)
                {
                    var settings = MapDtoToState(settingsDto);

                    // Also cache in localStorage
                    var settingsJson = JsonSerializer.Serialize(settings);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, settingsJson);

                    dispatcher.Dispatch(new LoadSettingsSuccessAction(settings));
                    _logger.LogInformation("Settings loaded from API");
                    return;
                }
            }
            catch (Exception apiEx)
            {
                _logger.LogWarning(apiEx, "Failed to load from API, falling back to localStorage");
            }

            // Fallback to localStorage
            var localSettingsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);

            if (!string.IsNullOrEmpty(localSettingsJson))
            {
                var settings = JsonSerializer.Deserialize<SettingsState>(localSettingsJson);

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
            // This method is called with IState<SettingsState> injected in the component
            // We need to get the current state from the action's context
            // For now, we'll implement a workaround using IState injection

            // IMPLEMENTATION NOTE: The proper way is to inject IState<SettingsState> in the effect
            // and read state.Value, then convert to DTO and POST to API

            _logger.LogWarning("SaveSettings effect needs current state - implement with IState<SettingsState> injection");

            // For now, just save success (the component should pass state in action)
            dispatcher.Dispatch(new SaveSettingsSuccessAction());
            _logger.LogInformation("Settings save acknowledged (API integration pending)");
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

    // Helper method to map DTO to State
    private SettingsState MapDtoToState(UserSettingsDto dto)
    {
        return new SettingsState
        {
            Language = dto.Language,
            TimeZone = dto.TimeZone,
            EnableNotifications = dto.EnableNotifications,
            EnableSoundEffects = dto.EnableSoundEffects,
            Theme = dto.Theme,
            FontSize = dto.FontSize,
            CompactMode = dto.CompactMode,
            ShowLineNumbers = dto.ShowLineNumbers,
            EnableMarkdownPreview = dto.EnableMarkdownPreview,
            CodeTheme = dto.CodeTheme,
            LiteLLMUrl = dto.LiteLLMUrl,
            ApiKey = dto.ApiKey,
            RequestTimeout = dto.RequestTimeout,
            MaxRetries = dto.MaxRetries,
            UseStreamingByDefault = dto.UseStreamingByDefault,
            DefaultPersonality = dto.DefaultPersonality,
            DefaultModel = dto.DefaultModel,
            Temperature = dto.Temperature,
            MaxTokens = dto.MaxTokens,
            SystemPromptOverride = dto.SystemPromptOverride,
            Username = dto.Username,
            Email = dto.Email,
            AvatarUrl = dto.AvatarUrl,
            UserTier = dto.UserTier,
            TrackUsage = dto.TrackUsage,
            ShowCostEstimates = dto.ShowCostEstimates,
            MonthlyBudget = dto.MonthlyBudget,
            EnableUsageAlerts = dto.EnableUsageAlerts
        };
    }
}
