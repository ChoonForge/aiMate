using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Knowledge service implementation for managing knowledge base articles
/// </summary>
public class KnowledgeService : IKnowledgeService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<KnowledgeService> _logger;

    public KnowledgeService(
        AiMateDbContext context,
        ILogger<KnowledgeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<KnowledgeItem>> GetUserKnowledgeItemsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeItems
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<KnowledgeItem?> GetKnowledgeItemByIdAsync(
        Guid knowledgeItemId,
        CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeItems
            .FirstOrDefaultAsync(k => k.Id == knowledgeItemId, cancellationToken);
    }

    public async Task<KnowledgeItem> CreateKnowledgeItemAsync(
        KnowledgeItem knowledgeItem,
        CancellationToken cancellationToken = default)
    {
        _context.KnowledgeItems.Add(knowledgeItem);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created knowledge item {KnowledgeItemId} for user {UserId}",
            knowledgeItem.Id, knowledgeItem.UserId);

        return knowledgeItem;
    }

    public async Task<KnowledgeItem> UpdateKnowledgeItemAsync(
        KnowledgeItem knowledgeItem,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.KnowledgeItems.FindAsync(
            new object[] { knowledgeItem.Id }, cancellationToken);

        if (existing == null)
        {
            throw new InvalidOperationException($"Knowledge item {knowledgeItem.Id} not found");
        }

        existing.Title = knowledgeItem.Title;
        existing.Content = knowledgeItem.Content;
        existing.Summary = knowledgeItem.Summary;
        existing.ContentType = knowledgeItem.ContentType;
        existing.Type = knowledgeItem.Type;
        existing.Tags = knowledgeItem.Tags;
        existing.Collection = knowledgeItem.Collection;
        existing.Category = knowledgeItem.Category;
        existing.SourceUrl = knowledgeItem.SourceUrl;
        existing.Source = knowledgeItem.Source;
        existing.Visibility = knowledgeItem.Visibility;
        existing.IsFeatured = knowledgeItem.IsFeatured;
        existing.IsVerified = knowledgeItem.IsVerified;
        existing.IsPublished = knowledgeItem.IsPublished;
        existing.IsPinned = knowledgeItem.IsPinned;
        existing.ViewCount = knowledgeItem.ViewCount;
        existing.ReferenceCount = knowledgeItem.ReferenceCount;
        existing.UpvoteCount = knowledgeItem.UpvoteCount;
        existing.DownvoteCount = knowledgeItem.DownvoteCount;
        existing.Embedding = knowledgeItem.Embedding;
        existing.WorkspaceId = knowledgeItem.WorkspaceId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated knowledge item {KnowledgeItemId}", knowledgeItem.Id);

        return existing;
    }

    public async Task DeleteKnowledgeItemAsync(
        Guid knowledgeItemId,
        CancellationToken cancellationToken = default)
    {
        var knowledgeItem = await _context.KnowledgeItems.FindAsync(
            new object[] { knowledgeItemId }, cancellationToken);

        if (knowledgeItem != null)
        {
            _context.KnowledgeItems.Remove(knowledgeItem);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted knowledge item {KnowledgeItemId}", knowledgeItemId);
        }
    }

    public async Task<List<KnowledgeItem>> SearchKnowledgeItemsAsync(
        Guid userId,
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeItems
            .Where(k => k.UserId == userId &&
                       (k.Title.Contains(searchTerm) || k.Content.Contains(searchTerm)))
            .OrderByDescending(k => k.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<KnowledgeItem>> GetKnowledgeItemsByTypeAsync(
        Guid userId,
        string type,
        CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeItems
            .Where(k => k.UserId == userId && k.Type == type)
            .OrderByDescending(k => k.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<KnowledgeItem>> GetKnowledgeItemsByTagAsync(
        Guid userId,
        string tag,
        CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeItems
            .Where(k => k.UserId == userId && k.Tags.Contains(tag))
            .OrderByDescending(k => k.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<KnowledgeItem>> GetKnowledgeItemsByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeItems
            .Where(k => k.WorkspaceId == workspaceId)
            .OrderByDescending(k => k.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetUserTagsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var items = await _context.KnowledgeItems
            .Where(k => k.UserId == userId)
            .ToListAsync(cancellationToken);

        return items
            .SelectMany(k => k.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<List<string>> GetUserTypesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeItems
            .Where(k => k.UserId == userId)
            .Select(k => k.Type)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(cancellationToken);
    }
}
