using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AiMate.Shared.Models;

/// <summary>
/// Represents a single conversation thread
/// </summary>
public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "New Chat";
    public List<Message> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsArchived { get; set; }
    public bool IsPinned { get; set; }
    public string? FolderId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Message within a conversation
/// </summary>
public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Role { get; set; } = "user"; // "user", "assistant", "system"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Model { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    public List<Attachment>? Attachments { get; set; }
    public MessageMetadata? Metadata { get; set; }
    public bool IsStreaming { get; set; }
}

/// <summary>
/// Message metadata for structured content, tokens, etc.
/// </summary>
public class MessageMetadata
{
    public int? TokensUsed { get; set; }
    public double? ResponseTime { get; set; }
    public string? FinishReason { get; set; }
    public Dictionary<string, object>? ToolCalls { get; set; }
    public StructuredContent? StructuredData { get; set; }
}

/// <summary>
/// Structured content rendering data
/// </summary>
public class StructuredContent
{
    public string Type { get; set; } = "text"; // "table", "form", "list", "keyvalue", "code"
    public object? Data { get; set; }
    public string? Language { get; set; } // For code blocks
}

/// <summary>
/// File or knowledge attachment
/// </summary>
public class Attachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "file"; // "file", "knowledge", "url", "image"
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }
    public long? Size { get; set; }
    public string? MimeType { get; set; }
    public string? Content { get; set; } // For text-based attachments
}

public enum MessageStatus
{
    Pending,
    Sent,
    Delivered,
    Error,
    Streaming
}

/// <summary>
/// AI Model configuration
/// </summary>
public class AIModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = "litellm";
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public ModelCapabilities? Capabilities { get; set; }
    public ModelPricing? Pricing { get; set; }
    public long Context { get; set; }
    public long MaxTokens { get; set; }
}

public class ModelCapabilities
{
    public bool SupportsVision { get; set; }
    public bool SupportsTools { get; set; }
    public bool SupportsStreaming { get; set; } = true;
    public int MaxTokens { get; set; } = 4096;
    public int ContextWindow { get; set; } = 4096;
}

public class ModelPricing
{
    public decimal InputCostPer1kTokens { get; set; }
    public decimal OutputCostPer1kTokens { get; set; }
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Knowledge base item
/// </summary>
public class KnowledgeItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public KnowledgeType Type { get; set; } = KnowledgeType.Document;
}

public enum KnowledgeType
{
    Document,
    WebPage,
    File,
    Note,
    Code
}

/// <summary>
/// User preferences
/// </summary>
public class UserPreferences
{
    public string Theme { get; set; } = "dark";
    public string ColorScheme { get; set; } = "purple"; // purple, blue, green, red
    public bool EnableSounds { get; set; } = true;
    public bool EnableNotifications { get; set; } = true;
    public string DefaultModel { get; set; } = "gpt-4";
    public int MessageFontSize { get; set; } = 14;
    public bool ShowTimestamps { get; set; } = true;
    public bool EnableMarkdown { get; set; } = true;
    public bool EnableCodeHighlighting { get; set; } = true;
}

/// <summary>
/// Folder for organizing conversations
/// </summary>
public class Folder
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
