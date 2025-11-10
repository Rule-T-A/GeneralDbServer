using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Mapping;

/// <summary>
/// Extension methods for mapping CollectionSchema to SchemaResponseDto.
/// </summary>
public static class CollectionSchemaExtensions
{
    /// <summary>
    /// Converts a CollectionSchema to a SchemaResponseDto.
    /// </summary>
    public static SchemaResponseDto ToDto(this CollectionSchema schema)
    {
        return new SchemaResponseDto
        {
            Name = schema.Name,
            Fields = schema.Fields.Select(f => f.ToDto()).ToList()
        };
    }
}

