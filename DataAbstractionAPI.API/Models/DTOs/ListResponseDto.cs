using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for list/query operation responses.
/// </summary>
public class ListResponseDto
{
    /// <summary>
    /// The list of records (compact key: "d").
    /// </summary>
    [JsonPropertyName("d")]
    public List<RecordDto> Data { get; set; } = new();

    /// <summary>
    /// Total count of matching records (compact key: "t").
    /// </summary>
    [JsonPropertyName("t")]
    public int Total { get; set; }

    /// <summary>
    /// Indicates whether more records are available.
    /// </summary>
    [JsonPropertyName("more")]
    public bool More { get; set; }

    /// <summary>
    /// Optional pagination cursor for next page (for future pagination support).
    /// </summary>
    [JsonPropertyName("cursor")]
    public string? Cursor { get; set; }
}

