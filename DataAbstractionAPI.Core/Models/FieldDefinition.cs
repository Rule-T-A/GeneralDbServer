namespace DataAbstractionAPI.Core.Models;

using DataAbstractionAPI.Core.Enums;

/// <summary>
/// Defines a field (column) in a collection.
/// </summary>
public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public FieldType Type { get; set; }
    public bool Nullable { get; set; } = true;
    public object? Default { get; set; }
}

