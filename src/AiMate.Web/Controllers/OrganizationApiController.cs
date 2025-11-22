using AiMate.Core.Entities;
using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing organizations and their members
/// </summary>
[ApiController]
[Route("api/v1/organizations")]
[Authorize]
public class OrganizationApiController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<OrganizationApiController> _logger;

    public OrganizationApiController(
        IOrganizationService organizationService,
        ILogger<OrganizationApiController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all organizations for a user
    /// </summary>
    /// <param name="userId">User ID to retrieve organizations for</param>
    /// <returns>List of organizations</returns>
    /// <response code="200">Returns list of user's organizations</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Organization>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserOrganizations([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var organizations = await _organizationService.GetUserOrganizationsAsync(userGuid);
            return Ok(organizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizations for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to get organizations", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific organization by ID
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>Organization details with members and groups</returns>
    /// <response code="200">Returns the organization</response>
    /// <response code="404">Organization not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganization(Guid id)
    {
        try
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null)
            {
                return NotFound($"Organization {id} not found");
            }

            return Ok(organization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization {OrganizationId}", id);
            return StatusCode(500, new { error = "Failed to get organization", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new organization
    /// </summary>
    /// <param name="request">Organization creation request</param>
    /// <returns>Created organization</returns>
    /// <response code="201">Organization created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
    {
        try
        {
            var organization = new Organization
            {
                Name = request.Name,
                Description = request.Description,
                OwnerId = request.OwnerId,
                MaxUsers = request.MaxUsers > 0 ? request.MaxUsers : 10
            };

            var created = await _organizationService.CreateOrganizationAsync(organization);

            _logger.LogInformation("Created organization {OrganizationId}", created.Id);

            return CreatedAtAction(nameof(GetOrganization), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization");
            return StatusCode(500, new { error = "Failed to create organization", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="request">Organization update request</param>
    /// <returns>Updated organization</returns>
    /// <response code="200">Organization updated successfully</response>
    /// <response code="404">Organization not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] UpdateOrganizationRequest request)
    {
        try
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null)
            {
                return NotFound($"Organization {id} not found");
            }

            if (!string.IsNullOrEmpty(request.Name))
                organization.Name = request.Name;
            if (request.Description != null)
                organization.Description = request.Description;
            if (request.IsActive.HasValue)
                organization.IsActive = request.IsActive.Value;
            if (request.MaxUsers.HasValue && request.MaxUsers.Value > 0)
                organization.MaxUsers = request.MaxUsers.Value;

            var updated = await _organizationService.UpdateOrganizationAsync(organization);

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId}", id);
            return StatusCode(500, new { error = "Failed to update organization", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete an organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Organization deleted successfully</response>
    /// <response code="404">Organization not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        try
        {
            await _organizationService.DeleteOrganizationAsync(id);

            _logger.LogInformation("Deleted organization {OrganizationId}", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {OrganizationId}", id);
            return StatusCode(500, new { error = "Failed to delete organization", details = ex.Message });
        }
    }

    // ============================================================================
    // Member Management
    // ============================================================================

    /// <summary>
    /// Get all members of an organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <returns>List of organization members</returns>
    /// <response code="200">Returns list of members</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}/members")]
    [ProducesResponseType(typeof(List<OrganizationMember>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        try
        {
            var members = await _organizationService.GetOrganizationMembersAsync(id);
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members for organization {OrganizationId}", id);
            return StatusCode(500, new { error = "Failed to get members", details = ex.Message });
        }
    }

    /// <summary>
    /// Add a member to an organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="request">Member addition request</param>
    /// <returns>Created member record</returns>
    /// <response code="201">Member added successfully</response>
    /// <response code="400">Invalid request or member limit reached</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id}/members")]
    [ProducesResponseType(typeof(OrganizationMember), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberRequest request)
    {
        try
        {
            var member = await _organizationService.AddMemberAsync(id, request.UserId, request.Role);

            _logger.LogInformation("Added member {UserId} to organization {OrganizationId}", request.UserId, id);

            return CreatedAtAction(nameof(GetMembers), new { id }, member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to organization {OrganizationId}", id);
            return StatusCode(500, new { error = "Failed to add member", details = ex.Message });
        }
    }

    /// <summary>
    /// Update a member's role in an organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="userId">User ID of the member</param>
    /// <param name="request">Role update request</param>
    /// <returns>Updated member record</returns>
    /// <response code="200">Member role updated successfully</response>
    /// <response code="404">Member not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}/members/{userId}")]
    [ProducesResponseType(typeof(OrganizationMember), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid userId, [FromBody] UpdateMemberRoleRequest request)
    {
        try
        {
            var member = await _organizationService.UpdateMemberRoleAsync(id, userId, request.Role);

            _logger.LogInformation("Updated role for member {UserId} in organization {OrganizationId}",
                userId, id);

            return Ok(member);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member role in organization {OrganizationId}", id);
            return StatusCode(500, new { error = "Failed to update member role", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove a member from an organization
    /// </summary>
    /// <param name="id">Organization ID</param>
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
            await _organizationService.RemoveMemberAsync(id, userId);

            _logger.LogInformation("Removed member {UserId} from organization {OrganizationId}", userId, id);

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
            _logger.LogError(ex, "Error removing member from organization {OrganizationId}", id);
            return StatusCode(500, new { error = "Failed to remove member", details = ex.Message });
        }
    }
}

// ============================================================================
// Request/Response Models
// ============================================================================

public class CreateOrganizationRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public int MaxUsers { get; set; } = 10;
}

public class UpdateOrganizationRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? MaxUsers { get; set; }
}

public class AddMemberRequest
{
    public Guid UserId { get; set; }
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;
}

public class UpdateMemberRoleRequest
{
    public OrganizationRole Role { get; set; }
}
