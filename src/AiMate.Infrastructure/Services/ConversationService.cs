using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Conversation service implementation
/// </summary>
public class ConversationService : IConversationService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        AiMateDbContext context,
        ILogger<ConversationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Conversation>> GetWorkspaceConversationsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.WorkspaceId == workspaceId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation?> GetConversationByIdAsync(
        Guid conversationId,
        bool includeMessages = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Conversations.AsQueryable();

        if (includeMessages)
        {
            query = query
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                    .ThenInclude(m => m.ToolCalls);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
    }

    public async Task<Conversation> CreateConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created conversation {ConversationId} in workspace {WorkspaceId}",
            conversation.Id, conversation.WorkspaceId);

        return conversation;
    }

    public async Task<Conversation> UpdateConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.Conversations.FindAsync(
            new object[] { conversation.Id }, cancellationToken);

        if (existing == null)
        {
            throw new InvalidOperationException($"Conversation {conversation.Id} not found");
        }

        existing.Title = conversation.Title;
        existing.ModelId = conversation.ModelId;
        existing.PersonalityOverride = conversation.PersonalityOverride;
        existing.IsPinned = conversation.IsPinned;
        existing.IsArchived = conversation.IsArchived;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated conversation {ConversationId}", conversation.Id);

        return existing;
    }

    public async Task DeleteConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _context.Conversations.FindAsync(
            new object[] { conversationId }, cancellationToken);

        if (conversation != null)
        {
            _context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted conversation {ConversationId}", conversationId);
        }
    }

    public async Task<Message> AddMessageAsync(
        Guid conversationId,
        Message message,
        CancellationToken cancellationToken = default)
    {
        message.ConversationId = conversationId;
        _context.Messages.Add(message);

        // Update conversation timestamp
        var conversation = await _context.Conversations.FindAsync(
            new object[] { conversationId }, cancellationToken);
        if (conversation != null)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added message {MessageId} to conversation {ConversationId}",
            message.Id, conversationId);

        return message;
    }

    public async Task<List<Message>> GetConversationMessagesAsync(
        Guid conversationId,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.ToolCalls)
            .OrderBy(m => m.CreatedAt);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<Message>)query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Message> UpdateMessageAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.Messages.FindAsync(
            new object[] { message.Id }, cancellationToken);

        if (existing == null)
        {
            throw new InvalidOperationException($"Message {message.Id} not found");
        }

        existing.Content = message.Content;
        existing.Rating = message.Rating;
        existing.Feedback = message.Feedback;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated message {MessageId}", message.Id);

        return existing;
    }

    public async Task DeleteMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var message = await _context.Messages.FindAsync(
            new object[] { messageId }, cancellationToken);

        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted message {MessageId}", messageId);
        }
    }

    public async Task<Conversation> BranchConversationAsync(
        Guid conversationId,
        Guid fromMessageId,
        string newTitle,
        CancellationToken cancellationToken = default)
    {
        var originalConversation = await GetConversationByIdAsync(
            conversationId, true, cancellationToken);

        if (originalConversation == null)
        {
            throw new InvalidOperationException($"Conversation {conversationId} not found");
        }

        // Create new conversation
        var branchedConversation = new Conversation
        {
            Title = newTitle,
            WorkspaceId = originalConversation.WorkspaceId,
            ModelId = originalConversation.ModelId,
            PersonalityOverride = originalConversation.PersonalityOverride,
            ParentConversationId = conversationId
        };

        _context.Conversations.Add(branchedConversation);
        await _context.SaveChangesAsync(cancellationToken);

        // Copy messages up to the branch point
        var messagesToCopy = originalConversation.Messages
            .TakeWhile(m => m.Id != fromMessageId)
            .Concat(originalConversation.Messages.Where(m => m.Id == fromMessageId))
            .ToList();

        foreach (var originalMessage in messagesToCopy)
        {
            var copiedMessage = new Message
            {
                ConversationId = branchedConversation.Id,
                Role = originalMessage.Role,
                Content = originalMessage.Content,
                CreatedAt = originalMessage.CreatedAt
            };

            _context.Messages.Add(copiedMessage);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Branched conversation {NewConversationId} from {OriginalConversationId} at message {MessageId}",
            branchedConversation.Id, conversationId, fromMessageId);

        return branchedConversation;
    }
}
