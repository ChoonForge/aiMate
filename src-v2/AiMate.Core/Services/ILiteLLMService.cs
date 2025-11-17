using AiMate.Shared.Models;

namespace AiMate.Core.Services;

/// <summary>
/// LiteLLM service interface for AI model interactions
/// </summary>
public interface ILiteLLMService
{
    /// <summary>
    /// Stream chat completion (efficient, token-by-token)
    /// </summary>
    IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get chat completion (non-streaming)
    /// </summary>
    Task<ChatCompletionResponse> GetChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available models
    /// </summary>
    Task<List<string>> GetModelsAsync(CancellationToken cancellationToken = default);
}
