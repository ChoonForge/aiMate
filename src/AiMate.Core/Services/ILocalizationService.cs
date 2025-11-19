namespace AiMate.Core.Services;

/// <summary>
/// Localization service for multi-language support
/// Built from the ground up - not an afterthought
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get localized string by key
    /// </summary>
    string GetString(string key, string? locale = null);

    /// <summary>
    /// Get localized string with parameters
    /// </summary>
    string GetString(string key, object parameters, string? locale = null);

    /// <summary>
    /// Get current locale
    /// </summary>
    string GetCurrentLocale();

    /// <summary>
    /// Set current locale
    /// </summary>
    void SetLocale(string locale);

    /// <summary>
    /// Get all available locales
    /// </summary>
    List<string> GetAvailableLocales();
}
