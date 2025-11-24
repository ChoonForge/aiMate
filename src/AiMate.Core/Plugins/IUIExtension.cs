using AiMate.Core.Entities;

namespace AiMate.Core.Plugins;

/// <summary>
/// Extends the chat UI with custom elements
/// Use cases: Message actions, input buttons, custom renderers
/// </summary>
public interface IUIExtension : IPlugin
{
    /// <summary>
    /// Add custom buttons to message actions (copy, share, etc.)
    /// </summary>
    IEnumerable<MessageActionButton> GetMessageActions(Message message);

    /// <summary>
    /// Add custom UI elements to chat input area (file upload, voice, etc.)
    /// </summary>
    IEnumerable<ChatInputExtension> GetInputExtensions();

    /// <summary>
    /// Add custom UI to settings/plugin settings modal
    /// </summary>
    PluginSettings? GetSettingsUI();

    /// <summary>
    /// Render custom content in message bubble (charts, tables, etc.)
    /// Returns HTML or component reference
    /// </summary>
    string? RenderCustomContent(Message message);
}

/// <summary>
/// Button action on messages
/// </summary>
public class MessageActionButton
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public string Color { get; set; } = "Default"; // Primary, Secondary, Success, Error, etc.
    public bool ShowOnUserMessages { get; set; } = false;
    public bool ShowOnAssistantMessages { get; set; } = true;
    public int Order { get; set; } = 0;

    /// <summary>
    /// Client-side callback (JS function name or Blazor component method)
    /// </summary>
    public string? OnClickHandler { get; set; }

    /// <summary>
    /// Context data for the action (e.g., messageId, conversationId)
    /// </summary>
    public Dictionary<string, string> Context { get; set; } = new();
}

/// <summary>
/// Extension to chat input area
/// </summary>
public class ChatInputExtension
{
    public string Id { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public int Order { get; set; } = 0;

    /// <summary>
    /// Client-side callback
    /// </summary>
    public string? OnClickHandler { get; set; }
}

/// <summary>
/// Plugin settings UI definition
/// </summary>
public class PluginSettings
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<SettingField> Fields { get; set; } = new();
}

/// <summary>
/// Individual setting field
/// </summary>
public class SettingField
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SettingFieldType Type { get; set; }
    public object? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public List<string>? Options { get; set; } // For dropdown
    public int? Min { get; set; } // For number
    public int? Max { get; set; } // For number
    public bool Required { get; set; } = false;
}

/// <summary>
/// Field types for plugin settings
/// </summary>
public enum SettingFieldType
{
    Text,
    Number,
    Boolean,
    Dropdown,
    TextArea,
    Color,
    Date,
    Time,
    Url,
    Email
}
