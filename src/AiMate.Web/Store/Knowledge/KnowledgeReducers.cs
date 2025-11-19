using Fluxor;

namespace AiMate.Web.Store.Knowledge;

public static class KnowledgeReducers
{
    // ========================================================================
    // LOAD ARTICLES
    // ========================================================================

    [ReducerMethod]
    public static KnowledgeState LoadArticles(KnowledgeState state, LoadArticlesAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static KnowledgeState LoadArticlesSuccess(KnowledgeState state, LoadArticlesSuccessAction action)
    {
        // Extract metadata
        var collections = action.Articles
            .Where(a => !string.IsNullOrEmpty(a.Collection))
            .Select(a => a.Collection!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var tags = action.Articles
            .SelectMany(a => a.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        var categories = action.Articles
            .Where(a => !string.IsNullOrEmpty(a.Category))
            .Select(a => a.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        return state with
        {
            Articles = action.Articles,
            Collections = collections,
            AllTags = tags,
            Categories = categories,
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static KnowledgeState LoadArticlesFailure(KnowledgeState state, LoadArticlesFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    // ========================================================================
    // LOAD ANALYTICS
    // ========================================================================

    [ReducerMethod]
    public static KnowledgeState LoadAnalytics(KnowledgeState state, LoadAnalyticsAction action)
        => state with { IsLoading = true };

    [ReducerMethod]
    public static KnowledgeState LoadAnalyticsSuccess(KnowledgeState state, LoadAnalyticsSuccessAction action)
        => state with { Analytics = action.Analytics, IsLoading = false };

    [ReducerMethod]
    public static KnowledgeState LoadAnalyticsFailure(KnowledgeState state, LoadAnalyticsFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    // ========================================================================
    // CREATE/UPDATE/DELETE
    // ========================================================================

    [ReducerMethod]
    public static KnowledgeState CreateArticle(KnowledgeState state, CreateArticleAction action)
        => state with { IsSaving = true, Error = null };

    [ReducerMethod]
    public static KnowledgeState CreateArticleSuccess(KnowledgeState state, CreateArticleSuccessAction action)
    {
        var articles = state.Articles.ToList();
        articles.Insert(0, action.Article);
        return state with { Articles = articles, IsSaving = false, IsEditDialogOpen = false };
    }

    [ReducerMethod]
    public static KnowledgeState CreateArticleFailure(KnowledgeState state, CreateArticleFailureAction action)
        => state with { IsSaving = false, Error = action.Error };

    [ReducerMethod]
    public static KnowledgeState UpdateArticle(KnowledgeState state, UpdateArticleAction action)
        => state with { IsSaving = true, Error = null };

    [ReducerMethod]
    public static KnowledgeState UpdateArticleSuccess(KnowledgeState state, UpdateArticleSuccessAction action)
    {
        var articles = state.Articles.ToList();
        var index = articles.FindIndex(a => a.Id == action.UpdatedArticle.Id);
        if (index >= 0) articles[index] = action.UpdatedArticle;
        return state with { Articles = articles, IsSaving = false, IsEditDialogOpen = false };
    }

    [ReducerMethod]
    public static KnowledgeState UpdateArticleFailure(KnowledgeState state, UpdateArticleFailureAction action)
        => state with { IsSaving = false, Error = action.Error };

    [ReducerMethod]
    public static KnowledgeState DeleteArticle(KnowledgeState state, DeleteArticleAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static KnowledgeState DeleteArticleSuccess(KnowledgeState state, DeleteArticleSuccessAction action)
    {
        var articles = state.Articles.Where(a => a.Id != action.ArticleId).ToList();
        return state with { Articles = articles, IsLoading = false };
    }

    [ReducerMethod]
    public static KnowledgeState DeleteArticleFailure(KnowledgeState state, DeleteArticleFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    // ========================================================================
    // ARTICLE INTERACTIONS
    // ========================================================================

    [ReducerMethod]
    public static KnowledgeState ViewArticle(KnowledgeState state, ViewArticleAction action)
    {
        var articles = state.Articles.ToList();
        var article = articles.FirstOrDefault(a => a.Id == action.ArticleId);
        if (article != null)
        {
            var index = articles.IndexOf(article);
            articles[index] = article with { ViewCount = article.ViewCount + 1, LastViewedAt = DateTime.UtcNow };
        }
        return state with { Articles = articles };
    }

    [ReducerMethod]
    public static KnowledgeState ReferenceArticle(KnowledgeState state, ReferenceArticleAction action)
    {
        var articles = state.Articles.ToList();
        var article = articles.FirstOrDefault(a => a.Id == action.ArticleId);
        if (article != null)
        {
            var index = articles.IndexOf(article);
            articles[index] = article with { ReferenceCount = article.ReferenceCount + 1 };
        }
        return state with { Articles = articles };
    }

    [ReducerMethod]
    public static KnowledgeState UpvoteArticle(KnowledgeState state, UpvoteArticleAction action)
    {
        var articles = state.Articles.ToList();
        var article = articles.FirstOrDefault(a => a.Id == action.ArticleId);
        if (article != null)
        {
            var index = articles.IndexOf(article);
            articles[index] = article with { UpvoteCount = article.UpvoteCount + 1 };
        }
        return state with { Articles = articles };
    }

    [ReducerMethod]
    public static KnowledgeState ToggleFeatured(KnowledgeState state, ToggleFeaturedAction action)
    {
        var articles = state.Articles.ToList();
        var article = articles.FirstOrDefault(a => a.Id == action.ArticleId);
        if (article != null)
        {
            var index = articles.IndexOf(article);
            articles[index] = article with { IsFeatured = !article.IsFeatured };
        }
        return state with { Articles = articles };
    }

    // ========================================================================
    // FILTERS
    // ========================================================================

    [ReducerMethod]
    public static KnowledgeState SetSearchQuery(KnowledgeState state, SetSearchQueryAction action)
        => state with { SearchQuery = action.Query };

    [ReducerMethod]
    public static KnowledgeState SetTypeFilter(KnowledgeState state, SetTypeFilterAction action)
        => state with { SelectedType = action.Type };

    [ReducerMethod]
    public static KnowledgeState SetCollectionFilter(KnowledgeState state, SetCollectionFilterAction action)
        => state with { SelectedCollection = action.Collection };

    [ReducerMethod]
    public static KnowledgeState ToggleShowFeatured(KnowledgeState state, ToggleShowFeaturedAction action)
        => state with { ShowFeaturedOnly = !state.ShowFeaturedOnly };

    [ReducerMethod]
    public static KnowledgeState ClearFilters(KnowledgeState state, ClearFiltersAction action)
        => state with
        {
            SearchQuery = string.Empty,
            SelectedType = null,
            SelectedCollection = null,
            SelectedCategory = null,
            SelectedTags = new(),
            ShowFeaturedOnly = false,
            ShowVerifiedOnly = false
        };

    // ========================================================================
    // UI
    // ========================================================================

    [ReducerMethod]
    public static KnowledgeState SetViewMode(KnowledgeState state, SetViewModeAction action)
        => state with { ViewMode = action.ViewMode };

    [ReducerMethod]
    public static KnowledgeState SetActiveTab(KnowledgeState state, SetActiveTabAction action)
        => state with { ActiveTab = action.Tab };

    [ReducerMethod]
    public static KnowledgeState OpenEditDialog(KnowledgeState state, OpenEditDialogAction action)
        => state with { IsEditDialogOpen = true, SelectedArticle = action.Article };

    [ReducerMethod]
    public static KnowledgeState CloseEditDialog(KnowledgeState state, CloseEditDialogAction action)
        => state with { IsEditDialogOpen = false, SelectedArticle = null };
}
