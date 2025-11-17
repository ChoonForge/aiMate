using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Authentication service - login, register, JWT management
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login with email and password
    /// </summary>
    Task<(User User, string Token, DateTime Expiry)?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register new user
    /// </summary>
    Task<(User User, string Token, DateTime Expiry)> RegisterAsync(
        string email,
        string username,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate JWT token and get user
    /// </summary>
    Task<User?> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    string GenerateToken(User user, out DateTime expiry);

    /// <summary>
    /// Hash password using bcrypt
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify password against hash
    /// </summary>
    bool VerifyPassword(string password, string hash);
}
