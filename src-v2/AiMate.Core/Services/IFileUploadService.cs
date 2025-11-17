using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// File upload service - handle file uploads to workspaces
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Upload file to workspace
    /// </summary>
    Task<WorkspaceFile> UploadFileAsync(
        Guid workspaceId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file from workspace
    /// </summary>
    Task<(Stream Stream, string ContentType)?> GetFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from workspace
    /// </summary>
    Task DeleteFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all files for workspace
    /// </summary>
    Task<List<WorkspaceFile>> GetWorkspaceFilesAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file size limit in bytes
    /// </summary>
    long GetFileSizeLimit();
}
