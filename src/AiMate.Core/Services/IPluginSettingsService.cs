using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Service for managing plugin settings persistence
/// </summary>
public interface IPluginSettingsService
{
    /// <summary>
    /// Get settings for a plugin
    /// </summary>
    Task<Dictionary<string, object>?> GetPluginSettingsAsync(string pluginId, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save settings for a plugin
    /// </summary>
    Task SavePluginSettingsAsync(string pluginId, Dictionary<string, object> settings, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete settings for a plugin
    /// </summary>
    Task DeletePluginSettingsAsync(string pluginId, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all plugin settings for a user
    /// </summary>
    Task<List<PluginSettings>> GetUserPluginSettingsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all global plugin settings (userId == null)
    /// </summary>
    Task<List<PluginSettings>> GetGlobalPluginSettingsAsync(CancellationToken cancellationToken = default);
}
