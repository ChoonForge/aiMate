using AiMate.Infrastructure.Data;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AiMate.Web.Controllers;

/// <summary>
/// Settings API for managing user preferences and configuration
/// </summary>
[ApiController]
[Route("api/v1/settings")]
[Authorize] // Requires authentication
public class SettingsApiController : ControllerBase
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<SettingsApiController> _logger;

    public SettingsApiController(
        AiMateDbContext context,
        ILogger<SettingsApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get user settings
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettings([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            _logger.LogInformation("Fetching settings for user {UserId}", userId);

            var user = await _context.Users.FindAsync(userGuid);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Deserialize settings from User.PreferencesJson or return defaults
            UserSettingsDto settings;
            if (!string.IsNullOrEmpty(user.PreferencesJson))
            {
                settings = JsonSerializer.Deserialize<UserSettingsDto>(user.PreferencesJson) ?? new UserSettingsDto();
            }
            else
            {
                settings = new UserSettingsDto();
            }

            // Populate account settings from User entity
            settings.Username = user.Username;
            settings.Email = user.Email;
            settings.UserTier = user.Tier.ToString();
            settings.DefaultPersonality = user.DefaultPersonality.ToString();

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching settings for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to fetch settings", message = ex.Message });
        }
    }

    /// <summary>
    /// Update user settings
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] UserSettingsDto settings, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            _logger.LogInformation("Updating settings for user {UserId}", userId);

            var user = await _context.Users.FindAsync(userGuid);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update User entity fields that are in settings
            if (settings.DefaultPersonality != null &&
                Enum.TryParse<AiMate.Core.Enums.PersonalityMode>(settings.DefaultPersonality, out var personality))
            {
                user.DefaultPersonality = personality;
            }

            // Serialize and save full settings to PreferencesJson
            user.PreferencesJson = JsonSerializer.Serialize(settings);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Settings updated for user {UserId}", userId);

            return Ok(new { success = true, message = "Settings updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to update settings", message = ex.Message });
        }
    }

    /// <summary>
    /// Reset settings to defaults
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetSettings([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            _logger.LogInformation("Resetting settings for user {UserId}", userId);

            var user = await _context.Users.FindAsync(userGuid);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Clear PreferencesJson to reset to defaults
            user.PreferencesJson = null;
            await _context.SaveChangesAsync();

            var defaultSettings = new UserSettingsDto
            {
                Username = user.Username,
                Email = user.Email,
                UserTier = user.Tier.ToString(),
                DefaultPersonality = user.DefaultPersonality.ToString()
            };

            _logger.LogInformation("Settings reset to defaults for user {UserId}", userId);

            return Ok(defaultSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting settings for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to reset settings", message = ex.Message });
        }
    }

    /// <summary>
    /// Export settings as JSON
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportSettings([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            _logger.LogInformation("Exporting settings for user {UserId}", userId);

            var user = await _context.Users.FindAsync(userGuid);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get current settings
            UserSettingsDto settings;
            if (!string.IsNullOrEmpty(user.PreferencesJson))
            {
                settings = JsonSerializer.Deserialize<UserSettingsDto>(user.PreferencesJson) ?? new UserSettingsDto();
            }
            else
            {
                settings = new UserSettingsDto();
            }

            // Populate account settings
            settings.Username = user.Username;
            settings.Email = user.Email;
            settings.UserTier = user.Tier.ToString();

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            return File(
                System.Text.Encoding.UTF8.GetBytes(json),
                "application/json",
                "aiMate-settings.json"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting settings for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to export settings", message = ex.Message });
        }
    }

    /// <summary>
    /// Import settings from JSON
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportSettings([FromBody] UserSettingsDto settings, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            _logger.LogInformation("Importing settings for user {UserId}", userId);

            var user = await _context.Users.FindAsync(userGuid);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update personality if provided
            if (settings.DefaultPersonality != null &&
                Enum.TryParse<AiMate.Core.Enums.PersonalityMode>(settings.DefaultPersonality, out var personality))
            {
                user.DefaultPersonality = personality;
            }

            // Save imported settings
            user.PreferencesJson = JsonSerializer.Serialize(settings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Settings imported for user {UserId}", userId);

            return Ok(new { success = true, message = "Settings imported successfully", settings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing settings for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to import settings", message = ex.Message });
        }
    }
}
