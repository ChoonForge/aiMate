using AiMate.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// Knowledge base item - documents, notes, code snippets, etc.
/// </summary>
public class KnowledgeItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(100000)]
    public required string Content { get; set; }

    /// <summary>
    /// Short summary of the content
    /// </summary>
    [MaxLength(1000)]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Content format: markdown, plain, html
    /// </summary>
    [MaxLength(50)]
    public string ContentType { get; set; } = "markdown";

    /// <summary>
    /// Type: Article, Guide, Reference, Tutorial, FAQ, Document, Note, Code, WebPage
    /// </summary>
    public KnowledgeType Type { get; set; } = KnowledgeType.Article;

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Collection name for grouping
    /// </summary>
    [MaxLength(200)]
    public string? Collection { get; set; }

    /// <summary>
    /// Category for organization
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Source URL if from web
    /// </summary>
    [Url]
    [MaxLength(2048)]
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Original source/author information
    /// </summary>
    [MaxLength(500)]
    public string? Source { get; set; }

    /// <summary>
    /// Visibility: Private, Shared, Public
    /// </summary>
    [MaxLength(50)]
    public string Visibility { get; set; } = "Private";

    /// <summary>
    /// Feature this article (show prominently)
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Article is verified/reviewed
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Article is published (visible to others based on visibility)
    /// </summary>
    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// Pin to top of list
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Number of times viewed
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ViewCount { get; set; }

    /// <summary>
    /// Number of times referenced in conversations/notes
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ReferenceCount { get; set; }

    /// <summary>
    /// Upvote count for rating
    /// </summary>
    [Range(0, int.MaxValue)]
    public int UpvoteCount { get; set; }

    /// <summary>
    /// Downvote count for rating
    /// </summary>
    [Range(0, int.MaxValue)]
    public int DownvoteCount { get; set; }

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

    public DateTime? PublishedAt { get; set; }

    public DateTime? LastViewedAt { get; set; }
}
