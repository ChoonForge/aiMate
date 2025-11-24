using System.Text.RegularExpressions;
using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Plugins;
using Microsoft.Extensions.Logging;
using PluginSettings = AiMate.Core.Plugins.PluginSettings;

namespace AiMate.Infrastructure.Plugins;

/// <summary>
/// Adds copy buttons to code blocks in messages
/// </summary>
public class CodeCopyPlugin : IUIExtension
{
    private readonly ILogger<CodeCopyPlugin>? _logger;

    public string Id => "code-copy";
    public string Name => "Code Copy";
    public string Description => "Add copy buttons to code blocks";
    public string Version => "1.0.0";
    public string Author => "aiMate";
    public string Icon => "Code";
    public PluginCategory Category => PluginCategory.MessageActions;
    public bool IsEnabled { get; set; } = true;

    public CodeCopyPlugin()
    {
        // Parameterless constructor
    }

    public CodeCopyPlugin(ILogger<CodeCopyPlugin> logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync()
    {
        _logger?.LogInformation("CodeCopyPlugin initialized");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _logger?.LogInformation("CodeCopyPlugin disposed");
        return Task.CompletedTask;
    }

    public IEnumerable<MessageActionButton> GetMessageActions(Message message)
    {
        // Only show on assistant messages with code blocks
        if (message.Role != MessageRole.Assistant)
            yield break;

        // Check if message contains code blocks (```...```)
        if (!HasCodeBlocks(message.Content))
            yield break;

        yield return new MessageActionButton
        {
            Id = "copy-all-code",
            Label = "Copy All Code",
            Icon = "ContentCopy",
            Tooltip = "Copy all code blocks from this message",
            Color = "Primary",
            ShowOnAssistantMessages = true,
            ShowOnUserMessages = false,
            Order = 15,
            OnClickHandler = "handleCopyAllCode",
            Context = new Dictionary<string, string>
            {
                { "messageId", message.Id.ToString() },
                { "content", message.Content }
            }
        };
    }

    public IEnumerable<ChatInputExtension> GetInputExtensions()
    {
        return Enumerable.Empty<ChatInputExtension>();
    }

    public Core.Plugins.PluginSettings? GetSettingsUI()
    {
        return new PluginSettings
        {
            Title = "Code Copy Settings",
            Description = "Customize code block copying behavior",
            Fields = new List<SettingField>
            {
                new()
                {
                    Key = "showLineNumbers",
                    Label = "Show Line Numbers",
                    Description = "Display line numbers in code blocks",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                },
                new()
                {
                    Key = "syntaxHighlighting",
                    Label = "Syntax Highlighting",
                    Description = "Enable syntax highlighting for code blocks",
                    Type = SettingFieldType.Boolean,
                    DefaultValue = true
                },
                new()
                {
                    Key = "copyButtonPosition",
                    Label = "Copy Button Position",
                    Type = SettingFieldType.Dropdown,
                    DefaultValue = "top-right",
                    Options = new List<string> { "top-right", "top-left", "bottom-right", "bottom-left" }
                }
            }
        };
    }

    public string? RenderCustomContent(Message message)
    {
        // Could render enhanced code blocks with copy buttons
        // For now, this is handled client-side
        return null;
    }

    private bool HasCodeBlocks(string content)
    {
        // Simple check for markdown code blocks
        return Regex.IsMatch(content, @"```[\s\S]*?```");
    }

    Core.Entities.PluginSettings? IPlugin.GetSettingsUI()
    {
        throw new NotImplementedException();
    }
}
