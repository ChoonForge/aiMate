using AiMate.Core.Enums;

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
    /// Short summary of the content
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Content format: markdown, plain, html
    /// </summary>
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
    public string? Collection { get; set; }

    /// <summary>
    /// Category for organization
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Source URL if from web
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Original source/author information
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Visibility: Private, Shared, Public
    /// </summary>
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
    public int ViewCount { get; set; }

    /// <summary>
    /// Number of times referenced in conversations/notes
    /// </summary>
    public int ReferenceCount { get; set; }

    /// <summary>
    /// Upvote count for rating
    /// </summary>
    public int UpvoteCount { get; set; }

    /// <summary>
    /// Downvote count for rating
    /// </summary>
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
