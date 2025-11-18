using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Plugins;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Plugins;

/// <summary>
/// Adds thumbs up/down rating buttons to AI messages
/// </summary>
public class MessageRatingPlugin : IUIExtension
{
    private readonly ILogger<MessageRatingPlugin>? _logger;

    public string Id => "message-rating";
    public string Name => "Message Rating";
    public string Description => "Rate AI responses with thumbs up/down or 5-star ratings";
    public string Version => "1.0.0";
    public string Author => "aiMate";
    public string Icon => "ThumbUp";
    public PluginCategory Category => PluginCategory.MessageActions;
    public bool IsEnabled { get; set; } = true;

    public MessageRatingPlugin()
    {
        // Parameterless constructor for reflection
    }

    public MessageRatingPlugin(ILogger<MessageRatingPlugin> logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync()
    {
        _logger?.LogInformation("MessageRatingPlugin initialized");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _logger?.LogInformation("MessageRatingPlugin disposed");
        return Task.CompletedTask;
    }

    public IEnumerable<MessageActionButton> GetMessageActions(Message message)
    {
        // Only show on assistant messages
        if (message.Role != MessageRole.Assistant)
            yield break;

        // Thumbs Up button
        yield return new MessageActionButton
        {
            Id = "thumbs-up",
            Label = "Good response",
            Icon = "ThumbUp",
            Tooltip = "This response was helpful",
            Color = message.Rating == 1 ? "Success" : "Default",
            ShowOnAssistantMessages = true,
            ShowOnUserMessages = false,
            Order = 100,
            OnClickHandler = "handleThumbsUp" // JS function or Blazor method
        };

        // Thumbs Down button
        yield return new MessageActionButton
        {
            Id = "thumbs-down",
            Label = "Bad response",
            Icon = "ThumbDown",
            Tooltip = "This response needs improvement",
            Color = message.Rating == -1 ? "Error" : "Default",
            ShowOnAssistantMessages = true,
            ShowOnUserMessages = false,
            Order = 101,
            OnClickHandler = "handleThumbsDown"
        };
    }

    public IEnumerable<ChatInputExtension> GetInputExtensions()
    {
        // No input extensions
        return Enumerable.Empty<ChatInputExtension>();
    }

    public PluginSettings? GetSettingsUI()
    {
        return new PluginSettings
        {
            Title = "Message Rating Settings",
            Description = "Configure how message ratings are displayed and collected",
            Fields = new List<SettingField>
            {
                new()
                {
                    Key = "ratingType",
                    Label = "Rating Type",
                    Description = "Choose between simple thumbs or 5-star rating",
                    Type = SettingFieldType.Dropdown,
                    DefaultValue = "thumbs",
                    Options = new List<string> { "thumbs", "stars" }
                },
                new()
                {
                    Key = "showFeedbackPrompt",
                    Label = "Show Feedback Prompt",
                    Description = "Ask for written feedback after rating",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                },
                new()
                {
                    Key = "collectAnonymously",
                    Label = "Collect Anonymously",
                    Description = "Don't link ratings to user profiles",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = false
                }
            }
        };
    }

    public string? RenderCustomContent(Message message)
    {
        // No custom rendering needed
        return null;
    }
}
