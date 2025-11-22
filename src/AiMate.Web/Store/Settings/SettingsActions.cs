namespace AiMate.Web.Store.Settings;

// Load settings
public record LoadSettingsAction;
public record LoadSettingsSuccessAction(SettingsState Settings);
public record LoadSettingsFailureAction(string Error);

// Save settings
public record SaveSettingsAction;
public record SaveSettingsSuccessAction;
public record SaveSettingsFailureAction(string Error);

// Reset to defaults
public record ResetSettingsAction;
public record ResetSettingsSuccessAction;

// General Settings
public record UpdateLanguageAction(string Language);
public record UpdateTimeZoneAction(string TimeZone);
public record UpdateNotificationsAction(bool Enabled);
public record UpdateSoundEffectsAction(bool Enabled);

// Interface Settings
public record UpdateThemeAction(string Theme);
public record UpdateFontSizeAction(int FontSize);
public record UpdateCompactModeAction(bool Enabled);
public record UpdateShowLineNumbersAction(bool Enabled);
public record UpdateMarkdownPreviewAction(bool Enabled);
public record UpdateCodeThemeAction(string CodeTheme);

// Connection Settings
public record UpdateLiteLLMUrlAction(string Url);
public record UpdateApiKeyAction(string? ApiKey);
public record UpdateOpenAIKeyAction(string? ApiKey);
public record UpdateAnthropicKeyAction(string? ApiKey);
public record UpdateOllamaUrlAction(string? Url);
public record UpdateRequestTimeoutAction(int Timeout);
public record UpdateMaxRetriesAction(int MaxRetries);
public record UpdateStreamingDefaultAction(bool Enabled);
public record TestConnectionAction;
public record TestConnectionSuccessAction(string Message);
public record TestConnectionFailureAction(string Error);

// Personalisation Settings
public record UpdateDefaultPersonalityAction(string Personality);
public record UpdateDefaultModelAction(string Model);
public record UpdateTemperatureAction(double Temperature);
public record UpdateMaxTokensAction(int MaxTokens);
public record UpdateSystemPromptOverrideAction(string SystemPrompt);

// Account Settings
public record UpdateUsernameAction(string Username);
public record UpdateEmailAction(string Email);
public record UpdateAvatarUrlAction(string AvatarUrl);
public record UpdateUserTierAction(string Tier);

// Usage Settings
public record UpdateTrackUsageAction(bool Enabled);
public record UpdateShowCostEstimatesAction(bool Enabled);
public record UpdateMonthlyBudgetAction(int Budget);
public record UpdateUsageAlertsAction(bool Enabled);
public record UpdateAllowAnalyticsAction(bool Enabled);

// UI actions
public record SetActiveTabAction(int TabIndex);
public record SetSettingsErrorAction(string Error);
public record ClearSettingsErrorAction;
