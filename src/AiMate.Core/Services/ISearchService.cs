using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Service for searching across all content types
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Search conversations by title, messages, or metadata
    /// </summary>
    Task<SearchResults<Conversation>> SearchConversationsAsync(
        Guid userId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search messages by content
    /// </summary>
    Task<SearchResults<Message>> SearchMessagesAsync(
        Guid userId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Semantic search in knowledge base using pgvector
    /// </summary>
    Task<SearchResults<KnowledgeItem>> SearchKnowledgeSemanticAsync(
        Guid userId,
        string query,
        double similarityThreshold = 0.7,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Full-text search in knowledge base
    /// </summary>
    Task<SearchResults<KnowledgeItem>> SearchKnowledgeFullTextAsync(
        Guid userId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Global search across all content types
    /// </summary>
    Task<GlobalSearchResults> SearchGlobalAsync(
        Guid userId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Search results with metadata
/// </summary>
public class SearchResults<T>
{
    public List<SearchResult<T>> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int ReturnedCount => Results.Count;
    public string Query { get; set; } = string.Empty;
    public long QueryTimeMs { get; set; }
}

/// <summary>
/// Individual search result with relevance score
/// </summary>
public class SearchResult<T>
{
    public T Item { get; set; } = default!;
    public double Score { get; set; }
    public string? Highlight { get; set; }
}

/// <summary>
/// Global search results across all types
/// </summary>
public class GlobalSearchResults
{
    public List<SearchResult<Conversation>> Conversations { get; set; } = new();
    public List<SearchResult<Message>> Messages { get; set; } = new();
    public List<SearchResult<KnowledgeItem>> KnowledgeItems { get; set; } = new();
    public int TotalResults => Conversations.Count + Messages.Count + KnowledgeItems.Count;
    public string Query { get; set; } = string.Empty;
    public long QueryTimeMs { get; set; }
}
