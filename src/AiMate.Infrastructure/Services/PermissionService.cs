using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for managing user permissions and access control
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly ILogger<PermissionService> _logger;
    private readonly Dictionary<UserTier, bool> _byokEnabledByTier;

    public PermissionService(ILogger<PermissionService> logger)
    {
        _logger = logger;

        // Default BYOK enabled settings (admin can override)
        _byokEnabledByTier = new Dictionary<UserTier, bool>
        {
            { UserTier.Free, true },       // Free tier can BYOK
            { UserTier.BYOK, true },       // BYOK tier (obviously)
            { UserTier.Developer, true },   // Developer tier can BYOK
            { UserTier.Admin, true }        // Admin can do everything
        };
    }

    public UserPermission GetPermissionsForTier(UserTier tier)
    {
        return tier switch
        {
            UserTier.Free => UserPermission.FreeTier,
            UserTier.BYOK => UserPermission.BYOKTier,
            UserTier.Developer => UserPermission.DeveloperTier,
            UserTier.Admin => UserPermission.AdminTier,
            _ => UserPermission.None
        };
    }

    public bool HasPermission(UserTier tier, UserPermission permission)
    {
        var tierPermissions = GetPermissionsForTier(tier);
        return (tierPermissions & permission) == permission;
    }

    public bool CanManageConnection(string userId, string connectionOwnerId, UserTier tier)
    {
        // Admin can manage everything
        if (tier == UserTier.Admin)
            return true;

        // User can manage their own connections if they have the permission
        if (userId == connectionOwnerId && HasPermission(tier, UserPermission.ManageOwnConnections))
            return true;

        return false;
    }

    public bool CanAccessConnection(string userId, ConnectionVisibility visibility,
                                    List<string> allowedGroups, string? organizationId, UserTier tier)
    {
        // Admin can access everything
        if (tier == UserTier.Admin)
            return true;

        return visibility switch
        {
            ConnectionVisibility.Public => true,  // Everyone can access public connections
            ConnectionVisibility.Private => false, // Would need to check if user is owner (handled elsewhere)
            ConnectionVisibility.Organization => !string.IsNullOrEmpty(organizationId), // Need org membership
            ConnectionVisibility.Group => allowedGroups.Any(), // Need group membership (simplified)
            _ => false
        };
    }

    public int GetMaxConnectionsForTier(UserTier tier)
    {
        return tier switch
        {
            UserTier.Free => 3,          // Free: 3 connections
            UserTier.BYOK => 10,         // BYOK: 10 connections
            UserTier.Developer => 50,    // Developer: 50 connections
            UserTier.Admin => int.MaxValue,  // Admin: unlimited
            _ => 0
        };
    }

    public bool IsBYOKEnabledForTier(UserTier tier)
    {
        return _byokEnabledByTier.GetValueOrDefault(tier, false);
    }

    /// <summary>
    /// Admin method to enable/disable BYOK for a tier
    /// </summary>
    public void SetBYOKEnabledForTier(UserTier tier, bool enabled)
    {
        _logger.LogInformation("Setting BYOK enabled for tier {Tier} to {Enabled}", tier, enabled);
        _byokEnabledByTier[tier] = enabled;
    }
}
