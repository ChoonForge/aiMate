using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AiMate.Web.Tests.Controllers;

/// <summary>
/// Tests for SearchApiController endpoints
/// </summary>
public class SearchApiControllerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly Mock<ILogger<SearchApiController>> _loggerMock;
    private readonly SearchApiController _controller;
    private readonly Guid _testUserId;

    public SearchApiControllerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _loggerMock = new Mock<ILogger<SearchApiController>>();
        _controller = new SearchApiController(_searchServiceMock.Object, _loggerMock.Object);

        _testUserId = Guid.NewGuid();

        // Setup controller context with authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        }, "TestAuthentication"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    #region SearchConversations Tests

    [Fact]
    public async Task SearchConversations_WithValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var query = "test query";
        var expectedResults = new SearchResults<Conversation>
        {
            Results = new List<SearchResult<Conversation>>
            {
                new() { Item = new Conversation { Id = Guid.NewGuid(), Title = "Test Conversation" }, Score = 0.95 }
            },
            TotalCount = 1,
            Query = query,
            QueryTimeMs = 50
        };

        _searchServiceMock
            .Setup(s => s.SearchConversationsAsync(_testUserId, query, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _controller.SearchConversations(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedResults);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchConversations_WithEmptyQuery_ReturnsBadRequest(string? query)
    {
        // Act
        var result = await _controller.SearchConversations(query!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().Be("Query cannot be empty");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]
    public async Task SearchConversations_WithInvalidLimit_ReturnsBadRequest(int limit)
    {
        // Act
        var result = await _controller.SearchConversations("query", limit);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().Be("Limit must be between 1 and 50");
    }

    #endregion

    #region SearchMessages Tests

    [Fact]
    public async Task SearchMessages_WithValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var query = "authentication";
        var expectedResults = new SearchResults<Message>
        {
            Results = new List<SearchResult<Message>>
            {
                new() { Item = new Message { Id = Guid.NewGuid(), Content = "Test authentication" }, Score = 0.85 }
            },
            TotalCount = 1,
            Query = query,
            QueryTimeMs = 75
        };

        _searchServiceMock
            .Setup(s => s.SearchMessagesAsync(_testUserId, query, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _controller.SearchMessages(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedResults);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task SearchMessages_WithEmptyQuery_ReturnsBadRequest(string? query)
    {
        // Act
        var result = await _controller.SearchMessages(query!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region SearchKnowledgeSemantic Tests

    [Fact]
    public async Task SearchKnowledgeSemantic_WithValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var query = "API design patterns";
        var expectedResults = new SearchResults<KnowledgeItem>
        {
            Results = new List<SearchResult<KnowledgeItem>>
            {
                new() { Item = new KnowledgeItem { Id = Guid.NewGuid(), Title = "REST API" }, Score = 0.9 }
            },
            TotalCount = 1,
            Query = query,
            QueryTimeMs = 120
        };

        _searchServiceMock
            .Setup(s => s.SearchKnowledgeSemanticAsync(_testUserId, query, 0.7, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _controller.SearchKnowledgeSemantic(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedResults);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public async Task SearchKnowledgeSemantic_WithInvalidThreshold_ReturnsBadRequest(double threshold)
    {
        // Act
        var result = await _controller.SearchKnowledgeSemantic("query", threshold);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().Be("Threshold must be between 0.0 and 1.0");
    }

    [Fact]
    public async Task SearchKnowledgeSemantic_WithCustomThreshold_PassesToService()
    {
        // Arrange
        var query = "test";
        var threshold = 0.85;
        var expectedResults = new SearchResults<KnowledgeItem>
        {
            Results = new List<SearchResult<KnowledgeItem>>(),
            TotalCount = 0,
            Query = query,
            QueryTimeMs = 50
        };

        _searchServiceMock
            .Setup(s => s.SearchKnowledgeSemanticAsync(_testUserId, query, threshold, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        await _controller.SearchKnowledgeSemantic(query, threshold);

        // Assert
        _searchServiceMock.Verify(
            s => s.SearchKnowledgeSemanticAsync(_testUserId, query, threshold, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region SearchKnowledge Tests

    [Fact]
    public async Task SearchKnowledge_WithValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var query = "database";
        var expectedResults = new SearchResults<KnowledgeItem>
        {
            Results = new List<SearchResult<KnowledgeItem>>
            {
                new() { Item = new KnowledgeItem { Id = Guid.NewGuid(), Title = "Database Indexing" }, Score = 0.95 }
            },
            TotalCount = 1,
            Query = query,
            QueryTimeMs = 45
        };

        _searchServiceMock
            .Setup(s => s.SearchKnowledgeFullTextAsync(_testUserId, query, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _controller.SearchKnowledge(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedResults);
    }

    #endregion

    #region SearchGlobal Tests

    [Fact]
    public async Task SearchGlobal_WithValidQuery_ReturnsOkWithCombinedResults()
    {
        // Arrange
        var query = "authentication";
        var expectedResults = new GlobalSearchResults
        {
            Conversations = new List<SearchResult<Conversation>>
            {
                new() { Item = new Conversation { Id = Guid.NewGuid(), Title = "Auth Flow" }, Score = 0.9 }
            },
            Messages = new List<SearchResult<Message>>
            {
                new() { Item = new Message { Id = Guid.NewGuid(), Content = "Authentication docs" }, Score = 0.85 }
            },
            KnowledgeItems = new List<SearchResult<KnowledgeItem>>
            {
                new() { Item = new KnowledgeItem { Id = Guid.NewGuid(), Title = "Auth Guide" }, Score = 0.95 }
            },
            Query = query,
            QueryTimeMs = 150
        };

        _searchServiceMock
            .Setup(s => s.SearchGlobalAsync(_testUserId, query, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _controller.SearchGlobal(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var resultValue = okResult.Value as GlobalSearchResults;
        resultValue.Should().NotBeNull();
        resultValue!.TotalResults.Should().Be(3);
        resultValue.Conversations.Should().HaveCount(1);
        resultValue.Messages.Should().HaveCount(1);
        resultValue.KnowledgeItems.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task SearchGlobal_WithInvalidLimit_ReturnsBadRequest(int limit)
    {
        // Act
        var result = await _controller.SearchGlobal("query", limit);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().Be("Limit must be between 1 and 20");
    }

    [Fact]
    public async Task SearchGlobal_ExtractsUserIdFromClaims()
    {
        // Arrange
        var query = "test";
        _searchServiceMock
            .Setup(s => s.SearchGlobalAsync(It.IsAny<Guid>(), query, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlobalSearchResults { Query = query, QueryTimeMs = 50 });

        // Act
        await _controller.SearchGlobal(query);

        // Assert
        _searchServiceMock.Verify(
            s => s.SearchGlobalAsync(_testUserId, query, 5, It.IsAny<CancellationToken>()),
            Times.Once,
            "should extract correct user ID from claims");
    }

    #endregion
}
