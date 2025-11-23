using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiMate.Infrastructure.Services.ActionHandlers;

/// <summary>
/// Handler for export actions
/// </summary>
public class ExportActionHandler : IActionHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExportActionHandler> _logger;

    public ActionHandlerType HandlerType => ActionHandlerType.Export;

    public ExportActionHandler(
        IServiceProvider serviceProvider,
        ILogger<ExportActionHandler> logger)
    {
        _serviceProvider = serviceProvider;
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

            var contentService = _serviceProvider.GetRequiredService<IStructuredContentService>();
            var exportResult = await contentService.ExportAsync(content, format, cancellationToken);

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

/// <summary>
/// CodeFile entity configuration for EF Core
/// </summary>
public static class CodeFileEntityConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CodeFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Path);
            entity.HasIndex(e => new { e.ProjectId, e.Path }).IsUnique();
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => e.LastModified);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Metadata dictionary as JSON column
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, default(JsonSerializerOptions)) ?? new Dictionary<string, string>())
                .HasColumnType("jsonb")
                .IsRequired(false);

            // Configure Tags list as JSON column
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<List<string>>(v, default(JsonSerializerOptions)) ?? new List<string>())
                .HasColumnType("jsonb")
                .IsRequired(false);
        });
    }
}
