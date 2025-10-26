namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Represents the schema (structure) of a collection.
/// </summary>
public class CollectionSchema
{
    public string Name { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = new();
}

