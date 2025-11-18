namespace AiMate.Core.Plugins;

/// <summary>
/// Provides tools/functions that can be called during chat
/// Use cases: Web search, code execution, file operations
/// </summary>
public interface IToolProvider : IPlugin
{
    /// <summary>
    /// Register available tools
    /// </summary>
    IEnumerable<PluginTool> GetTools();

    /// <summary>
    /// Execute a tool
    /// </summary>
    Task<ToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters);
}

/// <summary>
/// Tool definition
/// </summary>
public class PluginTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ToolParameter> Parameters { get; set; } = new();
    public bool RequiresConfirmation { get; set; } = false;
    public bool RequiresAuth { get; set; } = false;
}

/// <summary>
/// Tool parameter definition
/// </summary>
public class ToolParameter
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "string"; // string, number, boolean, array, object
    public bool Required { get; set; } = true;
    public object? DefaultValue { get; set; }
    public List<string>? Enum { get; set; } // Allowed values
}

/// <summary>
/// Result from tool execution
/// </summary>
public class ToolResult
{
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public int? TokensUsed { get; set; }
    public decimal? Cost { get; set; }
}
