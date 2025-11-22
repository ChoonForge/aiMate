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
        return await _context.Groups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Include(g => g.Organization)
            .Include(g => g.Owner)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<List<Group>> GetOrganizationGroupsAsync(Guid organizationId)
    {
        return await _context.Groups
            .Where(g => g.OrganizationId == organizationId && g.IsActive)
            .Include(g => g.Owner)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Group>> GetUserGroupsAsync(Guid userId)
    {
        return await _context.GroupMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.Group)
            .Where(g => g.IsActive)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Group> CreateGroupAsync(Group group)
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

    public async Task<Group> UpdateGroupAsync(Group group)
    {
        group.UpdatedAt = DateTime.UtcNow;
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated group {GroupId}", group.Id);

        return group;
    }

    public async Task DeleteGroupAsync(Guid groupId)
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

    public async Task<GroupMember> AddMemberAsync(Guid groupId, Guid userId, GroupRole role)
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

    public async Task<GroupMember> UpdateMemberRoleAsync(Guid groupId, Guid userId, GroupRole role)
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

    public async Task RemoveMemberAsync(Guid groupId, Guid userId)
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

    public async Task<List<GroupMember>> GetGroupMembersAsync(Guid groupId)
    {
        return await _context.GroupMembers
            .Where(m => m.GroupId == groupId)
            .Include(m => m.User)
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.JoinedAt)
            .ToListAsync();
    }

    public async Task<GroupMember?> GetMemberAsync(Guid groupId, Guid userId)
    {
        return await _context.GroupMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
    }

    public async Task<bool> IsUserMemberAsync(Guid groupId, Guid userId)
    {
        return await _context.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
    }

    public async Task<bool> HasPermissionAsync(Guid groupId, Guid userId, GroupRole minimumRole)
    {
        var member = await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

        if (member == null)
        {
            return false;
        }

        return member.Role >= minimumRole;
    }
}
