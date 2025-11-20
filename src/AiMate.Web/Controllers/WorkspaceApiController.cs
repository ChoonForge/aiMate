using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// Workspace API for managing workspaces and conversations
/// </summary>
[ApiController]
[Route("api/v1/workspaces")]
[Authorize] // Requires authentication
public class WorkspaceApiController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IConversationService _conversationService;
    private readonly ILogger<WorkspaceApiController> _logger;

    public WorkspaceApiController(
        IWorkspaceService workspaceService,
        IConversationService conversationService,
        ILogger<WorkspaceApiController> logger)
    {
        _workspaceService = workspaceService;
        _conversationService = conversationService;
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
    public async Task<IActionResult> GetConversations(Guid workspaceId)
    {
        try
        {
            _logger.LogInformation("Fetching conversations for workspace {WorkspaceId}", workspaceId);

            var conversations = await _conversationService.GetWorkspaceConversationsAsync(workspaceId);

            var conversationDtos = conversations.Select(c => new ConversationDto
            {
                Id = c.Id.GetHashCode(),
                WorkspaceId = c.WorkspaceId.GetHashCode(),
                Title = c.Title,
                ModelId = c.ModelId,
                IsPinned = c.IsPinned,
                IsArchived = c.IsArchived,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MessageCount = c.Messages?.Count ?? 0
            }).ToList();

            return Ok(conversationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversations");
            return StatusCode(500, new { error = "Failed to fetch conversations", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new conversation in a workspace
    /// </summary>
    [HttpPost("{workspaceId}/conversations")]
    public async Task<IActionResult> CreateConversation(Guid workspaceId, [FromBody] CreateConversationRequest request)
    {
        try
        {
            _logger.LogInformation("Creating conversation in workspace {WorkspaceId}", workspaceId);

            var conversation = new Core.Entities.Conversation
            {
                WorkspaceId = workspaceId,
                Title = request.Title
            };

            var created = await _conversationService.CreateConversationAsync(conversation);

            var conversationDto = new ConversationDto
            {
                Id = created.Id.GetHashCode(),
                WorkspaceId = created.WorkspaceId.GetHashCode(),
                Title = created.Title,
                ModelId = created.ModelId,
                IsPinned = created.IsPinned,
                IsArchived = created.IsArchived,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt,
                MessageCount = 0
            };

            return CreatedAtAction(
                nameof(GetConversations),
                new { workspaceId },
                conversationDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return StatusCode(500, new { error = "Failed to create conversation", message = ex.Message });
        }
    }
}
