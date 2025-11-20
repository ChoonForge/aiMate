using AiMate.Core.Enums;

namespace AiMate.Core.Entities;

/// <summary>
/// Application user
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Email { get; set; }

    public required string Username { get; set; }

    public required string PasswordHash { get; set; }

    public UserTier Tier { get; set; } = UserTier.Free;

    /// <summary>
    /// Preferred language/locale (en-NZ, mi-NZ, etc.)
    /// </summary>
    public string Locale { get; set; } = "en-NZ";

    /// <summary>
    /// Default personality preference
    /// </summary>
    public PersonalityMode DefaultPersonality { get; set; } = PersonalityMode.KiwiMate;

    /// <summary>
    /// User's workspaces
    /// </summary>
    public List<Workspace> Workspaces { get; set; } = new();

    /// <summary>
    /// User's projects
    /// </summary>
    public List<Project> Projects { get; set; } = new();

    /// <summary>
    /// User's knowledge items
    /// </summary>
    public List<KnowledgeItem> KnowledgeItems { get; set; } = new();

    /// <summary>
    /// User's API keys for Developer tier access
    /// </summary>
    public List<ApiKey> ApiKeys { get; set; } = new();

    /// <summary>
    /// API keys for BYOK tier (encrypted)
    /// </summary>
    public Dictionary<string, string> EncryptedApiKeys { get; set; } = new();

    /// <summary>
    /// User preferences (JSON)
    /// </summary>
    public string? PreferencesJson { get; set; }

    /// <summary>
    /// Organization memberships
    /// </summary>
    public List<OrganizationMember> OrganizationMemberships { get; set; } = new();

    /// <summary>
    /// Group memberships
    /// </summary>
    public List<GroupMember> GroupMemberships { get; set; } = new();

    /// <summary>
    /// Connections owned by this user
    /// </summary>
    public List<Connection> OwnedConnections { get; set; } = new();

    /// <summary>
    /// Groups owned by this user
    /// </summary>
    public List<Group> OwnedGroups { get; set; } = new();

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
}
