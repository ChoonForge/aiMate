namespace AiMate.Core.Entities;

/// <summary>
/// Projects organize multiple workspaces
/// </summary>
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

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
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public decimal? Budget { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
