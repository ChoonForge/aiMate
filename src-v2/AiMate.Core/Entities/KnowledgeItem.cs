namespace AiMate.Core.Entities;

/// <summary>
/// Knowledge base item - documents, notes, code snippets, etc.
/// </summary>
public class KnowledgeItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    /// <summary>
    /// Type: Document, Note, Code, WebPage, etc.
    /// </summary>
    public string Type { get; set; } = "Note";

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Source URL if from web
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Vector embedding for semantic search (stored in pgvector)
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Related workspace (optional)
    /// </summary>
    public Guid? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
