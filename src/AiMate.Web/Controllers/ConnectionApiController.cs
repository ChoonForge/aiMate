using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing LLM provider connections with BYOK support
/// </summary>
[ApiController]
[Route("api/v1/connections")]
public class ConnectionApiController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<ConnectionApiController> _logger;

    public ConnectionApiController(
        IPermissionService permissionService,
        ILogger<ConnectionApiController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all connections accessible to the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConnections([FromQuery] string userId, [FromQuery] string tierStr = "Free")
    {
        try
        {
            if (!Enum.TryParse<UserTier>(tierStr, out var tier))
            {
                tier = UserTier.Free;
            }

            _logger.LogInformation("Getting connections for user {UserId} with tier {Tier}", userId, tier);

            // Check if user can even add own keys
            if (!_permissionService.IsBYOKEnabledForTier(tier))
            {
                return Ok(new List<ProviderConnectionDto>());
            }

            // IMPLEMENTATION NEEDED: Get from database
            // For now, return mock data
            var connections = new List<ProviderConnectionDto>
            {
                new()
                {
                    Id = "conn-1",
                    Name = "My OpenAI Connection",
                    Type = "Cloud",
                    Url = "https://api.openai.com/v1",
                    Auth = "ApiKey",
                    ProviderType = "OpenAI",
                    ModelIds = new List<string> { "gpt-4", "gpt-3.5-turbo" },
                    Tags = new List<string> { "production", "main" },
                    IsEnabled = true,
                    OwnerId = userId,
                    Visibility = "Private"
                }
            };

            return Ok(connections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connections");
            return StatusCode(500, new { error = "Failed to get connections", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific connection by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConnection(string id, [FromQuery] string userId, [FromQuery] string tierStr = "Free")
    {
        try
        {
            if (!Enum.TryParse<UserTier>(tierStr, out var tier))
            {
                tier = UserTier.Free;
            }

            _logger.LogInformation("Getting connection {ConnectionId} for user {UserId}", id, userId);

            // IMPLEMENTATION NEEDED: Get from database
            var connection = new ProviderConnectionDto
            {
                Id = id,
                Name = "My OpenAI Connection",
                OwnerId = userId
            };

            // Check permissions
            if (!_permissionService.CanManageConnection(userId, connection.OwnerId ?? string.Empty, tier))
            {
                return Forbid("You don't have permission to access this connection");
            }

            return Ok(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connection {ConnectionId}", id);
            return StatusCode(500, new { error = "Failed to get connection", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new connection (BYOK)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateConnection(
        [FromBody] ProviderConnectionDto connection,
        [FromQuery] string userId,
        [FromQuery] string tierStr = "Free")
    {
        try
        {
            if (!Enum.TryParse<UserTier>(tierStr, out var tier))
            {
                tier = UserTier.Free;
            }

            _logger.LogInformation("Creating connection for user {UserId} with tier {Tier}", userId, tier);

            // Check if BYOK is enabled for this tier
            if (!_permissionService.IsBYOKEnabledForTier(tier))
            {
                return Forbid("BYOK is not enabled for your tier. Please upgrade.");
            }

            // Check if user has permission to add own keys
            if (!_permissionService.HasPermission(tier, UserPermission.AddOwnKeys))
            {
                return Forbid("You don't have permission to add connections");
            }

            // Check connection limit
            // IMPLEMENTATION NEEDED: Get current connection count from database
            var currentCount = 0;
            var maxConnections = _permissionService.GetMaxConnectionsForTier(tier);

            if (currentCount >= maxConnections)
            {
                return BadRequest(new
                {
                    error = "Connection limit reached",
                    message = $"Your tier allows {maxConnections} connections. Please upgrade for more.",
                    currentCount,
                    maxConnections
                });
            }

            // Validate custom endpoints permission
            if (connection.Type == "Local" && !_permissionService.HasPermission(tier, UserPermission.AddCustomEndpoints))
            {
                return Forbid("Custom endpoints require Developer tier or higher");
            }

            // Validate visibility permissions
            if (connection.Visibility == "Group" && !_permissionService.HasPermission(tier, UserPermission.ShareConnections))
            {
                return Forbid("Sharing connections requires Developer tier or higher");
            }

            if ((connection.Visibility == "Organization" || connection.Visibility == "Public") && tier != UserTier.Admin)
            {
                return Forbid("Only admins can create organization or public connections");
            }

            // Set owner
            connection.OwnerId = userId;
            connection.Id = Guid.NewGuid().ToString();

            // IMPLEMENTATION NEEDED: Save to database

            _logger.LogInformation("Created connection {ConnectionId} for user {UserId}", connection.Id, userId);

            return CreatedAtAction(nameof(GetConnection), new { id = connection.Id, userId, tierStr }, connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create connection");
            return StatusCode(500, new { error = "Failed to create connection", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing connection
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConnection(
        string id,
        [FromBody] ProviderConnectionDto connection,
        [FromQuery] string userId,
        [FromQuery] string tierStr = "Free")
    {
        try
        {
            if (!Enum.TryParse<UserTier>(tierStr, out var tier))
            {
                tier = UserTier.Free;
            }

            _logger.LogInformation("Updating connection {ConnectionId} for user {UserId}", id, userId);

            // IMPLEMENTATION NEEDED: Get existing connection from database
            var existingConnection = new ProviderConnectionDto { Id = id, OwnerId = userId };

            // Check permissions
            if (!_permissionService.CanManageConnection(userId, existingConnection.OwnerId ?? string.Empty, tier))
            {
                return Forbid("You don't have permission to update this connection");
            }

            // Validate tier-specific updates
            if (connection.Type != existingConnection.Type &&
                connection.Type == "Local" &&
                !_permissionService.HasPermission(tier, UserPermission.AddCustomEndpoints))
            {
                return Forbid("Custom endpoints require Developer tier or higher");
            }

            connection.Id = id;
            connection.OwnerId = existingConnection.OwnerId;

            // IMPLEMENTATION NEEDED: Update in database

            return Ok(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update connection {ConnectionId}", id);
            return StatusCode(500, new { error = "Failed to update connection", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a connection
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConnection(string id, [FromQuery] string userId, [FromQuery] string tierStr = "Free")
    {
        try
        {
            if (!Enum.TryParse<UserTier>(tierStr, out var tier))
            {
                tier = UserTier.Free;
            }

            _logger.LogInformation("Deleting connection {ConnectionId} for user {UserId}", id, userId);

            // IMPLEMENTATION NEEDED: Get connection from database
            var connection = new ProviderConnectionDto { Id = id, OwnerId = userId };

            // Check permissions
            if (!_permissionService.CanManageConnection(userId, connection.OwnerId ?? string.Empty, tier))
            {
                return Forbid("You don't have permission to delete this connection");
            }

            // IMPLEMENTATION NEEDED: Delete from database

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete connection {ConnectionId}", id);
            return StatusCode(500, new { error = "Failed to delete connection", details = ex.Message });
        }
    }

    /// <summary>
    /// Test a connection
    /// </summary>
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestConnection(string id, [FromQuery] string userId)
    {
        try
        {
            _logger.LogInformation("Testing connection {ConnectionId}", id);

            // IMPLEMENTATION NEEDED: Actually test the connection
            await Task.Delay(1000); // Simulate API call

            var isSuccess = new Random().Next(0, 2) == 1;

            if (isSuccess)
            {
                return Ok(new
                {
                    success = true,
                    message = "Connection successful! API is responding.",
                    modelCount = 12,
                    latencyMs = 245
                });
            }
            else
            {
                return Ok(new
                {
                    success = false,
                    message = "Connection failed: Invalid API key",
                    error = "Authentication failed"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test connection {ConnectionId}", id);
            return StatusCode(500, new { error = "Failed to test connection", details = ex.Message });
        }
    }

    /// <summary>
    /// Get connection limits for user's tier
    /// </summary>
    [HttpGet("limits")]
    public Task<IActionResult> GetLimits([FromQuery] string tierStr = "Free")
    {
        try
        {
            if (!Enum.TryParse<UserTier>(tierStr, out var tier))
            {
                tier = UserTier.Free;
            }

            var maxConnections = _permissionService.GetMaxConnectionsForTier(tier);
            var byokEnabled = _permissionService.IsBYOKEnabledForTier(tier);
            var permissions = _permissionService.GetPermissionsForTier(tier);

            return Task.FromResult<IActionResult>(Ok(new
            {
                tier = tier.ToString(),
                maxConnections,
                byokEnabled,
                canAddOwnKeys = _permissionService.HasPermission(tier, UserPermission.AddOwnKeys),
                canAddCustomEndpoints = _permissionService.HasPermission(tier, UserPermission.AddCustomEndpoints),
                canShareConnections = _permissionService.HasPermission(tier, UserPermission.ShareConnections),
                canUsePlatformKeys = _permissionService.HasPermission(tier, UserPermission.UsePlatformKeys)
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connection limits");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to get limits", details = ex.Message }));
        }
    }
}
