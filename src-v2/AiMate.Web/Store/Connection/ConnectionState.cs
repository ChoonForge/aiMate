using AiMate.Shared.Models;
using Fluxor;

namespace AiMate.Web.Store.Connection;

[FeatureState]
public record ConnectionState
{
    public List<ProviderConnectionDto> Connections { get; init; } = new();
    public bool IsLoading { get; init; }
    public string? Error { get; init; }
    public int MaxConnections { get; init; } = 3;
    public bool BYOKEnabled { get; init; } = true;
    public bool CanAddOwnKeys { get; init; } = true;
    public bool CanAddCustomEndpoints { get; init; } = false;
    public bool CanShareConnections { get; init; } = false;
}
