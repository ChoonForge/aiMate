using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Conversation service - CRUD operations for conversations and messages
/// </summary>
public interface IConversationService
{
    /// <summary>
    /// Get all conversations in a workspace
    /// </summary>
    Task<List<Conversation>> GetWorkspaceConversationsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conversation by ID with messages
    /// </summary>
    Task<Conversation?> GetConversationByIdAsync(
        Guid conversationId,
        bool includeMessages = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new conversation
    /// </summary>
    Task<Conversation> CreateConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing conversation
    /// </summary>
    Task<Conversation> UpdateConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete conversation
    /// </summary>
    Task DeleteConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add message to conversation
    /// </summary>
    Task<Message> AddMessageAsync(
        Guid conversationId,
        Message message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get messages in a conversation
    /// </summary>
    Task<List<Message>> GetConversationMessagesAsync(
        Guid conversationId,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update message
    /// </summary>
    Task<Message> UpdateMessageAsync(
        Message message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete message
    /// </summary>
    Task DeleteMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Branch conversation from a specific message
    /// </summary>
    Task<Conversation> BranchConversationAsync(
        Guid conversationId,
        Guid fromMessageId,
        string newTitle,
        CancellationToken cancellationToken = default);
}
