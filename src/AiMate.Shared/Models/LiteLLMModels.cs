namespace AiMate.Shared.Models;

/// <summary>
/// LiteLLM chat completion request
/// </summary>
public class ChatCompletionRequest
{
    public required string Model { get; set; }
    public required List<ChatMessage> Messages { get; set; }
    public bool Stream { get; set; } = false;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2000;
    public List<ToolDefinition>? Tools { get; set; }
}

/// <summary>
/// Chat message
/// </summary>
public class ChatMessage
{
    public required string Role { get; set; } // "system", "user", "assistant", "tool"
    public required string Content { get; set; }
    public string? Name { get; set; }
    public List<ToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// LiteLLM chat completion response
/// </summary>
public class ChatCompletionResponse
{
    public string? Id { get; set; }
    public required string Object { get; set; }
    public long Created { get; set; }
    public required string Model { get; set; }
    public required List<Choice> Choices { get; set; }
    public Usage? Usage { get; set; }
}

public class Choice
{
    public int Index { get; set; }
    public ChatMessage? Message { get; set; }
    public Delta? Delta { get; set; }
    public string? FinishReason { get; set; }
}

public class Delta
{
    public string? Role { get; set; }
    public string? Content { get; set; }
    public List<ToolCall>? ToolCalls { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

public class ToolCall
{
    public string? Id { get; set; }
    public required string Type { get; set; }
    public required ToolFunction Function { get; set; }
}

public class ToolFunction
{
    public required string Name { get; set; }
    public required string Arguments { get; set; }
}

public class ToolDefinition
{
    public required string Type { get; set; }
    public required ToolFunctionDefinition Function { get; set; }
}

public class ToolFunctionDefinition
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required Dictionary<string, object> Parameters { get; set; }
}
