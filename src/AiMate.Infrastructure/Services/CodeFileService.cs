using System.Security.Cryptography;
using System.Text;
using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for managing code files in the virtual file system
/// </summary>
public class CodeFileService : ICodeFileService
{
    private readonly AiMateDbContext _dbContext;
    private readonly ILogger<CodeFileService> _logger;

    public CodeFileService(
        AiMateDbContext dbContext,
        ILogger<CodeFileService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<CodeFile>> GetProjectFilesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.CodeFiles
                .Where(f => f.ProjectId == projectId)
                .OrderBy(f => f.Path)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<FileTreeNode> GetFileTreeAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var files = await GetProjectFilesAsync(projectId, cancellationToken);

            var root = new FileTreeNode
            {
                Name = "root",
                Path = "/",
                IsDirectory = true
            };

            foreach (var file in files)
            {
                AddFileToTree(root, file);
            }

            return root;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building file tree for project {ProjectId}", projectId);
            throw;
        }
    }

    private void AddFileToTree(FileTreeNode root, CodeFile file)
    {
        var parts = file.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = root;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            var isLastPart = i == parts.Length - 1;

            var existing = current.Children.FirstOrDefault(c => c.Name == part);
            if (existing == null)
            {
                var newNode = new FileTreeNode
                {
                    Name = part,
                    Path = string.Join("/", parts.Take(i + 1)),
                    IsDirectory = !isLastPart,
                    FileId = isLastPart ? file.Id : null
                };
                current.Children.Add(newNode);
                current = newNode;
            }
            else
            {
                current = existing;
            }
        }
    }

    public async Task<CodeFile?> GetFileByIdAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.CodeFiles
                .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file {FileId}", fileId);
            throw;
        }
    }

    public async Task<CodeFile?> GetFileByPathAsync(
        Guid projectId,
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.CodeFiles
                .FirstOrDefaultAsync(f => f.ProjectId == projectId && f.Path == path, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file by path {Path} in project {ProjectId}", path, projectId);
            throw;
        }
    }

    public async Task<CodeFile> CreateFileAsync(
        CodeFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if file already exists
            var exists = await FileExistsAsync(file.ProjectId, file.Path, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"File already exists at path: {file.Path}");
            }

            // Calculate content hash
            file.ContentHash = ComputeHash(file.Content);
            file.SizeBytes = Encoding.UTF8.GetByteCount(file.Content);
            file.CreatedAt = DateTime.UtcNow;
            file.LastModified = DateTime.UtcNow;
            file.Version = 1;

            _dbContext.CodeFiles.Add(file);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created file {Path} in project {ProjectId}", file.Path, file.ProjectId);

            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating file {Path}", file.Path);
            throw;
        }
    }

    public async Task<CodeFile> UpdateFileAsync(
        CodeFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await GetFileByIdAsync(file.Id, cancellationToken);
            if (existing == null)
            {
                throw new InvalidOperationException($"File not found: {file.Id}");
            }

            // Update content and metadata
            existing.Content = file.Content;
            existing.ContentHash = ComputeHash(file.Content);
            existing.SizeBytes = Encoding.UTF8.GetByteCount(file.Content);
            existing.LastModified = DateTime.UtcNow;
            existing.LastModifiedBy = file.LastModifiedBy;
            existing.Version++;
            existing.IsDirty = file.IsDirty;

            // Update optional fields
            if (!string.IsNullOrEmpty(file.Language))
                existing.Language = file.Language;
            if (file.Tags.Any())
                existing.Tags = file.Tags;
            if (file.Metadata.Any())
                existing.Metadata = file.Metadata;

            _dbContext.CodeFiles.Update(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated file {Path} (version {Version})", existing.Path, existing.Version);

            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating file {FileId}", file.Id);
            throw;
        }
    }

    public async Task DeleteFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var file = await GetFileByIdAsync(fileId, cancellationToken);
            if (file == null)
            {
                throw new InvalidOperationException($"File not found: {fileId}");
            }

            _dbContext.CodeFiles.Remove(file);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted file {Path}", file.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(
        Guid projectId,
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.CodeFiles
                .AnyAsync(f => f.ProjectId == projectId && f.Path == path, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if file exists {Path}", path);
            throw;
        }
    }

    public async Task<List<CodeFile>> SearchFilesAsync(
        Guid projectId,
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.CodeFiles
                .Where(f => f.ProjectId == projectId &&
                           (f.Path.Contains(searchTerm) || f.Content.Contains(searchTerm)))
                .OrderBy(f => f.Path)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files for term {SearchTerm}", searchTerm);
            throw;
        }
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
