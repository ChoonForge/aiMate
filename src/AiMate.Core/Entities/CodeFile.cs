namespace AiMate.Core.Entities;

/// <summary>
/// Represents a code file in the virtual file system
/// </summary>
public class CodeFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Project this file belongs to
    /// </summary>
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>
    /// File path relative to project root
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// File content
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Programming language (csharp, razor, json, etc.)
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    /// MIME type
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Whether the file has unsaved changes
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// Whether the file is read-only
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Version number (increments on save)
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Hash of file content for change detection
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// When the file was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the file was last modified
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created the file
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Who last modified the file
    /// </summary>
    public Guid LastModifiedBy { get; set; }

    /// <summary>
    /// Optional tags for organization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Metadata as key-value pairs
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
