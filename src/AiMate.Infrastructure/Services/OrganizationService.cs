using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for managing organizations and their members
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(
        AiMateDbContext context,
        ILogger<OrganizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Organization?> GetOrganizationByIdAsync(Guid organizationId)
    {
        try
        {
            return await _context.Organizations
                .Include(o => o.Members)
                    .ThenInclude(m => m.User)
                .Include(o => o.Groups)
                .FirstOrDefaultAsync(o => o.Id == organizationId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get organization by ID operation was cancelled for organization {OrganizationId}", organizationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization {OrganizationId}", organizationId);
            throw;
        }
    }

    public async Task<List<Organization>> GetUserOrganizationsAsync(Guid userId)
    {
        try
        {
            return await _context.OrganizationMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.Organization)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get user organizations operation was cancelled for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Organization> CreateOrganizationAsync(Organization organization)
    {
        try
        {
            _context.Organizations.Add(organization);

            // Add owner as a member with Owner role
            var ownerMember = new OrganizationMember
            {
                OrganizationId = organization.Id,
                UserId = organization.OwnerId,
                Role = OrganizationRole.Owner
            };
            _context.OrganizationMembers.Add(ownerMember);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Created organization {OrganizationId} with owner {UserId}",
                organization.Id, organization.OwnerId);

            return organization;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating organization {OrganizationId}", organization.Id);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Create organization operation was cancelled for organization {OrganizationId}", organization.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization {OrganizationId}", organization.Id);
            throw;
        }
    }

    public async Task<Organization> UpdateOrganizationAsync(Organization organization)
    {
        try
        {
            organization.UpdatedAt = DateTime.UtcNow;
            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated organization {OrganizationId}", organization.Id);

            return organization;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating organization {OrganizationId}", organization.Id);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating organization {OrganizationId}", organization.Id);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Update organization operation was cancelled for organization {OrganizationId}", organization.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId}", organization.Id);
            throw;
        }
    }

    public async Task DeleteOrganizationAsync(Guid organizationId)
    {
        try
        {
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null)
            {
                throw new InvalidOperationException($"Organization {organizationId} not found");
            }

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted organization {OrganizationId}", organizationId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting organization {OrganizationId}", organizationId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Delete organization operation was cancelled for organization {OrganizationId}", organizationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {OrganizationId}", organizationId);
            throw;
        }
    }

    public async Task<OrganizationMember> AddMemberAsync(Guid organizationId, Guid userId, OrganizationRole role)
    {
        try
        {
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null)
            {
                throw new InvalidOperationException($"Organization {organizationId} not found");
            }

            // Check if member already exists
            var existingMember = await _context.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId);

            if (existingMember != null)
            {
                throw new InvalidOperationException("User is already a member of this organization");
            }

            // Check member limit
            var memberCount = await _context.OrganizationMembers
                .CountAsync(m => m.OrganizationId == organizationId);

            if (memberCount >= organization.MaxUsers)
            {
                throw new InvalidOperationException("Organization has reached maximum member limit");
            }

            var member = new OrganizationMember
            {
                OrganizationId = organizationId,
                UserId = userId,
                Role = role
            };

            _context.OrganizationMembers.Add(member);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added user {UserId} to organization {OrganizationId} with role {Role}",
                userId, organizationId, role);

            return member;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error adding user {UserId} to organization {OrganizationId}", userId, organizationId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Add member operation was cancelled for organization {OrganizationId}", organizationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to organization {OrganizationId}", userId, organizationId);
            throw;
        }
    }

    public async Task<OrganizationMember> UpdateMemberRoleAsync(Guid organizationId, Guid userId, OrganizationRole role)
    {
        try
        {
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId);

            if (member == null)
            {
                throw new InvalidOperationException("Member not found");
            }

            member.Role = role;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated role for user {UserId} in organization {OrganizationId} to {Role}",
                userId, organizationId, role);

            return member;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating member role for user {UserId} in organization {OrganizationId}", userId, organizationId);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating member role for user {UserId} in organization {OrganizationId}", userId, organizationId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Update member role operation was cancelled for organization {OrganizationId}", organizationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for user {UserId} in organization {OrganizationId}", userId, organizationId);
            throw;
        }
    }

    public async Task RemoveMemberAsync(Guid organizationId, Guid userId)
    {
        try
        {
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId);

            if (member == null)
            {
                throw new InvalidOperationException("Member not found");
            }

            // Don't allow removing the owner
            if (member.Role == OrganizationRole.Owner)
            {
                throw new InvalidOperationException("Cannot remove organization owner");
            }

            _context.OrganizationMembers.Remove(member);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed user {UserId} from organization {OrganizationId}",
                userId, organizationId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error removing user {UserId} from organization {OrganizationId}", userId, organizationId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Remove member operation was cancelled for organization {OrganizationId}", organizationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from organization {OrganizationId}", userId, organizationId);
            throw;
        }
    }

    public async Task<List<OrganizationMember>> GetOrganizationMembersAsync(Guid organizationId)
    {
        try
        {
            return await _context.OrganizationMembers
                .Where(m => m.OrganizationId == organizationId)
                .Include(m => m.User)
                .OrderByDescending(m => m.Role)
                .ThenBy(m => m.JoinedAt)
                .ToListAsync();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get organization members operation was cancelled for organization {OrganizationId}", organizationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    public async Task<OrganizationMember?> GetMemberAsync(Guid organizationId, Guid userId)
    {
        try
        {
            return await _context.OrganizationMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get member operation was cancelled for organization {OrganizationId} and user {UserId}", organizationId, userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving member for organization {OrganizationId} and user {UserId}", organizationId, userId);
            throw;
        }
    }

    public async Task<bool> IsUserMemberAsync(Guid organizationId, Guid userId)
    {
        try
        {
            return await _context.OrganizationMembers
                .AnyAsync(m => m.OrganizationId == organizationId && m.UserId == userId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Is user member operation was cancelled for organization {OrganizationId} and user {UserId}", organizationId, userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is member of organization {OrganizationId}", userId, organizationId);
            throw;
        }
    }

    public async Task<bool> HasPermissionAsync(Guid organizationId, Guid userId, OrganizationRole minimumRole)
    {
        try
        {
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId);

            if (member == null)
            {
                return false;
            }

            return member.Role >= minimumRole;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Has permission operation was cancelled for organization {OrganizationId} and user {UserId}", organizationId, userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permissions for user {UserId} in organization {OrganizationId}", userId, organizationId);
            throw;
        }
    }
}
