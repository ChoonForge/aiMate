using AiMate.Core.Entities;
using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing groups and their members
/// </summary>
[ApiController]
[Route("api/v1/groups")]
[Authorize]
public class GroupApiController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<GroupApiController> _logger;

    public GroupApiController(
        IGroupService groupService,
        ILogger<GroupApiController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    /// <summary>
    /// Get all groups for a user
    /// </summary>
    /// <param name="userId">User ID to retrieve groups for</param>
    /// <returns>List of groups</returns>
    /// <response code="200">Returns list of user's groups</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Group>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserGroups([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var groups = await _groupService.GetUserGroupsAsync(userGuid);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to get groups", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all groups for an organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <returns>List of groups in the organization</returns>
    /// <response code="200">Returns list of organization's groups</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("organization/{organizationId}")]
    [ProducesResponseType(typeof(List<Group>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganizationGroups(Guid organizationId)
    {
        try
        {
            var groups = await _groupService.GetOrganizationGroupsAsync(organizationId);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups for organization {OrganizationId}", organizationId);
            return StatusCode(500, new { error = "Failed to get groups", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific group by ID
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <returns>Group details with members</returns>
    /// <response code="200">Returns the group</response>
    /// <response code="404">Group not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Group), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        try
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound($"Group {id} not found");
            }

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", id);
            return StatusCode(500, new { error = "Failed to get group", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new group
    /// </summary>
    /// <param name="request">Group creation request</param>
    /// <returns>Created group</returns>
    /// <response code="201">Group created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(Group), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        try
        {
            var group = new Group
            {
                Name = request.Name,
                Description = request.Description,
                OrganizationId = request.OrganizationId,
                OwnerId = request.OwnerId
            };

            var created = await _groupService.CreateGroupAsync(group);

            _logger.LogInformation("Created group {GroupId}", created.Id);

            return CreatedAtAction(nameof(GetGroup), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return StatusCode(500, new { error = "Failed to create group", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing group
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <param name="request">Group update request</param>
    /// <returns>Updated group</returns>
    /// <response code="200">Group updated successfully</response>
    /// <response code="404">Group not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Group), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupRequest request)
    {
        try
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound($"Group {id} not found");
            }

            if (!string.IsNullOrEmpty(request.Name))
                group.Name = request.Name;
            if (request.Description != null)
                group.Description = request.Description;
            if (request.IsActive.HasValue)
                group.IsActive = request.IsActive.Value;

            var updated = await _groupService.UpdateGroupAsync(group);

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", id);
            return StatusCode(500, new { error = "Failed to update group", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a group
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Group deleted successfully</response>
    /// <response code="404">Group not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        try
        {
            await _groupService.DeleteGroupAsync(id);

            _logger.LogInformation("Deleted group {GroupId}", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", id);
            return StatusCode(500, new { error = "Failed to delete group", details = ex.Message });
        }
    }

    // ============================================================================
    // Member Management
    // ============================================================================

    /// <summary>
    /// Get all members of a group
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <returns>List of group members</returns>
    /// <response code="200">Returns list of members</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}/members")]
    [ProducesResponseType(typeof(List<GroupMember>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        try
        {
            var members = await _groupService.GetGroupMembersAsync(id);
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members for group {GroupId}", id);
            return StatusCode(500, new { error = "Failed to get members", details = ex.Message });
        }
    }

    /// <summary>
    /// Add a member to a group
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <param name="request">Member addition request</param>
    /// <returns>Created member record</returns>
    /// <response code="201">Member added successfully</response>
    /// <response code="400">Invalid request or user already a member</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id}/members")]
    [ProducesResponseType(typeof(GroupMember), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddGroupMemberRequest request)
    {
        try
        {
            var member = await _groupService.AddMemberAsync(id, request.UserId, request.Role);

            _logger.LogInformation("Added member {UserId} to group {GroupId}", request.UserId, id);

            return CreatedAtAction(nameof(GetMembers), new { id }, member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to group {GroupId}", id);
            return StatusCode(500, new { error = "Failed to add member", details = ex.Message });
        }
    }

    /// <summary>
    /// Update a member's role in a group
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <param name="userId">User ID of the member</param>
    /// <param name="request">Role update request</param>
    /// <returns>Updated member record</returns>
    /// <response code="200">Member role updated successfully</response>
    /// <response code="404">Member not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}/members/{userId}")]
    [ProducesResponseType(typeof(GroupMember), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid userId, [FromBody] UpdateGroupMemberRoleRequest request)
    {
        try
        {
            var member = await _groupService.UpdateMemberRoleAsync(id, userId, request.Role);

            _logger.LogInformation("Updated role for member {UserId} in group {GroupId}", userId, id);

            return Ok(member);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member role in group {GroupId}", id);
            return StatusCode(500, new { error = "Failed to update member role", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove a member from a group
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <param name="userId">User ID of the member to remove</param>
    /// <returns>No content</returns>
    /// <response code="204">Member removed successfully</response>
    /// <response code="400">Cannot remove owner</response>
    /// <response code="404">Member not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        try
        {
            await _groupService.RemoveMemberAsync(id, userId);

            _logger.LogInformation("Removed member {UserId} from group {GroupId}", userId, id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("owner"))
            {
                return BadRequest(new { error = ex.Message });
            }
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from group {GroupId}", id);
            return StatusCode(500, new { error = "Failed to remove member", details = ex.Message });
        }
    }
}

// ============================================================================
// Request/Response Models
// ============================================================================

public class CreateGroupRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid OwnerId { get; set; }
}

public class UpdateGroupRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class AddGroupMemberRequest
{
    public Guid UserId { get; set; }
    public GroupRole Role { get; set; } = GroupRole.Member;
}

public class UpdateGroupMemberRoleRequest
{
    public GroupRole Role { get; set; }
}
