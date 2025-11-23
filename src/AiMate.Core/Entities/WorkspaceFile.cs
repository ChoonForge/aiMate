using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// File attached to a workspace
/// </summary>
public class WorkspaceFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    [MaxLength(500)]
    public required string FileName { get; set; }

    [MaxLength(200)]
    public required string ContentType { get; set; }

    [Range(0, long.MaxValue)]
    public long SizeBytes { get; set; }

    /// <summary>
    /// Storage path or blob reference
    /// </summary>
    [MaxLength(1000)]
    public required string StoragePath { get; set; }

    /// <summary>
    /// Extracted text content (for search/RAG)
    /// </summary>
    [MaxLength(100000)]
    public string? ExtractedText { get; set; }

    /// <summary>
    /// FilePath
    /// </summary>
    [MaxLength(1000)]
    public string? FilePath { get; set; }

    /// <summary>
    /// Extracted mimetype
    /// </summary>
    [MaxLength(200)]
    public string? MimeType { get; set; }

    /// <summary>
    /// File Size
    /// </summary>
    [Range(0, long.MaxValue)]
    public long? FileSize { get; set; }

    /// <summary>
    /// Vector embedding of content
    /// </summary>
    public float[]? Embedding { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Description { get; set; }
}
