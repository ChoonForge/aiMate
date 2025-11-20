using System.Net.Http.Json;
using AiMate.Core.Entities;
using AiMate.Core.Services;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace AiMate.Web.Store.Feedback;

/// <summary>
/// Feedback effects - side effects for API calls
/// </summary>
public class FeedbackEffects
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FeedbackEffects> _logger;

    public FeedbackEffects(IHttpClientFactory httpClientFactory, ILogger<FeedbackEffects> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadTagTemplatesAction(LoadTagTemplatesAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var templates = await httpClient.GetFromJsonAsync<List<FeedbackTagTemplate>>("/api/v1/feedback/templates");
            if (templates != null)
            {
                dispatcher.Dispatch(new LoadTagTemplatesSuccessAction(templates));
            }
            else
            {
                dispatcher.Dispatch(new LoadTagTemplatesFailureAction("Failed to load tag templates"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tag templates");
            dispatcher.Dispatch(new LoadTagTemplatesFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleSubmitFeedbackAction(SubmitFeedbackAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var request = new
            {
                userId = action.UserId,
                rating = action.Rating,
                textFeedback = action.TextFeedback,
                tags = action.Tags.Select(t => new
                {
                    key = t.Key,
                    value = t.Value,
                    color = t.Color,
                    sentiment = (int)t.Sentiment
                }).ToList(),
                modelId = action.ModelId,
                responseTimeMs = action.ResponseTimeMs
            };

            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var response = await httpClient.PostAsJsonAsync(
                $"/api/v1/feedback/messages/{action.MessageId}",
                request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MessageFeedback>();
                if (result != null)
                {
                    dispatcher.Dispatch(new SubmitFeedbackSuccessAction(result));
                    _logger.LogInformation("Feedback submitted for message {MessageId}", action.MessageId);
                }
                else
                {
                    dispatcher.Dispatch(new SubmitFeedbackFailureAction("Failed to parse feedback response"));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new SubmitFeedbackFailureAction($"API error: {error}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit feedback for message {MessageId}", action.MessageId);
            dispatcher.Dispatch(new SubmitFeedbackFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public Task HandleQuickRateMessageAction(QuickRateMessageAction action, IDispatcher dispatcher)
    {
        // Quick rating with no tags or text feedback
        dispatcher.Dispatch(new SubmitFeedbackAction(
            action.MessageId,
            action.UserId,
            action.Rating,
            null,
            new List<FeedbackTagDto>(),
            action.ModelId,
            null
        ));
        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleLoadMessageFeedbackAction(LoadMessageFeedbackAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var feedback = await httpClient.GetFromJsonAsync<MessageFeedback>(
                $"/api/v1/feedback/messages/{action.MessageId}");

            if (feedback != null)
            {
                dispatcher.Dispatch(new LoadMessageFeedbackSuccessAction(feedback));
            }
            else
            {
                dispatcher.Dispatch(new LoadMessageFeedbackFailureAction("Feedback not found"));
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // No feedback exists for this message, which is fine
            _logger.LogDebug("No feedback found for message {MessageId}", action.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load feedback for message {MessageId}", action.MessageId);
            dispatcher.Dispatch(new LoadMessageFeedbackFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleLoadModelStatsAction(LoadModelStatsAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var url = $"/api/v1/feedback/stats/models/{action.ModelId}";
            if (action.FromDate.HasValue || action.ToDate.HasValue)
            {
                var queryParams = new List<string>();
                if (action.FromDate.HasValue)
                    queryParams.Add($"fromDate={action.FromDate.Value:O}");
                if (action.ToDate.HasValue)
                    queryParams.Add($"toDate={action.ToDate.Value:O}");
                url += "?" + string.Join("&", queryParams);
            }

            var stats = await httpClient.GetFromJsonAsync<ModelFeedbackStats>(url);
            if (stats != null)
            {
                dispatcher.Dispatch(new LoadModelStatsSuccessAction(action.ModelId, stats));
            }
            else
            {
                dispatcher.Dispatch(new LoadModelStatsFailureAction("Failed to load model stats"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load stats for model {ModelId}", action.ModelId);
            dispatcher.Dispatch(new LoadModelStatsFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleCreateTagTemplateAction(CreateTagTemplateAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var request = new
            {
                category = action.Category,
                label = action.Label,
                description = action.Description,
                isRequired = action.IsRequired,
                options = action.Options.Select(o => new
                {
                    value = o.Value,
                    color = o.Color,
                    sentiment = (int)o.Sentiment,
                    icon = o.Icon,
                    displayOrder = o.DisplayOrder
                }).ToList()
            };

            var response = await httpClient.PostAsJsonAsync("/api/v1/feedback/templates", request);

            if (response.IsSuccessStatusCode)
            {
                var template = await response.Content.ReadFromJsonAsync<FeedbackTagTemplate>();
                if (template != null)
                {
                    dispatcher.Dispatch(new CreateTagTemplateSuccessAction(template));
                    _logger.LogInformation("Tag template created: {Category}", action.Category);
                }
                else
                {
                    dispatcher.Dispatch(new CreateTagTemplateFailureAction("Failed to parse template response"));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new CreateTagTemplateFailureAction($"API error: {error}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tag template");
            dispatcher.Dispatch(new CreateTagTemplateFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateTagTemplateAction(UpdateTagTemplateAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var request = new
            {
                category = action.Category,
                label = action.Label,
                description = action.Description,
                isActive = action.IsActive,
                isRequired = action.IsRequired,
                displayOrder = action.DisplayOrder
            };

            var response = await httpClient.PutAsJsonAsync(
                $"/api/v1/feedback/templates/{action.TemplateId}",
                request);

            if (response.IsSuccessStatusCode)
            {
                var template = await response.Content.ReadFromJsonAsync<FeedbackTagTemplate>();
                if (template != null)
                {
                    dispatcher.Dispatch(new UpdateTagTemplateSuccessAction(template));
                    _logger.LogInformation("Tag template updated: {TemplateId}", action.TemplateId);
                }
                else
                {
                    dispatcher.Dispatch(new UpdateTagTemplateFailureAction("Failed to parse template response"));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new UpdateTagTemplateFailureAction($"API error: {error}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tag template {TemplateId}", action.TemplateId);
            dispatcher.Dispatch(new UpdateTagTemplateFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteTagTemplateAction(DeleteTagTemplateAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var response = await httpClient.DeleteAsync($"/api/v1/feedback/templates/{action.TemplateId}");

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new DeleteTagTemplateSuccessAction(action.TemplateId));
                _logger.LogInformation("Tag template deleted: {TemplateId}", action.TemplateId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new DeleteTagTemplateFailureAction($"API error: {error}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete tag template {TemplateId}", action.TemplateId);
            dispatcher.Dispatch(new DeleteTagTemplateFailureAction(ex.Message));
        }
    }
}
