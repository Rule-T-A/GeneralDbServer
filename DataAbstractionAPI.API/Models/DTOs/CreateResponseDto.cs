using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for create operation responses.
/// </summary>
public class CreateResponseDto
{
    /// <summary>
    /// The created record (compact key: "d").
    /// </summary>
    [JsonPropertyName("d")]
    public RecordDto Record { get; set; } = new();

    /// <summary>
    /// The unique identifier of the created record.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

