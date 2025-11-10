using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Mapping;

/// <summary>
/// Extension methods for mapping BulkResult to BulkResponseDto.
/// </summary>
public static class BulkResultExtensions
{
    /// <summary>
    /// Converts a BulkResult to a BulkResponseDto.
    /// </summary>
    public static BulkResponseDto ToDto(this BulkResult result)
    {
        return new BulkResponseDto
        {
            Success = result.Success,
            Succeeded = result.Succeeded,
            Failed = result.Failed,
            Results = result.Results?.Select(r => r.ToDto()).ToList(),
            Ids = result.Ids,
            Error = result.Error,
            FailedIndex = result.FailedIndex,
            FailedError = result.FailedError
        };
    }

    /// <summary>
    /// Converts a BulkOperationItemResult to a BulkOperationItemResultDto.
    /// </summary>
    public static BulkOperationItemResultDto ToDto(this BulkOperationItemResult item)
    {
        return new BulkOperationItemResultDto
        {
            Index = item.Index,
            Id = item.Id,
            Success = item.Success,
            Error = item.Error
        };
    }
}

