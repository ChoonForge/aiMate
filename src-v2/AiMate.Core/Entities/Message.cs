using AiMate.Core.Enums;

namespace AiMate.Core.Entities;

/// <summary>
/// A message in a conversation
/// </summary>
public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }

    public MessageRole Role { get; set; }

    public required string Content { get; set; }

    /// <summary>
    /// Structured content (for tables, forms, etc.)
    /// </summary>
    public string? StructuredContentJson { get; set; }

    /// <summary>
    /// Tool calls made in this message
    /// </summary>
    public List<ToolCall> ToolCalls { get; set; } = new();

    /// <summary>
    /// User rating (1-5 stars, or thumbs up/down)
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// User feedback on this message
    /// </summary>
    public string? Feedback { get; set; }

    /// <summary>
    /// Tokens used for this message (if AI-generated)
    /// </summary>
    public int? TokensUsed { get; set; }

    /// <summary>
    /// Cost in NZD (for tracking)
    /// </summary>
    public decimal? Cost { get; set; }

    /// <summary>
    /// If this message was edited, link to parent version
    /// </summary>
    public Guid? ParentMessageId { get; set; }
    public Message? ParentMessage { get; set; }

    /// <summary>
    /// Edited versions of this message
    /// </summary>
    public List<Message> EditedVersions { get; set; } = new();

    public bool IsStreaming { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
