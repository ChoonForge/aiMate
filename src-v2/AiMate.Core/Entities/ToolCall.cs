namespace AiMate.Core.Entities;

/// <summary>
/// Record of a tool being called during a conversation
/// </summary>
public class ToolCall
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }
    public Message? Message { get; set; }

    public required string ToolName { get; set; }

    /// <summary>
    /// Arguments passed to the tool (JSON)
    /// </summary>
    public string? ArgumentsJson { get; set; }

    /// <summary>
    /// Tool execution result (JSON)
    /// </summary>
    public string? ResultJson { get; set; }

    /// <summary>
    /// Execution status
    /// </summary>
    public string Status { get; set; } = "pending"; // pending, success, error

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    public DateTime CalledAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}
