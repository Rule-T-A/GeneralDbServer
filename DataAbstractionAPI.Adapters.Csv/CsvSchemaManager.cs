namespace DataAbstractionAPI.Adapters.Csv;

using System.Text.Json;
using DataAbstractionAPI.Core.Models;

/// <summary>
/// Manages schema files for CSV collections.
/// Schemas are stored as JSON files in the .schema directory.
/// </summary>
public class CsvSchemaManager
{
    private readonly string _baseDirectory;
    private readonly string _schemaDirectory;

    public CsvSchemaManager(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        _schemaDirectory = Path.Combine(_baseDirectory, ".schema");
        
        // Ensure schema directory exists
        Directory.CreateDirectory(_schemaDirectory);
    }

    /// <summary>
    /// Saves a schema to a JSON file.
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="schema">Schema to save</param>
    public void SaveSchema(string collectionName, CollectionSchema schema)
    {
        var filePath = GetSchemaFilePath(collectionName);
        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a schema from a JSON file.
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>Schema if found, null otherwise</returns>
    public CollectionSchema? LoadSchema(string collectionName)
    {
        var filePath = GetSchemaFilePath(collectionName);
        
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<CollectionSchema>(json);
    }

    /// <summary>
    /// Checks if a schema file exists for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>True if schema exists, false otherwise</returns>
    public bool SchemaExists(string collectionName)
    {
        var filePath = GetSchemaFilePath(collectionName);
        return File.Exists(filePath);
    }

    private string GetSchemaFilePath(string collectionName)
    {
        return Path.Combine(_schemaDirectory, $"{collectionName}.json");
    }
}

