using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Mapping;

/// <summary>
/// Extension methods for mapping AggregateResult to AggregateResponseDto.
/// </summary>
public static class AggregateResultExtensions
{
    /// <summary>
    /// Converts an AggregateResult to an AggregateResponseDto.
    /// </summary>
    public static AggregateResponseDto ToDto(this AggregateResult result)
    {
        return new AggregateResponseDto
        {
            Data = result.Data
        };
    }
}

