namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Result of a bulk operation.
/// </summary>
public class BulkResult
{
    /// <summary>
    /// Whether the overall operation succeeded.
    /// For atomic mode, true only if all operations succeeded.
    /// For best-effort mode, true if at least one operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of operations that succeeded.
    /// </summary>
    public int Succeeded { get; set; }

    /// <summary>
    /// Number of operations that failed.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Per-item results (for best-effort mode).
    /// </summary>
    public List<BulkOperationItemResult> Results { get; set; } = new();

    /// <summary>
    /// List of created IDs (for atomic create mode when all succeed).
    /// </summary>
    public List<string>? Ids { get; set; }

    /// <summary>
    /// Error message (for atomic mode when operation fails).
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Index of the failed record (for atomic mode when operation fails).
    /// </summary>
    public int? FailedIndex { get; set; }

    /// <summary>
    /// Detailed error message for the failed record (for atomic mode).
    /// </summary>
    public string? FailedError { get; set; }
}

