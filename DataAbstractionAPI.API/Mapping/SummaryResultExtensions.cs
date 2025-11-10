using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Mapping;

/// <summary>
/// Extension methods for mapping SummaryResult to SummaryResponseDto.
/// </summary>
public static class SummaryResultExtensions
{
    /// <summary>
    /// Converts a SummaryResult to a SummaryResponseDto.
    /// </summary>
    public static SummaryResponseDto ToDto(this SummaryResult result)
    {
        var dto = new SummaryResponseDto();
        foreach (var kvp in result.Counts)
        {
            dto[kvp.Key] = kvp.Value;
        }
        return dto;
    }
}

