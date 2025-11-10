namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Result of an aggregation operation.
/// </summary>
public class AggregateResult
{
    /// <summary>
    /// List of grouped and aggregated results.
    /// Each dictionary contains group-by field values and aggregate function results.
    /// </summary>
    public List<Dictionary<string, object>> Data { get; set; } = new();
}

