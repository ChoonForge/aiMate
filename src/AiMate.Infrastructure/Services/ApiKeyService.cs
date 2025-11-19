using System.Security.Cryptography;
using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace AiMate.Infrastructure.Services;

/// <summary>
/// API key service - manage API keys for Developer tier access
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<ApiKeyService> _logger;
    private const string ApiKeyPrefix = "sk-aimate-";
    private const int ApiKeyLength = 32;

    public ApiKeyService(
        AiMateDbContext context,
        ILogger<ApiKeyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateApiKeyAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default)
    {
        // Verify user exists and is Developer tier
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.Tier != UserTier.Developer)
        {
            throw new InvalidOperationException("API keys are only available for Developer tier users");
        }

        // Generate secure random API key
        var randomBytes = new byte[ApiKeyLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var apiKey = ApiKeyPrefix + Convert.ToBase64String(randomBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .Substring(0, ApiKeyLength);

        // Hash the API key for storage (never store plaintext)
        var hashedKey = BCrypt.Net.BCrypt.HashPassword(apiKey, workFactor: 12);

        // Create API key entity
        var apiKeyEntity = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HashedKey = hashedKey,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _context.ApiKeys.Add(apiKeyEntity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated new API key for user {UserId} with name '{Name}'", userId, name);

        return apiKey; // Return plaintext key ONCE - user must save it
    }

    public async Task<Guid?> ValidateApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith(ApiKeyPrefix))
        {
            return null;
        }

        _logger.LogInformation("API key validation attempted");

        // Get all non-revoked API keys
        var keys = await _context.ApiKeys
            .Where(k => !k.IsRevoked)
            .ToListAsync(cancellationToken);

        // Verify against each hashed key
        foreach (var key in keys)
        {
            if (BCrypt.Net.BCrypt.Verify(apiKey, key.HashedKey))
            {
                // Update LastUsedAt
                key.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("API key validated successfully for user {UserId}", key.UserId);
                return key.UserId;
            }
        }

        _logger.LogWarning("Invalid API key attempted");
        return null;
    }

    public async Task<bool> RevokeApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API key revocation requested");

        if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith(ApiKeyPrefix))
        {
            return false;
        }

        // Get all non-revoked API keys
        var keys = await _context.ApiKeys
            .Where(k => !k.IsRevoked)
            .ToListAsync(cancellationToken);

        // Find matching key and revoke it
        foreach (var key in keys)
        {
            if (BCrypt.Net.BCrypt.Verify(apiKey, key.HashedKey))
            {
                key.IsRevoked = true;
                key.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("API key revoked for user {UserId}", key.UserId);
                return true;
            }
        }

        _logger.LogWarning("API key revocation failed - key not found");
        return false;
    }

    public async Task<List<ApiKeyInfo>> GetUserApiKeysAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching API keys for user {UserId}", userId);

        var keys = await _context.ApiKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);

        return keys.Select(k => new ApiKeyInfo
        {
            Id = k.Id,
            Name = k.Name,
            MaskedKey = ApiKeyPrefix + "****" + k.HashedKey.Substring(k.HashedKey.Length - 4),
            CreatedAt = k.CreatedAt,
            LastUsedAt = k.LastUsedAt,
            IsActive = !k.IsRevoked
        }).ToList();
    }
}
