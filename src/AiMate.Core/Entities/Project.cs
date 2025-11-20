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
    public string? Key { get; set; }

    public required string Name { get; set; }

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
    public string? Owner { get; set; }
    public string? OwnerEmail { get; set; }
    public string Status { get; set; } = "Planning";
    public string Priority { get; set; } = "Medium";
    public decimal? Budget { get; set; }

    /// <summary>
    /// Project timeline
    /// </summary>
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
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
    public string? Notes { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
