namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Options for querying records from a collection.
/// </summary>
public class QueryOptions
{
    public string[]? Fields { get; set; }
    public Dictionary<string, object>? Filter { get; set; }
    public int Limit { get; set; } = 10;
    public int Offset { get; set; } = 0;
    public string? Sort { get; set; }
}

