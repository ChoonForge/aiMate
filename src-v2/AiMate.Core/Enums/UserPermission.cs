namespace AiMate.Core.Enums;

/// <summary>
/// User permissions for connection and model management
/// Flags enum allows combining multiple permissions
/// </summary>
[Flags]
public enum UserPermission
{
    /// <summary>
    /// No permissions
    /// </summary>
    None = 0,

    /// <summary>
    /// Can use platform-provided API keys and models
    /// </summary>
    UsePlatformKeys = 1 << 0,  // 1

    /// <summary>
    /// Can add own API keys (BYOK - Bring Your Own Key)
    /// </summary>
    AddOwnKeys = 1 << 1,  // 2

    /// <summary>
    /// Can configure custom endpoints (non-standard APIs)
    /// </summary>
    AddCustomEndpoints = 1 << 2,  // 4

    /// <summary>
    /// Can manage own connections (edit, delete)
    /// </summary>
    ManageOwnConnections = 1 << 3,  // 8

    /// <summary>
    /// Can share connections with groups
    /// </summary>
    ShareConnections = 1 << 4,  // 16

    /// <summary>
    /// Can create and manage custom models
    /// </summary>
    ManageModels = 1 << 5,  // 32

    /// <summary>
    /// Can add and configure MCP servers
    /// </summary>
    ManageMCP = 1 << 6,  // 64

    /// <summary>
    /// Can view usage analytics for own connections
    /// </summary>
    ViewOwnAnalytics = 1 << 7,  // 128

    /// <summary>
    /// Can view all users' analytics (admin only)
    /// </summary>
    ViewAllAnalytics = 1 << 8,  // 256

    /// <summary>
    /// Full admin access to all connections
    /// </summary>
    AdminAccess = 1 << 9,  // 512

    /// <summary>
    /// Free tier permissions: BYOK only, manage own connections
    /// </summary>
    FreeTier = AddOwnKeys | ManageOwnConnections | ViewOwnAnalytics,

    /// <summary>
    /// BYOK tier permissions: Everything free tier has
    /// </summary>
    BYOKTier = FreeTier,

    /// <summary>
    /// Developer tier permissions: Platform keys + BYOK + custom endpoints + sharing
    /// </summary>
    DeveloperTier = UsePlatformKeys | AddOwnKeys | AddCustomEndpoints
                   | ManageOwnConnections | ShareConnections | ManageModels
                   | ManageMCP | ViewOwnAnalytics,

    /// <summary>
    /// Admin permissions: Everything
    /// </summary>
    AdminTier = UsePlatformKeys | AddOwnKeys | AddCustomEndpoints
               | ManageOwnConnections | ShareConnections | ManageModels
               | ManageMCP | ViewOwnAnalytics | ViewAllAnalytics | AdminAccess
}
