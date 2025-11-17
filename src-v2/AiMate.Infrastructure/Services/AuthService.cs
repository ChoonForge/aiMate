using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Authentication service implementation with JWT and BCrypt
/// </summary>
public class AuthService : IAuthService
{
    private readonly AiMateDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AiMateDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(User User, string Token, DateTime Expiry)?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for email {Email}", email);
            return null;
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for email {Email}", email);
            return null;
        }

        var token = GenerateToken(user, out var expiry);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return (user, token, expiry);
    }

    public async Task<(User User, string Token, DateTime Expiry)> RegisterAsync(
        string email,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == username, cancellationToken))
        {
            throw new InvalidOperationException("Username already taken");
        }

        var user = new User
        {
            Email = email,
            Username = username,
            PasswordHash = HashPassword(password),
            Tier = UserTier.Free,
            Locale = "en-NZ"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = GenerateToken(user, out var expiry);

        _logger.LogInformation("New user registered: {UserId} ({Email})", user.Id, email);

        return (user, token, expiry);
    }

    public async Task<User?> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtSecret());

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    public string GenerateToken(User user, out DateTime expiry)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(GetJwtSecret());

        expiry = DateTime.UtcNow.AddDays(7);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("tier", user.Tier.ToString())
            }),
            Expires = expiry,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string HashPassword(string password)
    {
        // Using BCrypt.Net-Next for password hashing
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    private string GetJwtSecret()
    {
        return _configuration["Jwt:Secret"]
            ?? "aiMate-super-secret-key-change-in-production-minimum-32-characters-long";
    }
}
