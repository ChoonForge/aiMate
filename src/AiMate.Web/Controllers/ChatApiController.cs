using System.Text;
using System.Text.Json;
using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// Chat API for Developer tier - REST API for external integrations
/// </summary>
[ApiController]
[Route("api/v1/chat")]
[Authorize] // Requires authentication
public class ChatApiController : ControllerBase
{
    private readonly ILiteLLMService _liteLLMService;
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<ChatApiController> _logger;

    public ChatApiController(
        ILiteLLMService liteLLMService,
        IApiKeyService apiKeyService,
        ILogger<ChatApiController> logger)
    {
        _liteLLMService = liteLLMService;
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    /// <summary>
    /// Send chat completion request (OpenAI-compatible)
    /// </summary>
    [HttpPost("completions")]
    public async Task<IActionResult> CreateCompletion([FromBody] ChatCompletionRequest request)
    {
        // Validate API key from Authorization header
        var apiKey = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _apiKeyService.ValidateApiKeyAsync(apiKey);

        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        try
        {
            _logger.LogInformation("Chat completion request for user {UserId}", userId);

            // Map to LiteLLM request format
            var liteLLMRequest = new Shared.Models.ChatCompletionRequest
            {
                Model = request.Model,
                Messages = [.. request.Messages.Select(m => new Shared.Models.ChatMessage
                {
                    Role = m.Role,
                    Content = m.Content
                })],
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                Stream = false
            };

            // Get completion from LiteLLM
            var response = await _liteLLMService.GetChatCompletionAsync(liteLLMRequest);

            // Return OpenAI-compatible response (already in correct format)
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat completion failed");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Stream chat completion (OpenAI-compatible)
    /// </summary>
    [HttpPost("completions/stream")]
    public async Task CreateCompletionStream([FromBody] ChatCompletionRequest request)
    {
        var apiKey = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = await _apiKeyService.ValidateApiKeyAsync(apiKey);

        if (userId == null)
        {
            Response.StatusCode = 401;
            await Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        try
        {
            _logger.LogInformation("Chat completion stream request for user {UserId}", userId);

            // Set up SSE response
            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            // Map to LiteLLM request format
            var liteLLMRequest = new Shared.Models.ChatCompletionRequest
            {
                Model = request.Model,
                Messages = [.. request.Messages.Select(m => new Shared.Models.ChatMessage
                {
                    Role = m.Role,
                    Content = m.Content
                })],
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                Stream = true
            };

            // Stream response chunks
            await foreach (var chunk in _liteLLMService.StreamChatCompletionAsync(liteLLMRequest))
            {
                var json = JsonSerializer.Serialize(chunk);
                await Response.WriteAsync($"data: {json}\n\n", Encoding.UTF8);
                await Response.Body.FlushAsync();
            }

            // Send completion marker
            await Response.WriteAsync("data: [DONE]\n\n", Encoding.UTF8);
            await Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat completion stream failed");
            var errorJson = JsonSerializer.Serialize(new { error = "Internal server error", message = ex.Message });
            await Response.WriteAsync($"data: {errorJson}\n\n", Encoding.UTF8);
            await Response.Body.FlushAsync();
        }
    }
}

/// <summary>
/// Chat completion request (OpenAI-compatible)
/// </summary>
public class ChatCompletionRequest
{
    public string Model { get; set; } = "gpt-4";
    public List<ChatMessage> Messages { get; set; } = [];
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 1000;
    public bool Stream { get; set; } = false;
}

public class ChatMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}
