using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Knowledge graph service - semantic memory that persists
/// </summary>
public interface IKnowledgeGraphService
{
    /// <summary>
    /// Extract knowledge entities from conversation
    /// </summary>
    Task<List<KnowledgeItem>> ExtractFromConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Semantic search with vector similarity
    /// </summary>
    Task<List<KnowledgeItem>> SearchAsync(
        string query,
        Guid userId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get related knowledge items
    /// </summary>
    Task<List<KnowledgeItem>> GetRelatedAsync(
        Guid knowledgeItemId,
        int limit = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate embedding for text
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add or update knowledge item
    /// </summary>
    Task<KnowledgeItem> UpsertKnowledgeItemAsync(
        KnowledgeItem item,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get relevant context for a query (for prompt injection)
    /// </summary>
    Task<string> GetRelevantContextAsync(
        string query,
        Guid userId,
        int maxItems = 5,
        CancellationToken cancellationToken = default);
}
