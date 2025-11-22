using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services.ActionHandlers;

/// <summary>
/// Handler for navigation actions
/// </summary>
public class NavigationActionHandler : IActionHandler
{
    private readonly ILogger<NavigationActionHandler> _logger;

    public ActionHandlerType HandlerType => ActionHandlerType.Navigation;

    public NavigationActionHandler(ILogger<NavigationActionHandler> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!context.Config.TryGetValue("url", out var urlObj) || urlObj is not string url)
            {
                return Task.FromResult(new ActionResult
                {
                    Success = false,
                    Error = "URL is required for navigation action"
                });
            }

            // Replace placeholders with parameters
            foreach (var param in context.Parameters)
            {
                url = url.Replace($"{{{param.Key}}}", param.Value?.ToString() ?? "");
            }

            // Replace item ID if present
            if (!string.IsNullOrEmpty(context.ItemId))
            {
                url = url.Replace("{id}", context.ItemId);
            }

            var target = context.Config.TryGetValue("target", out var targetObj)
                ? targetObj?.ToString() ?? "_self"
                : "_self";

            _logger.LogInformation("Navigation to {Url} (target: {Target})", url, target);

            return Task.FromResult(new ActionResult
            {
                Success = true,
                Data = new
                {
                    url,
                    target
                },
                Message = $"Navigating to {url}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing navigation action");
            return Task.FromResult(new ActionResult
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    public Task<ValidationResult> ValidateAsync(ActionContext context, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        if (!context.Config.ContainsKey("url"))
        {
            result.IsValid = false;
            result.Diagnostics.Add(new CodeDiagnostic
            {
                Severity = "Error",
                Message = "URL is required for navigation action"
            });
        }

        return Task.FromResult(result);
    }
}
