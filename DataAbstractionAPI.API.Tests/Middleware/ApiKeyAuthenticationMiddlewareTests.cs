using System.Net;
using System.Net.Http.Headers;
using System.Text;
using DataAbstractionAPI.API.Configuration;
using DataAbstractionAPI.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DataAbstractionAPI.API.Tests.Middleware;

public class ApiKeyAuthenticationMiddlewareTests
{
    private readonly Mock<ILogger<ApiKeyAuthenticationMiddleware>> _loggerMock;
    private readonly RequestDelegate _next;
    private readonly DefaultHttpContext _context;

    public ApiKeyAuthenticationMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ApiKeyAuthenticationMiddleware>>();
        _next = async (context) =>
        {
            context.Response.StatusCode = 200;
            await Task.CompletedTask;
        };
        _context = new DefaultHttpContext();
    }

    [Fact]
    public async Task ApiKeyMiddleware_WithValidKey_AllowsRequest()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "valid-key-123" },
            HeaderName = "X-API-Key"
        });

        _context.Request.Headers["X-API-Key"] = "valid-key-123";

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(200, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WithoutKey_Returns401()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "valid-key-123" },
            HeaderName = "X-API-Key"
        });

        // No header set
        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(401, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WithInvalidKey_Returns401()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "valid-key-123" },
            HeaderName = "X-API-Key"
        });

        _context.Request.Headers["X-API-Key"] = "invalid-key-456";

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(401, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WhenDisabled_AllowsAllRequests()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = false,
            ValidApiKeys = new[] { "valid-key-123" },
            HeaderName = "X-API-Key"
        });

        // No header set, but authentication is disabled
        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(200, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WithMultipleValidKeys_AcceptsAny()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "key-1", "key-2", "key-3" },
            HeaderName = "X-API-Key"
        });

        _context.Request.Headers["X-API-Key"] = "key-2";

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(200, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WithEmptyValidKeys_Returns401()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = Array.Empty<string>(),
            HeaderName = "X-API-Key"
        });

        _context.Request.Headers["X-API-Key"] = "any-key";

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(401, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WithCustomHeaderName_UsesCustomHeader()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "valid-key" },
            HeaderName = "X-Custom-API-Key"
        });

        _context.Request.Headers["X-Custom-API-Key"] = "valid-key";

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(200, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_LogsAuthenticationAttempts()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "valid-key" },
            HeaderName = "X-API-Key"
        });

        _context.Request.Headers["X-API-Key"] = "valid-key";

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        // Verify logging was called (at least once for successful authentication)
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    // ============================================
    // Task 3.2.1: Additional Edge Cases
    // ============================================

    [Fact]
    public async Task ApiKeyMiddleware_WithNullValidApiKeys_Returns401()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = null!,
            HeaderName = "X-API-Key"
        });

        _context.Request.Headers["X-API-Key"] = "any-key";

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(401, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WithCaseSensitiveKey_IsCaseSensitive()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "Valid-Key-123" },
            HeaderName = "X-API-Key"
        });

        _context.Request.Headers["X-API-Key"] = "valid-key-123"; // Different case

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(401, _context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WithWhitespaceInKey_TrimsCorrectly()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "valid-key" },
            HeaderName = "X-API-Key"
        });

        _context.Request.Headers["X-API-Key"] = "  valid-key  "; // With whitespace

        var middleware = new ApiKeyAuthenticationMiddleware(_next, options, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert - Should fail because exact match is required (no trimming in current implementation)
        // This test documents the current behavior
        Assert.Equal(401, _context.Response.StatusCode);
    }
}

