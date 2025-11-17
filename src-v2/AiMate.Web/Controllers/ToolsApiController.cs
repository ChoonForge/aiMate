using AiMate.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// Tools API for Developer tier - MCP tool execution
/// </summary>
[ApiController]
[Route("api/v1/tools")]
public class ToolsApiController : ControllerBase
{
    private readonly IMCPToolService _toolService;
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<ToolsApiController> _logger;

    public ToolsApiController(
        IMCPToolService toolService,
        IApiKeyService apiKeyService,
        ILogger<ToolsApiController> logger)
    {
        _toolService = toolService;
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    /// <summary>
    /// List available tools for workspace
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListTools([FromQuery] Guid workspaceId)
    {
        var apiKey = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = await _apiKeyService.ValidateApiKeyAsync(apiKey);

        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        try
        {
            _logger.LogInformation("Listing tools for workspace {WorkspaceId}", workspaceId);

            var tools = await _toolService.GetAvailableToolsAsync(
                workspaceId,
                userId.Value);

            return Ok(new
            {
                tools = tools.Select(t => new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = t.Parameters.Select(p => new
                    {
                        name = p.Key,
                        type = p.Value.Type,
                        description = p.Value.Description,
                        required = p.Value.Required
                    })
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list tools");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Execute a tool
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteTool([FromBody] ToolExecutionRequest request)
    {
        var apiKey = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = await _apiKeyService.ValidateApiKeyAsync(apiKey);

        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        try
        {
            _logger.LogInformation("Executing tool {ToolName} for workspace {WorkspaceId}",
                request.ToolName, request.WorkspaceId);

            var result = await _toolService.ExecuteToolAsync(
                request.ToolName,
                request.Parameters,
                request.WorkspaceId,
                userId.Value);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    error = result.Error,
                    toolName = request.ToolName
                });
            }

            return Ok(new
            {
                success = true,
                toolName = request.ToolName,
                result = result.Result,
                executedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool execution failed");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Check if a specific tool is available
    /// </summary>
    [HttpGet("{toolName}/available")]
    public async Task<IActionResult> IsToolAvailable(string toolName, [FromQuery] Guid workspaceId)
    {
        var apiKey = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = await _apiKeyService.ValidateApiKeyAsync(apiKey);

        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        try
        {
            var isAvailable = await _toolService.IsToolAvailableAsync(
                toolName,
                workspaceId,
                userId.Value);

            return Ok(new
            {
                toolName = toolName,
                available = isAvailable
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check tool availability");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class ToolExecutionRequest
{
    public Guid WorkspaceId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}
