using System.Net;
using System.Text.Json;
using DataAbstractionAPI.API.Middleware;
using DataAbstractionAPI.API.Models;
using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Core.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DataAbstractionAPI.API.Tests.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly DefaultHttpContext _context;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task GlobalExceptionHandler_CatchesFileNotFoundException_Returns404()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new FileNotFoundException("Collection not found"));
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, _context.Response.StatusCode);
        Assert.Equal("application/json", _context.Response.ContentType);

        var responseBody = await GetResponseBody(_context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal((int)HttpStatusCode.NotFound, errorResponse.StatusCode);
        Assert.Equal("Collection not found", errorResponse.Message);
    }

    [Fact]
    public async Task GlobalExceptionHandler_CatchesKeyNotFoundException_Returns404()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new KeyNotFoundException("Record not found"));
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, _context.Response.StatusCode);
        
        var responseBody = await GetResponseBody(_context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal((int)HttpStatusCode.NotFound, errorResponse.StatusCode);
        Assert.Equal("Record not found", errorResponse.Message);
    }

    [Fact]
    public async Task GlobalExceptionHandler_CatchesValidationException_Returns400()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new ValidationException("email", "Email is required"));
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _context.Response.StatusCode);
        
        var responseBody = await GetResponseBody(_context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal((int)HttpStatusCode.BadRequest, errorResponse.StatusCode);
        Assert.Equal("Email is required", errorResponse.Message);
        Assert.Equal("email", errorResponse.FieldName);
    }

    [Fact]
    public async Task GlobalExceptionHandler_CatchesConversionException_Returns400()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new ConversionException("age", "abc", FieldType.String, FieldType.Integer));
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _context.Response.StatusCode);
        
        var responseBody = await GetResponseBody(_context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal((int)HttpStatusCode.BadRequest, errorResponse.StatusCode);
        Assert.Equal("age", errorResponse.FieldName);
    }

    [Fact]
    public async Task GlobalExceptionHandler_CatchesArgumentException_Returns400()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new ArgumentException("Invalid collection name"));
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _context.Response.StatusCode);
        
        var responseBody = await GetResponseBody(_context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal((int)HttpStatusCode.BadRequest, errorResponse.StatusCode);
        Assert.Equal("Invalid collection name", errorResponse.Message);
    }

    [Fact]
    public async Task GlobalExceptionHandler_CatchesIOException_Returns500()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new IOException("File is locked"));
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _context.Response.StatusCode);
        
        var responseBody = await GetResponseBody(_context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal((int)HttpStatusCode.InternalServerError, errorResponse.StatusCode);
        Assert.Contains("I/O error", errorResponse.Message);
    }

    [Fact]
    public async Task GlobalExceptionHandler_CatchesGenericException_Returns500()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _context.Response.StatusCode);
        
        var responseBody = await GetResponseBody(_context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal((int)HttpStatusCode.InternalServerError, errorResponse.StatusCode);
        Assert.Equal("An error occurred while processing your request.", errorResponse.Message);
    }

    [Fact]
    public async Task GlobalExceptionHandler_InDevelopmentMode_IncludesDetails()
    {
        // Arrange
        var exception = new Exception("Test error");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        var responseBody = await GetResponseBody(_context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Test error", errorResponse.Message);
        Assert.NotNull(errorResponse.Details);
        Assert.NotNull(errorResponse.StackTrace);
    }

    [Fact]
    public async Task GlobalExceptionHandler_WhenNoException_ContinuesNormally()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var middleware = new GlobalExceptionHandlerMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(200, _context.Response.StatusCode);
        _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Once);
    }

    // ============================================
    // Task 3.2.2: Additional Edge Cases
    // ============================================

    [Fact]
    public async Task GlobalExceptionHandler_WithDifferentStatusCodes_SetsCorrectly()
    {
        // Arrange - Test that all different exception types set correct status codes
        var testCases = new[]
        {
            (Exception: (Exception)new FileNotFoundException("Not found"), ExpectedStatusCode: HttpStatusCode.NotFound),
            (Exception: (Exception)new KeyNotFoundException("Key not found"), ExpectedStatusCode: HttpStatusCode.NotFound),
            (Exception: (Exception)new ValidationException("field", "Invalid"), ExpectedStatusCode: HttpStatusCode.BadRequest),
            (Exception: (Exception)new ConversionException("field", "value", Core.Enums.FieldType.String, Core.Enums.FieldType.Integer), ExpectedStatusCode: HttpStatusCode.BadRequest),
            (Exception: (Exception)new ArgumentException("Invalid argument"), ExpectedStatusCode: HttpStatusCode.BadRequest),
            (Exception: (Exception)new IOException("IO error"), ExpectedStatusCode: HttpStatusCode.InternalServerError),
            (Exception: (Exception)new Exception("Generic error"), ExpectedStatusCode: HttpStatusCode.InternalServerError)
        };

        foreach (var testCase in testCases)
        {
            // Reset context for each test
            _context.Response.Body = new MemoryStream();
            _context.Response.StatusCode = 200;

            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(testCase.Exception);
            _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

            var middleware = new GlobalExceptionHandlerMiddleware(
                _nextMock.Object,
                _loggerMock.Object,
                _environmentMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            Assert.Equal((int)testCase.ExpectedStatusCode, _context.Response.StatusCode);
            
            var responseBody = await GetResponseBody(_context);
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(errorResponse);
            Assert.Equal((int)testCase.ExpectedStatusCode, errorResponse.StatusCode);
        }
    }

    private async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}

