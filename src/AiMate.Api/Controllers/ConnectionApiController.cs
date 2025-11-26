using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing LLM provider connections with BYOK support
/// </summary>
[ApiController]
[Route("api/v1/connections")]
[Authorize(Policy = "CanAddOwnKeys")] // Requires BYOK permission to manage connections
public class ConnectionApiController : ControllerBase
{
    private readonly IConnectionService _connectionService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<ConnectionApiController> _logger;

    public ConnectionApiController(
        IConnectionService connectionService,
        IPermissionService permissionService,
        ILogger<ConnectionApiController> logger)
    {
        _connectionService = connectionService;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all connections accessible to the current user
    /// </summary>
    /// <param name="userId">User ID (GUID)</param>
    /// <param name="tierStr">User tier (Free, BYOK, Developer, Admin)</param>
    /// <returns>List of provider connections</returns>
    /// <response code="200">Returns the list of connections</response>
    /// <response code="400">Invalid user ID format</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProviderConnectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var connections = await _connectionService.GetUserConnectionsAsync(userGuid);
            var connectionDtos = connections.Select(MapToDto).ToList();

            return Ok(connectionDtos);
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

            if (!Guid.TryParse(id, out var connectionGuid))
            {
                return BadRequest("Invalid connection ID");
            }

            var connection = await _connectionService.GetConnectionByIdAsync(connectionGuid);

            if (connection == null)
            {
                return NotFound($"Connection {id} not found");
            }

            // Check permissions
            if (!_permissionService.CanManageConnection(userId, connection.OwnerId.ToString(), tier))
            {
                return Forbid("You don't have permission to access this connection");
            }

            return Ok(MapToDto(connection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connection {ConnectionId}", id);
            return StatusCode(500, new { error = "Failed to get connection", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new provider connection (BYOK - Bring Your Own Key)
    /// </summary>
    /// <param name="connection">Connection details (name, type, API key, etc.)</param>
    /// <param name="userId">User ID (GUID)</param>
    /// <param name="tierStr">User tier (Free, BYOK, Developer, Admin)</param>
    /// <returns>Created connection with ID</returns>
    /// <response code="201">Connection created successfully</response>
    /// <response code="400">Invalid request or connection limit reached</response>
    /// <response code="403">Insufficient permissions for tier</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/connections?userId=abc123&amp;tierStr=Developer
    ///     {
    ///         "name": "My OpenAI Connection",
    ///         "type": "Cloud",
    ///         "providerType": "OpenAI",
    ///         "apiKey": "sk-...",
    ///         "url": "https://api.openai.com/v1",
    ///         "modelIds": ["gpt-4", "gpt-3.5-turbo"],
    ///         "visibility": "Private"
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ProviderConnectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            // Check connection limit
            var userConnections = await _connectionService.GetUserConnectionsAsync(userGuid);
            var currentCount = userConnections.Count;
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

            // Create entity from DTO
            var entity = MapToEntity(connection, userGuid);
            var created = await _connectionService.CreateConnectionAsync(entity);

            _logger.LogInformation("Created connection {ConnectionId} for user {UserId}", created.Id, userId);

            return CreatedAtAction(nameof(GetConnection), new { id = created.Id.ToString(), userId, tierStr }, MapToDto(created));
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

            if (!Guid.TryParse(id, out var connectionGuid))
            {
                return BadRequest("Invalid connection ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var existingConnection = await _connectionService.GetConnectionByIdAsync(connectionGuid);

            if (existingConnection == null)
            {
                return NotFound($"Connection {id} not found");
            }

            // Check permissions
            if (!_permissionService.CanManageConnection(userId, existingConnection.OwnerId.ToString(), tier))
            {
                return Forbid("You don't have permission to update this connection");
            }

            // Validate tier-specific updates
            var existingDto = MapToDto(existingConnection);
            if (connection.Type != existingDto.Type &&
                connection.Type == "Local" &&
                !_permissionService.HasPermission(tier, UserPermission.AddCustomEndpoints))
            {
                return Forbid("Custom endpoints require Developer tier or higher");
            }

            // Update entity from DTO
            var entity = MapToEntity(connection, existingConnection.OwnerId);
            entity.Id = connectionGuid; // Preserve original ID
            var updated = await _connectionService.UpdateConnectionAsync(entity);

            return Ok(MapToDto(updated));
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

            if (!Guid.TryParse(id, out var connectionGuid))
            {
                return BadRequest("Invalid connection ID");
            }

            var connection = await _connectionService.GetConnectionByIdAsync(connectionGuid);

            if (connection == null)
            {
                return NotFound($"Connection {id} not found");
            }

            // Check permissions
            if (!_permissionService.CanManageConnection(userId, connection.OwnerId.ToString(), tier))
            {
                return Forbid("You don't have permission to delete this connection");
            }

            await _connectionService.DeleteConnectionAsync(connectionGuid);

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

            if (!Guid.TryParse(id, out var connectionGuid))
            {
                return BadRequest("Invalid connection ID");
            }

            var connection = await _connectionService.GetConnectionByIdAsync(connectionGuid);

            if (connection == null)
            {
                return NotFound($"Connection {id} not found");
            }

            var (success, message) = await _connectionService.TestConnectionAsync(connection);

            return Ok(new
            {
                success,
                message,
                modelCount = connection.AvailableModels.Count,
                latencyMs = 0
            });
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

    // Helper method to map Connection entity to ProviderConnectionDto
    private static ProviderConnectionDto MapToDto(Connection connection)
    {
        return new ProviderConnectionDto
        {
            Id = connection.Id.ToString(),
            Name = connection.Name,
            Type = connection.Type switch
            {
                ConnectionType.CustomOpenAI => "Local",
                ConnectionType.OpenAI => "Cloud",
                ConnectionType.Anthropic => "Cloud",
                ConnectionType.Google => "Cloud",
                ConnectionType.AzureOpenAI => "Cloud",
                ConnectionType.MCP => "MCP",
                _ => "Cloud"
            },
            Url = connection.BaseUrl ?? string.Empty,
            Auth = "ApiKey",
            ProviderType = connection.Type.ToString(),
            ModelIds = connection.AvailableModels,
            IsEnabled = connection.IsEnabled,
            OwnerId = connection.OwnerId.ToString(),
            Visibility = connection.Visibility.ToString(),
            Tags = new List<string>()
        };
    }

    // Helper method to map ProviderConnectionDto to Connection entity
    private static Connection MapToEntity(ProviderConnectionDto dto, Guid ownerId)
    {
        return new Connection
        {
            Id = Guid.TryParse(dto.Id, out var id) ? id : Guid.NewGuid(),
            Name = dto.Name ?? "Unnamed Connection",
            Description = dto.Description,
            Type = dto.ProviderType switch
            {
                "OpenAI" => ConnectionType.OpenAI,
                "Anthropic" => ConnectionType.Anthropic,
                "Google" => ConnectionType.Google,
                "AzureOpenAI" => ConnectionType.AzureOpenAI,
                "MCP" => ConnectionType.MCP,
                _ when dto.Type == "Local" => ConnectionType.CustomOpenAI,
                _ => ConnectionType.Other
            },
            OwnerId = ownerId,
            BaseUrl = dto.Url,
            EncryptedCredentials = dto.ApiKey, // Would need encryption in production
            IsEnabled = dto.IsEnabled,
            IsBYOK = true,
            AvailableModels = dto.ModelIds ?? new List<string>(),
            Visibility = Enum.TryParse<ConnectionVisibility>(dto.Visibility, out var vis) ? vis : ConnectionVisibility.Private
        };
    }
}
