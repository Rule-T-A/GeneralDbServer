namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Result of a summary operation (field value counts).
/// </summary>
public class SummaryResult
{
    /// <summary>
    /// Dictionary mapping field values to their counts.
    /// </summary>
    public Dictionary<string, int> Counts { get; set; } = new();
}

