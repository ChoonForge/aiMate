using AiMate.Shared.Models;

namespace AiMate.Web.Store.Connection;

// Load
public record LoadConnectionsAction();
public record LoadConnectionsSuccessAction(List<ProviderConnectionDto> Connections);
public record LoadConnectionsFailureAction(string Error);

// Load Limits
public record LoadConnectionLimitsAction();
public record LoadConnectionLimitsSuccessAction(int MaxConnections, bool BYOKEnabled, bool CanAddOwnKeys, bool CanAddCustomEndpoints, bool CanShareConnections);

// Create
public record CreateConnectionAction(ProviderConnectionDto Connection);
public record CreateConnectionSuccessAction(ProviderConnectionDto Connection);
public record CreateConnectionFailureAction(string Error);

// Update
public record UpdateConnectionAction(string Id, ProviderConnectionDto Connection);
public record UpdateConnectionSuccessAction(ProviderConnectionDto Connection);
public record UpdateConnectionFailureAction(string Error);

// Delete
public record DeleteConnectionAction(string Id);
public record DeleteConnectionSuccessAction(string Id);
public record DeleteConnectionFailureAction(string Error);

// Test
public record TestConnectionAction(string Id);
public record TestConnectionSuccessAction(string Id, bool Success, string Message);
public record TestConnectionFailureAction(string Error);

// Clear Error
public record ClearConnectionErrorAction();
