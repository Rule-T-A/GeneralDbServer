namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Result of a list/query operation.
/// </summary>
public class ListResult
{
    public List<Record> Data { get; set; } = new();
    public int Total { get; set; }
    public bool More { get; set; }
}

