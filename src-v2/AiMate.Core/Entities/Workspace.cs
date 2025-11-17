using AiMate.Core.Enums;

namespace AiMate.Core.Entities;

/// <summary>
/// A workspace is the core unit of organization - not just a conversation, but an environment
/// </summary>
public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public string? Description { get; set; }

    public WorkspaceType Type { get; set; } = WorkspaceType.General;

    public PersonalityMode DefaultPersonality { get; set; } = PersonalityMode.KiwiMate;

    /// <summary>
    /// User who owns this workspace
    /// </summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Optional project this workspace belongs to
    /// </summary>
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }

    /// <summary>
    /// Conversations within this workspace
    /// </summary>
    public List<Conversation> Conversations { get; set; } = new();

    /// <summary>
    /// Files attached to this workspace
    /// </summary>
    public List<WorkspaceFile> Files { get; set; } = new();

    /// <summary>
    /// Enabled MCP tools for this workspace
    /// </summary>
    public List<string> EnabledTools { get; set; } = new();

    /// <summary>
    /// Workspace context (key-value pairs for persistent state)
    /// </summary>
    public Dictionary<string, string> Context { get; set; } = new();

    public bool IsArchived { get; set; }

    public bool IsPinned { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
