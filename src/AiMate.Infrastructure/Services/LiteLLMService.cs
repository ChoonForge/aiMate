using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// LiteLLM service implementation with efficient streaming
/// </summary>
public class LiteLLMService : ILiteLLMService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LiteLLMService> _logger;
    private readonly string _baseUrl;
    private readonly string? _apiKey;

    public LiteLLMService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<LiteLLMService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["LiteLLM:BaseUrl"] ?? "http://localhost:4000";
        _apiKey = configuration["LiteLLM:ApiKey"];

        _httpClient.BaseAddress = new Uri(_baseUrl);
        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
    }

    public async IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;

        var jsonContent = JsonContent.Create(request);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
        {
            Content = jsonContent
        };

        var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            // LiteLLM streams in SSE format: "data: {json}"
            if (line.StartsWith("data: "))
            {
                var jsonData = line.Substring(6).Trim();

                // Check for [DONE] marker
                if (jsonData == "[DONE]")
                {
                    _logger.LogDebug("Stream completed");
                    break;
                }

                ChatCompletionResponse? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<ChatCompletionResponse>(jsonData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse streaming chunk: {JsonData}", jsonData);
                    continue;
                }

                if (chunk != null)
                {
                    yield return chunk;
                }
            }
        }
    }

    public async Task<ChatCompletionResponse> GetChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;

        var response = await _httpClient.PostAsJsonAsync(
            "/v1/chat/completions",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
            cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<List<string>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/v1/models", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ModelsResponse>(
                cancellationToken: cancellationToken);

            return result?.Data?.Select(m => m.Id).ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch models from LiteLLM");
            // Return fallback models
            return new List<string>
            {
                "gpt-4",
                "gpt-3.5-turbo",
                "claude-3-5-sonnet-20241022",
                "claude-3-opus-20240229"
            };
        }
    }

    private class ModelsResponse
    {
        public List<ModelData>? Data { get; set; }
    }

    private class ModelData
    {
        public required string Id { get; set; }
    }
}
