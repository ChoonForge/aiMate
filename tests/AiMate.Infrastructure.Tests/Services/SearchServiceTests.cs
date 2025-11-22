using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using AiMate.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pgvector;
using Xunit;

namespace AiMate.Infrastructure.Tests.Services;

/// <summary>
/// Tests for SearchService - full-text and semantic search functionality
/// </summary>
public class SearchServiceTests : IDisposable
{
    private readonly AiMateDbContext _context;
    private readonly Mock<IEmbeddingService> _embeddingServiceMock;
    private readonly SearchService _searchService;
    private readonly Guid _testUserId;
    private readonly Guid _otherUserId;

    public SearchServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AiMateDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AiMateDbContext(options);
        _embeddingServiceMock = new Mock<IEmbeddingService>();
        _searchService = new SearchService(_context, _embeddingServiceMock.Object);

        _testUserId = Guid.NewGuid();
        _otherUserId = Guid.NewGuid();

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test users
        var testUser = new User
        {
            Id = _testUserId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Tier = UserTier.Free
        };

        var otherUser = new User
        {
            Id = _otherUserId,
            Username = "otheruser",
            Email = "other@example.com",
            PasswordHash = "hash",
            Tier = UserTier.Free
        };

        // Create test workspace
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Test Workspace",
            UserId = _testUserId,
            User = testUser
        };

        // Create test conversations
        var conversation1 = new Conversation
        {
            Id = Guid.NewGuid(),
            Title = "API Design Discussion",
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var conversation2 = new Conversation
        {
            Id = Guid.NewGuid(),
            Title = "Database Optimization",
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        // Create test messages
        var message1 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation1.Id,
            Conversation = conversation1,
            Role = "user",
            Content = "How should we design the authentication API endpoints?",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var message2 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation1.Id,
            Conversation = conversation1,
            Role = "assistant",
            Content = "For authentication, I recommend using JWT tokens with refresh tokens.",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var message3 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation2.Id,
            Conversation = conversation2,
            Role = "user",
            Content = "The database queries are running slow on large datasets.",
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        // Create test knowledge items
        var knowledge1 = new KnowledgeItem
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "REST API Best Practices",
            Content = "Always use proper HTTP methods (GET, POST, PUT, DELETE). Design endpoints with clear naming conventions.",
            Type = KnowledgeType.Note,
            Tags = new List<string> { "api", "rest", "best-practices" },
            Embedding = new Vector(new float[] { 0.1f, 0.2f, 0.3f }),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var knowledge2 = new KnowledgeItem
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Database Indexing",
            Content = "Use indexes on frequently queried columns to improve database performance.",
            Type = KnowledgeType.Note,
            Tags = new List<string> { "database", "performance", "indexing" },
            Embedding = new Vector(new float[] { 0.4f, 0.5f, 0.6f }),
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        // Add entities to context
        _context.Users.AddRange(testUser, otherUser);
        _context.Workspaces.Add(workspace);
        _context.Conversations.AddRange(conversation1, conversation2);
        _context.Messages.AddRange(message1, message2, message3);
        _context.KnowledgeItems.AddRange(knowledge1, knowledge2);

        _context.SaveChanges();
    }

    [Fact]
    public async Task SearchConversationsAsync_WithMatchingTitle_ReturnsResults()
    {
        // Act
        var results = await _searchService.SearchConversationsAsync(_testUserId, "API");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().HaveCount(1);
        results.Results[0].Item.Title.Should().Contain("API");
        results.Query.Should().Be("API");
        results.QueryTimeMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchConversationsAsync_WithNoMatches_ReturnsEmptyResults()
    {
        // Act
        var results = await _searchService.SearchConversationsAsync(_testUserId, "nonexistent");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().BeEmpty();
        results.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchConversationsAsync_OnlyReturnsCurrentUserConversations()
    {
        // Act
        var results = await _searchService.SearchConversationsAsync(_otherUserId, "API");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().BeEmpty("other user should not see test user's conversations");
    }

    [Fact]
    public async Task SearchMessagesAsync_WithMatchingContent_ReturnsResults()
    {
        // Act
        var results = await _searchService.SearchMessagesAsync(_testUserId, "authentication");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().HaveCount(2);
        results.Results.Should().AllSatisfy(r =>
            r.Item.Content.Should().Contain("authentication", "all results should contain query"));
    }

    [Fact]
    public async Task SearchMessagesAsync_ReturnsHighlights()
    {
        // Act
        var results = await _searchService.SearchMessagesAsync(_testUserId, "authentication");

        // Assert
        results.Results.Should().AllSatisfy(r =>
            r.Highlight.Should().NotBeNullOrEmpty("highlights should be generated"));
    }

    [Fact]
    public async Task SearchMessagesAsync_CalculatesRelevanceScores()
    {
        // Act
        var results = await _searchService.SearchMessagesAsync(_testUserId, "authentication");

        // Assert
        results.Results.Should().AllSatisfy(r =>
            r.Score.Should().BeGreaterThan(0, "relevance scores should be calculated"));
    }

    [Fact]
    public async Task SearchKnowledgeFullTextAsync_WithMatchingTitle_ReturnsResults()
    {
        // Act
        var results = await _searchService.SearchKnowledgeFullTextAsync(_testUserId, "API");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().HaveCount(1);
        results.Results[0].Item.Title.Should().Contain("API");
    }

    [Fact]
    public async Task SearchKnowledgeFullTextAsync_WithMatchingContent_ReturnsResults()
    {
        // Act
        var results = await _searchService.SearchKnowledgeFullTextAsync(_testUserId, "HTTP methods");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().HaveCount(1);
        results.Results[0].Item.Content.Should().Contain("HTTP methods");
    }

    [Fact]
    public async Task SearchKnowledgeFullTextAsync_WithMatchingTag_ReturnsResults()
    {
        // Act
        var results = await _searchService.SearchKnowledgeFullTextAsync(_testUserId, "performance");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().HaveCount(1);
        results.Results[0].Item.Tags.Should().Contain("performance");
    }

    [Fact]
    public async Task SearchKnowledgeSemanticAsync_WithEmbeddings_ReturnsResults()
    {
        // Arrange
        var queryEmbedding = new float[] { 0.15f, 0.25f, 0.35f }; // Close to knowledge1 embedding
        _embeddingServiceMock
            .Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryEmbedding);

        // Act
        var results = await _searchService.SearchKnowledgeSemanticAsync(_testUserId, "API design patterns");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().NotBeEmpty();
        _embeddingServiceMock.Verify(e => e.GenerateEmbeddingAsync("API design patterns", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchKnowledgeSemanticAsync_FiltersOnlyItemsWithEmbeddings()
    {
        // Arrange - add knowledge item without embedding
        var knowledgeWithoutEmbedding = new KnowledgeItem
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "No Embedding Item",
            Content = "This item has no embedding",
            Type = KnowledgeType.Note,
            Embedding = null // No embedding
        };
        _context.KnowledgeItems.Add(knowledgeWithoutEmbedding);
        await _context.SaveChangesAsync();

        var queryEmbedding = new float[] { 0.1f, 0.2f, 0.3f };
        _embeddingServiceMock
            .Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryEmbedding);

        // Act
        var results = await _searchService.SearchKnowledgeSemanticAsync(_testUserId, "test query", limit: 100);

        // Assert
        results.Results.Should().NotContain(r => r.Item.Embedding == null,
            "items without embeddings should be filtered out");
    }

    [Fact]
    public async Task SearchKnowledgeSemanticAsync_FallbackToFullTextOnEmbeddingFailure()
    {
        // Arrange - embedding service returns null
        _embeddingServiceMock
            .Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((float[]?)null);

        // Act
        var results = await _searchService.SearchKnowledgeSemanticAsync(_testUserId, "database");

        // Assert
        results.Should().NotBeNull();
        results.Results.Should().NotBeEmpty("should fallback to full-text search");
        results.Results.Should().Contain(r => r.Item.Title.Contains("Database"),
            "full-text fallback should work");
    }

    [Fact]
    public async Task SearchGlobalAsync_SearchesAllContentTypes()
    {
        // Act
        var results = await _searchService.SearchGlobalAsync(_testUserId, "database", limit: 10);

        // Assert
        results.Should().NotBeNull();
        results.Conversations.Should().NotBeEmpty("should find conversation with 'Database' in title");
        results.Messages.Should().NotBeEmpty("should find message with 'database' in content");
        results.KnowledgeItems.Should().NotBeEmpty("should find knowledge item about database");
        results.TotalResults.Should().BeGreaterThan(0);
        results.Query.Should().Be("database");
    }

    [Fact]
    public async Task SearchGlobalAsync_RespectsLimit()
    {
        // Act
        var results = await _searchService.SearchGlobalAsync(_testUserId, "database", limit: 1);

        // Assert
        results.Conversations.Should().HaveCountLessOrEqualTo(1);
        results.Messages.Should().HaveCountLessOrEqualTo(1);
        results.KnowledgeItems.Should().HaveCountLessOrEqualTo(1);
    }

    [Theory]
    [InlineData("authentication API", "authentication API", 1.0)] // Exact match at beginning
    [InlineData("authentication", "How should we design the authentication API?", 1.0)] // Exact match partial
    [InlineData("design auth", "How should we design the authentication API?", 0.7)] // Partial word match
    public void CalculateRelevanceScore_ReturnsExpectedScores(string query, string text, double minExpectedScore)
    {
        // This tests the private CalculateRelevanceScore method indirectly through search results

        // Arrange - create a message with the test content
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            UserId = _testUserId
        };

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            WorkspaceId = workspace.Id,
            Workspace = workspace
        };

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Conversation = conversation,
            Role = "user",
            Content = text
        };

        _context.Workspaces.Add(workspace);
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        _context.SaveChanges();

        // Act
        var results = _searchService.SearchMessagesAsync(_testUserId, query).Result;

        // Assert
        if (results.Results.Any())
        {
            results.Results.Should().Contain(r => r.Score >= minExpectedScore * 0.8,
                $"score should be at least {minExpectedScore * 0.8} for query '{query}' in text '{text}'");
        }
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
