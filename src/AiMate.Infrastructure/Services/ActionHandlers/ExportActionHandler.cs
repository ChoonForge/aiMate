using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services.ActionHandlers;

/// <summary>
/// Handler for export actions
/// </summary>
public class ExportActionHandler : IActionHandler
{
    private readonly IStructuredContentService _contentService;
    private readonly ILogger<ExportActionHandler> _logger;

    public ActionHandlerType HandlerType => ActionHandlerType.Export;

    public ExportActionHandler(
        IStructuredContentService contentService,
        ILogger<ExportActionHandler> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    public async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!context.Config.TryGetValue("format", out var formatObj) || formatObj is not string formatStr)
            {
                formatStr = "json"; // Default to JSON
            }

            if (!Enum.TryParse<ExportFormat>(formatStr, true, out var format))
            {
                return new ActionResult
                {
                    Success = false,
                    Error = $"Invalid export format: {formatStr}"
                };
            }

            // Get the content to export from metadata
            if (!context.Metadata.TryGetValue("content", out var contentObj) ||
                contentObj is not StructuredContent content)
            {
                return new ActionResult
                {
                    Success = false,
                    Error = "Content to export not found in context"
                };
            }

            _logger.LogInformation("Exporting content as {Format}", format);

            var exportResult = await _contentService.ExportAsync(content, format, cancellationToken);

            if (!exportResult.Success)
            {
                return new ActionResult
                {
                    Success = false,
                    Error = exportResult.Error
                };
            }

            return new ActionResult
            {
                Success = true,
                Data = new
                {
                    exportResult.Data,
                    exportResult.FileName,
                    exportResult.ContentType
                },
                Message = $"Exported successfully as {format}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing export action");
            return new ActionResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public Task<ValidationResult> ValidateAsync(ActionContext context, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        if (context.Config.TryGetValue("format", out var formatObj))
        {
            var formatStr = formatObj?.ToString();
            if (!string.IsNullOrEmpty(formatStr) && !Enum.TryParse<ExportFormat>(formatStr, true, out _))
            {
                result.IsValid = false;
                result.Diagnostics.Add(new CodeDiagnostic
                {
                    Severity = "Error",
                    Message = $"Invalid export format: {formatStr}"
                });
            }
        }

        return Task.FromResult(result);
    }
}
