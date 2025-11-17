namespace AiMate.Core.Entities;

/// <summary>
/// File attached to a workspace
/// </summary>
public class WorkspaceFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public required string FileName { get; set; }

    public required string ContentType { get; set; }

    public long SizeBytes { get; set; }

    /// <summary>
    /// Storage path or blob reference
    /// </summary>
    public required string StoragePath { get; set; }

    /// <summary>
    /// Extracted text content (for search/RAG)
    /// </summary>
    public string? ExtractedText { get; set; }

    /// <summary>
    /// Vector embedding of content
    /// </summary>
    public float[]? Embedding { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
