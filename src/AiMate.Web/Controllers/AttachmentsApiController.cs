using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// Attachments API for handling file uploads, webpage fetching, etc.
/// </summary>
[ApiController]
[Route("api/v1/attachments")]
[Authorize]
public class AttachmentsApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AttachmentsApiController> _logger;

    public AttachmentsApiController(
        IHttpClientFactory httpClientFactory,
        ILogger<AttachmentsApiController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Fetch and parse webpage content
    /// </summary>
    [HttpPost("fetch-webpage")]
    public async Task<IActionResult> FetchWebpage([FromBody] FetchWebpageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "URL is required" });
        }

        // Validate URL
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest(new { error = "Invalid URL format" });
        }

        try
        {
            _logger.LogInformation("Fetching webpage: {Url}", request.Url);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; AiMate/1.0)");
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await httpClient.GetAsync(request.Url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            // Parse HTML
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extract title
            var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ?? uri.Host;

            // Extract main content (try multiple strategies)
            var content = ExtractMainContent(doc);

            // Extract metadata
            var description = doc.DocumentNode
                .SelectSingleNode("//meta[@name='description']")
                ?.GetAttributeValue("content", "") ?? "";

            var ogImage = doc.DocumentNode
                .SelectSingleNode("//meta[@property='og:image']")
                ?.GetAttributeValue("content", "") ?? "";

            return Ok(new FetchWebpageResponse
            {
                Url = request.Url,
                Title = CleanText(title),
                Content = CleanText(content),
                Description = CleanText(description),
                Image = ogImage,
                FetchedAt = DateTime.UtcNow
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch webpage: {Url}", request.Url);
            return BadRequest(new { error = $"Failed to fetch webpage: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webpage: {Url}", request.Url);
            return StatusCode(500, new { error = "Internal server error processing webpage" });
        }
    }

    /// <summary>
    /// Upload files (base64 encoded)
    /// </summary>
    [HttpPost("upload-file")]
    public async Task<IActionResult> UploadFile([FromBody] UploadFileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Data))
        {
            return BadRequest(new { error = "File name and data are required" });
        }

        try
        {
            // Validate file size (max 10MB for base64)
            var estimatedSize = (request.Data.Length * 3) / 4; // Base64 to bytes approximation
            if (estimatedSize > 10 * 1024 * 1024)
            {
                return BadRequest(new { error = "File size exceeds 10MB limit" });
            }

            // TODO: Save file to storage (local, S3, Azure Blob, etc.)
            // For now, just return success with file info

            _logger.LogInformation("File uploaded: {FileName}, Size: {Size}", request.Name, estimatedSize);

            return Ok(new UploadFileResponse
            {
                Name = request.Name,
                Size = estimatedSize,
                Type = request.Type,
                Url = $"/api/v1/files/{Guid.NewGuid()}", // Placeholder
                UploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", request.Name);
            return StatusCode(500, new { error = "Internal server error uploading file" });
        }
    }

    private static string ExtractMainContent(HtmlDocument doc)
    {
        // Try to find main content using common selectors
        var contentNode = doc.DocumentNode.SelectSingleNode("//main") ??
                         doc.DocumentNode.SelectSingleNode("//article") ??
                         doc.DocumentNode.SelectSingleNode("//div[@id='content']") ??
                         doc.DocumentNode.SelectSingleNode("//div[@class='content']") ??
                         doc.DocumentNode.SelectSingleNode("//body");

        if (contentNode == null)
            return "";

        // Remove script and style elements
        foreach (var node in contentNode.SelectNodes(".//script|.//style") ?? Enumerable.Empty<HtmlNode>())
        {
            node.Remove();
        }

        // Get text content
        var text = contentNode.InnerText;

        return text;
    }

    private static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        // Decode HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);

        // Remove extra whitespace
        text = Regex.Replace(text, @"\s+", " ");

        // Trim
        return text.Trim();
    }
}

public class FetchWebpageRequest
{
    public string Url { get; set; } = string.Empty;
}

public class FetchWebpageResponse
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; }
}

public class UploadFileRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty; // Base64 encoded
}

public class UploadFileResponse
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
