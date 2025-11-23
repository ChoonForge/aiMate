using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for managing groups and their members
/// </summary>
public class GroupService : IGroupService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<GroupService> _logger;

    public GroupService(
        AiMateDbContext context,
        ILogger<GroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Group?> GetGroupByIdAsync(Guid groupId)
    {
        try
        {
            return await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .Include(g => g.Organization)
                .Include(g => g.Owner)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get group by ID operation was cancelled for group {GroupId}", groupId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving group {GroupId}", groupId);
            throw;
        }
    }

    public async Task<List<Group>> GetOrganizationGroupsAsync(Guid organizationId)
    {
        try
        {
            return await _context.Groups
                .Where(g => g.OrganizationId == organizationId && g.IsActive)
                .Include(g => g.Owner)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get organization groups operation was cancelled for organization {OrganizationId}", organizationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving groups for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    public async Task<List<Group>> GetUserGroupsAsync(Guid userId)
    {
        try
        {
            return await _context.GroupMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.Group)
                .Where(g => g.IsActive)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get user groups operation was cancelled for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving groups for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Group> CreateGroupAsync(Group group)
    {
        try
        {
            _context.Groups.Add(group);

            // Add owner as a member with Owner role
            var ownerMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = group.OwnerId,
                Role = GroupRole.Owner
            };
            _context.GroupMembers.Add(ownerMember);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Created group {GroupId} with owner {UserId}",
                group.Id, group.OwnerId);

            return group;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating group {GroupId}", group.Id);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Create group operation was cancelled for group {GroupId}", group.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group {GroupId}", group.Id);
            throw;
        }
    }

    public async Task<Group> UpdateGroupAsync(Group group)
    {
        try
        {
            group.UpdatedAt = DateTime.UtcNow;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated group {GroupId}", group.Id);

            return group;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating group {GroupId}", group.Id);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating group {GroupId}", group.Id);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Update group operation was cancelled for group {GroupId}", group.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", group.Id);
            throw;
        }
    }

    public async Task DeleteGroupAsync(Guid groupId)
    {
        try
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                throw new InvalidOperationException($"Group {groupId} not found");
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted group {GroupId}", groupId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting group {GroupId}", groupId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Delete group operation was cancelled for group {GroupId}", groupId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", groupId);
            throw;
        }
    }

    public async Task<GroupMember> AddMemberAsync(Guid groupId, Guid userId, GroupRole role)
    {
        try
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                throw new InvalidOperationException($"Group {groupId} not found");
            }

            // Check if member already exists
            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (existingMember != null)
            {
                throw new InvalidOperationException("User is already a member of this group");
            }

            var member = new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                Role = role
            };

            _context.GroupMembers.Add(member);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added user {UserId} to group {GroupId} with role {Role}",
                userId, groupId, role);

            return member;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error adding user {UserId} to group {GroupId}", userId, groupId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Add member operation was cancelled for group {GroupId}", groupId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to group {GroupId}", userId, groupId);
            throw;
        }
    }

    public async Task<GroupMember> UpdateMemberRoleAsync(Guid groupId, Guid userId, GroupRole role)
    {
        try
        {
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (member == null)
            {
                throw new InvalidOperationException("Member not found");
            }

            member.Role = role;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated role for user {UserId} in group {GroupId} to {Role}",
                userId, groupId, role);

            return member;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating member role for user {UserId} in group {GroupId}", userId, groupId);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating member role for user {UserId} in group {GroupId}", userId, groupId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Update member role operation was cancelled for group {GroupId}", groupId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for user {UserId} in group {GroupId}", userId, groupId);
            throw;
        }
    }

    public async Task RemoveMemberAsync(Guid groupId, Guid userId)
    {
        try
        {
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (member == null)
            {
                throw new InvalidOperationException("Member not found");
            }

            // Don't allow removing the owner
            if (member.Role == GroupRole.Owner)
            {
                throw new InvalidOperationException("Cannot remove group owner");
            }

            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed user {UserId} from group {GroupId}",
                userId, groupId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error removing user {UserId} from group {GroupId}", userId, groupId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Remove member operation was cancelled for group {GroupId}", groupId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from group {GroupId}", userId, groupId);
            throw;
        }
    }

    public async Task<List<GroupMember>> GetGroupMembersAsync(Guid groupId)
    {
        try
        {
            return await _context.GroupMembers
                .Where(m => m.GroupId == groupId)
                .Include(m => m.User)
                .OrderByDescending(m => m.Role)
                .ThenBy(m => m.JoinedAt)
                .ToListAsync();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get group members operation was cancelled for group {GroupId}", groupId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for group {GroupId}", groupId);
            throw;
        }
    }

    public async Task<GroupMember?> GetMemberAsync(Guid groupId, Guid userId)
    {
        try
        {
            return await _context.GroupMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get member operation was cancelled for group {GroupId} and user {UserId}", groupId, userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving member for group {GroupId} and user {UserId}", groupId, userId);
            throw;
        }
    }

    public async Task<bool> IsUserMemberAsync(Guid groupId, Guid userId)
    {
        try
        {
            return await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Is user member operation was cancelled for group {GroupId} and user {UserId}", groupId, userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is member of group {GroupId}", userId, groupId);
            throw;
        }
    }

    public async Task<bool> HasPermissionAsync(Guid groupId, Guid userId, GroupRole minimumRole)
    {
        try
        {
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (member == null)
            {
                return false;
            }

            return member.Role >= minimumRole;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Has permission operation was cancelled for group {GroupId} and user {UserId}", groupId, userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permissions for user {UserId} in group {GroupId}", userId, groupId);
            throw;
        }
    }
}
