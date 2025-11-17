using Fluxor;

namespace AiMate.Web.Store.Auth;

public static class AuthReducers
{
    // Login
    [ReducerMethod]
    public static AuthState OnLogin(AuthState state, LoginAction action)
    {
        return state with { IsLoggingIn = true, Error = null };
    }

    [ReducerMethod]
    public static AuthState OnLoginSuccess(AuthState state, LoginSuccessAction action)
    {
        return state with
        {
            CurrentUser = action.User,
            Token = action.Token,
            TokenExpiry = action.TokenExpiry,
            IsAuthenticated = true,
            IsLoggingIn = false,
            Error = null
        };
    }

    [ReducerMethod]
    public static AuthState OnLoginFailure(AuthState state, LoginFailureAction action)
    {
        return state with
        {
            IsLoggingIn = false,
            IsAuthenticated = false,
            Error = action.Error
        };
    }

    // Register
    [ReducerMethod]
    public static AuthState OnRegister(AuthState state, RegisterAction action)
    {
        return state with { IsRegistering = true, Error = null };
    }

    [ReducerMethod]
    public static AuthState OnRegisterSuccess(AuthState state, RegisterSuccessAction action)
    {
        return state with
        {
            CurrentUser = action.User,
            Token = action.Token,
            TokenExpiry = action.TokenExpiry,
            IsAuthenticated = true,
            IsRegistering = false,
            Error = null
        };
    }

    [ReducerMethod]
    public static AuthState OnRegisterFailure(AuthState state, RegisterFailureAction action)
    {
        return state with
        {
            IsRegistering = false,
            IsAuthenticated = false,
            Error = action.Error
        };
    }

    // Logout
    [ReducerMethod]
    public static AuthState OnLogout(AuthState state, LogoutAction action)
    {
        return new AuthState();
    }

    // Check auth
    [ReducerMethod]
    public static AuthState OnCheckAuth(AuthState state, CheckAuthAction action)
    {
        return state with { IsLoading = true };
    }

    [ReducerMethod]
    public static AuthState OnCheckAuthSuccess(AuthState state, CheckAuthSuccessAction action)
    {
        return state with
        {
            CurrentUser = action.User,
            Token = action.Token,
            TokenExpiry = action.TokenExpiry,
            IsAuthenticated = true,
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static AuthState OnCheckAuthFailure(AuthState state, CheckAuthFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            IsAuthenticated = false
        };
    }

    // Refresh token
    [ReducerMethod]
    public static AuthState OnRefreshTokenSuccess(AuthState state, RefreshTokenSuccessAction action)
    {
        return state with
        {
            Token = action.Token,
            TokenExpiry = action.TokenExpiry
        };
    }

    [ReducerMethod]
    public static AuthState OnRefreshTokenFailure(AuthState state, RefreshTokenFailureAction action)
    {
        return new AuthState { Error = action.Error };
    }

    // Error handling
    [ReducerMethod]
    public static AuthState OnClearAuthError(AuthState state, ClearAuthErrorAction action)
    {
        return state with { Error = null };
    }
}
