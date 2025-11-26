using AiMate.Core.Plugins;
using AiMate.Core.Services;
using AiMate.Web.Store.Plugin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// Plugin API for managing plugins and extensions
/// </summary>
[ApiController]
[Route("api/v1/plugins")]
[Authorize] // Requires authentication
public class PluginApiController : ControllerBase
{
    private readonly IPluginManager _pluginManager;
    private readonly IPluginSettingsService _pluginSettingsService;
    private readonly ILogger<PluginApiController> _logger;

    public PluginApiController(
        IPluginManager pluginManager,
        IPluginSettingsService pluginSettingsService,
        ILogger<PluginApiController> logger)
    {
        _pluginManager = pluginManager;
        _pluginSettingsService = pluginSettingsService;
        _logger = logger;
    }

    // ============================================================================
    // PLUGIN MANAGEMENT
    // ============================================================================

    /// <summary>
    /// Get all loaded plugins
    /// </summary>
    [HttpGet]
    public Task<IActionResult> GetPlugins()
    {
        try
        {
            _logger.LogInformation("Fetching all plugins");

            var plugins = _pluginManager.GetLoadedPlugins()
                .Select(p => new PluginInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Version = p.Version,
                    Author = p.Author,
                    Icon = p.Icon,
                    Category = p.Category,
                    IsEnabled = p.IsEnabled,
                    HasSettings = p is IUIExtension uiExt && uiExt.GetSettingsUI() != null,
                    ActionCount = p is IUIExtension ui ? ui.GetMessageActions(new Core.Entities.Message { Content = "" }).Count() : 0,
                    ToolCount = p is IToolProvider tp ? tp.GetTools().Count() : 0
                })
                .ToList();

            return Task.FromResult<IActionResult>(Ok(plugins));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch plugins");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to fetch plugins", details = ex.Message }));
        }
    }

    /// <summary>
    /// Get a specific plugin by ID
    /// </summary>
    [HttpGet("{id}")]
    public Task<IActionResult> GetPlugin(string id)
    {
        try
        {
            _logger.LogInformation("Fetching plugin {PluginId}", id);

            var plugin = _pluginManager.GetPlugin(id);
            if (plugin == null)
            {
                return Task.FromResult<IActionResult>(NotFound(new { error = "Plugin not found", pluginId = id }));
            }

            var pluginInfo = new PluginInfo
            {
                Id = plugin.Id,
                Name = plugin.Name,
                Description = plugin.Description,
                Version = plugin.Version,
                Author = plugin.Author,
                Icon = plugin.Icon,
                Category = plugin.Category,
                IsEnabled = plugin.IsEnabled,
                HasSettings = plugin is IUIExtension uiExt && uiExt.GetSettingsUI() != null,
                ActionCount = plugin is IUIExtension ui ? ui.GetMessageActions(new Core.Entities.Message { Content = "" }).Count() : 0,
                ToolCount = plugin is IToolProvider tp ? tp.GetTools().Count() : 0
            };

            return Task.FromResult<IActionResult>(Ok(pluginInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch plugin {PluginId}", id);
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to fetch plugin", details = ex.Message }));
        }
    }

    /// <summary>
    /// Toggle a plugin's enabled state
    /// </summary>
    [HttpPost("{id}/toggle")]
    public async Task<IActionResult> TogglePlugin(string id)
    {
        try
        {
            _logger.LogInformation("Toggling plugin {PluginId}", id);

            var plugin = _pluginManager.GetPlugin(id);
            if (plugin == null)
            {
                return NotFound(new { error = "Plugin not found", pluginId = id });
            }

            // Toggle the plugin state
            var newState = !plugin.IsEnabled;
            await _pluginManager.SetPluginEnabledAsync(id, newState);

            return Ok(new { isEnabled = newState });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle plugin {PluginId}", id);
            return StatusCode(500, new { error = "Failed to toggle plugin", details = ex.Message });
        }
    }

    /// <summary>
    /// Get plugin settings (UI schema and current values)
    /// </summary>
    [HttpGet("{id}/settings")]
    public async Task<IActionResult> GetPluginSettings(string id, [FromQuery] string? userId = null)
    {
        try
        {
            _logger.LogInformation("Fetching settings for plugin {PluginId}", id);

            var plugin = _pluginManager.GetPlugin(id);
            if (plugin == null)
            {
                return NotFound(new { error = "Plugin not found", pluginId = id });
            }

            if (plugin is not IUIExtension uiExtension)
            {
                return BadRequest(new { error = "Plugin does not support settings" });
            }

            var settingsUI = uiExtension.GetSettingsUI();
            if (settingsUI == null)
            {
                return NotFound(new { error = "Plugin has no settings" });
            }

            // Parse userId if provided
            Guid? userGuid = null;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsedUserId))
            {
                userGuid = parsedUserId;
            }

            // Load persisted settings values
            var savedSettings = await _pluginSettingsService.GetPluginSettingsAsync(id, userGuid);

            return Ok(new
            {
                schema = settingsUI,
                values = savedSettings ?? new Dictionary<string, object>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch settings for plugin {PluginId}", id);
            return StatusCode(500, new { error = "Failed to fetch plugin settings", details = ex.Message });
        }
    }

    /// <summary>
    /// Update plugin settings
    /// </summary>
    [HttpPost("{id}/settings")]
    public async Task<IActionResult> UpdatePluginSettings(string id, [FromBody] Dictionary<string, object> settings, [FromQuery] string? userId = null)
    {
        try
        {
            _logger.LogInformation("Updating settings for plugin {PluginId}", id);

            var plugin = _pluginManager.GetPlugin(id);
            if (plugin == null)
            {
                return NotFound(new { error = "Plugin not found", pluginId = id });
            }

            // Parse userId if provided (for user-specific settings)
            Guid? userGuid = null;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsedUserId))
            {
                userGuid = parsedUserId;
            }

            // Persist settings to database
            await _pluginSettingsService.SavePluginSettingsAsync(id, settings, userGuid);

            _logger.LogInformation("Plugin settings updated and persisted for plugin {PluginId}", id);

            return Ok(new { message = "Settings updated successfully", userId = userGuid?.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update settings for plugin {PluginId}", id);
            return StatusCode(500, new { error = "Failed to update plugin settings", details = ex.Message });
        }
    }

    /// <summary>
    /// Reload all plugins
    /// </summary>
    [HttpPost("reload")]
    public async Task<IActionResult> ReloadPlugins()
    {
        try
        {
            _logger.LogInformation("Reloading all plugins");

            await _pluginManager.LoadPluginsAsync();

            var count = _pluginManager.GetLoadedPlugins().Count();
            return Ok(new { message = $"Reloaded {count} plugins" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload plugins");
            return StatusCode(500, new { error = "Failed to reload plugins", details = ex.Message });
        }
    }

    // ============================================================================
    // PLUGIN ACTIONS & TOOLS
    // ============================================================================

    /// <summary>
    /// Get message actions for a specific message
    /// </summary>
    [HttpPost("actions")]
    public Task<IActionResult> GetMessageActions([FromBody] GetMessageActionsRequest request)
    {
        try
        {
            _logger.LogInformation("Getting message actions for message {MessageId}", request.MessageId);

            var message = new Core.Entities.Message
            {
                Id = Guid.TryParse(request.MessageId, out var msgId) ? msgId : Guid.NewGuid(),
                Role = request.Role,
                Content = request.Content ?? string.Empty
            };

            var actions = _pluginManager.GetMessageActions(message)
                .OrderBy(a => a.Order)
                .ToList();

            return Task.FromResult<IActionResult>(Ok(actions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get message actions");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to get message actions", details = ex.Message }));
        }
    }

    /// <summary>
    /// Get available tools from all plugins
    /// </summary>
    [HttpGet("tools")]
    public Task<IActionResult> GetTools()
    {
        try
        {
            _logger.LogInformation("Fetching all plugin tools");

            var tools = _pluginManager.GetAllTools();

            return Task.FromResult<IActionResult>(Ok(tools));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch plugin tools");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to fetch tools", details = ex.Message }));
        }
    }
}

/// <summary>
/// Request model for getting message actions
/// </summary>
public class GetMessageActionsRequest
{
    public string MessageId { get; set; } = string.Empty;
    public Core.Enums.MessageRole Role { get; set; }
    public string? Content { get; set; }
}
