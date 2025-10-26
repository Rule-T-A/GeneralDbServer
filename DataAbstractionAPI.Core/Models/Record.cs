namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Represents a single record (row) in a collection.
/// </summary>
public class Record
{
    public string Id { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

