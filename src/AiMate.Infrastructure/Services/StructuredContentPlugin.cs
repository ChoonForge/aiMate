using AiMate.Core.Entities;
using AiMate.Core.Plugins;
using AiMate.Core.Services;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Plugin that intercepts messages containing structured content
/// </summary>
public class StructuredContentPlugin : IPlugin, IMessageInterceptor
{
    private readonly IStructuredContentService _contentService;
    private readonly ILogger<StructuredContentPlugin> _logger;

    public string Id => "structured-content";
    public string Name => "Structured Content Renderer";
    public string Version => "1.0.0";
    public string Description => "Renders rich, interactive structured data from AI responses";
    public string Author => "aiMate";
    public string? Icon => "table-cells";
    public string Category => "Content Enhancement";
    public bool IsEnabled { get; set; } = true;

    public StructuredContentPlugin(
        IStructuredContentService contentService,
        ILogger<StructuredContentPlugin> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    public Task InitializeAsync()
    {
        _logger.LogInformation("Initialized {PluginName} plugin", Name);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposed {PluginName} plugin", Name);
        return ValueTask.CompletedTask;
    }

    public async Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context)
    {
        // No modification on outgoing messages
        return await Task.FromResult(new MessageInterceptResult { Continue = true });
    }

    public async Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context)
    {
        try
        {
            // Check if message contains structured content
            if (string.IsNullOrEmpty(message.Content))
            {
                return new MessageInterceptResult { Continue = true };
            }

            // Try to parse structured content
            var structuredContent = await _contentService.ParseContentAsync(message.Content);

            if (structuredContent == null)
            {
                // No structured content found, continue normally
                return new MessageInterceptResult { Continue = true };
            }

            _logger.LogInformation(
                "Found structured content in message {MessageId}: Type={Type}, Template={Template}",
                message.Id, structuredContent.Type, structuredContent.Template ?? "default"
            );

            // Validate the content
            var validation = await _contentService.ValidateContentAsync(structuredContent);

            if (!validation.IsValid)
            {
                _logger.LogWarning(
                    "Structured content validation failed: {Errors}",
                    string.Join(", ", validation.Errors)
                );

                // Add validation warnings to metadata
                message.Metadata = message.Metadata ?? new Dictionary<string, object>();
                message.Metadata["structuredContentValidation"] = new
                {
                    validation.IsValid,
                    validation.Errors,
                    validation.Warnings
                };
            }

            // Render the content
            var rendered = await _contentService.RenderContentAsync(structuredContent);

            // Add structured content metadata to the message
            message.Metadata = message.Metadata ?? new Dictionary<string, object>();
            message.Metadata["hasStructuredContent"] = true;
            message.Metadata["structuredContent"] = structuredContent;
            message.Metadata["structuredContentType"] = structuredContent.Type.ToString();
            message.Metadata["structuredContentTemplate"] = structuredContent.Template ?? "default";
            message.Metadata["renderedContent"] = rendered;

            // Optionally modify the message content to remove the JSON block
            // and add a placeholder (UI can decide how to render)
            if (message.Content.Contains("```structuredcontent"))
            {
                var cleanedContent = System.Text.RegularExpressions.Regex.Replace(
                    message.Content,
                    @"```structuredcontent\s*\n[\s\S]*?\n```",
                    "[Structured Content Rendered Below]"
                );

                message.Content = cleanedContent;
            }

            _logger.LogInformation(
                "Processed structured content for message {MessageId}",
                message.Id
            );

            return new MessageInterceptResult
            {
                ModifiedMessage = message,
                Continue = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing structured content in message {MessageId}", message.Id);

            // Don't block message on error, just continue
            return new MessageInterceptResult { Continue = true };
        }
    }
}
