namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Request model for complex aggregation operations.
/// </summary>
public class AggregateRequest
{
    /// <summary>
    /// Fields to group by. Can be multiple fields for multi-level grouping.
    /// </summary>
    public string[]? GroupBy { get; set; }

    /// <summary>
    /// List of aggregate functions to apply.
    /// </summary>
    public List<AggregateFunction> Aggregates { get; set; } = new();

    /// <summary>
    /// Optional filter to apply before aggregation.
    /// </summary>
    public Dictionary<string, object>? Filter { get; set; }
}

