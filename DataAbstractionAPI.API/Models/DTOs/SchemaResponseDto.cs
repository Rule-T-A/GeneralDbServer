using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for collection schema in API responses.
/// </summary>
public class SchemaResponseDto
{
    /// <summary>
    /// The name of the collection.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The list of field definitions.
    /// </summary>
    [JsonPropertyName("fields")]
    public List<FieldDefinitionDto> Fields { get; set; } = new();
}

