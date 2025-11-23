using AiMate.Core.Enums;
using System.ComponentModel.DataAnnotations;

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

    [MaxLength(100000)]
    public required string Content { get; set; }

    /// <summary>
    /// Structured content (for tables, forms, etc.)
    /// </summary>
    [MaxLength(100000)]
    public string? StructuredContentJson { get; set; }

    /// <summary>
    /// Tool calls made in this message
    /// </summary>
    public List<ToolCall> ToolCalls { get; set; } = new();

    /// <summary>
    /// User rating (1-5 stars, or thumbs up/down)
    /// </summary>
    [Range(1, 5)]
    public int? Rating { get; set; }

    /// <summary>
    /// User feedback on this message
    /// </summary>
    [MaxLength(5000)]
    public string? Feedback { get; set; }

    /// <summary>
    /// Tokens used for this message (if AI-generated)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? TokensUsed { get; set; }

    /// <summary>
    /// Cost in NZD (for tracking)
    /// </summary>
    [Range(0, 999999.99)]
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

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object>? Metadata { get; set; }
}
