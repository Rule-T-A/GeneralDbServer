namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for summary operation responses.
/// This is serialized as a flat JSON object (e.g., {"active": 45, "inactive": 12}).
/// </summary>
public class SummaryResponseDto : Dictionary<string, int>
{
    // Inherits from Dictionary<string, int> to serialize as flat JSON object
}

