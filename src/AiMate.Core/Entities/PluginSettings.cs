using System.Text.Json;

namespace AiMate.Core.Entities;

/// <summary>
/// Persisted settings for a plugin instance
/// </summary>
public class PluginSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Plugin ID that these settings belong to
    /// </summary>
    public required string PluginId { get; set; }

    /// <summary>
    /// User who owns these settings (null for global settings)
    /// </summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Settings data as JSON
    /// </summary>
    public required string SettingsJson { get; set; }

    /// <summary>
    /// When the settings were last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Deserialize settings to dictionary
    /// </summary>
    public Dictionary<string, object> GetSettings()
    {
        return JsonSerializer.Deserialize<Dictionary<string, object>>(SettingsJson)
            ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Serialize settings from dictionary
    /// </summary>
    public void SetSettings(Dictionary<string, object> settings)
    {
        SettingsJson = JsonSerializer.Serialize(settings);
        UpdatedAt = DateTime.UtcNow;
    }
}
