using Fluxor;

namespace AiMate.Web.Store.Feedback;

/// <summary>
/// Feedback reducers - pure state mutation functions
/// </summary>
public static class FeedbackReducers
{
    // Load tag templates
    [ReducerMethod]
    public static FeedbackState ReduceLoadTagTemplatesAction(FeedbackState state, LoadTagTemplatesAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static FeedbackState ReduceLoadTagTemplatesSuccessAction(FeedbackState state, LoadTagTemplatesSuccessAction action)
    {
        return state with
        {
            TagTemplates = action.Templates,
            IsLoading = false,
            Error = null
        };
    }

    [ReducerMethod]
    public static FeedbackState ReduceLoadTagTemplatesFailureAction(FeedbackState state, LoadTagTemplatesFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            Error = action.Error
        };
    }

    // Submit feedback
    [ReducerMethod]
    public static FeedbackState ReduceSubmitFeedbackAction(FeedbackState state, SubmitFeedbackAction action)
    {
        return state with { IsSubmitting = true, Error = null };
    }

    [ReducerMethod]
    public static FeedbackState ReduceSubmitFeedbackSuccessAction(FeedbackState state, SubmitFeedbackSuccessAction action)
    {
        var updatedFeedback = new Dictionary<Guid, Core.Entities.MessageFeedback>(state.FeedbackByMessageId)
        {
            [action.Feedback.MessageId] = action.Feedback
        };

        return state with
        {
            FeedbackByMessageId = updatedFeedback,
            IsSubmitting = false,
            ActiveFeedbackMessageId = null,
            Error = null
        };
    }

    [ReducerMethod]
    public static FeedbackState ReduceSubmitFeedbackFailureAction(FeedbackState state, SubmitFeedbackFailureAction action)
    {
        return state with
        {
            IsSubmitting = false,
            Error = action.Error
        };
    }

    // Load message feedback
    [ReducerMethod]
    public static FeedbackState ReduceLoadMessageFeedbackAction(FeedbackState state, LoadMessageFeedbackAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static FeedbackState ReduceLoadMessageFeedbackSuccessAction(FeedbackState state, LoadMessageFeedbackSuccessAction action)
    {
        var updatedFeedback = new Dictionary<Guid, Core.Entities.MessageFeedback>(state.FeedbackByMessageId)
        {
            [action.Feedback.MessageId] = action.Feedback
        };

        return state with
        {
            FeedbackByMessageId = updatedFeedback,
            IsLoading = false,
            Error = null
        };
    }

    [ReducerMethod]
    public static FeedbackState ReduceLoadMessageFeedbackFailureAction(FeedbackState state, LoadMessageFeedbackFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            Error = action.Error
        };
    }

    // Open/close feedback dialog
    [ReducerMethod]
    public static FeedbackState ReduceOpenFeedbackDialogAction(FeedbackState state, OpenFeedbackDialogAction action)
    {
        return state with { ActiveFeedbackMessageId = action.MessageId };
    }

    [ReducerMethod]
    public static FeedbackState ReduceCloseFeedbackDialogAction(FeedbackState state, CloseFeedbackDialogAction action)
    {
        return state with { ActiveFeedbackMessageId = null };
    }

    // Load model stats
    [ReducerMethod]
    public static FeedbackState ReduceLoadModelStatsAction(FeedbackState state, LoadModelStatsAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static FeedbackState ReduceLoadModelStatsSuccessAction(FeedbackState state, LoadModelStatsSuccessAction action)
    {
        var updatedStats = new Dictionary<string, Core.Services.ModelFeedbackStats>(state.ModelStats)
        {
            [action.ModelId] = action.Stats
        };

        return state with
        {
            ModelStats = updatedStats,
            IsLoading = false,
            Error = null
        };
    }

    [ReducerMethod]
    public static FeedbackState ReduceLoadModelStatsFailureAction(FeedbackState state, LoadModelStatsFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            Error = action.Error
        };
    }

    // Tag template management
    [ReducerMethod]
    public static FeedbackState ReduceCreateTagTemplateSuccessAction(FeedbackState state, CreateTagTemplateSuccessAction action)
    {
        var updatedTemplates = new List<Core.Entities.FeedbackTagTemplate>(state.TagTemplates)
        {
            action.Template
        };

        return state with
        {
            TagTemplates = updatedTemplates,
            IsSubmitting = false,
            Error = null
        };
    }

    [ReducerMethod]
    public static FeedbackState ReduceUpdateTagTemplateSuccessAction(FeedbackState state, UpdateTagTemplateSuccessAction action)
    {
        var updatedTemplates = state.TagTemplates
            .Select(t => t.Id == action.Template.Id ? action.Template : t)
            .ToList();

        return state with
        {
            TagTemplates = updatedTemplates,
            IsSubmitting = false,
            Error = null
        };
    }

    [ReducerMethod]
    public static FeedbackState ReduceDeleteTagTemplateSuccessAction(FeedbackState state, DeleteTagTemplateSuccessAction action)
    {
        var updatedTemplates = state.TagTemplates
            .Where(t => t.Id != action.TemplateId)
            .ToList();

        return state with
        {
            TagTemplates = updatedTemplates,
            IsSubmitting = false,
            Error = null
        };
    }
}
