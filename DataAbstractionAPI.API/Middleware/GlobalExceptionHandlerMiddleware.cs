using System.Net;
using System.Text.Json;
using DataAbstractionAPI.API.Models;
using DataAbstractionAPI.Core.Exceptions;

namespace DataAbstractionAPI.API.Middleware;

/// <summary>
/// Global exception handler middleware that catches all unhandled exceptions
/// and returns consistent error responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Message = exception.Message
        };

        switch (exception)
        {
            case FileNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.FieldName = validationEx.FieldName;
                break;

            case ConversionException conversionEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.FieldName = conversionEx.FieldName;
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case IOException:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An I/O error occurred. The file may be locked by another process.";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An error occurred while processing your request.";
                break;
        }

        // Add details and stack trace in development mode
        if (_environment.IsDevelopment())
        {
            errorResponse.Details = exception.ToString();
            errorResponse.StackTrace = exception.StackTrace;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await response.WriteAsync(json);
    }
}

