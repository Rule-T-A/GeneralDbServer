namespace DataAbstractionAPI.Adapters.Csv;

/// <summary>
/// Configuration options for retry logic when handling concurrent file operations.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds for exponential backoff. Default is 50ms.
    /// </summary>
    public int BaseDelayMs { get; set; } = 50;

    /// <summary>
    /// Whether retry logic is enabled. Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

