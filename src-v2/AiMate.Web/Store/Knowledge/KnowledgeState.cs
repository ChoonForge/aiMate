using AiMate.Core.Entities;
using Fluxor;

namespace AiMate.Web.Store.Knowledge;

/// <summary>
/// Knowledge state - all knowledge base state
/// </summary>
[FeatureState]
public record KnowledgeState
{
    public Dictionary<Guid, KnowledgeItem> KnowledgeItems { get; init; } = new();
    public List<KnowledgeItem> SearchResults { get; init; } = new();
    public string SearchQuery { get; init; } = string.Empty;
    public List<string> SelectedTags { get; init; } = new();
    public List<string> AllTags { get; init; } = new();
    public Guid? SelectedItemId { get; init; }
    public List<KnowledgeItem> RelatedItems { get; init; } = new();
    public bool IsSearching { get; init; }
    public bool IsLoading { get; init; }
    public bool IsSaving { get; init; }
    public bool ShowItemEditor { get; init; }
    public Guid? EditingItemId { get; init; }
    public string? Error { get; init; }
}
