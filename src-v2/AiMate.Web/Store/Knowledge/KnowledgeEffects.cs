using AiMate.Shared.Models;
using Fluxor;
using System.Net.Http.Json;

namespace AiMate.Web.Store.Knowledge;

public class KnowledgeEffects
{
    private readonly HttpClient _httpClient;

    public KnowledgeEffects(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ========================================================================
    // LOAD ARTICLES
    // ========================================================================

    [EffectMethod]
    public async Task HandleLoadArticles(LoadArticlesAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = "user-1"; // IMPLEMENTATION NEEDED: Get from IState<AuthState>.Value.CurrentUser?.Id
            var articles = await _httpClient.GetFromJsonAsync<List<KnowledgeArticleDto>>(
                $"/api/v1/knowledge?userId={userId}");

            if (articles != null)
            {
                dispatcher.Dispatch(new LoadArticlesSuccessAction(articles));
            }
            else
            {
                dispatcher.Dispatch(new LoadArticlesFailureAction("No articles found"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadArticlesFailureAction(ex.Message));
        }
    }

    // ========================================================================
    // LOAD ANALYTICS
    // ========================================================================

    [EffectMethod]
    public async Task HandleLoadAnalytics(LoadAnalyticsAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = "user-1"; // IMPLEMENTATION NEEDED: Get from IState<AuthState>.Value.CurrentUser?.Id
            var analytics = await _httpClient.GetFromJsonAsync<KnowledgeAnalyticsDto>(
                $"/api/v1/knowledge/analytics?userId={userId}");

            if (analytics != null)
            {
                dispatcher.Dispatch(new LoadAnalyticsSuccessAction(analytics));
            }
            else
            {
                dispatcher.Dispatch(new LoadAnalyticsFailureAction("Failed to load analytics"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadAnalyticsFailureAction(ex.Message));
        }
    }

    // ========================================================================
    // CREATE ARTICLE
    // ========================================================================

    [EffectMethod]
    public async Task HandleCreateArticle(CreateArticleAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new CreateKnowledgeArticleRequest
            {
                Title = action.Article.Title,
                Content = action.Article.Content,
                ContentType = action.Article.ContentType,
                Summary = action.Article.Summary,
                Type = action.Article.Type,
                Tags = action.Article.Tags.Count > 0 ? action.Article.Tags : null,
                Collection = action.Article.Collection,
                Category = action.Article.Category,
                Source = action.Article.Source
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/knowledge", request);

            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
                if (created != null)
                {
                    dispatcher.Dispatch(new CreateArticleSuccessAction(created));
                }
                else
                {
                    dispatcher.Dispatch(new CreateArticleFailureAction("Failed to create article"));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new CreateArticleFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CreateArticleFailureAction(ex.Message));
        }
    }

    // ========================================================================
    // UPDATE ARTICLE
    // ========================================================================

    [EffectMethod]
    public async Task HandleUpdateArticle(UpdateArticleAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new UpdateKnowledgeArticleRequest
            {
                Title = action.UpdatedArticle.Title,
                Content = action.UpdatedArticle.Content,
                ContentType = action.UpdatedArticle.ContentType,
                Summary = action.UpdatedArticle.Summary,
                Type = action.UpdatedArticle.Type,
                Tags = action.UpdatedArticle.Tags.Count > 0 ? action.UpdatedArticle.Tags : null,
                Collection = action.UpdatedArticle.Collection,
                Category = action.UpdatedArticle.Category,
                Source = action.UpdatedArticle.Source,
                IsFeatured = action.UpdatedArticle.IsFeatured,
                IsPublished = action.UpdatedArticle.IsPublished,
                IsVerified = action.UpdatedArticle.IsVerified
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/v1/knowledge/{action.ArticleId}", request);

            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<KnowledgeArticleDto>();
                if (updated != null)
                {
                    dispatcher.Dispatch(new UpdateArticleSuccessAction(updated));
                }
                else
                {
                    dispatcher.Dispatch(new UpdateArticleFailureAction("Failed to update article"));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new UpdateArticleFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new UpdateArticleFailureAction(ex.Message));
        }
    }

    // ========================================================================
    // DELETE ARTICLE
    // ========================================================================

    [EffectMethod]
    public async Task HandleDeleteArticle(DeleteArticleAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/knowledge/{action.ArticleId}");

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new DeleteArticleSuccessAction(action.ArticleId));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new DeleteArticleFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new DeleteArticleFailureAction(ex.Message));
        }
    }
}
