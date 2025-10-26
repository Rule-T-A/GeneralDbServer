namespace DataAbstractionAPI.Core.Models;

/// <summary>
/// Provides context for generating default values.
/// </summary>
public class DefaultGenerationContext
{
    /// <summary>
    /// The name of the collection.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Existing records in the collection (for context-based defaults).
    /// </summary>
    public List<Record>? ExistingRecords { get; set; }
}

