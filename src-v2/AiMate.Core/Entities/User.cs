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
    /// API keys for BYOK tier (encrypted)
    /// </summary>
    public Dictionary<string, string> EncryptedApiKeys { get; set; } = new();

    /// <summary>
    /// User preferences (JSON)
    /// </summary>
    public string? PreferencesJson { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
}
