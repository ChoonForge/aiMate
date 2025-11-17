using AiMate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AiMate.Shared.Services;

/// <summary>
/// Service for interacting with LiteLLM proxy
/// Handles both streaming and non-streaming completions
/// </summary>
public class LiteLLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string? _apiKey;

    public LiteLLMService(HttpClient httpClient, string baseUrl = "http://localhost:4000", string? apiKey = null)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _apiKey = apiKey;
    }

    /// <summary>
    /// Send a chat completion request (non-streaming)
    /// </summary>
    public async Task<ChatCompletionResponse> CreateChatCompletionAsync(
        string model,
        List<Message> messages,
        double temperature = 0.7,
        int? maxTokens = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatCompletionRequest
        {
            Model = model,
            Messages = messages.Select(m => new ChatMessage
            {
                Role = m.Role,
                Content = m.Content
            }).ToList(),
            Temperature = temperature,
            MaxTokens = maxTokens,
            Stream = false
        };

        var response = await SendRequestAsync<ChatCompletionResponse>(
            "/chat/completions",
            request,
            cancellationToken
        );

        return response;
    }

    /// <summary>
    /// Send a streaming chat completion request
    /// Yields chunks as they arrive
    /// </summary>
    public async IAsyncEnumerable<ChatCompletionChunk> CreateChatCompletionStreamAsync(
        string model,
        List<Message> messages,
        double temperature = 0.7,
        int? maxTokens = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new ChatCompletionRequest
        {
            Model = model,
            Messages = messages.Select(m => new ChatMessage
            {
                Role = m.Role,
                Content = m.Content
            }).ToList(),
            Temperature = temperature,
            MaxTokens = maxTokens,
            Stream = true
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrEmpty(_apiKey))
        {
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
        }

        using var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new System.IO.StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6).Trim();
                if (data == "[DONE]") break;

                ChatCompletionChunk? chunk = null;
                try
                {
                    chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(data);
                }
                catch (JsonException)
                {
                    // Skip malformed chunks
                    continue;
                }

                if (chunk != null)
                {
                    yield return chunk;
                }
            }
        }
    }

    /// <summary>
    /// Get available models from LiteLLM
    /// </summary>
    public async Task<List<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendRequestAsync<ModelsResponse>("/models", null, cancellationToken);
            
            return response.Data.Select(m => new AIModel
            {
                Id = m.Id,
                Name = m.Id, // LiteLLM doesn't provide a separate name
                Provider = "litellm",
                IsEnabled = true,
                Capabilities = new ModelCapabilities
                {
                    SupportsStreaming = true,
                    MaxTokens = 4096, // Default, can be overridden
                    ContextWindow = 4096
                }
            }).ToList();
        }
        catch
        {
            // Return default models if API call fails
            return GetDefaultModels();
        }
    }

    private List<AIModel> GetDefaultModels()
    {
        return new List<AIModel>
        {
            new AIModel
            {
                Id = "gpt-4",
                Name = "GPT-4",
                Provider = "openai",
                IsEnabled = true,
                Description = "Most capable GPT-4 model",
                Capabilities = new ModelCapabilities
                {
                    SupportsVision = false,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    MaxTokens = 8192,
                    ContextWindow = 8192
                }
            },
            new AIModel
            {
                Id = "gpt-3.5-turbo",
                Name = "GPT-3.5 Turbo",
                Provider = "openai",
                IsEnabled = true,
                Description = "Fast and efficient",
                Capabilities = new ModelCapabilities
                {
                    SupportsStreaming = true,
                    MaxTokens = 4096,
                    ContextWindow = 4096
                }
            },
            new AIModel
            {
                Id = "claude-3-5-sonnet-20241022",
                Name = "Claude 3.5 Sonnet",
                Provider = "anthropic",
                IsEnabled = true,
                Description = "Anthropic's most intelligent model",
                Capabilities = new ModelCapabilities
                {
                    SupportsVision = true,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    MaxTokens = 8192,
                    ContextWindow = 200000
                }
            }
        };
    }

    private async Task<T> SendRequestAsync<T>(
        string endpoint,
        object? payload,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            payload == null ? HttpMethod.Get : HttpMethod.Post,
            $"{_baseUrl}{endpoint}"
        );

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload);
        }

        if (!string.IsNullOrEmpty(_apiKey))
        {
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}

#region DTOs

public class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}

public class Choice
{
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class ChatCompletionChunk
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<ChunkChoice> Choices { get; set; } = new();

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}

public class ChunkChoice
{
    [JsonPropertyName("delta")]
    public Delta? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class Delta
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

public class ModelsResponse
{
    [JsonPropertyName("data")]
    public List<ModelData> Data { get; set; } = new();
}

public class ModelData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;
}

#endregion
