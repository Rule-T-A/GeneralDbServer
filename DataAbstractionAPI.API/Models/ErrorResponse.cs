namespace DataAbstractionAPI.API.Models;

/// <summary>
/// Standard error response model for API errors.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional field name for validation/conversion errors.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Optional details for debugging (only in development).
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Optional stack trace (only in development).
    /// </summary>
    public string? StackTrace { get; set; }
}

