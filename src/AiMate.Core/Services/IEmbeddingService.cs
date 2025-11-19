namespace AiMate.Core.Services;

/// <summary>
/// Embedding service - generate vector embeddings for text
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generate embedding for text using OpenAI text-embedding-ada-002
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate embeddings for multiple texts in batch
    /// </summary>
    Task<List<float[]>> GenerateEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get embedding dimension size (1536 for ada-002)
    /// </summary>
    int GetEmbeddingDimension();
}
