namespace AiMate.Core.Enums;

/// <summary>
/// Visibility level for connections (models, MCP servers, etc.)
/// Controls who can see and use a connection
/// </summary>
public enum ConnectionVisibility
{
    /// <summary>
    /// Private - Only the creator can use this connection
    /// </summary>
    Private = 0,

    /// <summary>
    /// Group - Only specific groups can use this connection
    /// Requires group membership
    /// </summary>
    Group = 1,

    /// <summary>
    /// Organization - All users in the organization can use
    /// </summary>
    Organization = 2,

    /// <summary>
    /// Public - Available to all users platform-wide
    /// Only admins can create public connections
    /// </summary>
    Public = 3
}
