using AiMate.Core.Entities;

namespace AiMate.Web.Store.Auth;

// Login
public record LoginAction(string Email, string Password);
public record LoginSuccessAction(User User, string Token, DateTime TokenExpiry);
public record LoginFailureAction(string Error);

// Register
public record RegisterAction(string Email, string Username, string Password);
public record RegisterSuccessAction(User User, string Token, DateTime TokenExpiry);
public record RegisterFailureAction(string Error);

// Logout
public record LogoutAction;

// Check auth status (on app load)
public record CheckAuthAction;
public record CheckAuthSuccessAction(User User, string Token, DateTime TokenExpiry);
public record CheckAuthFailureAction;

// Refresh token
public record RefreshTokenAction;
public record RefreshTokenSuccessAction(string Token, DateTime TokenExpiry);
public record RefreshTokenFailureAction(string Error);

// Error handling
public record ClearAuthErrorAction;
