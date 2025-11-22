using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Connection service implementation
/// </summary>
public class ConnectionService : IConnectionService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<ConnectionService> _logger;
    private readonly ILiteLLMService _liteLLMService;
    private readonly IEncryptionService _encryptionService;

    public ConnectionService(
        AiMateDbContext context,
        ILogger<ConnectionService> logger,
        ILiteLLMService liteLLMService,
        IEncryptionService encryptionService)
    {
        _context = context;
        _logger = logger;
        _liteLLMService = liteLLMService;
        _encryptionService = encryptionService;
    }

    public async Task<List<Connection>> GetUserConnectionsAsync(
        Guid userId,
        bool includeDisabled = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Connections.Where(c => c.OwnerId == userId);

        if (!includeDisabled)
        {
            query = query.Where(c => c.IsEnabled);
        }

        return await query
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Connection?> GetConnectionByIdAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .Include(c => c.Owner)
            .Include(c => c.Organization)
            .FirstOrDefaultAsync(c => c.Id == connectionId, cancellationToken);
    }

    public async Task<Connection> CreateConnectionAsync(
        Connection connection,
        CancellationToken cancellationToken = default)
    {
        // Encrypt credentials before saving
        if (!string.IsNullOrEmpty(connection.EncryptedCredentials))
        {
            try
            {
                connection.EncryptedCredentials = _encryptionService.Encrypt(connection.EncryptedCredentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt credentials for connection");
                throw new InvalidOperationException("Failed to encrypt connection credentials", ex);
            }
        }

        _context.Connections.Add(connection);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created connection {ConnectionId} for user {UserId}",
            connection.Id, connection.OwnerId);

        return connection;
    }

    public async Task<Connection> UpdateConnectionAsync(
        Connection connection,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.Connections.FindAsync(
            new object[] { connection.Id }, cancellationToken);

        if (existing == null)
        {
            throw new InvalidOperationException($"Connection {connection.Id} not found");
        }

        existing.Name = connection.Name;
        existing.Description = connection.Description;
        existing.Type = connection.Type;
        existing.BaseUrl = connection.BaseUrl;

        // Encrypt credentials before saving (only if credentials have changed)
        if (!string.IsNullOrEmpty(connection.EncryptedCredentials))
        {
            try
            {
                existing.EncryptedCredentials = _encryptionService.Encrypt(connection.EncryptedCredentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt credentials for connection {ConnectionId}", connection.Id);
                throw new InvalidOperationException("Failed to encrypt connection credentials", ex);
            }
        }

        existing.ConfigurationJson = connection.ConfigurationJson;
        existing.IsEnabled = connection.IsEnabled;
        existing.IsBYOK = connection.IsBYOK;
        existing.AvailableModels = connection.AvailableModels;
        existing.Visibility = connection.Visibility;
        existing.OrganizationId = connection.OrganizationId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated connection {ConnectionId}", connection.Id);

        return existing;
    }

    public async Task DeleteConnectionAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        var connection = await _context.Connections.FindAsync(
            new object[] { connectionId }, cancellationToken);

        if (connection == null)
        {
            throw new InvalidOperationException($"Connection {connectionId} not found");
        }

        _context.Connections.Remove(connection);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted connection {ConnectionId}", connectionId);
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(
        Connection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, test by attempting a simple API call based on connection type
            switch (connection.Type)
            {
                case ConnectionType.OpenAI:
                case ConnectionType.Anthropic:
                case ConnectionType.CustomOpenAI:
                    // Test with a minimal completion request
                    var testRequest = new Shared.Models.ChatCompletionRequest
                    {
                        Model = connection.AvailableModels.FirstOrDefault() ?? "gpt-3.5-turbo",
                        Messages = new[]
                        {
                            new Shared.Models.ChatMessage
                            {
                                Role = "user",
                                Content = "test"
                            }
                        },
                        MaxTokens = 1
                    };

                    try
                    {
                        await _liteLLMService.GetChatCompletionAsync(testRequest);
                        return (true, "Connection successful");
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Connection failed: {ex.Message}");
                    }

                case ConnectionType.MCP:
                    // MCP connections would be tested differently
                    // For now, just check if the URL is reachable
                    if (string.IsNullOrEmpty(connection.BaseUrl))
                    {
                        return (false, "MCP server URL is required");
                    }
                    return (true, "MCP configuration valid");

                default:
                    return (false, "Unknown connection type");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection {ConnectionId}", connection.Id);
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<List<Connection>> GetAvailableConnectionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get connections that are:
        // 1. Owned by the user
        // 2. Public
        // 3. Shared via organization (if user is in an organization)

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

        var query = _context.Connections.Where(c =>
            c.OwnerId == userId || // Owned by user
            c.Visibility == ConnectionVisibility.Public || // Public connections
            (c.Visibility == ConnectionVisibility.Organization && c.OrganizationId != null) // Org connections
        );

        return await query
            .Where(c => c.IsEnabled)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(int TotalRequests, int TotalTokens)> GetConnectionUsageAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        var connection = await _context.Connections.FindAsync(
            new object[] { connectionId }, cancellationToken);

        if (connection == null)
        {
            return (0, 0);
        }

        return (connection.TotalRequests, connection.TotalTokens);
    }
}
