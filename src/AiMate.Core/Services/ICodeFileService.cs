using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Service for managing code files in the virtual file system
/// </summary>
public interface ICodeFileService
{
    /// <summary>
    /// Get all files for a project
    /// </summary>
    Task<List<CodeFile>> GetProjectFilesAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file tree structure for a project
    /// </summary>
    Task<FileTreeNode> GetFileTreeAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific file by ID
    /// </summary>
    Task<CodeFile?> GetFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a file by path within a project
    /// </summary>
    Task<CodeFile?> GetFileByPathAsync(Guid projectId, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new file
    /// </summary>
    Task<CodeFile> CreateFileAsync(CodeFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing file
    /// </summary>
    Task<CodeFile> UpdateFileAsync(CodeFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file
    /// </summary>
    Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a file path exists in a project
    /// </summary>
    Task<bool> FileExistsAsync(Guid projectId, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search files by content
    /// </summary>
    Task<List<CodeFile>> SearchFilesAsync(Guid projectId, string searchTerm, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a node in the file tree
/// </summary>
public class FileTreeNode
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public Guid? FileId { get; set; }
    public List<FileTreeNode> Children { get; set; } = new();
}
