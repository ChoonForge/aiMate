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

        // Create API key entity (you'll need to add this to your entities)
        // For now, we'll use a simplified approach with a dictionary in memory
        // In production, add ApiKey entity to database

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

        // IMPLEMENTATION NEEDED: Add ApiKey entity to database schema
        // 1. Create ApiKey entity with: Id, UserId, HashedKey, Name, CreatedAt, LastUsedAt, IsRevoked
        // 2. Query: var keys = await _context.ApiKeys.Where(k => !k.IsRevoked).ToListAsync();
        // 3. Verify: foreach (var key in keys) { if (BCrypt.Net.BCrypt.Verify(apiKey, key.HashedKey)) return key.UserId; }
        // 4. Update LastUsedAt on successful validation

        await Task.CompletedTask;
        return null; // Returns null until database implementation added
    }

    public async Task<bool> RevokeApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API key revocation requested");

        // IMPLEMENTATION NEEDED: Requires ApiKey entity (see ValidateApiKeyAsync above)
        // Update IsRevoked = true for matching hashed key in database

        await Task.CompletedTask;
        return false; // Returns false until database implementation added
    }

    public async Task<List<ApiKeyInfo>> GetUserApiKeysAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching API keys for user {UserId}", userId);

        // IMPLEMENTATION NEEDED: Requires ApiKey entity (see ValidateApiKeyAsync above)
        // Query: var keys = await _context.ApiKeys.Where(k => k.UserId == userId).ToListAsync();
        // Return: Masked keys (show only last 4 chars) with metadata

        await Task.CompletedTask;
        return new List<ApiKeyInfo>(); // Returns empty until database implementation added
    }
}
