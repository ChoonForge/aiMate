using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System.Diagnostics;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Implementation of search service with full-text and semantic search
/// </summary>
public class SearchService : ISearchService
{
    private readonly AiMateDbContext _context;
    private readonly IEmbeddingService _embeddingService;

    public SearchService(AiMateDbContext context, IEmbeddingService embeddingService)
    {
        _context = context;
        _embeddingService = embeddingService;
    }

    public async Task<SearchResults<Conversation>> SearchConversationsAsync(
        Guid userId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var lowerQuery = query.ToLower();

        // Search conversations by title or ID
        var conversations = await _context.Conversations
            .Include(c => c.Workspace)
            .Where(c => c.Workspace != null && c.Workspace.UserId == userId)
            .Where(c => c.Title.ToLower().Contains(lowerQuery) ||
                       c.Id.ToString().Contains(lowerQuery))
            .OrderByDescending(c => c.UpdatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        stopwatch.Stop();

        return new SearchResults<Conversation>
        {
            Results = conversations.Select(c => new SearchResult<Conversation>
            {
                Item = c,
                Score = CalculateRelevanceScore(c.Title, query),
                Highlight = GetHighlight(c.Title, query, 100)
            }).ToList(),
            TotalCount = conversations.Count,
            Query = query,
            QueryTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<SearchResults<Message>> SearchMessagesAsync(
        Guid userId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var lowerQuery = query.ToLower();

        // Search messages by content
        var messages = await _context.Messages
            .Include(m => m.Conversation)
                .ThenInclude(c => c!.Workspace)
            .Where(m => m.Conversation != null &&
                       m.Conversation.Workspace != null &&
                       m.Conversation.Workspace.UserId == userId)
            .Where(m => m.Content.ToLower().Contains(lowerQuery))
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        stopwatch.Stop();

        return new SearchResults<Message>
        {
            Results = messages.Select(m => new SearchResult<Message>
            {
                Item = m,
                Score = CalculateRelevanceScore(m.Content, query),
                Highlight = GetHighlight(m.Content, query, 200)
            }).ToList(),
            TotalCount = messages.Count,
            Query = query,
            QueryTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<SearchResults<KnowledgeItem>> SearchKnowledgeSemanticAsync(
        Guid userId,
        string query,
        double similarityThreshold = 0.7,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Generate embedding for the query
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

        if (queryEmbedding == null || queryEmbedding.Length == 0)
        {
            // Fallback to full-text search if embedding fails
            return await SearchKnowledgeFullTextAsync(userId, query, limit, cancellationToken);
        }

        var queryVector = new Vector(queryEmbedding);

        // Perform semantic search using cosine similarity
        // Note: pgvector uses <=> for cosine distance (1 - similarity)
        var results = await _context.KnowledgeItems
            .Where(k => k.UserId == userId && k.Embedding != null)
            .OrderBy(k => k.Embedding!.CosineDistance(queryVector))
            .Take(limit * 2) // Get more results to filter by threshold
            .ToListAsync(cancellationToken);

        // Calculate similarity scores and filter by threshold
        var scoredResults = results
            .Select(k => new SearchResult<KnowledgeItem>
            {
                Item = k,
                Score = 1 - k.Embedding!.CosineDistance(queryVector), // Convert distance to similarity
                Highlight = GetHighlight(k.Content, query, 200)
            })
            .Where(r => r.Score >= similarityThreshold)
            .Take(limit)
            .ToList();

        stopwatch.Stop();

        return new SearchResults<KnowledgeItem>
        {
            Results = scoredResults,
            TotalCount = scoredResults.Count,
            Query = query,
            QueryTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<SearchResults<KnowledgeItem>> SearchKnowledgeFullTextAsync(
        Guid userId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var lowerQuery = query.ToLower();

        // Full-text search in title and content
        var results = await _context.KnowledgeItems
            .Where(k => k.UserId == userId)
            .Where(k => k.Title.ToLower().Contains(lowerQuery) ||
                       k.Content.ToLower().Contains(lowerQuery) ||
                       (k.Tags != null && k.Tags.Any(t => t.ToLower().Contains(lowerQuery))))
            .OrderByDescending(k => k.UpdatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        stopwatch.Stop();

        return new SearchResults<KnowledgeItem>
        {
            Results = results.Select(k => new SearchResult<KnowledgeItem>
            {
                Item = k,
                Score = CalculateRelevanceScore(k.Title + " " + k.Content, query),
                Highlight = GetHighlight(k.Content, query, 200)
            }).ToList(),
            TotalCount = results.Count,
            Query = query,
            QueryTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<GlobalSearchResults> SearchGlobalAsync(
        Guid userId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Search all content types in parallel
        var conversationsTask = SearchConversationsAsync(userId, query, limit, cancellationToken);
        var messagesTask = SearchMessagesAsync(userId, query, limit, cancellationToken);
        var knowledgeTask = SearchKnowledgeFullTextAsync(userId, query, limit, cancellationToken);

        await Task.WhenAll(conversationsTask, messagesTask, knowledgeTask);

        stopwatch.Stop();

        return new GlobalSearchResults
        {
            Conversations = conversationsTask.Result.Results,
            Messages = messagesTask.Result.Results,
            KnowledgeItems = knowledgeTask.Result.Results,
            Query = query,
            QueryTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    #region Helper Methods

    /// <summary>
    /// Calculate relevance score based on keyword frequency and position
    /// </summary>
    private static double CalculateRelevanceScore(string text, string query)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query))
            return 0;

        var lowerText = text.ToLower();
        var lowerQuery = query.ToLower();

        // Exact match gets highest score
        if (lowerText.Contains(lowerQuery))
        {
            var index = lowerText.IndexOf(lowerQuery);
            // Boost score if match is near the beginning
            var positionBoost = 1.0 - (index / (double)lowerText.Length) * 0.5;
            return 1.0 * positionBoost;
        }

        // Partial word match
        var queryWords = lowerQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchCount = queryWords.Count(word => lowerText.Contains(word));

        return (double)matchCount / queryWords.Length * 0.7;
    }

    /// <summary>
    /// Get highlighted excerpt around the matched text
    /// </summary>
    private static string? GetHighlight(string text, string query, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query))
            return null;

        var lowerText = text.ToLower();
        var lowerQuery = query.ToLower();

        var index = lowerText.IndexOf(lowerQuery);
        if (index == -1)
        {
            // If exact query not found, try first word
            var firstWord = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (firstWord != null)
                index = lowerText.IndexOf(firstWord.ToLower());
        }

        if (index == -1)
        {
            // No match found, return beginning of text
            return text.Length > maxLength
                ? text.Substring(0, maxLength) + "..."
                : text;
        }

        // Get excerpt around the match
        var start = Math.Max(0, index - maxLength / 2);
        var length = Math.Min(maxLength, text.Length - start);

        var excerpt = text.Substring(start, length);

        // Add ellipsis
        if (start > 0) excerpt = "..." + excerpt;
        if (start + length < text.Length) excerpt += "...";

        return excerpt;
    }

    #endregion
}
