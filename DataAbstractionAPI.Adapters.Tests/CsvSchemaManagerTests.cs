namespace DataAbstractionAPI.Adapters.Tests;

using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Adapters.Csv;

public class CsvSchemaManagerTests
{
    [Fact]
    public void CsvSchemaManager_SavesSchema_ToJsonFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var schema = new CollectionSchema
        {
            Name = "users",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String },
                new FieldDefinition { Name = "name", Type = FieldType.String }
            }
        };

        var manager = new CsvSchemaManager(tempDir);

        // Act
        manager.SaveSchema("users", schema);

        // Assert
        var schemaPath = Path.Combine(tempDir, ".schema", "users.json");
        Assert.True(File.Exists(schemaPath), "Schema file should exist");

        var json = File.ReadAllText(schemaPath);
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        
        // Verify it's valid JSON by parsing it
        var parsed = Assert.IsType<CollectionSchema>(System.Text.Json.JsonSerializer.Deserialize<CollectionSchema>(json));
        Assert.NotNull(parsed);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_LoadsSchema_FromJsonFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var schemaDir = Path.Combine(tempDir, ".schema");
        Directory.CreateDirectory(schemaDir);
        
        var schema = new CollectionSchema
        {
            Name = "users",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String },
                new FieldDefinition { Name = "name", Type = FieldType.String, Nullable = false }
            }
        };

        // Save schema manually
        var json = System.Text.Json.JsonSerializer.Serialize(schema);
        var schemaPath = Path.Combine(schemaDir, "users.json");
        File.WriteAllText(schemaPath, json);

        var manager = new CsvSchemaManager(tempDir);

        // Act
        var loadedSchema = manager.LoadSchema("users");

        // Assert
        Assert.NotNull(loadedSchema);
        Assert.Equal("users", loadedSchema!.Name);
        Assert.Equal(2, loadedSchema.Fields.Count);
        Assert.Equal("id", loadedSchema.Fields[0].Name);
        Assert.Equal("name", loadedSchema.Fields[1].Name);
        Assert.False(loadedSchema.Fields[1].Nullable);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_SavesAndLoads_SchemaRoundtrip()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var originalSchema = new CollectionSchema
        {
            Name = "products",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String },
                new FieldDefinition { Name = "name", Type = FieldType.String, Nullable = false },
                new FieldDefinition { Name = "price", Type = FieldType.Float, Nullable = false, Default = 0.0 },
                new FieldDefinition { Name = "active", Type = FieldType.Boolean, Nullable = true, Default = true }
            }
        };

        var manager = new CsvSchemaManager(tempDir);

        // Act
        manager.SaveSchema("products", originalSchema);
        var loadedSchema = manager.LoadSchema("products");

        // Assert
        Assert.NotNull(loadedSchema);
        Assert.Equal(originalSchema.Name, loadedSchema!.Name);
        Assert.Equal(originalSchema.Fields.Count, loadedSchema.Fields.Count);
        
        for (int i = 0; i < originalSchema.Fields.Count; i++)
        {
            Assert.Equal(originalSchema.Fields[i].Name, loadedSchema.Fields[i].Name);
            Assert.Equal(originalSchema.Fields[i].Type, loadedSchema.Fields[i].Type);
            Assert.Equal(originalSchema.Fields[i].Nullable, loadedSchema.Fields[i].Nullable);
        }
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_LoadsSchema_WhenFileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);

        // Act
        var loadedSchema = manager.LoadSchema("nonexistent");

        // Assert
        Assert.Null(loadedSchema);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }
}

