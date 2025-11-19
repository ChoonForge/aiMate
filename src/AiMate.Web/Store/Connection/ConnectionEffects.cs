using System.Net.Http.Json;
using AiMate.Shared.Models;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace AiMate.Web.Store.Connection;

public class ConnectionEffects
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConnectionEffects> _logger;

    public ConnectionEffects(HttpClient httpClient, ILogger<ConnectionEffects> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadConnections(LoadConnectionsAction action, IDispatcher dispatcher)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Inject IState<AuthState> to get userId and tier from authenticated user
            var userId = "user-1";
            var tier = "Free";

            var connections = await _httpClient.GetFromJsonAsync<List<ProviderConnectionDto>>(
                $"/api/v1/connections?userId={userId}&tierStr={tier}");

            if (connections != null)
            {
                dispatcher.Dispatch(new LoadConnectionsSuccessAction(connections));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load connections");
            dispatcher.Dispatch(new LoadConnectionsFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleLoadConnectionLimits(LoadConnectionLimitsAction action, IDispatcher dispatcher)
    {
        try
        {
            var tier = "Free"; // IMPLEMENTATION NEEDED: Get from IState<AuthState>.Value.CurrentUser?.Tier

            var response = await _httpClient.GetFromJsonAsync<ConnectionLimitsResponse>(
                $"/api/v1/connections/limits?tierStr={tier}");

            if (response != null)
            {
                dispatcher.Dispatch(new LoadConnectionLimitsSuccessAction(
                    response.MaxConnections,
                    response.BYOKEnabled,
                    response.CanAddOwnKeys,
                    response.CanAddCustomEndpoints,
                    response.CanShareConnections
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load connection limits");
        }
    }

    [EffectMethod]
    public async Task HandleCreateConnection(CreateConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = "user-1"; // IMPLEMENTATION NEEDED: Get from IState<AuthState>.Value.CurrentUser?.Id
            var tier = "Free";

            var response = await _httpClient.PostAsJsonAsync(
                $"/api/v1/connections?userId={userId}&tierStr={tier}",
                action.Connection);

            if (response.IsSuccessStatusCode)
            {
                var connection = await response.Content.ReadFromJsonAsync<ProviderConnectionDto>();
                if (connection != null)
                {
                    dispatcher.Dispatch(new CreateConnectionSuccessAction(connection));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new CreateConnectionFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create connection");
            dispatcher.Dispatch(new CreateConnectionFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateConnection(UpdateConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = "user-1"; // IMPLEMENTATION NEEDED: Get from IState<AuthState>.Value.CurrentUser?.Id
            var tier = "Free";

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/v1/connections/{action.Id}?userId={userId}&tierStr={tier}",
                action.Connection);

            if (response.IsSuccessStatusCode)
            {
                var connection = await response.Content.ReadFromJsonAsync<ProviderConnectionDto>();
                if (connection != null)
                {
                    dispatcher.Dispatch(new UpdateConnectionSuccessAction(connection));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new UpdateConnectionFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update connection");
            dispatcher.Dispatch(new UpdateConnectionFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteConnection(DeleteConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = "user-1"; // IMPLEMENTATION NEEDED: Get from IState<AuthState>.Value.CurrentUser?.Id
            var tier = "Free";

            var response = await _httpClient.DeleteAsync(
                $"/api/v1/connections/{action.Id}?userId={userId}&tierStr={tier}");

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new DeleteConnectionSuccessAction(action.Id));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new DeleteConnectionFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete connection");
            dispatcher.Dispatch(new DeleteConnectionFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleTestConnection(TestConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = "user-1"; // IMPLEMENTATION NEEDED: Get from IState<AuthState>.Value.CurrentUser?.Id

            var response = await _httpClient.PostAsync(
                $"/api/v1/connections/{action.Id}/test?userId={userId}", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TestConnectionResponse>();
                if (result != null)
                {
                    dispatcher.Dispatch(new TestConnectionSuccessAction(action.Id, result.Success, result.Message));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test connection");
            dispatcher.Dispatch(new TestConnectionFailureAction(ex.Message));
        }
    }
}

// Response DTOs
public class ConnectionLimitsResponse
{
    public int MaxConnections { get; set; }
    public bool BYOKEnabled { get; set; }
    public bool CanAddOwnKeys { get; set; }
    public bool CanAddCustomEndpoints { get; set; }
    public bool CanShareConnections { get; set; }
}

public class TestConnectionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
