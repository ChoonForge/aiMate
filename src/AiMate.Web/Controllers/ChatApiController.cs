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
    /// <param name="request">Chat completion request with model, messages, and parameters</param>
    /// <returns>Chat completion response with generated message</returns>
    /// <response code="200">Returns the chat completion response</response>
    /// <response code="401">Invalid or missing API key</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Requires Developer tier or higher. Use Bearer token authentication with your API key.
    ///
    /// Example request:
    ///
    ///     POST /api/v1/chat/completions
    ///     {
    ///         "model": "gpt-4",
    ///         "messages": [
    ///             {"role": "user", "content": "Hello, how are you?"}
    ///         ],
    ///         "temperature": 0.7,
    ///         "maxTokens": 1000
    ///     }
    ///
    /// </remarks>
    [HttpPost("completions")]
    [ProducesResponseType(typeof(Shared.Models.ChatCompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Stream chat completion using Server-Sent Events (OpenAI-compatible)
    /// </summary>
    /// <param name="request">Chat completion request with model, messages, and parameters</param>
    /// <returns>SSE stream of chat completion chunks</returns>
    /// <response code="200">Returns SSE stream with completion chunks</response>
    /// <response code="401">Invalid or missing API key</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Requires Developer tier or higher. Use Bearer token authentication with your API key.
    ///
    /// This endpoint returns a Server-Sent Events (SSE) stream. Each chunk is sent as:
    ///
    ///     data: {"id":"...", "choices":[{"delta":{"content":"..."}}]}\n\n
    ///
    /// The stream ends with:
    ///
    ///     data: [DONE]\n\n
    ///
    /// Example request:
    ///
    ///     POST /api/v1/chat/completions/stream
    ///     {
    ///         "model": "gpt-4",
    ///         "messages": [
    ///             {"role": "user", "content": "Write a haiku"}
    ///         ],
    ///         "temperature": 0.8,
    ///         "stream": true
    ///     }
    ///
    /// </remarks>
    [HttpPost("completions/stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <summary>
    /// Model to use for chat completion (e.g. "gpt-4", "claude-3-opus")
    /// </summary>
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    /// List of messages in the conversation
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// Sampling temperature (0.0 to 2.0). Higher values make output more random.
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Maximum number of tokens to generate in the completion
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Whether to stream the response using Server-Sent Events
    /// </summary>
    public bool Stream { get; set; } = false;
}

/// <summary>
/// A single message in a chat conversation
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Role of the message sender ("system", "user", or "assistant")
    /// </summary>
    public string Role { get; set; } = "user";

    /// <summary>
    /// Content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
