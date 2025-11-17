namespace AiMate.Core.Enums;

/// <summary>
/// User subscription tiers
/// </summary>
public enum UserTier
{
    /// <summary>
    /// Free tier - All Kiwis, fair use limits
    /// </summary>
    Free,

    /// <summary>
    /// BYOK tier - $10/month, bring your own API keys
    /// </summary>
    BYOK,

    /// <summary>
    /// Developer tier - $30/month, Blazor generator + API access
    /// </summary>
    Developer,

    /// <summary>
    /// Admin user
    /// </summary>
    Admin
}
