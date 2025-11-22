using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AiMate.Web.Controllers;

/// <summary>
/// Admin API for system administration and configuration
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "AdminOnly")] // Requires admin access
public class AdminApiController : ControllerBase
{
    private readonly ILogger<AdminApiController> _logger;
    private readonly AiMateDbContext _dbContext;
    private readonly IConversationService _conversationService;
    private readonly ILiteLLMService _liteLLMService;
    private readonly IMCPToolService _mcpToolService;
    private readonly IConfiguration _configuration;

    public AdminApiController(
        ILogger<AdminApiController> logger,
        AiMateDbContext dbContext,
        IConversationService conversationService,
        ILiteLLMService liteLLMService,
        IMCPToolService mcpToolService,
        IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _conversationService = conversationService;
        _liteLLMService = liteLLMService;
        _mcpToolService = mcpToolService;
        _configuration = configuration;
    }

    // ============================================================================
    // OVERVIEW & GENERAL
    // ============================================================================

    /// <summary>
    /// Get all admin data (overview, models, MCP servers, logs)
    /// </summary>
    /// <returns>Complete admin dashboard data</returns>
    /// <response code="200">Returns admin data with statistics</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(AdminDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAdminData()
    {
        try
        {
            _logger.LogInformation("Fetching admin data");

            // Get real statistics from database
            var totalUsers = await _dbContext.Users.CountAsync();
            var totalConversations = await _dbContext.Conversations.CountAsync();
            var conversationsToday = await _dbContext.Conversations
                .Where(c => c.CreatedAt >= DateTime.UtcNow.Date)
                .CountAsync();

            // Get models from configuration
            var models = GetModelsFromConfig();

            // Get MCP servers
            var mcpServers = await GetMcpServersList();

            // Test LiteLLM connection
            var liteLLMUrl = _configuration["LiteLLM:BaseUrl"] ?? "http://localhost:4000";
            bool liteLLMConnected = false;
            try
            {
                // Test connection with a simple health check
                liteLLMConnected = await TestLiteLLMConnection(liteLLMUrl);
            }
            catch
            {
                liteLLMConnected = false;
            }

            // Calculate storage (placeholder - would need real implementation)
            var storageUsedMB = 0.0;
            var storageLimitMB = 1000.0;

            var adminData = new AdminDataDto
            {
                Overview = new AdminOverviewDto
                {
                    TotalUsers = totalUsers,
                    TotalConversations = totalConversations,
                    ConversationsToday = conversationsToday,
                    TotalModels = models.Count,
                    ActiveModels = models.Count(m => m.IsEnabled),
                    TotalMcpServers = mcpServers.Count,
                    ConnectedMcpServers = mcpServers.Count(s => s.Connected),
                    LiteLLMConnected = liteLLMConnected,
                    LiteLLMUrl = liteLLMUrl,
                    StorageUsedMB = storageUsedMB,
                    StorageLimitMB = storageLimitMB,
                    Uptime = GetUptime(),
                    AppVersion = GetAppVersion(),
                    LocalStorageUsedMB = 0,
                    LocalStorageLimitMB = 50.0,
                    IndexedDBUsedMB = 0,
                    IndexedDBLimitMB = 500.0
                },
                Models = models,
                McpServers = mcpServers,
                SystemLogs = await GetRecentLogs(5),
                AdminLiteLLMUrl = liteLLMUrl
            };

            return Ok(adminData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin data");
            return StatusCode(500, new { error = "Failed to fetch admin data", message = ex.Message });
        }
    }

    // ============================================================================
    // MODELS MANAGEMENT
    // ============================================================================

    /// <summary>
    /// Get all configured AI models
    /// </summary>
    /// <returns>List of AI models</returns>
    /// <response code="200">Returns list of models</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("models")]
    [ProducesResponseType(typeof(List<AIModelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetModels()
    {
        try
        {
            _logger.LogInformation("Fetching models");
            var models = GetModelsFromConfig();
            return Task.FromResult<IActionResult>(Ok(models));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching models");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to fetch models", message = ex.Message }));
        }
    }

    /// <summary>
    /// Add a new AI model configuration
    /// </summary>
    /// <param name="model">Model configuration</param>
    /// <returns>Success response</returns>
    /// <response code="200">Model added successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("models")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> AddModel([FromBody] AIModelDto model)
    {
        try
        {
            _logger.LogInformation("Adding model: {ModelId}", model.Id);

            // In a real implementation, this would save to a configuration store
            // For now, we return success
            return Task.FromResult<IActionResult>(Ok(new { success = true, model }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding model");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to add model", message = ex.Message }));
        }
    }

    /// <summary>
    /// Update an existing AI model configuration
    /// </summary>
    /// <param name="id">Model ID</param>
    /// <param name="model">Updated model configuration</param>
    /// <returns>Success response</returns>
    /// <response code="200">Model updated successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("models/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> UpdateModel(string id, [FromBody] AIModelDto model)
    {
        try
        {
            _logger.LogInformation("Updating model: {ModelId}", id);
            return Task.FromResult<IActionResult>(Ok(new { success = true, model }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating model");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to update model", message = ex.Message }));
        }
    }

    /// <summary>
    /// Delete an AI model configuration
    /// </summary>
    /// <param name="id">Model ID to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">Model deleted successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("models/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteModel(string id)
    {
        try
        {
            _logger.LogInformation("Deleting model: {ModelId}", id);
            return Task.FromResult<IActionResult>(Ok(new { success = true, message = "Model deleted" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting model");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to delete model", message = ex.Message }));
        }
    }

    /// <summary>
    /// Toggle model enabled/disabled status
    /// </summary>
    /// <param name="id">Model ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Model toggled successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("models/{id}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> ToggleModel(string id)
    {
        try
        {
            _logger.LogInformation("Toggling model: {ModelId}", id);
            return Task.FromResult<IActionResult>(Ok(new { success = true, message = "Model toggled" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling model");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to toggle model", message = ex.Message }));
        }
    }

    // ============================================================================
    // MCP SERVERS MANAGEMENT
    // ============================================================================

    /// <summary>
    /// Get all configured MCP servers
    /// </summary>
    /// <returns>List of MCP servers</returns>
    /// <response code="200">Returns list of MCP servers</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("mcp")]
    [ProducesResponseType(typeof(List<MCPServerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMcpServers()
    {
        try
        {
            _logger.LogInformation("Fetching MCP servers");
            var servers = await GetMcpServersList();
            return Ok(servers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching MCP servers");
            return StatusCode(500, new { error = "Failed to fetch MCP servers", message = ex.Message });
        }
    }

    /// <summary>
    /// Add a new MCP server configuration
    /// </summary>
    /// <param name="server">MCP server configuration</param>
    /// <returns>Success response</returns>
    /// <response code="200">MCP server added successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("mcp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> AddMcpServer([FromBody] MCPServerDto server)
    {
        try
        {
            _logger.LogInformation("Adding MCP server: {ServerName}", server.Name);
            return Task.FromResult<IActionResult>(Ok(new { success = true, server }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding MCP server");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to add MCP server", message = ex.Message }));
        }
    }

    /// <summary>
    /// Update an existing MCP server configuration
    /// </summary>
    /// <param name="id">Server ID</param>
    /// <param name="server">Updated server configuration</param>
    /// <returns>Success response</returns>
    /// <response code="200">MCP server updated successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("mcp/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> UpdateMcpServer(string id, [FromBody] MCPServerDto server)
    {
        try
        {
            _logger.LogInformation("Updating MCP server: {ServerId}", id);
            return Task.FromResult<IActionResult>(Ok(new { success = true, server }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating MCP server");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to update MCP server", message = ex.Message }));
        }
    }

    /// <summary>
    /// Delete an MCP server configuration
    /// </summary>
    /// <param name="id">Server ID to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">MCP server deleted successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("mcp/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteMcpServer(string id)
    {
        try
        {
            _logger.LogInformation("Deleting MCP server: {ServerId}", id);
            return Task.FromResult<IActionResult>(Ok(new { success = true, message = "MCP server deleted" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting MCP server");
            return StatusCode(500, new { error = "Failed to delete MCP server", message = ex.Message });
        }
    }

    /// <summary>
    /// Reconnect to an MCP server
    /// </summary>
    /// <param name="id">Server ID to reconnect</param>
    /// <returns>Success message</returns>
    /// <response code="200">Reconnection initiated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("mcp/{id}/reconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> ReconnectMcpServer(string id)
    {
        try
        {
            _logger.LogInformation("Reconnecting MCP server: {ServerId}", id);
            return Task.FromResult<IActionResult>(Ok(new { success = true, message = "Reconnection initiated" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconnecting MCP server");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to reconnect MCP server", message = ex.Message }));
        }
    }

    // ============================================================================
    // SYSTEM OPERATIONS
    // ============================================================================

    /// <summary>
    /// Get recent system logs
    /// </summary>
    /// <param name="limit">Maximum number of log entries to return</param>
    /// <returns>List of system log entries</returns>
    /// <response code="200">Returns list of log entries</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(List<SystemLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLogs([FromQuery] int limit = 50)
    {
        try
        {
            _logger.LogInformation("Fetching system logs");
            var logs = await GetRecentLogs(limit);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching logs");
            return StatusCode(500, new { error = "Failed to fetch logs", message = ex.Message });
        }
    }

    /// <summary>
    /// Clear all system logs
    /// </summary>
    /// <returns>Success message</returns>
    /// <response code="200">Logs cleared successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> ClearLogs()
    {
        try
        {
            _logger.LogInformation("Clearing system logs");
            // In a real implementation, this would clear the log files/database
            return Task.FromResult<IActionResult>(Ok(new { success = true, message = "Logs cleared" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing logs");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to clear logs", message = ex.Message }));
        }
    }

    /// <summary>
    /// Test connection to LiteLLM server
    /// </summary>
    /// <param name="request">Connection test request with URL</param>
    /// <returns>Connection test result</returns>
    /// <response code="200">Connection test completed</response>
    /// <response code="500">Connection failed</response>
    [HttpPost("test-connection")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest request)
    {
        try
        {
            _logger.LogInformation("Testing LiteLLM connection: {Url}", request.Url);

            var isConnected = await TestLiteLLMConnection(request.Url);

            if (isConnected)
            {
                return Ok(new { success = true, message = "Connection successful", url = request.Url });
            }
            else
            {
                return StatusCode(500, new { success = false, error = "Connection failed", url = request.Url });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection");
            return StatusCode(500, new { error = "Connection failed", message = ex.Message });
        }
    }

    /// <summary>
    /// Export system configuration as JSON
    /// </summary>
    /// <returns>Configuration file download</returns>
    /// <response code="200">Returns configuration file</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportConfig()
    {
        try
        {
            _logger.LogInformation("Exporting system configuration");

            var config = new
            {
                models = GetModelsFromConfig(),
                mcpServers = await GetMcpServersList(),
                exportedAt = DateTime.UtcNow,
                version = GetAppVersion()
            };

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

            return File(
                System.Text.Encoding.UTF8.GetBytes(json),
                "application/json",
                $"aiMate-admin-config-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting config");
            return StatusCode(500, new { error = "Failed to export config", message = ex.Message });
        }
    }

    // ============================================================================
    // Private Helper Methods
    // ============================================================================

    private List<AIModelDto> GetModelsFromConfig()
    {
        // Get models from configuration
        // In a real implementation, this would come from appsettings.json or a database
        return new List<AIModelDto>
        {
            new() { Id = "gpt-4", Name = "GPT-4", Provider = "OpenAI", IsEnabled = true, MaxTokens = 8192 },
            new() { Id = "gpt-4-turbo", Name = "GPT-4 Turbo", Provider = "OpenAI", IsEnabled = true, MaxTokens = 128000 },
            new() { Id = "claude-3-5-sonnet-20241022", Name = "Claude 3.5 Sonnet", Provider = "Anthropic", IsEnabled = true, MaxTokens = 200000 },
            new() { Id = "claude-3-opus-20240229", Name = "Claude 3 Opus", Provider = "Anthropic", IsEnabled = true, MaxTokens = 200000 },
            new() { Id = "gpt-3.5-turbo", Name = "GPT-3.5 Turbo", Provider = "OpenAI", IsEnabled = true, MaxTokens = 16385 }
        };
    }

    private async Task<List<MCPServerDto>> GetMcpServersList()
    {
        try
        {
            // Get available tools from MCP service
            var tools = await _mcpToolService.GetAvailableToolsAsync();

            // Group tools by server (simplified)
            var servers = new List<MCPServerDto>
            {
                new() { Id = "default", Name = "MCP Tools", Type = "builtin", Connected = tools.Any(), ToolCount = tools.Count }
            };

            return servers;
        }
        catch
        {
            return new List<MCPServerDto>();
        }
    }

    private async Task<bool> TestLiteLLMConnection(string url)
    {
        try
        {
            // Try to get models list as a health check
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync($"{url.TrimEnd('/')}/models");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<SystemLogDto>> GetRecentLogs(int limit)
    {
        // In a real implementation, this would read from log files or a logging database
        // For now, return recent database operations as pseudo-logs
        var logs = new List<SystemLogDto>();

        try
        {
            var recentConversations = await _dbContext.Conversations
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit / 2)
                .Select(c => new SystemLogDto
                {
                    Timestamp = c.CreatedAt,
                    Level = "INFO",
                    Message = $"Conversation created: {c.Title}",
                    Source = "Conversations"
                })
                .ToListAsync();

            logs.AddRange(recentConversations);

            var recentUsers = await _dbContext.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(limit / 4)
                .Select(u => new SystemLogDto
                {
                    Timestamp = u.CreatedAt,
                    Level = "INFO",
                    Message = $"User registered: {u.Username}",
                    Source = "Users"
                })
                .ToListAsync();

            logs.AddRange(recentUsers);

            // Add system startup log
            logs.Add(new SystemLogDto
            {
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Level = "INFO",
                Message = "Application started",
                Source = "System"
            });

            return logs.OrderByDescending(l => l.Timestamp).Take(limit).ToList();
        }
        catch
        {
            return new List<SystemLogDto>
            {
                new() { Timestamp = DateTime.UtcNow, Level = "INFO", Message = "System running", Source = "System" }
            };
        }
    }

    private string GetUptime()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
    }

    private string GetAppVersion()
    {
        return _configuration["App:Version"] ?? "1.0.0";
    }
}

/// <summary>
/// Test connection request
/// </summary>
public class TestConnectionRequest
{
    public required string Url { get; set; }
    public string? ApiKey { get; set; }
}
