using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// Projects organize multiple workspaces
/// </summary>
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Unique project key (e.g., "PROJ-001")
    /// </summary>
    [MaxLength(50)]
    public string? Key { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Workspaces in this project
    /// </summary>
    public List<Workspace> Workspaces { get; set; } = new();

    /// <summary>
    /// Project metadata
    /// </summary>
    [MaxLength(200)]
    public string? Owner { get; set; }

    [EmailAddress]
    [MaxLength(320)]
    public string? OwnerEmail { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Planning";

    [MaxLength(50)]
    public string Priority { get; set; } = "Medium";

    [Range(0, 999999999999.99)]
    public decimal? Budget { get; set; }

    /// <summary>
    /// Project timeline
    /// </summary>
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }

    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    /// <summary>
    /// Tags for organization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Team member IDs or email addresses
    /// </summary>
    public List<string> TeamMembers { get; set; } = new();

    /// <summary>
    /// Additional notes
    /// </summary>
    [MaxLength(5000)]
    public string? Notes { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
