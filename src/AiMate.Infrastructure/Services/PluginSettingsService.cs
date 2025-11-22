using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for managing plugin settings persistence
/// </summary>
public class PluginSettingsService : IPluginSettingsService
{
    private readonly AiMateDbContext _dbContext;
    private readonly ILogger<PluginSettingsService> _logger;

    public PluginSettingsService(
        AiMateDbContext dbContext,
        ILogger<PluginSettingsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Dictionary<string, object>?> GetPluginSettingsAsync(
        string pluginId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _dbContext.PluginSettings
                .Where(ps => ps.PluginId == pluginId && ps.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            return settings?.GetSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings for plugin {PluginId}", pluginId);
            throw;
        }
    }

    public async Task SavePluginSettingsAsync(
        string pluginId,
        Dictionary<string, object> settings,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingSettings = await _dbContext.PluginSettings
                .Where(ps => ps.PluginId == pluginId && ps.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingSettings != null)
            {
                // Update existing settings
                existingSettings.SetSettings(settings);
                _dbContext.PluginSettings.Update(existingSettings);
                _logger.LogInformation("Updated settings for plugin {PluginId}", pluginId);
            }
            else
            {
                // Create new settings
                var newSettings = new PluginSettings
                {
                    PluginId = pluginId,
                    UserId = userId,
                    SettingsJson = "{}"
                };
                newSettings.SetSettings(settings);

                _dbContext.PluginSettings.Add(newSettings);
                _logger.LogInformation("Created settings for plugin {PluginId}", pluginId);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings for plugin {PluginId}", pluginId);
            throw;
        }
    }

    public async Task DeletePluginSettingsAsync(
        string pluginId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _dbContext.PluginSettings
                .Where(ps => ps.PluginId == pluginId && ps.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (settings != null)
            {
                _dbContext.PluginSettings.Remove(settings);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Deleted settings for plugin {PluginId}", pluginId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting settings for plugin {PluginId}", pluginId);
            throw;
        }
    }

    public async Task<List<PluginSettings>> GetUserPluginSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.PluginSettings
                .Where(ps => ps.UserId == userId)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user plugin settings for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<PluginSettings>> GetGlobalPluginSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.PluginSettings
                .Where(ps => ps.UserId == null)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting global plugin settings");
            throw;
        }
    }
}
