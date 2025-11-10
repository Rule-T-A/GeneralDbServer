using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Mapping;

/// <summary>
/// Extension methods for mapping ListResult to ListResponseDto.
/// </summary>
public static class ListResultExtensions
{
    /// <summary>
    /// Converts a ListResult to a ListResponseDto.
    /// </summary>
    public static ListResponseDto ToDto(this ListResult result)
    {
        return new ListResponseDto
        {
            Data = result.Data.Select(r => r.ToDto()).ToList(),
            Total = result.Total,
            More = result.More
        };
    }
}

