using AiMate.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AiMate.Infrastructure.Tests.Services;

/// <summary>
/// Tests for LocalFileStorageService
/// </summary>
public class LocalFileStorageServiceTests : IDisposable
{
    private readonly LocalFileStorageService _fileStorageService;
    private readonly string _testStoragePath;
    private readonly Mock<ILogger<LocalFileStorageService>> _loggerMock;
    private readonly Guid _testUserId;

    public LocalFileStorageServiceTests()
    {
        // Create temporary directory for testing
        _testStoragePath = Path.Combine(Path.GetTempPath(), "AiMate.Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testStoragePath);

        // Setup configuration
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["FileStorage:LocalPath"]).Returns(_testStoragePath);
        configurationMock.Setup(c => c.GetSection("FileStorage:MaxFileSizeMB").Value).Returns("50");

        _loggerMock = new Mock<ILogger<LocalFileStorageService>>();

        _fileStorageService = new LocalFileStorageService(configurationMock.Object, _loggerMock.Object);
        _testUserId = Guid.NewGuid();
    }

    [Fact]
    public async Task UploadFileAsync_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var fileContent = "Test file content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));
        var fileName = "test.txt";

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            stream, fileName, "text/plain", _testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.FilePath.Should().NotBeNullOrEmpty();
        result.FileName.Should().Be(fileName);
        result.FileSize.Should().Be(stream.Length);

        // Verify file actually exists
        var fullPath = Path.Combine(_testStoragePath, result.FilePath!);
        File.Exists(fullPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_CreatesUserSpecificDirectory()
    {
        // Arrange
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        var fileName = "test.txt";

        // Act
        await _fileStorageService.UploadFileAsync(stream, fileName, "text/plain", _testUserId);

        // Assert
        var userDirectory = Path.Combine(_testStoragePath, _testUserId.ToString());
        Directory.Exists(userDirectory).Should().BeTrue("user-specific directory should be created");
    }

    [Fact]
    public async Task UploadFileAsync_GeneratesUniqueFileNames()
    {
        // Arrange
        var stream1 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content1"));
        var stream2 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content2"));
        var fileName = "same-name.txt";

        // Act
        var result1 = await _fileStorageService.UploadFileAsync(stream1, fileName, "text/plain", _testUserId);
        var result2 = await _fileStorageService.UploadFileAsync(stream2, fileName, "text/plain", _testUserId);

        // Assert
        result1.FilePath.Should().NotBe(result2.FilePath, "file paths should be unique even with same name");

        // Both files should exist
        var fullPath1 = Path.Combine(_testStoragePath, result1.FilePath!);
        var fullPath2 = Path.Combine(_testStoragePath, result2.FilePath!);
        File.Exists(fullPath1).Should().BeTrue();
        File.Exists(fullPath2).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_SanitizesInvalidFileNames()
    {
        // Arrange
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        var invalidFileName = "file<>:\"|?*.txt"; // Contains invalid characters

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            stream, invalidFileName, "text/plain", _testUserId);

        // Assert
        result.Success.Should().BeTrue();
        result.FileName.Should().NotContain("<");
        result.FileName.Should().NotContain(">");
        result.FileName.Should().NotContain(":");
        result.FileName.Should().NotContain("|");
        result.FileName.Should().NotContain("?");
        result.FileName.Should().NotContain("*");
    }

    [Fact]
    public async Task UploadFileAsync_WithFileSizeTooLarge_ReturnsFail()
    {
        // Arrange - create a stream larger than 50MB
        var largeSize = 51 * 1024 * 1024; // 51MB
        var stream = new MemoryStream(new byte[largeSize]);

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            stream, "large-file.txt", "text/plain", _testUserId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeds maximum");
    }

    [Fact]
    public async Task DownloadFileAsync_WithExistingFile_ReturnsFileStream()
    {
        // Arrange - first upload a file
        var fileContent = "Test download content";
        var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));
        var fileName = "download-test.txt";
        var uploadResult = await _fileStorageService.UploadFileAsync(
            uploadStream, fileName, "text/plain", _testUserId);

        // Act
        var downloadResult = await _fileStorageService.DownloadFileAsync(uploadResult.FilePath!);

        // Assert
        downloadResult.Should().NotBeNull();
        downloadResult!.FileStream.Should().NotBeNull();
        downloadResult.FileName.Should().NotBeNullOrEmpty();
        downloadResult.ContentType.Should().NotBeNullOrEmpty();
        downloadResult.FileSize.Should().BeGreaterThan(0);

        // Verify content
        using var reader = new StreamReader(downloadResult.FileStream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(fileContent);
    }

    [Fact]
    public async Task DownloadFileAsync_WithNonExistentFile_ReturnsNull()
    {
        // Act
        var result = await _fileStorageService.DownloadFileAsync("non-existent/file.txt");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_ReturnsTrue()
    {
        // Arrange - upload a file first
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content to delete"));
        var uploadResult = await _fileStorageService.UploadFileAsync(
            stream, "delete-test.txt", "text/plain", _testUserId);

        // Act
        var deleteResult = await _fileStorageService.DeleteFileAsync(uploadResult.FilePath!);

        // Assert
        deleteResult.Should().BeTrue();

        // Verify file is actually deleted
        var fullPath = Path.Combine(_testStoragePath, uploadResult.FilePath!);
        File.Exists(fullPath).Should().BeFalse("file should be deleted");
    }

    [Fact]
    public async Task DeleteFileAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Act
        var result = await _fileStorageService.DeleteFileAsync("non-existent/file.txt");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task FileExistsAsync_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        var uploadResult = await _fileStorageService.UploadFileAsync(
            stream, "exists-test.txt", "text/plain", _testUserId);

        // Act
        var exists = await _fileStorageService.FileExistsAsync(uploadResult.FilePath!);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Act
        var exists = await _fileStorageService.FileExistsAsync("non-existent/file.txt");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetFileSizeAsync_WithExistingFile_ReturnsCorrectSize()
    {
        // Arrange
        var fileContent = "Test file size content";
        var expectedSize = System.Text.Encoding.UTF8.GetBytes(fileContent).Length;
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));
        var uploadResult = await _fileStorageService.UploadFileAsync(
            stream, "size-test.txt", "text/plain", _testUserId);

        // Act
        var size = await _fileStorageService.GetFileSizeAsync(uploadResult.FilePath!);

        // Assert
        size.Should().Be(expectedSize);
    }

    [Fact]
    public async Task GetFileSizeAsync_WithNonExistentFile_ReturnsZero()
    {
        // Act
        var size = await _fileStorageService.GetFileSizeAsync("non-existent/file.txt");

        // Assert
        size.Should().Be(0);
    }

    [Theory]
    [InlineData(".txt", "text/plain")]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".png", "image/png")]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".json", "application/json")]
    [InlineData(".csv", "text/csv")]
    public async Task DownloadFileAsync_ReturnsCorrectContentType(string extension, string expectedContentType)
    {
        // Arrange
        var fileName = $"test{extension}";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        var uploadResult = await _fileStorageService.UploadFileAsync(
            stream, fileName, "application/octet-stream", _testUserId);

        // Act
        var downloadResult = await _fileStorageService.DownloadFileAsync(uploadResult.FilePath!);

        // Assert
        downloadResult.Should().NotBeNull();
        downloadResult!.ContentType.Should().Be(expectedContentType);
    }

    [Fact]
    public async Task UploadFileAsync_WithLongFileName_TruncatesTo200Characters()
    {
        // Arrange
        var longFileName = new string('a', 250) + ".txt";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            stream, longFileName, "text/plain", _testUserId);

        // Assert
        result.Success.Should().BeTrue();
        result.FileName!.Length.Should().BeLessOrEqualTo(200, "file name should be truncated");
    }

    [Fact]
    public async Task UploadFileAsync_PreservesFileExtension()
    {
        // Arrange
        var fileName = "test-file.special.extension.txt";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));

        // Act
        var result = await _fileStorageService.UploadFileAsync(
            stream, fileName, "text/plain", _testUserId);

        // Assert
        result.Success.Should().BeTrue();
        result.FilePath.Should().EndWith(".txt", "file extension should be preserved");
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testStoragePath))
        {
            try
            {
                Directory.Delete(_testStoragePath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
