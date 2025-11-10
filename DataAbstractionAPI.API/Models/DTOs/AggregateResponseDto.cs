using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for aggregate operation responses.
/// </summary>
public class AggregateResponseDto
{
    /// <summary>
    /// List of grouped and aggregated results (compact key: "d").
    /// Each dictionary contains group-by field values and aggregate function results.
    /// </summary>
    [JsonPropertyName("d")]
    public List<Dictionary<string, object>> Data { get; set; } = new();
}

