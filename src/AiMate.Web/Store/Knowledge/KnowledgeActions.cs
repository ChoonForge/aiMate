using AiMate.Shared.Models;

namespace AiMate.Web.Store.Knowledge;

// ============================================================================
// LOAD ARTICLES
// ============================================================================

public record LoadArticlesAction();
public record LoadArticlesSuccessAction(List<KnowledgeArticleDto> Articles);
public record LoadArticlesFailureAction(string Error);

// ============================================================================
// LOAD ANALYTICS
// ============================================================================

public record LoadAnalyticsAction();
public record LoadAnalyticsSuccessAction(KnowledgeAnalyticsDto Analytics);
public record LoadAnalyticsFailureAction(string Error);

// ============================================================================
// CREATE ARTICLE
// ============================================================================

public record CreateArticleAction(KnowledgeArticleDto Article);
public record CreateArticleSuccessAction(KnowledgeArticleDto Article);
public record CreateArticleFailureAction(string Error);

// ============================================================================
// UPDATE ARTICLE
// ============================================================================

public record UpdateArticleAction(string ArticleId, KnowledgeArticleDto UpdatedArticle);
public record UpdateArticleSuccessAction(KnowledgeArticleDto UpdatedArticle);
public record UpdateArticleFailureAction(string Error);

// ============================================================================
// DELETE ARTICLE
// ============================================================================

public record DeleteArticleAction(string ArticleId);
public record DeleteArticleSuccessAction(string ArticleId);
public record DeleteArticleFailureAction(string Error);

// ============================================================================
// ARTICLE INTERACTIONS
// ============================================================================

public record ViewArticleAction(string ArticleId); // Increment view count
public record ReferenceArticleAction(string ArticleId); // Increment reference count
public record UpvoteArticleAction(string ArticleId);
public record DownvoteArticleAction(string ArticleId);
public record ToggleFeaturedAction(string ArticleId);
public record ToggleVerifiedAction(string ArticleId);
public record TogglePublishedAction(string ArticleId);

// ============================================================================
// FILTER & SEARCH ACTIONS
// ============================================================================

public record SetSearchQueryAction(string Query);
public record SetTypeFilterAction(string? Type);
public record SetCollectionFilterAction(string? Collection);
public record SetCategoryFilterAction(string? Category);
public record SetTagsFilterAction(List<string> Tags);
public record ToggleShowFeaturedAction();
public record ToggleShowVerifiedAction();
public record ToggleShowUnpublishedAction();
public record ClearFiltersAction();

// ============================================================================
// VIEW & SORTING ACTIONS
// ============================================================================

public record SetViewModeAction(string ViewMode); // Grid, List, Analytics
public record SetActiveTabAction(string Tab); // All, Featured, Recent, Popular
public record SetSortByAction(string SortBy);
public record ToggleSortDirectionAction();

// ============================================================================
// UI ACTIONS
// ============================================================================

public record SelectArticleAction(KnowledgeArticleDto? Article);
public record OpenEditDialogAction(KnowledgeArticleDto? Article = null);
public record CloseEditDialogAction();
public record ClearKnowledgeErrorAction();
