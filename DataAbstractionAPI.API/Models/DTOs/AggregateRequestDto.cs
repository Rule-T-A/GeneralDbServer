using System.Text.Json.Serialization;

namespace DataAbstractionAPI.API.Models.DTOs;

/// <summary>
/// Data Transfer Object for aggregate operation requests.
/// </summary>
public class AggregateRequestDto
{
    /// <summary>
    /// Fields to group by. Can be multiple fields for multi-level grouping.
    /// </summary>
    [JsonPropertyName("group_by")]
    public string[]? GroupBy { get; set; }

    /// <summary>
    /// List of aggregate functions to apply.
    /// </summary>
    [JsonPropertyName("aggregates")]
    public List<AggregateFunctionDto> Aggregates { get; set; } = new();

    /// <summary>
    /// Optional filter to apply before aggregation.
    /// </summary>
    [JsonPropertyName("filter")]
    public Dictionary<string, object>? Filter { get; set; }
}

/// <summary>
/// Data Transfer Object for an aggregate function definition.
/// </summary>
public class AggregateFunctionDto
{
    /// <summary>
    /// The field name to aggregate.
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// The aggregate function: "count", "sum", "avg", "min", or "max".
    /// </summary>
    [JsonPropertyName("function")]
    public string Function { get; set; } = string.Empty;

    /// <summary>
    /// The alias for the aggregated result in the output.
    /// </summary>
    [JsonPropertyName("alias")]
    public string Alias { get; set; } = string.Empty;
}

