namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Result for a single item in a bulk operation (best-effort mode).
/// </summary>
public class BulkOperationItemResult
{
    /// <summary>
    /// The index of the record in the original request.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The ID of the created/updated record (for successful operations).
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? Error { get; set; }
}

