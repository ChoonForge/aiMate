using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Api.Controllers;

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
    /// <param name="userId">User ID to retrieve workspaces for</param>
    /// <returns>List of workspaces owned by the user</returns>
    /// <response code="200">Returns list of workspaces</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<WorkspaceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkspaces([FromQuery] string userId)
    {
        try
        {
            _logger.LogInformation("Fetching workspaces for user {UserId}", userId);

            // TODO: Get userId from HttpContext.User claims once authentication is fully configured
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var workspaces = await _workspaceService.GetUserWorkspacesAsync(userGuid);

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
    /// <param name="id">Workspace ID</param>
    /// <returns>Workspace details</returns>
    /// <response code="200">Returns the workspace</response>
    /// <response code="404">Workspace not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkspaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="request">Workspace creation request with name, description, type, and personality</param>
    /// <param name="userId">User ID creating the workspace</param>
    /// <returns>Created workspace details</returns>
    /// <response code="201">Workspace created successfully</response>
    /// <response code="400">Invalid request or user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkspaceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceRequest request, [FromQuery] string userId)
    {
        try
        {
            _logger.LogInformation("Creating workspace: {Name} for user {UserId}", request.Name, userId);

            // TODO: Get userId from HttpContext.User claims once authentication is fully configured
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

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
                UserId = userGuid
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
    /// <param name="id">Workspace ID to update</param>
    /// <param name="request">Workspace update request with modified fields</param>
    /// <returns>Updated workspace details</returns>
    /// <response code="200">Workspace updated successfully</response>
    /// <response code="404">Workspace not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WorkspaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Delete a workspace and all its conversations
    /// </summary>
    /// <param name="id">Workspace ID to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">Workspace deleted successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="workspaceId">Workspace ID to retrieve conversations from</param>
    /// <returns>List of conversations in the workspace</returns>
    /// <response code="200">Returns list of conversations with message counts</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{workspaceId}/conversations")]
    [ProducesResponseType(typeof(List<ConversationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get a specific conversation by ID
    /// </summary>
    /// <param name="workspaceId">Workspace ID containing the conversation</param>
    /// <param name="conversationId">Conversation ID to retrieve</param>
    /// <returns>Conversation details</returns>
    /// <response code="200">Returns the conversation</response>
    /// <response code="404">Conversation not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{workspaceId}/conversations/{conversationId}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetConversation(Guid workspaceId, Guid conversationId)
    {
        try
        {
            _logger.LogInformation("Fetching conversation {ConversationId} from workspace {WorkspaceId}", conversationId, workspaceId);

            var conversation = await _conversationService.GetConversationByIdAsync(conversationId);

            if (conversation == null || conversation.WorkspaceId != workspaceId)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            var conversationDto = new ConversationDto
            {
                Id = conversation.Id.GetHashCode(),
                WorkspaceId = conversation.WorkspaceId.GetHashCode(),
                Title = conversation.Title,
                ModelId = conversation.ModelId,
                IsPinned = conversation.IsPinned,
                IsArchived = conversation.IsArchived,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                MessageCount = conversation.Messages?.Count ?? 0
            };

            return Ok(conversationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversation");
            return StatusCode(500, new { error = "Failed to fetch conversation", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new conversation in a workspace
    /// </summary>
    /// <param name="workspaceId">Workspace ID to create conversation in</param>
    /// <param name="request">Conversation creation request with title</param>
    /// <returns>Created conversation details</returns>
    /// <response code="201">Conversation created successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{workspaceId}/conversations")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                nameof(GetConversation),
                new { workspaceId, conversationId = created.Id },
                conversationDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return StatusCode(500, new { error = "Failed to create conversation", message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing conversation
    /// </summary>
    /// <param name="workspaceId">Workspace ID containing the conversation</param>
    /// <param name="conversationId">Conversation ID to update</param>
    /// <param name="request">Conversation update request with modified fields</param>
    /// <returns>Updated conversation details</returns>
    /// <response code="200">Conversation updated successfully</response>
    /// <response code="404">Conversation not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{workspaceId}/conversations/{conversationId}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateConversation(Guid workspaceId, Guid conversationId, [FromBody] UpdateConversationRequest request)
    {
        try
        {
            _logger.LogInformation("Updating conversation {ConversationId} in workspace {WorkspaceId}", conversationId, workspaceId);

            var conversation = await _conversationService.GetConversationByIdAsync(conversationId);

            if (conversation == null || conversation.WorkspaceId != workspaceId)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Title))
            {
                conversation.Title = request.Title;
            }

            if (!string.IsNullOrEmpty(request.ModelId))
            {
                conversation.ModelId = request.ModelId;
            }

            if (request.IsPinned.HasValue)
            {
                conversation.IsPinned = request.IsPinned.Value;
            }

            if (request.IsArchived.HasValue)
            {
                conversation.IsArchived = request.IsArchived.Value;
            }

            await _conversationService.UpdateConversationAsync(conversation);

            var conversationDto = new ConversationDto
            {
                Id = conversation.Id.GetHashCode(),
                WorkspaceId = conversation.WorkspaceId.GetHashCode(),
                Title = conversation.Title,
                ModelId = conversation.ModelId,
                IsPinned = conversation.IsPinned,
                IsArchived = conversation.IsArchived,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                MessageCount = conversation.Messages?.Count ?? 0
            };

            return Ok(conversationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation");
            return StatusCode(500, new { error = "Failed to update conversation", message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a conversation
    /// </summary>
    /// <param name="workspaceId">Workspace ID containing the conversation</param>
    /// <param name="conversationId">Conversation ID to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">Conversation deleted successfully</response>
    /// <response code="404">Conversation not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{workspaceId}/conversations/{conversationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteConversation(Guid workspaceId, Guid conversationId)
    {
        try
        {
            _logger.LogInformation("Deleting conversation {ConversationId} from workspace {WorkspaceId}", conversationId, workspaceId);

            var conversation = await _conversationService.GetConversationByIdAsync(conversationId);

            if (conversation == null || conversation.WorkspaceId != workspaceId)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            await _conversationService.DeleteConversationAsync(conversationId);

            return Ok(new { success = true, message = "Conversation deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation");
            return StatusCode(500, new { error = "Failed to delete conversation", message = ex.Message });
        }
    }
}
