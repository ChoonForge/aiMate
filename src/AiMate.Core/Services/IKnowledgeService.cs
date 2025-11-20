using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Knowledge service - CRUD operations for knowledge base articles
/// Note: This is separate from IKnowledgeGraphService which handles semantic search
/// This service manages the knowledge articles/items themselves
/// </summary>
public interface IKnowledgeService
{
    /// <summary>
    /// Get all knowledge articles for a user
    /// </summary>
    Task<List<KnowledgeItem>> GetUserKnowledgeItemsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get knowledge item by ID
    /// </summary>
    Task<KnowledgeItem?> GetKnowledgeItemByIdAsync(
        Guid knowledgeItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new knowledge item
    /// </summary>
    Task<KnowledgeItem> CreateKnowledgeItemAsync(
        KnowledgeItem knowledgeItem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing knowledge item
    /// </summary>
    Task<KnowledgeItem> UpdateKnowledgeItemAsync(
        KnowledgeItem knowledgeItem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete knowledge item
    /// </summary>
    Task DeleteKnowledgeItemAsync(
        Guid knowledgeItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search knowledge items by title or content
    /// </summary>
    Task<List<KnowledgeItem>> SearchKnowledgeItemsAsync(
        Guid userId,
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get knowledge items by type (Article, Guide, Reference, etc.)
    /// </summary>
    Task<List<KnowledgeItem>> GetKnowledgeItemsByTypeAsync(
        Guid userId,
        string type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get knowledge items by tag
    /// </summary>
    Task<List<KnowledgeItem>> GetKnowledgeItemsByTagAsync(
        Guid userId,
        string tag,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get knowledge items by workspace
    /// </summary>
    Task<List<KnowledgeItem>> GetKnowledgeItemsByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all unique tags used by a user
    /// </summary>
    Task<List<string>> GetUserTagsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all unique types used by a user
    /// </summary>
    Task<List<string>> GetUserTypesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
