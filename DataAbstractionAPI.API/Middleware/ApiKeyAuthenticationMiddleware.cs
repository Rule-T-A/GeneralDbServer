using System.Security.Cryptography;
using System.Text;
using DataAbstractionAPI.API.Configuration;
using Microsoft.Extensions.Options;

namespace DataAbstractionAPI.API.Middleware;

/// <summary>
/// Middleware for API key authentication.
/// Validates the X-API-Key header against configured valid API keys.
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiKeyAuthenticationOptions _options;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IOptions<ApiKeyAuthenticationOptions> options,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication if disabled
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        // Check if API key is provided
        if (!context.Request.Headers.TryGetValue(_options.HeaderName, out var apiKeyHeader))
        {
            _logger.LogWarning("API key authentication failed: Missing {HeaderName} header", _options.HeaderName);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: API key is required");
            return;
        }

        var providedApiKey = apiKeyHeader.ToString();

        // Validate API key
        if (!IsValidApiKey(providedApiKey))
        {
            _logger.LogWarning("API key authentication failed: Invalid API key provided");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Invalid API key");
            return;
        }

        _logger.LogDebug("API key authentication successful");
        await _next(context);
    }

    private bool IsValidApiKey(string providedKey)
    {
        if (string.IsNullOrWhiteSpace(providedKey))
        {
            return false;
        }

        if (_options.ValidApiKeys == null || _options.ValidApiKeys.Length == 0)
        {
            return false;
        }

        // Use constant-time comparison to prevent timing attacks
        foreach (var validKey in _options.ValidApiKeys)
        {
            if (ConstantTimeEquals(providedKey, validKey))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Performs a constant-time comparison of two strings to prevent timing attacks.
    /// </summary>
    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a == null || b == null)
        {
            return a == b;
        }

        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}

