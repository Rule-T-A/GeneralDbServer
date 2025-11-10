namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Defines an aggregate function to apply to a field.
/// </summary>
public class AggregateFunction
{
    /// <summary>
    /// The field name to aggregate.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// The aggregate function: "count", "sum", "avg", "min", or "max".
    /// </summary>
    public string Function { get; set; } = string.Empty;

    /// <summary>
    /// The alias for the aggregated result in the output.
    /// </summary>
    public string Alias { get; set; } = string.Empty;
}

