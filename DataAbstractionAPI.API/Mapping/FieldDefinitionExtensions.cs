using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Mapping;

/// <summary>
/// Extension methods for mapping FieldDefinition to FieldDefinitionDto.
/// </summary>
public static class FieldDefinitionExtensions
{
    /// <summary>
    /// Converts a FieldDefinition to a FieldDefinitionDto.
    /// </summary>
    public static FieldDefinitionDto ToDto(this FieldDefinition field)
    {
        return new FieldDefinitionDto
        {
            Name = field.Name,
            Type = field.Type.ToString(), // Serialize enum as string
            Nullable = field.Nullable,
            Default = field.Default
        };
    }
}

