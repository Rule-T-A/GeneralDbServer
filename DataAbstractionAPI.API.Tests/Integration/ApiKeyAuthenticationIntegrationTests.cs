using System.Net;
using DataAbstractionAPI.API.Configuration;
using DataAbstractionAPI.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DataAbstractionAPI.API.Tests.Integration;

public class ApiKeyAuthenticationIntegrationTests
{
    // Note: Full integration tests with WebApplicationFactory require Program class to be accessible
    // For now, we test the middleware behavior which is the core functionality
    // Full HTTP integration can be tested manually via Swagger UI or Postman
    
    [Fact]
    public async Task ApiKeyMiddleware_Integration_WithValidKey_AllowsRequest()
    {
        // This test verifies the middleware works correctly in an integration scenario
        // Full HTTP integration tests would require Program class accessibility
        
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "test-api-key-123", "test-api-key-456" },
            HeaderName = "X-API-Key"
        });

        var loggerMock = new Mock<ILogger<ApiKeyAuthenticationMiddleware>>();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-Key"] = "test-api-key-123";
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) =>
        {
            nextCalled = true;
            ctx.Response.StatusCode = 200;
            await Task.CompletedTask;
        };

        var middleware = new ApiKeyAuthenticationMiddleware(next, options, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_Integration_WithMultipleValidKeys_AcceptsAny()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = true,
            ValidApiKeys = new[] { "key-1", "key-2", "key-3" },
            HeaderName = "X-API-Key"
        });

        var loggerMock = new Mock<ILogger<ApiKeyAuthenticationMiddleware>>();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-Key"] = "key-2";
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) =>
        {
            nextCalled = true;
            ctx.Response.StatusCode = 200;
            await Task.CompletedTask;
        };

        var middleware = new ApiKeyAuthenticationMiddleware(next, options, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_Integration_WhenDisabled_AllowsAllRequests()
    {
        // Arrange
        var options = Options.Create(new ApiKeyAuthenticationOptions
        {
            Enabled = false,
            ValidApiKeys = new[] { "valid-key" },
            HeaderName = "X-API-Key"
        });

        var loggerMock = new Mock<ILogger<ApiKeyAuthenticationMiddleware>>();
        var context = new DefaultHttpContext();
        // No header set, but authentication is disabled
        
        var nextCalled = false;
        RequestDelegate next = async (ctx) =>
        {
            nextCalled = true;
            ctx.Response.StatusCode = 200;
            await Task.CompletedTask;
        };

        var middleware = new ApiKeyAuthenticationMiddleware(next, options, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }
}

