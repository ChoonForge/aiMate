namespace AiMate.Core.Services;

/// <summary>
/// Service for file storage operations (local, Azure Blob, S3, etc.)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return its stored path/URL
    /// </summary>
    Task<FileUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Download a file by its path
    /// </summary>
    Task<FileDownloadResult?> DownloadFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file
    /// </summary>
    Task<bool> DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a file exists
    /// </summary>
    Task<bool> FileExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file size in bytes
    /// </summary>
    Task<long> GetFileSizeAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of file upload operation
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of file download operation
/// </summary>
public class FileDownloadResult
{
    public Stream FileStream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long FileSize { get; set; }
}
