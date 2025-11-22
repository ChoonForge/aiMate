using AiMate.Core.Entities;
using AiMate.Core.Services;
using System.Net.Http;
using System.Net.Http.Json;

namespace AiMate.Web.Services;

/// <summary>
/// File upload service implementation for Blazor WebAssembly/Server
/// Calls backend HTTP API for file operations
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly HttpClient _http;
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

    public FileUploadService(HttpClient http)
    {
        _http = http;
    }

    public async Task<WorkspaceFile> UploadFileAsync(
        Guid workspaceId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync(
            $"/api/v1/files/upload?workspaceId={workspaceId}",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<WorkspaceFileDto>(cancellationToken: cancellationToken);

        if (result == null)
        {
            throw new Exception("Failed to parse upload response");
        }

        return new WorkspaceFile
        {
            Id = result.Id,
            WorkspaceId = result.WorkspaceId,
            FileName = result.FileName,
            FilePath = result.FilePath,
            FileSize = result.FileSize,
            ContentType = result.ContentType,
            Description = result.Description,
            UploadedAt = result.UploadedAt
        };
    }

    public async Task<(Stream Stream, string ContentType)?> GetFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"/api/v1/files/{fileId}/download", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return (stream, contentType);
    }

    public async Task DeleteFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync($"/api/v1/files/{fileId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<WorkspaceFile>> GetWorkspaceFilesAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetFromJsonAsync<List<WorkspaceFileDto>>(
            $"/api/v1/files?workspaceId={workspaceId}",
            cancellationToken);

        if (response == null)
        {
            return new List<WorkspaceFile>();
        }

        return response.Select(dto => new WorkspaceFile
        {
            Id = dto.Id,
            WorkspaceId = dto.WorkspaceId,
            FileName = dto.FileName,
            FilePath = dto.FilePath,
            FileSize = dto.FileSize,
            ContentType = dto.ContentType,
            Description = dto.Description,
            UploadedAt = dto.UploadedAt
        }).ToList();
    }

    public long GetFileSizeLimit() => MaxFileSize;

    // DTO for API responses
    private class WorkspaceFileDto
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
