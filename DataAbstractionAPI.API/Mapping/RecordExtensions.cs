using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Mapping;

/// <summary>
/// Extension methods for mapping Record to RecordDto.
/// </summary>
public static class RecordExtensions
{
    /// <summary>
    /// Converts a Record to a RecordDto.
    /// </summary>
    public static RecordDto ToDto(this Record record)
    {
        return new RecordDto
        {
            Id = record.Id,
            Data = record.Data
        };
    }
}

