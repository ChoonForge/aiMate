using System.Net.Http.Json;
using System.Text.Json;
using AiMate.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// OpenAI embedding service - generates embeddings using text-embedding-ada-002
/// </summary>
public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIEmbeddingService> _logger;
    private const int EmbeddingDimension = 1536;

    public OpenAIEmbeddingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAIEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1/");
        var apiKey = _configuration["OpenAI:ApiKey"];

        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var embeddings = await GenerateEmbeddingsAsync(new List<string> { text }, cancellationToken);
        return embeddings.First();
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                model = "text-embedding-ada-002",
                input = texts
            };

            var response = await _httpClient.PostAsJsonAsync("embeddings", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken);

            if (result?.Data == null || !result.Data.Any())
            {
                throw new InvalidOperationException("No embeddings returned from API");
            }

            _logger.LogInformation("Generated {Count} embeddings", texts.Count);

            return result.Data.Select(d => d.Embedding).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings from OpenAI API");

            // Fall back to placeholder embeddings if API fails
            _logger.LogWarning("Falling back to placeholder embeddings");
            return texts.Select(_ => GeneratePlaceholderEmbedding()).ToList();
        }
    }

    public int GetEmbeddingDimension()
    {
        return EmbeddingDimension;
    }

    private float[] GeneratePlaceholderEmbedding()
    {
        // Generate deterministic placeholder for development/fallback
        var embedding = new float[EmbeddingDimension];
        var random = new Random(42);

        for (int i = 0; i < EmbeddingDimension; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }

        // Normalize
        var magnitude = Math.Sqrt(embedding.Sum(x => x * x));
        for (int i = 0; i < EmbeddingDimension; i++)
        {
            embedding[i] /= (float)magnitude;
        }

        return embedding;
    }

    private class EmbeddingResponse
    {
        public List<EmbeddingData> Data { get; set; } = new();
    }

    private class EmbeddingData
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
