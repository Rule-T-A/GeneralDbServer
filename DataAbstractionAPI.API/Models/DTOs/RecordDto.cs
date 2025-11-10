using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for a single record in API responses.
/// </summary>
public class RecordDto
{
    /// <summary>
    /// The unique identifier of the record.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The record data as a dictionary of field names to values.
    /// </summary>
    [JsonPropertyName("d")]
    public Dictionary<string, object> Data { get; set; } = new();
}

