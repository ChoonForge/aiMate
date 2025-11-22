using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Service for managing organizations and their members
/// </summary>
public interface IOrganizationService
{
    // Organization management
    Task<Organization?> GetOrganizationByIdAsync(Guid organizationId);
    Task<List<Organization>> GetUserOrganizationsAsync(Guid userId);
    Task<Organization> CreateOrganizationAsync(Organization organization);
    Task<Organization> UpdateOrganizationAsync(Organization organization);
    Task DeleteOrganizationAsync(Guid organizationId);

    // Member management
    Task<OrganizationMember> AddMemberAsync(Guid organizationId, Guid userId, OrganizationRole role);
    Task<OrganizationMember> UpdateMemberRoleAsync(Guid organizationId, Guid userId, OrganizationRole role);
    Task RemoveMemberAsync(Guid organizationId, Guid userId);
    Task<List<OrganizationMember>> GetOrganizationMembersAsync(Guid organizationId);
    Task<OrganizationMember?> GetMemberAsync(Guid organizationId, Guid userId);

    // Permission checks
    Task<bool> IsUserMemberAsync(Guid organizationId, Guid userId);
    Task<bool> HasPermissionAsync(Guid organizationId, Guid userId, OrganizationRole minimumRole);
}
