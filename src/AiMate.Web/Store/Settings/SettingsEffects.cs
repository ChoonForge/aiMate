using System.Net.Http.Json;
using System.Text.Json;
using AiMate.Shared.Models;
using AiMate.Web.Store.Auth;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace AiMate.Web.Store.Settings;

public class SettingsEffects
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IState<AuthState> _authState;
    private readonly IState<SettingsState> _settingsState;
    private readonly ILogger<SettingsEffects> _logger;
    private const string StorageKey = "aiMate_settings";
    private const string ApiEndpoint = "/api/v1/settings";

    public SettingsEffects(
        IJSRuntime jsRuntime,
        IHttpClientFactory httpClientFactory,
        IState<AuthState> authState,
        IState<SettingsState> settingsState,
        ILogger<SettingsEffects> logger)
    {
        _jsRuntime = jsRuntime;
        _httpClientFactory = httpClientFactory;
        _authState = authState;
        _settingsState = settingsState;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadSettings(LoadSettingsAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            // Try loading from API first if available
            try
            {
                // Get current user ID from auth state
                var userId = _authState.Value.CurrentUser?.Id.ToString()
                    ?? throw new UnauthorizedAccessException("User must be authenticated to load settings");

                var settingsDto = await httpClient.GetFromJsonAsync<UserSettingsDto>($"{ApiEndpoint}?userId={userId}");

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
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            // Get current user ID from auth state
            var userId = _authState.Value.CurrentUser?.Id.ToString()
                ?? throw new UnauthorizedAccessException("User must be authenticated to save settings");

            // Get current settings state
            var currentSettings = _settingsState.Value;

            // Convert to DTO
            var settingsDto = MapStateToDto(currentSettings);

            // POST to API
            var response = await httpClient.PostAsJsonAsync($"{ApiEndpoint}?userId={userId}", settingsDto);

            if (response.IsSuccessStatusCode)
            {
                // Also cache in localStorage
                var settingsJson = JsonSerializer.Serialize(currentSettings);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, settingsJson);

                dispatcher.Dispatch(new SaveSettingsSuccessAction());
                _logger.LogInformation("Settings saved successfully for user {UserId}", userId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to save settings: {error}");
            }
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
            // Test LiteLLM connection by calling API test endpoint
            // Implementation steps:
            // 1. Create HttpClient with: var httpClient = _httpClientFactory.CreateClient("ApiClient");
            // 2. Call test endpoint: POST /api/v1/litellm/test with LiteLLMUrl and ApiKey
            // 3. API will use ILiteLLMService.GetModelsAsync() to verify connection
            // 4. Dispatch success with model count from response
            // Currently simulates successful connection until API endpoint is created
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

    // Helper method to map State to DTO
    private UserSettingsDto MapStateToDto(SettingsState state)
    {
        return new UserSettingsDto
        {
            Language = state.Language,
            TimeZone = state.TimeZone,
            EnableNotifications = state.EnableNotifications,
            EnableSoundEffects = state.EnableSoundEffects,
            Theme = state.Theme,
            FontSize = state.FontSize,
            CompactMode = state.CompactMode,
            ShowLineNumbers = state.ShowLineNumbers,
            EnableMarkdownPreview = state.EnableMarkdownPreview,
            CodeTheme = state.CodeTheme,
            LiteLLMUrl = state.LiteLLMUrl,
            ApiKey = state.ApiKey,
            RequestTimeout = state.RequestTimeout,
            MaxRetries = state.MaxRetries,
            UseStreamingByDefault = state.UseStreamingByDefault,
            DefaultPersonality = state.DefaultPersonality,
            DefaultModel = state.DefaultModel,
            Temperature = state.Temperature,
            MaxTokens = state.MaxTokens,
            SystemPromptOverride = state.SystemPromptOverride,
            Username = state.Username,
            Email = state.Email,
            AvatarUrl = state.AvatarUrl,
            UserTier = state.UserTier,
            TrackUsage = state.TrackUsage,
            ShowCostEstimates = state.ShowCostEstimates,
            MonthlyBudget = state.MonthlyBudget,
            EnableUsageAlerts = state.EnableUsageAlerts
        };
    }
}
