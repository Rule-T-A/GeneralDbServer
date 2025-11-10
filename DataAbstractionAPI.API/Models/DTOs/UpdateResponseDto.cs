using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for update operation responses.
/// </summary>
public class UpdateResponseDto
{
    /// <summary>
    /// The fields that were updated (compact key: "d").
    /// </summary>
    [JsonPropertyName("d")]
    public Dictionary<string, object> UpdatedFields { get; set; } = new();

    /// <summary>
    /// Indicates whether the update was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;
}

