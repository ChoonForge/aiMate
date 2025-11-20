using AiMate.Core.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AiMate.Web.Authorization;

/// <summary>
/// Authorization requirement that checks if user has specific permission
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public UserPermission Permission { get; }

    public PermissionRequirement(UserPermission permission)
    {
        Permission = permission;
    }
}
