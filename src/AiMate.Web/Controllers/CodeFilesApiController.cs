using AiMate.Core.Entities;
using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing code files in the virtual file system
/// </summary>
[ApiController]
[Route("api/v1/code/files")]
[Authorize(Policy = "CanAddCustomEndpoints")] // Requires Developer tier for code editing
public class CodeFilesApiController : ControllerBase
{
    private readonly ICodeFileService _codeFileService;
    private readonly IProjectService _projectService;
    private readonly ILogger<CodeFilesApiController> _logger;

    public CodeFilesApiController(
        ICodeFileService codeFileService,
        IProjectService projectService,
        ILogger<CodeFilesApiController> logger)
    {
        _codeFileService = codeFileService;
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Get all files for a project
    /// </summary>
    /// <param name="projectId">Project ID (GUID)</param>
    /// <returns>List of code files</returns>
    /// <response code="200">Returns the list of files</response>
    /// <response code="400">Invalid project ID</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CodeFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFiles([FromQuery] string projectId)
    {
        try
        {
            if (!Guid.TryParse(projectId, out var projectGuid))
            {
                return BadRequest("Invalid project ID");
            }

            var project = await _projectService.GetProjectByIdAsync(projectGuid);
            if (project == null)
            {
                return NotFound($"Project {projectId} not found");
            }

            var files = await _codeFileService.GetProjectFilesAsync(projectGuid);
            var fileDtos = files.Select(MapToDto).ToList();

            return Ok(fileDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files for project {ProjectId}", projectId);
            return StatusCode(500, new { error = "Failed to get files", details = ex.Message });
        }
    }

    /// <summary>
    /// Get file tree structure for a project
    /// </summary>
    /// <param name="projectId">Project ID (GUID)</param>
    /// <returns>File tree structure</returns>
    /// <response code="200">Returns the file tree</response>
    /// <response code="400">Invalid project ID</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(FileTreeNode), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileTree([FromQuery] string projectId)
    {
        try
        {
            if (!Guid.TryParse(projectId, out var projectGuid))
            {
                return BadRequest("Invalid project ID");
            }

            var project = await _projectService.GetProjectByIdAsync(projectGuid);
            if (project == null)
            {
                return NotFound($"Project {projectId} not found");
            }

            var tree = await _codeFileService.GetFileTreeAsync(projectGuid);

            return Ok(tree);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file tree for project {ProjectId}", projectId);
            return StatusCode(500, new { error = "Failed to get file tree", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific file by ID
    /// </summary>
    /// <param name="id">File ID (GUID)</param>
    /// <returns>File details and content</returns>
    /// <response code="200">Returns the file</response>
    /// <response code="400">Invalid file ID</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CodeFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFile(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var fileGuid))
            {
                return BadRequest("Invalid file ID");
            }

            var file = await _codeFileService.GetFileByIdAsync(fileGuid);
            if (file == null)
            {
                return NotFound($"File {id} not found");
            }

            return Ok(MapToDto(file));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file {FileId}", id);
            return StatusCode(500, new { error = "Failed to get file", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a file by path within a project
    /// </summary>
    /// <param name="projectId">Project ID (GUID)</param>
    /// <param name="path">File path</param>
    /// <returns>File details and content</returns>
    /// <response code="200">Returns the file</response>
    /// <response code="400">Invalid project ID or path</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("by-path")]
    [ProducesResponseType(typeof(CodeFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileByPath([FromQuery] string projectId, [FromQuery] string path)
    {
        try
        {
            if (!Guid.TryParse(projectId, out var projectGuid))
            {
                return BadRequest("Invalid project ID");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is required");
            }

            var file = await _codeFileService.GetFileByPathAsync(projectGuid, path);
            if (file == null)
            {
                return NotFound($"File {path} not found in project {projectId}");
            }

            return Ok(MapToDto(file));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file by path {Path} in project {ProjectId}", path, projectId);
            return StatusCode(500, new { error = "Failed to get file", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new file
    /// </summary>
    /// <param name="request">File creation request</param>
    /// <param name="userId">User ID for auditing</param>
    /// <returns>Created file</returns>
    /// <response code="201">File created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="409">File already exists</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/code/files?userId=abc123
    ///     {
    ///         "projectId": "project-guid",
    ///         "path": "src/Components/MyComponent.razor",
    ///         "content": "@page \"/mycomponent\"\n\n<h3>MyComponent</h3>",
    ///         "language": "razor"
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CodeFileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFile([FromBody] CreateCodeFileRequest request, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(request.ProjectId, out var projectGuid))
            {
                return BadRequest("Invalid project ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var project = await _projectService.GetProjectByIdAsync(projectGuid);
            if (project == null)
            {
                return NotFound($"Project {request.ProjectId} not found");
            }

            var file = new CodeFile
            {
                ProjectId = projectGuid,
                Path = request.Path,
                Content = request.Content ?? "",
                Language = request.Language,
                IsReadOnly = request.IsReadOnly,
                CreatedBy = userGuid,
                LastModifiedBy = userGuid,
                Tags = request.Tags ?? new List<string>()
            };

            var created = await _codeFileService.CreateFileAsync(file);

            _logger.LogInformation("Created file {Path} in project {ProjectId}", created.Path, request.ProjectId);

            return CreatedAtAction(nameof(GetFile), new { id = created.Id.ToString() }, MapToDto(created));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating file");
            return StatusCode(500, new { error = "Failed to create file", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing file
    /// </summary>
    /// <param name="id">File ID (GUID)</param>
    /// <param name="request">File update request</param>
    /// <param name="userId">User ID for auditing</param>
    /// <returns>Updated file</returns>
    /// <response code="200">File updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CodeFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFile(string id, [FromBody] UpdateCodeFileRequest request, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(id, out var fileGuid))
            {
                return BadRequest("Invalid file ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var file = await _codeFileService.GetFileByIdAsync(fileGuid);
            if (file == null)
            {
                return NotFound($"File {id} not found");
            }

            file.Content = request.Content ?? file.Content;
            file.Language = request.Language ?? file.Language;
            file.IsDirty = request.IsDirty ?? file.IsDirty;
            file.Tags = request.Tags ?? file.Tags;
            file.LastModifiedBy = userGuid;

            var updated = await _codeFileService.UpdateFileAsync(file);

            _logger.LogInformation("Updated file {Path} (version {Version})", updated.Path, updated.Version);

            return Ok(MapToDto(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating file {FileId}", id);
            return StatusCode(500, new { error = "Failed to update file", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="id">File ID (GUID)</param>
    /// <returns>No content</returns>
    /// <response code="204">File deleted successfully</response>
    /// <response code="400">Invalid file ID</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFile(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var fileGuid))
            {
                return BadRequest("Invalid file ID");
            }

            await _codeFileService.DeleteFileAsync(fileGuid);

            _logger.LogInformation("Deleted file {FileId}", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", id);
            return StatusCode(500, new { error = "Failed to delete file", details = ex.Message });
        }
    }

    /// <summary>
    /// Search files by content or path
    /// </summary>
    /// <param name="projectId">Project ID (GUID)</param>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching files</returns>
    /// <response code="200">Returns the matching files</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<CodeFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchFiles([FromQuery] string projectId, [FromQuery] string searchTerm)
    {
        try
        {
            if (!Guid.TryParse(projectId, out var projectGuid))
            {
                return BadRequest("Invalid project ID");
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term is required");
            }

            var files = await _codeFileService.SearchFilesAsync(projectGuid, searchTerm);
            var fileDtos = files.Select(MapToDto).ToList();

            return Ok(fileDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files for term {SearchTerm}", searchTerm);
            return StatusCode(500, new { error = "Failed to search files", details = ex.Message });
        }
    }

    private static CodeFileDto MapToDto(CodeFile file)
    {
        return new CodeFileDto
        {
            Id = file.Id.ToString(),
            ProjectId = file.ProjectId.ToString(),
            Path = file.Path,
            Content = file.Content,
            Language = file.Language,
            MimeType = file.MimeType,
            SizeBytes = file.SizeBytes,
            IsDirty = file.IsDirty,
            IsReadOnly = file.IsReadOnly,
            Version = file.Version,
            ContentHash = file.ContentHash,
            CreatedAt = file.CreatedAt,
            LastModified = file.LastModified,
            Tags = file.Tags,
            Metadata = file.Metadata
        };
    }
}

/// <summary>
/// DTO for code file
/// </summary>
public class CodeFileDto
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long SizeBytes { get; set; }
    public bool IsDirty { get; set; }
    public bool IsReadOnly { get; set; }
    public int Version { get; set; }
    public string? ContentHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Request to create a code file
/// </summary>
public class CreateCodeFileRequest
{
    public required string ProjectId { get; set; }
    public required string Path { get; set; }
    public string? Content { get; set; }
    public required string Language { get; set; }
    public bool IsReadOnly { get; set; } = false;
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Request to update a code file
/// </summary>
public class UpdateCodeFileRequest
{
    public string? Content { get; set; }
    public string? Language { get; set; }
    public bool? IsDirty { get; set; }
    public List<string>? Tags { get; set; }
}
