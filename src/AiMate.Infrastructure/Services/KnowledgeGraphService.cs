using System.Text;
using System.Text.Json;
using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using AiMate.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Knowledge graph service - semantic memory with pgvector
/// </summary>
public class KnowledgeGraphService : IKnowledgeGraphService
{
    private readonly AiMateDbContext _context;
    private readonly ILiteLLMService _liteLLM;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<KnowledgeGraphService> _logger;

    public KnowledgeGraphService(
        AiMateDbContext context,
        ILiteLLMService liteLLM,
        IEmbeddingService embeddingService,
        ILogger<KnowledgeGraphService> logger)
    {
        _context = context;
        _liteLLM = liteLLM;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<List<KnowledgeItem>> ExtractFromConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        // Build conversation summary
        var conversationText = string.Join("\n\n", conversation.Messages.Select(m =>
            $"{m.Role}: {m.Content}"));

        // Use LLM to extract entities/facts
        var extractionPrompt = @"Extract key facts, concepts, and entities from this conversation.
Return a JSON array of objects with: title, content, tags.

Conversation:
" + conversationText;

        var request = new ChatCompletionRequest
        {
            Model = "gpt-4", // or configured extraction model
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You extract structured knowledge from conversations." },
                new() { Role = "user", Content = extractionPrompt }
            },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _liteLLM.GetChatCompletionAsync(request, cancellationToken);
        var extractedJson = response.Choices.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(extractedJson))
        {
            _logger.LogWarning("No knowledge extracted from conversation {ConversationId}", conversation.Id);
            return new List<KnowledgeItem>();
        }

        try
        {
            // Parse extracted knowledge
            var extracted = JsonSerializer.Deserialize<List<ExtractedKnowledge>>(extractedJson);
            if (extracted == null) return new List<KnowledgeItem>();

            var knowledgeItems = new List<KnowledgeItem>();

            foreach (var item in extracted)
            {
                var embedding = await GenerateEmbeddingAsync(item.Content, cancellationToken);

                var knowledgeItem = new KnowledgeItem
                {
                    UserId = conversation.Workspace!.UserId,
                    WorkspaceId = conversation.WorkspaceId,
                    Title = item.Title,
                    Content = item.Content,
                    Tags = item.Tags ?? new List<string>(),
                    Type = "ExtractedFromConversation",
                    Embedding = embedding
                };

                knowledgeItems.Add(knowledgeItem);
            }

            _context.KnowledgeItems.AddRange(knowledgeItems);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Extracted {Count} knowledge items from conversation {ConversationId}",
                knowledgeItems.Count, conversation.Id);

            return knowledgeItems;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse extracted knowledge: {Json}", extractedJson);
            return new List<KnowledgeItem>();
        }
    }

    public async Task<List<KnowledgeItem>> SearchAsync(
        string query,
        Guid userId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await GenerateEmbeddingAsync(query, cancellationToken);

        // Vector similarity search using pgvector
        // Note: This uses raw SQL because EF Core doesn't fully support pgvector operators yet
        var sql = @"
            SELECT * FROM ""KnowledgeItems""
            WHERE ""UserId"" = {0}
            ORDER BY ""Embedding"" <=> {1}
            LIMIT {2}";

        var results = await _context.KnowledgeItems
            .FromSqlRaw(sql, userId, queryEmbedding, limit)
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<List<KnowledgeItem>> GetRelatedAsync(
        Guid knowledgeItemId,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.KnowledgeItems.FindAsync(new object[] { knowledgeItemId }, cancellationToken);
        if (item?.Embedding == null) return new List<KnowledgeItem>();

        var sql = @"
            SELECT * FROM ""KnowledgeItems""
            WHERE ""Id"" != {0} AND ""UserId"" = {1}
            ORDER BY ""Embedding"" <=> {2}
            LIMIT {3}";

        var results = await _context.KnowledgeItems
            .FromSqlRaw(sql, knowledgeItemId, item.UserId, item.Embedding, limit)
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        // Use real OpenAI embedding service
        return await _embeddingService.GenerateEmbeddingAsync(text, cancellationToken);
    }

    public async Task<KnowledgeItem> UpsertKnowledgeItemAsync(
        KnowledgeItem item,
        CancellationToken cancellationToken = default)
    {
        // Generate or update embedding
        if (item.Embedding == null || item.Embedding.Length == 0)
        {
            item.Embedding = await GenerateEmbeddingAsync(item.Content, cancellationToken);
        }

        var existing = await _context.KnowledgeItems.FindAsync(new object[] { item.Id }, cancellationToken);

        if (existing != null)
        {
            // Update
            existing.Title = item.Title;
            existing.Content = item.Content;
            existing.Tags = item.Tags;
            existing.Embedding = item.Embedding;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Insert
            _context.KnowledgeItems.Add(item);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return item;
    }

    public async Task<string> GetRelevantContextAsync(
        string query,
        Guid userId,
        int maxItems = 5,
        CancellationToken cancellationToken = default)
    {
        var relatedItems = await SearchAsync(query, userId, maxItems, cancellationToken);

        if (!relatedItems.Any())
            return string.Empty;

        var context = new StringBuilder();
        context.AppendLine("Relevant knowledge:");
        context.AppendLine();

        foreach (var item in relatedItems)
        {
            context.AppendLine($"**{item.Title}**");
            context.AppendLine(item.Content);
            if (item.Tags.Any())
            {
                context.AppendLine($"Tags: {string.Join(", ", item.Tags)}");
            }
            context.AppendLine();
        }

        return context.ToString();
    }

    private class ExtractedKnowledge
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public List<string>? Tags { get; set; }
    }
}
