using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Plugins;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Plugins;

/// <summary>
/// Adds common action buttons to messages (copy, edit, regenerate, share)
/// </summary>
public class MessageActionsPlugin : IUIExtension
{
    private readonly ILogger<MessageActionsPlugin>? _logger;

    public string Id => "message-actions";
    public string Name => "Message Actions";
    public string Description => "Copy, edit, regenerate, and share messages";
    public string Version => "1.0.0";
    public string Author => "aiMate";
    public string Icon => "MoreVert";
    public PluginCategory Category => PluginCategory.MessageActions;
    public bool IsEnabled { get; set; } = true;

    public MessageActionsPlugin()
    {
        // Parameterless constructor
    }

    public MessageActionsPlugin(ILogger<MessageActionsPlugin> logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync()
    {
        _logger?.LogInformation("MessageActionsPlugin initialized");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _logger?.LogInformation("MessageActionsPlugin disposed");
        return Task.CompletedTask;
    }

    public IEnumerable<MessageActionButton> GetMessageActions(Message message)
    {
        // Copy button - show on all messages
        yield return new MessageActionButton
        {
            Id = "copy",
            Label = "Copy",
            Icon = "ContentCopy",
            Tooltip = "Copy message to clipboard",
            Color = "Default",
            ShowOnAssistantMessages = true,
            ShowOnUserMessages = true,
            Order = 10,
            OnClickHandler = "handleCopyMessage"
        };

        // Edit button - only on user messages
        if (message.Role == MessageRole.User)
        {
            yield return new MessageActionButton
            {
                Id = "edit",
                Label = "Edit",
                Icon = "Edit",
                Tooltip = "Edit and resend",
                Color = "Primary",
                ShowOnAssistantMessages = false,
                ShowOnUserMessages = true,
                Order = 20,
                OnClickHandler = "handleEditMessage"
            };
        }

        // Regenerate button - only on assistant messages
        if (message.Role == MessageRole.Assistant)
        {
            yield return new MessageActionButton
            {
                Id = "regenerate",
                Label = "Regenerate",
                Icon = "Refresh",
                Tooltip = "Generate a new response",
                Color = "Secondary",
                ShowOnAssistantMessages = true,
                ShowOnUserMessages = false,
                Order = 30,
                OnClickHandler = "handleRegenerateMessage"
            };
        }

        // Share button - show on all messages
        yield return new MessageActionButton
        {
            Id = "share",
            Label = "Share",
            Icon = "Share",
            Tooltip = "Share this message",
            Color = "Default",
            ShowOnAssistantMessages = true,
            ShowOnUserMessages = true,
            Order = 40,
            OnClickHandler = "handleShareMessage"
        };

        // Delete button - show on all messages
        yield return new MessageActionButton
        {
            Id = "delete",
            Label = "Delete",
            Icon = "Delete",
            Tooltip = "Delete this message",
            Color = "Error",
            ShowOnAssistantMessages = true,
            ShowOnUserMessages = true,
            Order = 50,
            OnClickHandler = "handleDeleteMessage"
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
            Title = "Message Actions Settings",
            Description = "Customize which action buttons are shown",
            Fields = new List<SettingField>
            {
                new()
                {
                    Key = "showCopyButton",
                    Label = "Show Copy Button",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                },
                new()
                {
                    Key = "showEditButton",
                    Label = "Show Edit Button",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                },
                new()
                {
                    Key = "showRegenerateButton",
                    Label = "Show Regenerate Button",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                },
                new()
                {
                    Key = "showShareButton",
                    Label = "Show Share Button",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                },
                new()
                {
                    Key = "showDeleteButton",
                    Label = "Show Delete Button",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                },
                new()
                {
                    Key = "confirmDelete",
                    Label = "Confirm Before Delete",
                    Description = "Show confirmation dialog before deleting",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                }
            }
        };
    }

    public string? RenderCustomContent(Message message)
    {
        return null;
    }
}
