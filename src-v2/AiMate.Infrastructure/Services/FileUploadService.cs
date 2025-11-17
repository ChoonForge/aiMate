using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// File upload service implementation - stores files locally
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly AiMateDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileUploadService> _logger;
    private const long DefaultFileSizeLimit = 10 * 1024 * 1024; // 10MB

    public FileUploadService(
        AiMateDbContext context,
        IConfiguration configuration,
        ILogger<FileUploadService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<WorkspaceFile> UploadFileAsync(
        Guid workspaceId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // Validate file size
        if (fileStream.Length > GetFileSizeLimit())
        {
            throw new InvalidOperationException($"File size exceeds limit of {GetFileSizeLimit() / 1024 / 1024}MB");
        }

        // Create uploads directory if it doesn't exist
        var uploadPath = GetUploadPath();
        Directory.CreateDirectory(uploadPath);

        // Generate unique file name
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        // Save file to disk
        using (var fileStreamDisk = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamDisk, cancellationToken);
        }

        // Create database record
        var workspaceFile = new WorkspaceFile
        {
            WorkspaceId = workspaceId,
            FileName = fileName,
            FilePath = filePath,
            FileSize = fileStream.Length,
            MimeType = contentType
        };

        _context.WorkspaceFiles.Add(workspaceFile);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("File uploaded: {FileName} ({FileSize} bytes) to workspace {WorkspaceId}",
            fileName, fileStream.Length, workspaceId);

        return workspaceFile;
    }

    public async Task<(Stream Stream, string ContentType)?> GetFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var workspaceFile = await _context.WorkspaceFiles
            .FindAsync(new object[] { fileId }, cancellationToken);

        if (workspaceFile == null || !File.Exists(workspaceFile.FilePath))
        {
            return null;
        }

        var stream = new FileStream(workspaceFile.FilePath, FileMode.Open, FileAccess.Read);
        return (stream, workspaceFile.MimeType);
    }

    public async Task DeleteFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var workspaceFile = await _context.WorkspaceFiles
            .FindAsync(new object[] { fileId }, cancellationToken);

        if (workspaceFile != null)
        {
            // Delete from disk
            if (File.Exists(workspaceFile.FilePath))
            {
                File.Delete(workspaceFile.FilePath);
            }

            // Delete from database
            _context.WorkspaceFiles.Remove(workspaceFile);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("File deleted: {FileId}", fileId);
        }
    }

    public async Task<List<WorkspaceFile>> GetWorkspaceFilesAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceFiles
            .Where(f => f.WorkspaceId == workspaceId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public long GetFileSizeLimit()
    {
        return _configuration.GetValue<long?>("FileUpload:MaxSizeBytes") ?? DefaultFileSizeLimit;
    }

    private string GetUploadPath()
    {
        return _configuration["FileUpload:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    }
}
