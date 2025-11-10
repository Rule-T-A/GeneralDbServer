using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for complex query requests (POST /data/{collection}/query).
/// </summary>
public class QueryRequestDto
{
    /// <summary>
    /// Optional list of fields to return.
    /// </summary>
    [JsonPropertyName("fields")]
    public string[]? Fields { get; set; }

    /// <summary>
    /// Optional filter criteria.
    /// </summary>
    [JsonPropertyName("filter")]
    public Dictionary<string, object>? Filter { get; set; }

    /// <summary>
    /// Maximum number of records to return.
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Number of records to skip.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; set; } = 0;

    /// <summary>
    /// Sort order (e.g., "name:asc" or "created:desc").
    /// </summary>
    [JsonPropertyName("sort")]
    public string? Sort { get; set; }
}

