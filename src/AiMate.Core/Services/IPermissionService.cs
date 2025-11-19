using AiMate.Core.Enums;

namespace AiMate.Core.Services;

/// <summary>
/// Service for checking user permissions and access control
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Get permissions for a specific tier
    /// </summary>
    UserPermission GetPermissionsForTier(UserTier tier);

    /// <summary>
    /// Check if user has a specific permission
    /// </summary>
    bool HasPermission(UserTier tier, UserPermission permission);

    /// <summary>
    /// Check if user can manage a connection (must be owner or admin)
    /// </summary>
    bool CanManageConnection(string userId, string connectionOwnerId, UserTier tier);

    /// <summary>
    /// Check if user can see a connection based on visibility
    /// </summary>
    bool CanAccessConnection(string userId, ConnectionVisibility visibility,
                            List<string> allowedGroups, string? organizationId, UserTier tier);

    /// <summary>
    /// Get maximum connections allowed for tier
    /// </summary>
    int GetMaxConnectionsForTier(UserTier tier);

    /// <summary>
    /// Check if BYOK is enabled for a tier (admin configurable)
    /// </summary>
    bool IsBYOKEnabledForTier(UserTier tier);

    /// <summary>
    /// Set if BYOK is enabled for a tier (admin only)
    /// </summary>
    void SetBYOKEnabledForTier(UserTier tier, bool enabled);
}
