using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace AiMate.Web.Authorization;

/// <summary>
/// Authorization handler that checks user permissions via PermissionService
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionHandler> _logger;

    public PermissionHandler(
        IPermissionService permissionService,
        ILogger<PermissionHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user tier from claims
        var tierClaim = context.User.FindFirst("tier");
        if (tierClaim == null)
        {
            _logger.LogWarning("User has no tier claim, denying access");
            return Task.CompletedTask; // Fail - no tier claim
        }

        if (!Enum.TryParse<UserTier>(tierClaim.Value, out var tier))
        {
            _logger.LogWarning("Invalid tier value: {Tier}", tierClaim.Value);
            return Task.CompletedTask; // Fail - invalid tier
        }

        // Check if user's tier has the required permission
        if (_permissionService.HasPermission(tier, requirement.Permission))
        {
            context.Succeed(requirement);
            _logger.LogDebug("User with tier {Tier} has permission {Permission}",
                tier, requirement.Permission);
        }
        else
        {
            _logger.LogWarning("User with tier {Tier} lacks permission {Permission}",
                tier, requirement.Permission);
        }

        return Task.CompletedTask;
    }
}
