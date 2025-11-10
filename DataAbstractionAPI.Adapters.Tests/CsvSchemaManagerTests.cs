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

    // ============================================
    // Task 1.3.1: SaveSchema Edge Cases
    // ============================================

    [Fact]
    public void CsvSchemaManager_SaveSchema_OverwritesExisting_UpdatesFile()
    {
        // Arrange - Create initial schema
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);
        var initialSchema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String },
                new FieldDefinition { Name = "name", Type = FieldType.String }
            }
        };
        manager.SaveSchema("test", initialSchema);

        // Act - Save updated schema
        var updatedSchema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String },
                new FieldDefinition { Name = "name", Type = FieldType.String },
                new FieldDefinition { Name = "email", Type = FieldType.String } // New field
            }
        };
        manager.SaveSchema("test", updatedSchema);

        // Assert - File should be overwritten with new schema
        var loadedSchema = manager.LoadSchema("test");
        Assert.NotNull(loadedSchema);
        Assert.Equal(3, loadedSchema!.Fields.Count);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "email");

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_SaveSchema_WithAllFieldTypes_SavesCorrectly()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);
        var schema = new CollectionSchema
        {
            Name = "alltypes",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "stringField", Type = FieldType.String },
                new FieldDefinition { Name = "integerField", Type = FieldType.Integer },
                new FieldDefinition { Name = "floatField", Type = FieldType.Float },
                new FieldDefinition { Name = "booleanField", Type = FieldType.Boolean },
                new FieldDefinition { Name = "dateTimeField", Type = FieldType.DateTime },
                new FieldDefinition { Name = "dateField", Type = FieldType.Date },
                new FieldDefinition { Name = "arrayField", Type = FieldType.Array },
                new FieldDefinition { Name = "objectField", Type = FieldType.Object }
            }
        };

        // Act
        manager.SaveSchema("alltypes", schema);

        // Assert - All field types should be saved and loaded correctly
        var loadedSchema = manager.LoadSchema("alltypes");
        Assert.NotNull(loadedSchema);
        Assert.Equal(8, loadedSchema!.Fields.Count);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "stringField" && f.Type == FieldType.String);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "integerField" && f.Type == FieldType.Integer);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "floatField" && f.Type == FieldType.Float);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "booleanField" && f.Type == FieldType.Boolean);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "dateTimeField" && f.Type == FieldType.DateTime);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "dateField" && f.Type == FieldType.Date);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "arrayField" && f.Type == FieldType.Array);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "objectField" && f.Type == FieldType.Object);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_SaveSchema_CreatesDirectory_IfNotExists()
    {
        // Arrange - Create temp directory but don't create .schema subdirectory
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        // Delete .schema directory if it exists (shouldn't, but just in case)
        var schemaDir = Path.Combine(tempDir, ".schema");
        if (Directory.Exists(schemaDir))
        {
            Directory.Delete(schemaDir, true);
        }

        var manager = new CsvSchemaManager(tempDir);
        var schema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String }
            }
        };

        // Act
        manager.SaveSchema("test", schema);

        // Assert - Directory should be created and file should exist
        Assert.True(Directory.Exists(schemaDir), ".schema directory should be created");
        var schemaPath = Path.Combine(schemaDir, "test.json");
        Assert.True(File.Exists(schemaPath), "Schema file should exist");

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_SaveSchema_WithNestedDirectory_CreatesPath()
    {
        // Arrange - Test that constructor creates directory even if base directory is new
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        // Don't create tempDir - let CsvSchemaManager constructor create it via Directory.CreateDirectory
        
        var manager = new CsvSchemaManager(tempDir);
        var schema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String }
            }
        };

        // Act
        manager.SaveSchema("test", schema);

        // Assert - Both base directory and .schema directory should exist
        Assert.True(Directory.Exists(tempDir), "Base directory should exist");
        var schemaDir = Path.Combine(tempDir, ".schema");
        Assert.True(Directory.Exists(schemaDir), ".schema directory should exist");
        var schemaPath = Path.Combine(schemaDir, "test.json");
        Assert.True(File.Exists(schemaPath), "Schema file should exist");

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    // ============================================
    // Task 1.3.2: LoadSchema Edge Cases
    // ============================================

    [Fact]
    public void CsvSchemaManager_LoadSchema_WithMalformedFile_HandlesGracefully()
    {
        // Arrange - Create malformed JSON file
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var schemaDir = Path.Combine(tempDir, ".schema");
        Directory.CreateDirectory(schemaDir);
        var schemaPath = Path.Combine(schemaDir, "malformed.json");
        File.WriteAllText(schemaPath, "{ invalid json }"); // Invalid JSON
        
        var manager = new CsvSchemaManager(tempDir);

        // Act & Assert - Should throw JsonException when deserializing malformed JSON
        Assert.Throws<System.Text.Json.JsonException>(() => manager.LoadSchema("malformed"));

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_LoadSchema_WithAllFieldTypes_LoadsCorrectly()
    {
        // Arrange - Save schema with all field types
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);
        var schema = new CollectionSchema
        {
            Name = "alltypes",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "stringField", Type = FieldType.String, Nullable = false, Default = "default" },
                new FieldDefinition { Name = "integerField", Type = FieldType.Integer, Nullable = true, Default = 42 },
                new FieldDefinition { Name = "floatField", Type = FieldType.Float, Nullable = false, Default = 3.14 },
                new FieldDefinition { Name = "booleanField", Type = FieldType.Boolean, Nullable = true, Default = true },
                new FieldDefinition { Name = "dateTimeField", Type = FieldType.DateTime, Nullable = false },
                new FieldDefinition { Name = "dateField", Type = FieldType.Date, Nullable = true },
                new FieldDefinition { Name = "arrayField", Type = FieldType.Array, Nullable = false },
                new FieldDefinition { Name = "objectField", Type = FieldType.Object, Nullable = true }
            }
        };
        manager.SaveSchema("alltypes", schema);

        // Act
        var loadedSchema = manager.LoadSchema("alltypes");

        // Assert - All field types should load correctly with their properties
        Assert.NotNull(loadedSchema);
        Assert.Equal(8, loadedSchema!.Fields.Count);
        
        var stringField = loadedSchema.Fields.First(f => f.Name == "stringField");
        Assert.Equal(FieldType.String, stringField.Type);
        Assert.False(stringField.Nullable);
        // JSON deserialization may convert to JsonElement, so check string representation
        Assert.NotNull(stringField.Default);
        Assert.Equal("default", stringField.Default.ToString());
        
        var integerField = loadedSchema.Fields.First(f => f.Name == "integerField");
        Assert.Equal(FieldType.Integer, integerField.Type);
        Assert.True(integerField.Nullable);
        
        var floatField = loadedSchema.Fields.First(f => f.Name == "floatField");
        Assert.Equal(FieldType.Float, floatField.Type);
        Assert.False(floatField.Nullable);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_LoadSchema_WithEmptyJsonFile_HandlesGracefully()
    {
        // Arrange - Create empty JSON file
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var schemaDir = Path.Combine(tempDir, ".schema");
        Directory.CreateDirectory(schemaDir);
        var schemaPath = Path.Combine(schemaDir, "empty.json");
        File.WriteAllText(schemaPath, ""); // Empty file
        
        var manager = new CsvSchemaManager(tempDir);

        // Act & Assert - Should throw JsonException when deserializing empty file
        Assert.Throws<System.Text.Json.JsonException>(() => manager.LoadSchema("empty"));

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    // ============================================
    // Task 1.3.3: UpdateSchemaField Tests
    // ============================================

    [Fact]
    public void CsvSchemaManager_UpdateSchemaField_UpdatesExistingField()
    {
        // Arrange - Create schema with existing field
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);
        var initialSchema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String, Nullable = false },
                new FieldDefinition { Name = "name", Type = FieldType.String, Nullable = true }
            }
        };
        manager.SaveSchema("test", initialSchema);

        // Act - Update existing field
        var updatedField = new FieldDefinition 
        { 
            Name = "name", 
            Type = FieldType.String, 
            Nullable = false, // Changed from true to false
            Default = "Unknown" // Added default value
        };
        manager.UpdateSchemaField("test", updatedField);

        // Assert - Field should be updated, not duplicated
        var loadedSchema = manager.LoadSchema("test");
        Assert.NotNull(loadedSchema);
        Assert.Equal(2, loadedSchema!.Fields.Count);
        var nameField = loadedSchema.Fields.First(f => f.Name == "name");
        Assert.Equal(FieldType.String, nameField.Type);
        Assert.False(nameField.Nullable); // Should be updated
        // JSON deserialization may convert to JsonElement, so check string representation
        Assert.NotNull(nameField.Default);
        Assert.Equal("Unknown", nameField.Default.ToString()); // Should have default value

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_UpdateSchemaField_AddsNewField()
    {
        // Arrange - Create schema without the field
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);
        var initialSchema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String }
            }
        };
        manager.SaveSchema("test", initialSchema);

        // Act - Add new field
        var newField = new FieldDefinition 
        { 
            Name = "email", 
            Type = FieldType.String, 
            Nullable = false 
        };
        manager.UpdateSchemaField("test", newField);

        // Assert - New field should be added
        var loadedSchema = manager.LoadSchema("test");
        Assert.NotNull(loadedSchema);
        Assert.Equal(2, loadedSchema!.Fields.Count);
        Assert.Contains(loadedSchema.Fields, f => f.Name == "id");
        Assert.Contains(loadedSchema.Fields, f => f.Name == "email" && f.Type == FieldType.String && !f.Nullable);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_UpdateSchemaField_WithDifferentType_UpdatesType()
    {
        // Arrange - Create schema with field of one type
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);
        var initialSchema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "age", Type = FieldType.String } // Initially String
            }
        };
        manager.SaveSchema("test", initialSchema);

        // Act - Update field to different type
        var updatedField = new FieldDefinition 
        { 
            Name = "age", 
            Type = FieldType.Integer, // Changed to Integer
            Nullable = false
        };
        manager.UpdateSchemaField("test", updatedField);

        // Assert - Field type should be updated
        var loadedSchema = manager.LoadSchema("test");
        Assert.NotNull(loadedSchema);
        var ageField = loadedSchema!.Fields.First(f => f.Name == "age");
        Assert.Equal(FieldType.Integer, ageField.Type); // Should be Integer now
        Assert.False(ageField.Nullable);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_UpdateSchemaField_WithNullSchemaFile_CreatesNewSchema()
    {
        // Arrange - No schema file exists
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);

        // Act - Update field on non-existent schema
        var newField = new FieldDefinition 
        { 
            Name = "id", 
            Type = FieldType.String, 
            Nullable = false 
        };
        manager.UpdateSchemaField("newcollection", newField);

        // Assert - New schema should be created
        var loadedSchema = manager.LoadSchema("newcollection");
        Assert.NotNull(loadedSchema);
        Assert.Equal("newcollection", loadedSchema!.Name);
        Assert.Single(loadedSchema.Fields);
        Assert.Equal("id", loadedSchema.Fields[0].Name);
        Assert.Equal(FieldType.String, loadedSchema.Fields[0].Type);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_UpdateSchemaField_WithNullFieldsList_HandlesGracefully()
    {
        // Arrange - Create schema with null Fields (edge case)
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var schemaDir = Path.Combine(tempDir, ".schema");
        Directory.CreateDirectory(schemaDir);
        var schemaPath = Path.Combine(schemaDir, "test.json");
        
        // Create schema JSON with null Fields array (simulating corrupted or manually edited file)
        var schemaJson = "{\"Name\":\"test\",\"Fields\":null}";
        File.WriteAllText(schemaPath, schemaJson);
        
        var manager = new CsvSchemaManager(tempDir);

        // Act - Update field on schema with null Fields
        var newField = new FieldDefinition 
        { 
            Name = "id", 
            Type = FieldType.String 
        };
        manager.UpdateSchemaField("test", newField);

        // Assert - Should handle null Fields gracefully and create new list
        var loadedSchema = manager.LoadSchema("test");
        Assert.NotNull(loadedSchema);
        Assert.NotNull(loadedSchema!.Fields);
        Assert.Single(loadedSchema.Fields);
        Assert.Equal("id", loadedSchema.Fields[0].Name);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvSchemaManager_UpdateSchemaField_MaintainsFieldOrder()
    {
        // Arrange - Create schema with multiple fields
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        var manager = new CsvSchemaManager(tempDir);
        var initialSchema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String },
                new FieldDefinition { Name = "name", Type = FieldType.String },
                new FieldDefinition { Name = "email", Type = FieldType.String }
            }
        };
        manager.SaveSchema("test", initialSchema);

        // Act - Update middle field
        var updatedField = new FieldDefinition 
        { 
            Name = "name", 
            Type = FieldType.String, 
            Nullable = false 
        };
        manager.UpdateSchemaField("test", updatedField);

        // Assert - Field order should be maintained
        var loadedSchema = manager.LoadSchema("test");
        Assert.NotNull(loadedSchema);
        Assert.Equal(3, loadedSchema!.Fields.Count);
        Assert.Equal("id", loadedSchema.Fields[0].Name);
        Assert.Equal("name", loadedSchema.Fields[1].Name); // Should still be in same position
        Assert.Equal("email", loadedSchema.Fields[2].Name);

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}

