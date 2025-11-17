using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// MCP Tool Service - Model Context Protocol tool integration
/// </summary>
public interface IMCPToolService
{
    /// <summary>
    /// Get all available tools for a workspace
    /// </summary>
    Task<List<MCPTool>> GetAvailableToolsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a tool with given parameters
    /// </summary>
    Task<MCPToolResult> ExecuteToolAsync(
        string toolName,
        Dictionary<string, object> parameters,
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a new tool
    /// </summary>
    Task RegisterToolAsync(
        MCPTool tool,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a tool is available
    /// </summary>
    Task<bool> IsToolAvailableAsync(
        string toolName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// MCP Tool definition
/// </summary>
public class MCPTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, MCPToolParameter> Parameters { get; set; } = new();
    public bool RequiresAuth { get; set; }
    public List<string> RequiredScopes { get; set; } = new();
}

/// <summary>
/// Tool parameter definition
/// </summary>
public class MCPToolParameter
{
    public string Type { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
}

/// <summary>
/// Tool execution result
/// </summary>
public class MCPToolResult
{
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
