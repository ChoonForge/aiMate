using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// Workspace API for managing workspaces and conversations
/// </summary>
[ApiController]
[Route("api/v1/workspaces")]
public class WorkspaceApiController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly ILogger<WorkspaceApiController> _logger;

    public WorkspaceApiController(
        IWorkspaceService workspaceService,
        ILogger<WorkspaceApiController> logger)
    {
        _workspaceService = workspaceService;
        _logger = logger;
    }

    // ============================================================================
    // WORKSPACE CRUD
    // ============================================================================

    /// <summary>
    /// Get all workspaces for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkspaces()
    {
        try
        {
            _logger.LogInformation("Fetching workspaces");

            // DEMO MODE: Using hardcoded user ID
            // IMPLEMENTATION NEEDED: Get from authenticated user context
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var workspaces = await _workspaceService.GetUserWorkspacesAsync(userId);

            var workspaceDtos = workspaces.Select(w => new WorkspaceDto
            {
                Id = w.Id.GetHashCode(), // Convert Guid to int for DTO
                Name = w.Name,
                Description = w.Description,
                Type = w.Type.ToString(),
                Personality = w.DefaultPersonality.ToString(),
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            }).ToList();

            return Ok(workspaceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching workspaces");
            return StatusCode(500, new { error = "Failed to fetch workspaces", message = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific workspace by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkspace(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching workspace {WorkspaceId}", id);

            var workspace = await _workspaceService.GetWorkspaceByIdAsync(id);

            if (workspace == null)
            {
                return NotFound(new { error = "Workspace not found" });
            }

            var workspaceDto = new WorkspaceDto
            {
                Id = workspace.Id.GetHashCode(), // Convert Guid to int for DTO
                Name = workspace.Name,
                Description = workspace.Description,
                Type = workspace.Type.ToString(),
                Personality = workspace.DefaultPersonality.ToString(),
                CreatedAt = workspace.CreatedAt,
                UpdatedAt = workspace.UpdatedAt
            };

            return Ok(workspaceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching workspace");
            return StatusCode(500, new { error = "Failed to fetch workspace", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new workspace
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceRequest request)
    {
        try
        {
            _logger.LogInformation("Creating workspace: {Name}", request.Name);

            // DEMO MODE: Using hardcoded user ID
            // IMPLEMENTATION NEEDED: Get from authenticated user context
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            // Parse enums
            if (!Enum.TryParse<Core.Enums.WorkspaceType>(request.Type, out var workspaceType))
            {
                workspaceType = Core.Enums.WorkspaceType.General;
            }

            if (!Enum.TryParse<Core.Enums.PersonalityMode>(request.Personality, out var personality))
            {
                personality = Core.Enums.PersonalityMode.KiwiMate;
            }

            var workspace = new Core.Entities.Workspace
            {
                Name = request.Name,
                Description = request.Description,
                Type = workspaceType,
                DefaultPersonality = personality,
                UserId = userId
            };

            var created = await _workspaceService.CreateWorkspaceAsync(workspace);

            var workspaceDto = new WorkspaceDto
            {
                Id = created.Id.GetHashCode(), // Convert Guid to int for DTO
                Name = created.Name,
                Description = created.Description,
                Type = created.Type.ToString(),
                Personality = created.DefaultPersonality.ToString(),
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt
            };

            return CreatedAtAction(
                nameof(GetWorkspace),
                new { id = created.Id },
                workspaceDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workspace");
            return StatusCode(500, new { error = "Failed to create workspace", message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing workspace
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] UpdateWorkspaceRequest request)
    {
        try
        {
            _logger.LogInformation("Updating workspace {WorkspaceId}", id);

            var workspace = await _workspaceService.GetWorkspaceByIdAsync(id);

            if (workspace == null)
            {
                return NotFound(new { error = "Workspace not found" });
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Name))
            {
                workspace.Name = request.Name;
            }

            if (request.Description != null)
            {
                workspace.Description = request.Description;
            }

            if (!string.IsNullOrEmpty(request.Type))
            {
                if (Enum.TryParse<Core.Enums.WorkspaceType>(request.Type, out var workspaceType))
                {
                    workspace.Type = workspaceType;
                }
            }

            if (!string.IsNullOrEmpty(request.Personality))
            {
                if (Enum.TryParse<Core.Enums.PersonalityMode>(request.Personality, out var personality))
                {
                    workspace.DefaultPersonality = personality;
                }
            }

            await _workspaceService.UpdateWorkspaceAsync(workspace);

            var workspaceDto = new WorkspaceDto
            {
                Id = workspace.Id.GetHashCode(), // Convert Guid to int for DTO
                Name = workspace.Name,
                Description = workspace.Description,
                Type = workspace.Type.ToString(),
                Personality = workspace.DefaultPersonality.ToString(),
                CreatedAt = workspace.CreatedAt,
                UpdatedAt = workspace.UpdatedAt
            };

            return Ok(workspaceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workspace");
            return StatusCode(500, new { error = "Failed to update workspace", message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a workspace
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkspace(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting workspace {WorkspaceId}", id);

            await _workspaceService.DeleteWorkspaceAsync(id);

            return Ok(new { success = true, message = "Workspace deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workspace");
            return StatusCode(500, new { error = "Failed to delete workspace", message = ex.Message });
        }
    }

    // ============================================================================
    // CONVERSATION ENDPOINTS (Future)
    // ============================================================================

    /// <summary>
    /// Get all conversations in a workspace
    /// </summary>
    [HttpGet("{workspaceId}/conversations")]
    public Task<IActionResult> GetConversations(int workspaceId)
    {
        try
        {
            _logger.LogInformation("Fetching conversations for workspace {WorkspaceId}", workspaceId);

            // IMPLEMENTATION NEEDED: Get conversations from database
            var conversations = new List<ConversationDto>();

            return Task.FromResult<IActionResult>(Ok(conversations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversations");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to fetch conversations", message = ex.Message }));
        }
    }

    /// <summary>
    /// Create a new conversation in a workspace
    /// </summary>
    [HttpPost("{workspaceId}/conversations")]
    public Task<IActionResult> CreateConversation(int workspaceId, [FromBody] CreateConversationRequest request)
    {
        try
        {
            _logger.LogInformation("Creating conversation in workspace {WorkspaceId}", workspaceId);

            // IMPLEMENTATION NEEDED: Create conversation in database
            var conversation = new ConversationDto
            {
                Id = 1,
                WorkspaceId = workspaceId,
                Title = request.Title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                MessageCount = 0
            };

            return Task.FromResult<IActionResult>(CreatedAtAction(
                nameof(GetConversations),
                new { workspaceId },
                conversation
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to create conversation", message = ex.Message }));
        }
    }
}
