using AiMate.Core.Services;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace AiMate.Web.Store.Auth;

public class AuthEffects
{
    private readonly IAuthService _authService;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthEffects> _logger;
    private const string TokenStorageKey = "aiMate_token";
    private const string TokenExpiryStorageKey = "aiMate_token_expiry";

    public AuthEffects(
        IAuthService authService,
        IJSRuntime jsRuntime,
        ILogger<AuthEffects> logger)
    {
        _authService = authService;
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLogin(LoginAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _authService.LoginAsync(action.Email, action.Password);

            if (result == null)
            {
                dispatcher.Dispatch(new LoginFailureAction("Invalid email or password"));
                return;
            }

            var (user, token, expiry) = result.Value;

            // Store token in localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenExpiryStorageKey, expiry.ToString("O"));

            dispatcher.Dispatch(new LoginSuccessAction(user, token, expiry));

            _logger.LogInformation("Login successful for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            dispatcher.Dispatch(new LoginFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleRegister(RegisterAction action, IDispatcher dispatcher)
    {
        try
        {
            var (user, token, expiry) = await _authService.RegisterAsync(
                action.Email,
                action.Username,
                action.Password);

            // Store token in localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenExpiryStorageKey, expiry.ToString("O"));

            dispatcher.Dispatch(new RegisterSuccessAction(user, token, expiry));

            _logger.LogInformation("Registration successful for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            dispatcher.Dispatch(new RegisterFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleLogout(LogoutAction action, IDispatcher dispatcher)
    {
        try
        {
            // Clear token from localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenExpiryStorageKey);

            _logger.LogInformation("User logged out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    [EffectMethod]
    public async Task HandleCheckAuth(CheckAuthAction action, IDispatcher dispatcher)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenStorageKey);
            var expiryString = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenExpiryStorageKey);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiryString))
            {
                dispatcher.Dispatch(new CheckAuthFailureAction());
                return;
            }

            var expiry = DateTime.Parse(expiryString);

            // Check if token is expired
            if (expiry < DateTime.UtcNow)
            {
                dispatcher.Dispatch(new CheckAuthFailureAction());
                return;
            }

            // Validate token
            var user = await _authService.ValidateTokenAsync(token);

            if (user == null)
            {
                dispatcher.Dispatch(new CheckAuthFailureAction());
                return;
            }

            dispatcher.Dispatch(new CheckAuthSuccessAction(user, token, expiry));

            _logger.LogInformation("Auth check successful for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auth check failed");
            dispatcher.Dispatch(new CheckAuthFailureAction());
        }
    }

    [EffectMethod]
    public async Task HandleRefreshToken(RefreshTokenAction action, IDispatcher dispatcher)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenStorageKey);

            if (string.IsNullOrEmpty(token))
            {
                dispatcher.Dispatch(new RefreshTokenFailureAction("No token found"));
                return;
            }

            var user = await _authService.ValidateTokenAsync(token);

            if (user == null)
            {
                dispatcher.Dispatch(new RefreshTokenFailureAction("Invalid token"));
                return;
            }

            var newToken = _authService.GenerateToken(user, out var expiry);

            // Store new token
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, newToken);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenExpiryStorageKey, expiry.ToString("O"));

            dispatcher.Dispatch(new RefreshTokenSuccessAction(newToken, expiry));

            _logger.LogInformation("Token refreshed for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            dispatcher.Dispatch(new RefreshTokenFailureAction(ex.Message));
        }
    }
}
