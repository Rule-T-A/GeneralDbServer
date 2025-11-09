namespace DataAbstractionAPI.API.Configuration;

/// <summary>
/// Configuration options for API key authentication middleware.
/// </summary>
public class ApiKeyAuthenticationOptions
{
    /// <summary>
    /// Gets or sets whether API key authentication is enabled.
    /// Default: true
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of valid API keys.
    /// </summary>
    public string[] ValidApiKeys { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the HTTP header name to read the API key from.
    /// Default: "X-API-Key"
    /// </summary>
    public string HeaderName { get; set; } = "X-API-Key";
}

