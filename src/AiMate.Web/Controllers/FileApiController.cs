using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for file upload, download, and management
/// </summary>
[ApiController]
[Route("api/v1/files")]
[Authorize]
public class FileApiController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private readonly AiMateDbContext _context;
    private readonly ILogger<FileApiController> _logger;

    // Allowed file extensions
    private static readonly string[] AllowedExtensions = new[]
    {
        ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx",
        ".png", ".jpg", ".jpeg", ".gif", ".svg",
        ".json", ".xml", ".csv", ".md", ".zip"
    };

    public FileApiController(
        IFileStorageService fileStorage,
        AiMateDbContext context,
        ILogger<FileApiController> logger)
    {
        _fileStorage = fileStorage;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file to a workspace
    /// </summary>
    /// <param name="workspaceId">Workspace ID</param>
    /// <param name="file">File to upload</param>
    /// <param name="description">Optional file description</param>
    /// <returns>Uploaded file metadata</returns>
    /// <response code="201">File uploaded successfully</response>
    /// <response code="400">Invalid file or file type not allowed</response>
    /// <response code="404">Workspace not found</response>
    /// <response code="413">File too large</response>
    /// <remarks>
    /// Upload files to a workspace. Supported formats: PDF, Word, Excel, images, text, JSON, XML, CSV, Markdown, ZIP.
    /// Maximum file size: 50MB (configurable).
    ///
    /// Example:
    ///
    ///     POST /api/v1/files/upload?workspaceId=guid
    ///     Content-Type: multipart/form-data
    ///
    ///     file: [binary data]
    ///     description: "Project documentation"
    ///
    /// </remarks>
    [HttpPost("upload")]
    [RequestSizeLimit(52428800)] // 50MB
    [ProducesResponseType(typeof(WorkspaceFile), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UploadFile(
        [FromQuery] Guid workspaceId,
        IFormFile file,
        [FromForm] string? description = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest($"File type '{extension}' not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
        }

        var userId = GetUserId();

        // Verify workspace exists and user has access
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == workspaceId && w.UserId == userId, cancellationToken);

        if (workspace == null)
            return NotFound("Workspace not found or access denied");

        // Upload file to storage
        using var stream = file.OpenReadStream();
        var uploadResult = await _fileStorage.UploadFileAsync(
            stream, file.FileName, file.ContentType, userId, cancellationToken);

        if (!uploadResult.Success)
        {
            _logger.LogError("File upload failed: {Error}", uploadResult.ErrorMessage);
            return BadRequest(uploadResult.ErrorMessage);
        }

        // Create database record
        var workspaceFile = new WorkspaceFile
        {
            WorkspaceId = workspaceId,
            FileName = uploadResult.FileName!,
            FilePath = uploadResult.FilePath!,
            FileSize = uploadResult.FileSize,
            MimeType = file.ContentType,
            Description = description,
            UploadedAt = DateTime.UtcNow
        };

        _context.WorkspaceFiles.Add(workspaceFile);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} uploaded file {FileName} to workspace {WorkspaceId}",
            userId, file.FileName, workspaceId);

        return CreatedAtAction(nameof(GetFile), new { id = workspaceFile.Id }, workspaceFile);
    }

    /// <summary>
    /// Get file metadata by ID
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>File metadata</returns>
    /// <response code="200">Returns file metadata</response>
    /// <response code="404">File not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkspaceFile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var file = await _context.WorkspaceFiles
            .Include(f => f.Workspace)
            .FirstOrDefaultAsync(f => f.Id == id && f.Workspace!.UserId == userId, cancellationToken);

        if (file == null)
            return NotFound();

        return Ok(file);
    }

    /// <summary>
    /// Download a file
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>File binary data</returns>
    /// <response code="200">Returns file for download</response>
    /// <response code="404">File not found</response>
    /// <remarks>
    /// Download a file from a workspace. Returns the raw file data.
    ///
    /// Example:
    ///
    ///     GET /api/v1/files/guid/download
    ///
    /// </remarks>
    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        // Get file metadata from database
        var fileMetadata = await _context.WorkspaceFiles
            .Include(f => f.Workspace)
            .FirstOrDefaultAsync(f => f.Id == id && f.Workspace!.UserId == userId, cancellationToken);

        if (fileMetadata == null)
            return NotFound("File not found or access denied");

        // Download file from storage
        var downloadResult = await _fileStorage.DownloadFileAsync(fileMetadata.FilePath, cancellationToken);

        if (downloadResult == null)
        {
            _logger.LogError("File not found in storage: {FilePath}", fileMetadata.FilePath);
            return NotFound("File not found in storage");
        }

        _logger.LogInformation("User {UserId} downloaded file {FileName}", userId, fileMetadata.FileName);

        return File(downloadResult.FileStream, downloadResult.ContentType, downloadResult.FileName);
    }

    /// <summary>
    /// Get all files in a workspace
    /// </summary>
    /// <param name="workspaceId">Workspace ID</param>
    /// <returns>List of files</returns>
    /// <response code="200">Returns list of files</response>
    /// <response code="404">Workspace not found</response>
    [HttpGet("workspace/{workspaceId}")]
    [ProducesResponseType(typeof(List<WorkspaceFile>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkspaceFiles(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        // Verify workspace access
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == workspaceId && w.UserId == userId, cancellationToken);

        if (workspace == null)
            return NotFound("Workspace not found or access denied");

        var files = await _context.WorkspaceFiles
            .Where(f => f.WorkspaceId == workspaceId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync(cancellationToken);

        return Ok(files);
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>No content</returns>
    /// <response code="204">File deleted successfully</response>
    /// <response code="404">File not found</response>
    /// <remarks>
    /// Permanently delete a file from storage and database.
    ///
    /// Example:
    ///
    ///     DELETE /api/v1/files/guid
    ///
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var file = await _context.WorkspaceFiles
            .Include(f => f.Workspace)
            .FirstOrDefaultAsync(f => f.Id == id && f.Workspace!.UserId == userId, cancellationToken);

        if (file == null)
            return NotFound("File not found or access denied");

        // Delete from storage
        var deleted = await _fileStorage.DeleteFileAsync(file.FilePath, cancellationToken);

        if (!deleted)
        {
            _logger.LogWarning("File not found in storage, removing database record anyway: {FilePath}", file.FilePath);
        }

        // Delete database record
        _context.WorkspaceFiles.Remove(file);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} deleted file {FileName}", userId, file.FileName);

        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
