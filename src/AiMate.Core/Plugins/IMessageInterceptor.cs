using AiMate.Core.Entities;

namespace AiMate.Core.Plugins;

/// <summary>
/// Intercepts messages before/after processing
/// Use cases: Content filtering, context injection, logging
/// </summary>
public interface IMessageInterceptor : IPlugin
{
    /// <summary>
    /// Called BEFORE message is sent to LLM
    /// Can modify the message, add context, or cancel
    /// </summary>
    Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context);

    /// <summary>
    /// Called AFTER receiving response from LLM
    /// Can modify response, trigger actions, or add metadata
    /// </summary>
    Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context);
}

/// <summary>
/// Result from message interception
/// </summary>
public class MessageInterceptResult
{
    /// <summary>
    /// Should processing continue?
    /// </summary>
    public bool Continue { get; set; } = true;

    /// <summary>
    /// Modified message (null = no changes)
    /// </summary>
    public Message? ModifiedMessage { get; set; }

    /// <summary>
    /// Reason for cancellation (if Continue = false)
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// Additional metadata to store
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Context provided to interceptors
/// </summary>
public class ConversationContext
{
    public Guid ConversationId { get; set; }
    public List<Message> MessageHistory { get; set; } = new();
    public Dictionary<string, object> UserSettings { get; set; } = new();
    public Dictionary<string, object> PluginData { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public string WorkspaceId { get; set; } = string.Empty;
}
