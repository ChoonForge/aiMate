namespace AiMate.Core.Enums;

/// <summary>
/// Message roles in a conversation
/// </summary>
public enum MessageRole
{
    /// <summary>
    /// Message from the user
    /// </summary>
    User,

    /// <summary>
    /// Message from the AI assistant
    /// </summary>
    Assistant,

    /// <summary>
    /// System message (prompts, instructions)
    /// </summary>
    System,

    /// <summary>
    /// Tool execution result
    /// </summary>
    Tool
}
