using AiMate.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Local filesystem implementation of file storage
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _storagePath;
    private readonly long _maxFileSizeBytes;

    public LocalFileStorageService(
        IConfiguration configuration,
        ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;

        // Get storage path from configuration, default to "uploads" directory
        _storagePath = configuration["FileStorage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        // Max file size (default 50MB)
        _maxFileSizeBytes = configuration.GetValue<long>("FileStorage:MaxFileSizeMB", 50) * 1024 * 1024;

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
            _logger.LogInformation("Created storage directory at {Path}", _storagePath);
        }
    }

    public async Task<FileUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file size
            if (fileStream.Length > _maxFileSizeBytes)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"File size exceeds maximum allowed size of {_maxFileSizeBytes / 1024 / 1024}MB"
                };
            }

            // Sanitize filename
            var sanitizedFileName = SanitizeFileName(fileName);

            // Create user-specific subdirectory
            var userDirectory = Path.Combine(_storagePath, userId.ToString());
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            // Generate unique filename to prevent collisions
            var fileExtension = Path.GetExtension(sanitizedFileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sanitizedFileName);
            var uniqueFileName = $"{fileNameWithoutExtension}_{Guid.NewGuid()}{fileExtension}";

            var filePath = Path.Combine(userDirectory, uniqueFileName);

            // Write file to disk
            using (var fileStreamOut = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamOut, cancellationToken);
            }

            // Get relative path for storage
            var relativePath = Path.GetRelativePath(_storagePath, filePath);

            _logger.LogInformation("File uploaded successfully: {FilePath} ({Size} bytes)", relativePath, fileStream.Length);

            return new FileUploadResult
            {
                Success = true,
                FilePath = relativePath,
                FileName = sanitizedFileName,
                FileSize = fileStream.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = $"Upload failed: {ex.Message}"
            };
        }
    }

    public async Task<FileDownloadResult?> DownloadFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, filePath);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return null;
            }

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = Path.GetFileName(fullPath);
            var fileInfo = new FileInfo(fullPath);

            return new FileDownloadResult
            {
                FileStream = fileStream,
                FileName = fileName,
                ContentType = GetContentType(fileName),
                FileSize = fileInfo.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file {FilePath}", filePath);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_storagePath, filePath);
        return File.Exists(fullPath);
    }

    public async Task<long> GetFileSizeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_storagePath, filePath);

        if (!File.Exists(fullPath))
            return 0;

        var fileInfo = new FileInfo(fullPath);
        return fileInfo.Length;
    }

    #region Helper Methods

    private static string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Limit length
        if (sanitized.Length > 200)
            sanitized = sanitized.Substring(0, 200);

        return sanitized;
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            ".csv" => "text/csv",
            ".md" => "text/markdown",
            _ => "application/octet-stream"
        };
    }

    #endregion
}
