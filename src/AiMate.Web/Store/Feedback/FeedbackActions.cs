using AiMate.Core.Entities;
using AiMate.Core.Services;

namespace AiMate.Web.Store.Feedback;

/// <summary>
/// Feedback actions
/// </summary>

// Load tag templates
public record LoadTagTemplatesAction();
public record LoadTagTemplatesSuccessAction(List<FeedbackTagTemplate> Templates);
public record LoadTagTemplatesFailureAction(string Error);

// Submit feedback
public record SubmitFeedbackAction(
    Guid MessageId,
    Guid UserId,
    int Rating,
    string? TextFeedback,
    List<FeedbackTagDto> Tags,
    string? ModelId,
    long? ResponseTimeMs);

public record SubmitFeedbackSuccessAction(MessageFeedback Feedback);
public record SubmitFeedbackFailureAction(string Error);

// Load feedback for message
public record LoadMessageFeedbackAction(Guid MessageId);
public record LoadMessageFeedbackSuccessAction(MessageFeedback Feedback);
public record LoadMessageFeedbackFailureAction(string Error);

// Open/close feedback dialog
public record OpenFeedbackDialogAction(Guid MessageId);
public record CloseFeedbackDialogAction();

// Quick rating (thumbs up/down)
public record QuickRateMessageAction(Guid MessageId, Guid UserId, int Rating, string? ModelId);

// Load model stats
public record LoadModelStatsAction(string ModelId, DateTime? FromDate = null, DateTime? ToDate = null);
public record LoadModelStatsSuccessAction(string ModelId, ModelFeedbackStats Stats);
public record LoadModelStatsFailureAction(string Error);

// Tag template management (admin)
public record CreateTagTemplateAction(
    string Category,
    string Label,
    string? Description,
    List<FeedbackTagOption> Options,
    bool IsRequired);

public record CreateTagTemplateSuccessAction(FeedbackTagTemplate Template);
public record CreateTagTemplateFailureAction(string Error);

public record UpdateTagTemplateAction(
    Guid TemplateId,
    string? Category,
    string? Label,
    string? Description,
    bool? IsActive,
    bool? IsRequired,
    int? DisplayOrder);

public record UpdateTagTemplateSuccessAction(FeedbackTagTemplate Template);
public record UpdateTagTemplateFailureAction(string Error);

public record DeleteTagTemplateAction(Guid TemplateId);
public record DeleteTagTemplateSuccessAction(Guid TemplateId);
public record DeleteTagTemplateFailureAction(string Error);

// DTO for feedback tags (matches API)
public record FeedbackTagDto(
    string Key,
    string Value,
    string? Color,
    TagSentiment Sentiment);
