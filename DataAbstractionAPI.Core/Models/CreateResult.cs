namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Result of a create operation.
/// </summary>
public class CreateResult
{
    public Record Record { get; set; } = new();
    public string Id { get; set; } = string.Empty;
}

