using AiMate.Shared.Models;
using Fluxor;

namespace AiMate.Web.Store.Knowledge;

/// <summary>
/// Knowledge base state - reference materials and documentation with analytics
/// </summary>
[FeatureState]
public record KnowledgeState
{
    public List<KnowledgeArticleDto> Articles { get; init; } = new();
    public KnowledgeAnalyticsDto? Analytics { get; init; }

    // Organization
    public List<string> Collections { get; init; } = new();
    public List<string> AllTags { get; init; } = new();
    public List<string> Categories { get; init; } = new();

    // Filters
    public string SearchQuery { get; init; } = string.Empty;
    public string? SelectedType { get; init; } // Article, Guide, Reference, Tutorial, FAQ
    public string? SelectedCollection { get; init; }
    public string? SelectedCategory { get; init; }
    public List<string> SelectedTags { get; init; } = new();
    public bool ShowFeaturedOnly { get; init; } = false;
    public bool ShowVerifiedOnly { get; init; } = false;
    public bool ShowUnpublishedOnly { get; init; } = false;

    // View Mode
    public string ViewMode { get; init; } = "Grid"; // Grid, List, Analytics
    public string ActiveTab { get; init; } = "All"; // All, Featured, Recent, Popular

    // Sorting
    public string SortBy { get; init; } = "UpdatedAt"; // UpdatedAt, ViewCount, ReferenceCount, Title
    public bool SortDescending { get; init; } = true;

    // UI State
    public bool IsLoading { get; init; }
    public bool IsSaving { get; init; }
    public bool IsSearching { get; init; }
    public string? Error { get; init; }
    public KnowledgeArticleDto? SelectedArticle { get; init; }
    public bool IsEditDialogOpen { get; init; }

    // Additional state for Knowledge components compatibility
    public Guid? SelectedItemId { get; init; }
    public Dictionary<Guid, Core.Entities.KnowledgeItem> KnowledgeItems { get; init; } = new();
    public List<Core.Entities.KnowledgeItem> SearchResults { get; init; } = new();
    public List<Core.Entities.KnowledgeItem> RelatedItems { get; init; } = new();

    // Item editor state
    public bool ShowItemEditor { get; init; }
    public Guid? EditingItemId { get; init; }
}
