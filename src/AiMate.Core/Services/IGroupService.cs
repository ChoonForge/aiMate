using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Service for managing groups and their members
/// </summary>
public interface IGroupService
{
    // Group management
    Task<Group?> GetGroupByIdAsync(Guid groupId);
    Task<List<Group>> GetOrganizationGroupsAsync(Guid organizationId);
    Task<List<Group>> GetUserGroupsAsync(Guid userId);
    Task<Group> CreateGroupAsync(Group group);
    Task<Group> UpdateGroupAsync(Group group);
    Task DeleteGroupAsync(Guid groupId);

    // Member management
    Task<GroupMember> AddMemberAsync(Guid groupId, Guid userId, GroupRole role);
    Task<GroupMember> UpdateMemberRoleAsync(Guid groupId, Guid userId, GroupRole role);
    Task RemoveMemberAsync(Guid groupId, Guid userId);
    Task<List<GroupMember>> GetGroupMembersAsync(Guid groupId);
    Task<GroupMember?> GetMemberAsync(Guid groupId, Guid userId);

    // Permission checks
    Task<bool> IsUserMemberAsync(Guid groupId, Guid userId);
    Task<bool> HasPermissionAsync(Guid groupId, Guid userId, GroupRole minimumRole);
}
