using Fluxor;

namespace AiMate.Web.Store.Settings;

public static class SettingsReducers
{
    // Load settings
    [ReducerMethod]
    public static SettingsState OnLoadSettings(SettingsState state, LoadSettingsAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static SettingsState OnLoadSettingsSuccess(SettingsState state, LoadSettingsSuccessAction action)
    {
        return action.Settings with { IsLoading = false, Error = null };
    }

    [ReducerMethod]
    public static SettingsState OnLoadSettingsFailure(SettingsState state, LoadSettingsFailureAction action)
    {
        return state with { IsLoading = false, Error = action.Error };
    }

    // Save settings
    [ReducerMethod]
    public static SettingsState OnSaveSettings(SettingsState state, SaveSettingsAction action)
    {
        return state with { IsSaving = true, Error = null };
    }

    [ReducerMethod]
    public static SettingsState OnSaveSettingsSuccess(SettingsState state, SaveSettingsSuccessAction action)
    {
        return state with { IsSaving = false, Error = null };
    }

    [ReducerMethod]
    public static SettingsState OnSaveSettingsFailure(SettingsState state, SaveSettingsFailureAction action)
    {
        return state with { IsSaving = false, Error = action.Error };
    }

    // Reset to defaults
    [ReducerMethod]
    public static SettingsState OnResetSettings(SettingsState state, ResetSettingsAction action)
    {
        return new SettingsState();
    }

    // General Settings
    [ReducerMethod]
    public static SettingsState OnUpdateLanguage(SettingsState state, UpdateLanguageAction action)
    {
        return state with { Language = action.Language };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateTimeZone(SettingsState state, UpdateTimeZoneAction action)
    {
        return state with { TimeZone = action.TimeZone };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateNotifications(SettingsState state, UpdateNotificationsAction action)
    {
        return state with { EnableNotifications = action.Enabled };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateSoundEffects(SettingsState state, UpdateSoundEffectsAction action)
    {
        return state with { EnableSoundEffects = action.Enabled };
    }

    // Interface Settings
    [ReducerMethod]
    public static SettingsState OnUpdateTheme(SettingsState state, UpdateThemeAction action)
    {
        return state with { Theme = action.Theme };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateFontSize(SettingsState state, UpdateFontSizeAction action)
    {
        return state with { FontSize = action.FontSize };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateCompactMode(SettingsState state, UpdateCompactModeAction action)
    {
        return state with { CompactMode = action.Enabled };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateShowLineNumbers(SettingsState state, UpdateShowLineNumbersAction action)
    {
        return state with { ShowLineNumbers = action.Enabled };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateMarkdownPreview(SettingsState state, UpdateMarkdownPreviewAction action)
    {
        return state with { EnableMarkdownPreview = action.Enabled };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateCodeTheme(SettingsState state, UpdateCodeThemeAction action)
    {
        return state with { CodeTheme = action.CodeTheme };
    }

    // Connection Settings
    [ReducerMethod]
    public static SettingsState OnUpdateLiteLLMUrl(SettingsState state, UpdateLiteLLMUrlAction action)
    {
        return state with { LiteLLMUrl = action.Url };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateApiKey(SettingsState state, UpdateApiKeyAction action)
    {
        return state with { ApiKey = action.ApiKey };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateRequestTimeout(SettingsState state, UpdateRequestTimeoutAction action)
    {
        return state with { RequestTimeout = action.Timeout };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateMaxRetries(SettingsState state, UpdateMaxRetriesAction action)
    {
        return state with { MaxRetries = action.MaxRetries };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateStreamingDefault(SettingsState state, UpdateStreamingDefaultAction action)
    {
        return state with { UseStreamingByDefault = action.Enabled };
    }

    // Personalisation Settings
    [ReducerMethod]
    public static SettingsState OnUpdateDefaultPersonality(SettingsState state, UpdateDefaultPersonalityAction action)
    {
        return state with { DefaultPersonality = action.Personality };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateDefaultModel(SettingsState state, UpdateDefaultModelAction action)
    {
        return state with { DefaultModel = action.Model };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateTemperature(SettingsState state, UpdateTemperatureAction action)
    {
        return state with { Temperature = action.Temperature };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateMaxTokens(SettingsState state, UpdateMaxTokensAction action)
    {
        return state with { MaxTokens = action.MaxTokens };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateSystemPromptOverride(SettingsState state, UpdateSystemPromptOverrideAction action)
    {
        return state with { SystemPromptOverride = action.SystemPrompt };
    }

    // Account Settings
    [ReducerMethod]
    public static SettingsState OnUpdateUsername(SettingsState state, UpdateUsernameAction action)
    {
        return state with { Username = action.Username };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateEmail(SettingsState state, UpdateEmailAction action)
    {
        return state with { Email = action.Email };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateAvatarUrl(SettingsState state, UpdateAvatarUrlAction action)
    {
        return state with { AvatarUrl = action.AvatarUrl };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateUserTier(SettingsState state, UpdateUserTierAction action)
    {
        return state with { UserTier = action.Tier };
    }

    // Usage Settings
    [ReducerMethod]
    public static SettingsState OnUpdateTrackUsage(SettingsState state, UpdateTrackUsageAction action)
    {
        return state with { TrackUsage = action.Enabled };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateShowCostEstimates(SettingsState state, UpdateShowCostEstimatesAction action)
    {
        return state with { ShowCostEstimates = action.Enabled };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateMonthlyBudget(SettingsState state, UpdateMonthlyBudgetAction action)
    {
        return state with { MonthlyBudget = action.Budget };
    }

    [ReducerMethod]
    public static SettingsState OnUpdateUsageAlerts(SettingsState state, UpdateUsageAlertsAction action)
    {
        return state with { EnableUsageAlerts = action.Enabled };
    }

    // UI actions
    [ReducerMethod]
    public static SettingsState OnSetActiveTab(SettingsState state, SetActiveTabAction action)
    {
        return state with { ActiveTab = action.TabIndex };
    }

    [ReducerMethod]
    public static SettingsState OnSetSettingsError(SettingsState state, SetSettingsErrorAction action)
    {
        return state with { Error = action.Error };
    }

    [ReducerMethod]
    public static SettingsState OnClearSettingsError(SettingsState state, ClearSettingsErrorAction action)
    {
        return state with { Error = null };
    }
}
