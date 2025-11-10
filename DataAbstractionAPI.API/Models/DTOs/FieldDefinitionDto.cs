using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for field definition in API responses.
/// </summary>
public class FieldDefinitionDto
{
    /// <summary>
    /// The name of the field.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The type of the field as a string (enum serialized as string).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the field can be null.
    /// </summary>
    [JsonPropertyName("nullable")]
    public bool Nullable { get; set; } = true;

    /// <summary>
    /// The default value for the field.
    /// </summary>
    [JsonPropertyName("default")]
    public object? Default { get; set; }
}

