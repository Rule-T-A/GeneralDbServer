using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Mapping;

/// <summary>
/// Extension methods for mapping CreateResult to CreateResponseDto.
/// </summary>
public static class CreateResultExtensions
{
    /// <summary>
    /// Converts a CreateResult to a CreateResponseDto.
    /// </summary>
    public static CreateResponseDto ToDto(this CreateResult result)
    {
        return new CreateResponseDto
        {
            Record = result.Record.ToDto(),
            Id = result.Id
        };
    }
}

