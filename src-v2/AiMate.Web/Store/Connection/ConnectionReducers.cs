using Fluxor;

namespace AiMate.Web.Store.Connection;

public static class ConnectionReducers
{
    [ReducerMethod]
    public static ConnectionState LoadConnections(ConnectionState state, LoadConnectionsAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ConnectionState LoadConnectionsSuccess(ConnectionState state, LoadConnectionsSuccessAction action)
        => state with { Connections = action.Connections, IsLoading = false };

    [ReducerMethod]
    public static ConnectionState LoadConnectionsFailure(ConnectionState state, LoadConnectionsFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static ConnectionState LoadConnectionLimitsSuccess(ConnectionState state, LoadConnectionLimitsSuccessAction action)
        => state with
        {
            MaxConnections = action.MaxConnections,
            BYOKEnabled = action.BYOKEnabled,
            CanAddOwnKeys = action.CanAddOwnKeys,
            CanAddCustomEndpoints = action.CanAddCustomEndpoints,
            CanShareConnections = action.CanShareConnections
        };

    [ReducerMethod]
    public static ConnectionState CreateConnectionSuccess(ConnectionState state, CreateConnectionSuccessAction action)
    {
        var connections = state.Connections.ToList();
        connections.Add(action.Connection);
        return state with { Connections = connections, IsLoading = false };
    }

    [ReducerMethod]
    public static ConnectionState UpdateConnectionSuccess(ConnectionState state, UpdateConnectionSuccessAction action)
    {
        var connections = state.Connections.Select(c =>
            c.Id == action.Connection.Id ? action.Connection : c
        ).ToList();

        return state with { Connections = connections, IsLoading = false };
    }

    [ReducerMethod]
    public static ConnectionState DeleteConnectionSuccess(ConnectionState state, DeleteConnectionSuccessAction action)
    {
        var connections = state.Connections.Where(c => c.Id != action.Id).ToList();
        return state with { Connections = connections, IsLoading = false };
    }

    [ReducerMethod]
    public static ConnectionState ClearError(ConnectionState state, ClearConnectionErrorAction action)
        => state with { Error = null };
}
