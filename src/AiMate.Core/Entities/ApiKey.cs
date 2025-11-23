using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// API Key entity for Developer tier access
/// </summary>
public class ApiKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string HashedKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    [MaxLength(1000)]
    public string? RevokedReason { get; set; }

    // Rate limiting
    [Range(1, 10000)]
    public int RequestsPerMinute { get; set; } = 60;

    [Range(1, 1000000)]
    public int RequestsPerDay { get; set; } = 10000;

    // Navigation properties
    public User User { get; set; } = null!;
}
