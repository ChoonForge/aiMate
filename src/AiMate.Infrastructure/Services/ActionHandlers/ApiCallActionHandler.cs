using System.Text;
using System.Text.Json;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services.ActionHandlers;

/// <summary>
/// Handler for API call actions
/// </summary>
public class ApiCallActionHandler : IActionHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiCallActionHandler> _logger;

    public ActionHandlerType HandlerType => ActionHandlerType.ApiCall;

    public ApiCallActionHandler(
        IHttpClientFactory httpClientFactory,
        ILogger<ApiCallActionHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!context.Config.TryGetValue("endpoint", out var endpointObj) || endpointObj is not string endpoint)
            {
                return new ActionResult
                {
                    Success = false,
                    Error = "Endpoint is required for API call action"
                };
            }

            var method = context.Config.TryGetValue("method", out var methodObj)
                ? methodObj?.ToString()?.ToUpper() ?? "GET"
                : "GET";

            // Replace placeholders
            foreach (var param in context.Parameters)
            {
                endpoint = endpoint.Replace($"{{{param.Key}}}", param.Value?.ToString() ?? "");
            }

            if (!string.IsNullOrEmpty(context.ItemId))
            {
                endpoint = endpoint.Replace("{id}", context.ItemId);
            }

            _logger.LogInformation("Executing API call: {Method} {Endpoint}", method, endpoint);

            var httpClient = _httpClientFactory.CreateClient();

            // Add custom headers
            if (context.Config.TryGetValue("headers", out var headersObj) &&
                headersObj is Dictionary<string, object> headers)
            {
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value?.ToString());
                }
            }

            HttpResponseMessage response;

            switch (method)
            {
                case "GET":
                    response = await httpClient.GetAsync(endpoint, cancellationToken);
                    break;

                case "POST":
                case "PUT":
                case "PATCH":
                    var body = context.Config.TryGetValue("body", out var bodyObj)
                        ? JsonSerializer.Serialize(bodyObj)
                        : "{}";
                    var content = new StringContent(body, Encoding.UTF8, "application/json");

                    response = method == "POST"
                        ? await httpClient.PostAsync(endpoint, content, cancellationToken)
                        : method == "PUT"
                            ? await httpClient.PutAsync(endpoint, content, cancellationToken)
                            : await httpClient.PatchAsync(endpoint, content, cancellationToken);
                    break;

                case "DELETE":
                    response = await httpClient.DeleteAsync(endpoint, cancellationToken);
                    break;

                default:
                    return new ActionResult
                    {
                        Success = false,
                        Error = $"Unsupported HTTP method: {method}"
                    };
            }

            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
            var isSuccess = response.IsSuccessStatusCode;

            var refreshOnSuccess = context.Config.TryGetValue("refreshOnSuccess", out var refreshObj) &&
                                   refreshObj is bool refresh && refresh;

            _logger.LogInformation("API call completed: {StatusCode}", response.StatusCode);

            return new ActionResult
            {
                Success = isSuccess,
                Data = responseData,
                Message = isSuccess
                    ? context.Config.TryGetValue("successMessage", out var successMsg)
                        ? successMsg?.ToString()
                        : "Action completed successfully"
                    : context.Config.TryGetValue("errorMessage", out var errorMsg)
                        ? errorMsg?.ToString()
                        : $"Action failed with status {response.StatusCode}",
                RefreshContent = isSuccess && refreshOnSuccess
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing API call action");
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

        if (!context.Config.ContainsKey("endpoint"))
        {
            result.IsValid = false;
            result.Diagnostics.Add(new CodeDiagnostic
            {
                Severity = "Error",
                Message = "Endpoint is required for API call action"
            });
        }

        return Task.FromResult(result);
    }
}
