using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Service for managing provider connections
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Get all connections for a user
    /// </summary>
    Task<List<Connection>> GetUserConnectionsAsync(
        Guid userId,
        bool includeDisabled = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a connection by ID
    /// </summary>
    Task<Connection?> GetConnectionByIdAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new connection
    /// </summary>
    Task<Connection> CreateConnectionAsync(
        Connection connection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing connection
    /// </summary>
    Task<Connection> UpdateConnectionAsync(
        Connection connection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a connection
    /// </summary>
    Task DeleteConnectionAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Test a connection to verify credentials and connectivity
    /// </summary>
    Task<(bool Success, string Message)> TestConnectionAsync(
        Connection connection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get connections available to a user (owned + shared via groups/org)
    /// </summary>
    Task<List<Connection>> GetAvailableConnectionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get connection usage statistics
    /// </summary>
    Task<(int TotalRequests, int TotalTokens)> GetConnectionUsageAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default);
}
