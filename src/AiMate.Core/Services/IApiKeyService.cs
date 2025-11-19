namespace AiMate.Core.Services;

/// <summary>
/// API key service for Developer tier API access
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Generate new API key for user
    /// </summary>
    Task<string> GenerateApiKeyAsync(Guid userId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate API key and get associated user ID
    /// </summary>
    Task<Guid?> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke API key
    /// </summary>
    Task<bool> RevokeApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all API keys for user (masked for security)
    /// </summary>
    Task<List<ApiKeyInfo>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// API key information (masked for security)
/// </summary>
public class ApiKeyInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MaskedKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; }
}
