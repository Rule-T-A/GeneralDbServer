namespace DataAbstractionAPI.API.Configuration;

/// <summary>
/// Configuration options for Cross-Origin Resource Sharing (CORS).
/// </summary>
public class CorsOptions
{
    /// <summary>
    /// Gets or sets the list of allowed origins.
    /// If empty, defaults will be used based on environment.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the list of allowed HTTP methods.
    /// If empty, defaults to: GET, POST, PUT, PATCH, DELETE, OPTIONS
    /// </summary>
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the list of allowed HTTP headers.
    /// If empty, defaults to: Content-Type, X-API-Key, Authorization
    /// </summary>
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether credentials are allowed.
    /// Default: false
    /// </summary>
    public bool AllowCredentials { get; set; } = false;

    /// <summary>
    /// Gets or sets the preflight max age in seconds.
    /// Default: 86400 (24 hours)
    /// </summary>
    public int? PreflightMaxAge { get; set; } = 86400;
}

