using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for managing structured content
/// </summary>
public class StructuredContentService : IStructuredContentService
{
    private readonly AiMateDbContext _dbContext;
    private readonly ILogger<StructuredContentService> _logger;
    private readonly Dictionary<ActionHandlerType, IActionHandler> _actionHandlers;

    public StructuredContentService(
        AiMateDbContext dbContext,
        ILogger<StructuredContentService> logger,
        IEnumerable<IActionHandler> actionHandlers)
    {
        _dbContext = dbContext;
        _logger = logger;
        _actionHandlers = actionHandlers.ToDictionary(h => h.HandlerType, h => h);
    }

    // ============================================================================
    // Template Management
    // ============================================================================

    public async Task<List<StructuredContentTemplate>> GetTemplatesAsync(
        StructuredContentType? type = null,
        bool includePrivate = false,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.StructuredContentTemplates.AsQueryable();

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            if (!includePrivate)
            {
                query = query.Where(t => t.IsPublic || t.IsBuiltIn || t.CreatedBy == userId);
            }
            else if (userId.HasValue)
            {
                query = query.Where(t => t.CreatedBy == userId || t.IsPublic || t.IsBuiltIn);
            }

            return await query
                .OrderByDescending(t => t.IsBuiltIn)
                .ThenByDescending(t => t.UsageCount)
                .ThenBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates");
            throw;
        }
    }

    public async Task<StructuredContentTemplate?> GetTemplateByIdAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.StructuredContentTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<StructuredContentTemplate?> GetTemplateByNameAsync(
        string name,
        StructuredContentType type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.StructuredContentTemplates
                .Where(t => t.Name == name && t.Type == type)
                .OrderByDescending(t => t.IsBuiltIn)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {Name} of type {Type}", name, type);
            throw;
        }
    }

    public async Task<StructuredContentTemplate> CreateTemplateAsync(
        StructuredContentTemplate template,
        CancellationToken cancellationToken = default)
    {
        try
        {
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;

            _dbContext.StructuredContentTemplates.Add(template);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created template {TemplateName} ({TemplateId})", template.Name, template.Id);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            throw;
        }
    }

    public async Task<StructuredContentTemplate> UpdateTemplateAsync(
        StructuredContentTemplate template,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await GetTemplateByIdAsync(template.Id, cancellationToken);
            if (existing == null)
            {
                throw new InvalidOperationException($"Template {template.Id} not found");
            }

            template.UpdatedAt = DateTime.UtcNow;
            _dbContext.StructuredContentTemplates.Update(template);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated template {TemplateName} ({TemplateId})", template.Name, template.Id);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", template.Id);
            throw;
        }
    }

    public async Task DeleteTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await GetTemplateByIdAsync(templateId, cancellationToken);
            if (template == null)
            {
                throw new InvalidOperationException($"Template {templateId} not found");
            }

            if (template.IsBuiltIn)
            {
                throw new InvalidOperationException("Cannot delete built-in templates");
            }

            _dbContext.StructuredContentTemplates.Remove(template);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted template {TemplateId}", templateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", templateId);
            throw;
        }
    }

    // ============================================================================
    // Content Operations
    // ============================================================================

    public async Task<StructuredContent?> ParseContentAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Look for structured content markers
            var match = Regex.Match(
                content,
                @"```structuredcontent\s*\n([\s\S]*?)\n```",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
            {
                // Try JSON block
                match = Regex.Match(
                    content,
                    @"\{[\s\S]*""structuredContent""[\s\S]*\}",
                    RegexOptions.IgnoreCase
                );

                if (!match.Success)
                {
                    return null;
                }
            }

            var jsonContent = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;

            // Try to parse as wrapper format first
            var wrapper = JsonSerializer.Deserialize<StructuredContentWrapper>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (wrapper?.StructuredContent != null)
            {
                return wrapper.StructuredContent;
            }

            // Try direct parse
            var direct = JsonSerializer.Deserialize<StructuredContent>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return direct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing structured content");
            return null;
        }
    }

    public async Task<ContentValidationResult> ValidateContentAsync(
        StructuredContent content,
        CancellationToken cancellationToken = default)
    {
        var result = new ContentValidationResult { IsValid = true };

        try
        {
            // Basic validation
            if (content.Data == null)
            {
                result.Errors.Add("Content data is required");
                result.IsValid = false;
            }

            // Get template if specified
            if (!string.IsNullOrEmpty(content.Template))
            {
                var template = await GetTemplateByNameAsync(content.Template, content.Type, cancellationToken);
                if (template == null)
                {
                    result.Warnings.Add($"Template '{content.Template}' not found, will use default");
                }
            }

            // Type-specific validation
            switch (content.Type)
            {
                case StructuredContentType.Table:
                    ValidateTableContent(content, result);
                    break;
                case StructuredContentType.KeyValueList:
                    ValidateKeyValueListContent(content, result);
                    break;
                // Add more type-specific validation as needed
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating content");
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
            return result;
        }
    }

    public async Task<RenderedContent> RenderContentAsync(
        StructuredContent content,
        string? templateName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            templateName ??= content.Template ?? "default";

            var template = await GetTemplateByNameAsync(templateName, content.Type, cancellationToken);

            // Increment usage count
            if (template != null)
            {
                template.UsageCount++;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var rendered = new RenderedContent
            {
                Content = content,
                Template = template,
                RenderData = new Dictionary<string, object>
                {
                    ["type"] = content.Type.ToString(),
                    ["hasTemplate"] = template != null,
                    ["templateName"] = templateName
                }
            };

            return rendered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering content");
            throw;
        }
    }

    // ============================================================================
    // Action Execution
    // ============================================================================

    public async Task<ActionResult> ExecuteActionAsync(
        ActionContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_actionHandlers.TryGetValue(context.HandlerType, out var handler))
            {
                return new ActionResult
                {
                    Success = false,
                    Error = $"Handler for {context.HandlerType} not found"
                };
            }

            _logger.LogInformation("Executing action {ActionId} with handler {HandlerType}",
                context.ActionId, context.HandlerType);

            return await handler.ExecuteAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action {ActionId}", context.ActionId);
            return new ActionResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    // ============================================================================
    // Export
    // ============================================================================

    public async Task<ExportResult> ExportAsync(
        StructuredContent content,
        ExportFormat format,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting content as {Format}", format);

            return format switch
            {
                ExportFormat.Json => await ExportAsJsonAsync(content),
                ExportFormat.Csv => await ExportAsCsvAsync(content),
                ExportFormat.Excel => await ExportAsExcelAsync(content),
                ExportFormat.Pdf => await ExportAsPdfAsync(content),
                _ => new ExportResult { Success = false, Error = "Unsupported format" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting content");
            return new ExportResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    // ============================================================================
    // Private Helper Methods
    // ============================================================================

    private void ValidateTableContent(StructuredContent content, ContentValidationResult result)
    {
        // Add table-specific validation
        // Check for columns, data format, etc.
    }

    private void ValidateKeyValueListContent(StructuredContent content, ContentValidationResult result)
    {
        // Add key-value list specific validation
    }

    private Task<ExportResult> ExportAsJsonAsync(StructuredContent content)
    {
        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Task.FromResult(new ExportResult
        {
            Success = true,
            Data = System.Text.Encoding.UTF8.GetBytes(json),
            FileName = $"export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json",
            ContentType = "application/json"
        });
    }

    private Task<ExportResult> ExportAsCsvAsync(StructuredContent content)
    {
        try
        {
            var csv = new StringBuilder();

            // CSV export is primarily for tabular content (Table, KeyValueList)
            if (content.Type == StructuredContentType.Table)
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(content.Data.ToString() ?? "{}");

                if (data == null)
                {
                    return Task.FromResult(new ExportResult
                    {
                        Success = false,
                        Error = "Failed to parse table data"
                    });
                }

                // Get columns
                var columns = new List<string>();
                if (data.TryGetValue("columns", out var columnsObj) && columnsObj is JsonElement columnsElement)
                {
                    columns = JsonSerializer.Deserialize<List<string>>(columnsElement.GetRawText()) ?? new List<string>();
                }

                // Write header
                csv.AppendLine(string.Join(",", columns.Select(EscapeCsvValue)));

                // Write rows
                if (data.TryGetValue("rows", out var rowsObj) && rowsObj is JsonElement rowsElement)
                {
                    var rows = JsonSerializer.Deserialize<List<List<object>>>(rowsElement.GetRawText());
                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            csv.AppendLine(string.Join(",", row.Select(cell => EscapeCsvValue(cell?.ToString() ?? ""))));
                        }
                    }
                }
            }
            else if (content.Type == StructuredContentType.KeyValueList)
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(content.Data.ToString() ?? "{}");

                if (data == null)
                {
                    return Task.FromResult(new ExportResult
                    {
                        Success = false,
                        Error = "Failed to parse key-value data"
                    });
                }

                // Header
                csv.AppendLine("Key,Value");

                // Items
                if (data.TryGetValue("items", out var itemsObj) && itemsObj is JsonElement itemsElement)
                {
                    var items = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(itemsElement.GetRawText());
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var key = item.ContainsKey("key") ? item["key"]?.ToString() ?? "" : "";
                            var value = item.ContainsKey("value") ? item["value"]?.ToString() ?? "" : "";
                            csv.AppendLine($"{EscapeCsvValue(key)},{EscapeCsvValue(value)}");
                        }
                    }
                }
            }
            else
            {
                return Task.FromResult(new ExportResult
                {
                    Success = false,
                    Error = $"CSV export not supported for content type: {content.Type}"
                });
            }

            return Task.FromResult(new ExportResult
            {
                Success = true,
                Data = Encoding.UTF8.GetBytes(csv.ToString()),
                FileName = $"export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv",
                ContentType = "text/csv"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting as CSV");
            return Task.FromResult(new ExportResult
            {
                Success = false,
                Error = $"CSV export failed: {ex.Message}"
            });
        }
    }

    private static string EscapeCsvValue(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private Task<ExportResult> ExportAsExcelAsync(StructuredContent content)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Export");

            // Excel export is primarily for tabular content (Table, KeyValueList)
            if (content.Type == StructuredContentType.Table)
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(content.Data.ToString() ?? "{}");

                if (data == null)
                {
                    return Task.FromResult(new ExportResult
                    {
                        Success = false,
                        Error = "Failed to parse table data"
                    });
                }

                // Get columns
                var columns = new List<string>();
                if (data.TryGetValue("columns", out var columnsObj) && columnsObj is JsonElement columnsElement)
                {
                    columns = JsonSerializer.Deserialize<List<string>>(columnsElement.GetRawText()) ?? new List<string>();
                }

                // Write header
                for (int i = 0; i < columns.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = columns[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                }

                // Write rows
                if (data.TryGetValue("rows", out var rowsObj) && rowsObj is JsonElement rowsElement)
                {
                    var rows = JsonSerializer.Deserialize<List<List<object>>>(rowsElement.GetRawText());
                    if (rows != null)
                    {
                        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                        {
                            var row = rows[rowIndex];
                            for (int colIndex = 0; colIndex < row.Count; colIndex++)
                            {
                                worksheet.Cell(rowIndex + 2, colIndex + 1).Value = row[colIndex]?.ToString() ?? "";
                            }
                        }
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();
            }
            else if (content.Type == StructuredContentType.KeyValueList)
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(content.Data.ToString() ?? "{}");

                if (data == null)
                {
                    return Task.FromResult(new ExportResult
                    {
                        Success = false,
                        Error = "Failed to parse key-value data"
                    });
                }

                // Header
                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "Value";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 2).Style.Font.Bold = true;

                // Items
                if (data.TryGetValue("items", out var itemsObj) && itemsObj is JsonElement itemsElement)
                {
                    var items = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(itemsElement.GetRawText());
                    if (items != null)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            var item = items[i];
                            var key = item.ContainsKey("key") ? item["key"]?.ToString() ?? "" : "";
                            var value = item.ContainsKey("value") ? item["value"]?.ToString() ?? "" : "";

                            worksheet.Cell(i + 2, 1).Value = key;
                            worksheet.Cell(i + 2, 2).Value = value;
                        }
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();
            }
            else
            {
                return Task.FromResult(new ExportResult
                {
                    Success = false,
                    Error = $"Excel export not supported for content type: {content.Type}"
                });
            }

            // Save to memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return Task.FromResult(new ExportResult
            {
                Success = true,
                Data = stream.ToArray(),
                FileName = $"export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting as Excel");
            return Task.FromResult(new ExportResult
            {
                Success = false,
                Error = $"Excel export failed: {ex.Message}"
            });
        }
    }

    private Task<ExportResult> ExportAsPdfAsync(StructuredContent content)
    {
        // TODO: Implement PDF export
        return Task.FromResult(new ExportResult
        {
            Success = false,
            Error = "PDF export not yet implemented"
        });
    }
}

/// <summary>
/// Wrapper for parsing structured content from AI responses
/// </summary>
internal class StructuredContentWrapper
{
    public StructuredContent? StructuredContent { get; set; }
}
