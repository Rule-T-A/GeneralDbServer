using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for bulk operation responses.
/// </summary>
public class BulkResponseDto
{
    /// <summary>
    /// Whether the overall operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Number of operations that succeeded.
    /// </summary>
    [JsonPropertyName("succeeded")]
    public int Succeeded { get; set; }

    /// <summary>
    /// Number of operations that failed.
    /// </summary>
    [JsonPropertyName("failed")]
    public int Failed { get; set; }

    /// <summary>
    /// Per-item results (for best-effort mode).
    /// </summary>
    [JsonPropertyName("results")]
    public List<BulkOperationItemResultDto>? Results { get; set; }

    /// <summary>
    /// List of created IDs (for atomic create mode when all succeed).
    /// </summary>
    [JsonPropertyName("ids")]
    public List<string>? Ids { get; set; }

    /// <summary>
    /// Error message (for atomic mode when operation fails).
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Index of the failed record (for atomic mode when operation fails).
    /// </summary>
    [JsonPropertyName("failed_index")]
    public int? FailedIndex { get; set; }

    /// <summary>
    /// Detailed error message for the failed record (for atomic mode).
    /// </summary>
    [JsonPropertyName("failed_error")]
    public string? FailedError { get; set; }
}

/// <summary>
/// Data Transfer Object for a single item result in a bulk operation.
/// </summary>
public class BulkOperationItemResultDto
{
    /// <summary>
    /// The index of the record in the original request.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// The ID of the created/updated record (for successful operations).
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

