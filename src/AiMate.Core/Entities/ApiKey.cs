namespace AiMate.Core.Entities;

/// <summary>
/// API Key entity for Developer tier access
/// </summary>
public class ApiKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string HashedKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }

    // Rate limiting
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerDay { get; set; } = 10000;

    // Navigation properties
    public User User { get; set; } = null!;
}
