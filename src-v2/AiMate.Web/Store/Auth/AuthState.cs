using AiMate.Core.Entities;
using Fluxor;

namespace AiMate.Web.Store.Auth;

/// <summary>
/// Authentication state - current user and auth status
/// </summary>
[FeatureState]
public record AuthState
{
    public User? CurrentUser { get; init; }
    public string? Token { get; init; }
    public bool IsAuthenticated { get; init; }
    public bool IsLoading { get; init; }
    public bool IsLoggingIn { get; init; }
    public bool IsRegistering { get; init; }
    public string? Error { get; init; }
    public DateTime? TokenExpiry { get; init; }
}
