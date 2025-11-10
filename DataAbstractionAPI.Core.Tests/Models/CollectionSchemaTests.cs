namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;

public class CollectionSchemaTests
{
    [Fact]
    public void CollectionSchema_Initializes_WithDefaults()
    {
        // Arrange & Act
        var schema = new CollectionSchema();

        // Assert
        Assert.Equal(string.Empty, schema.Name);
        Assert.NotNull(schema.Fields);
        Assert.Empty(schema.Fields);
    }

    [Fact]
    public void CollectionSchema_CanSetName_Property()
    {
        // Arrange
        var schema = new CollectionSchema();

        // Act
        schema.Name = "users";

        // Assert
        Assert.Equal("users", schema.Name);
    }

    [Fact]
    public void CollectionSchema_CanSetFields_Property()
    {
        // Arrange
        var schema = new CollectionSchema();
        var fields = new List<FieldDefinition>
        {
            new FieldDefinition { Name = "id", Type = FieldType.String },
            new FieldDefinition { Name = "name", Type = FieldType.String }
        };

        // Act
        schema.Fields = fields;

        // Assert
        Assert.NotNull(schema.Fields);
        Assert.Equal(2, schema.Fields.Count);
        Assert.Equal("id", schema.Fields[0].Name);
        Assert.Equal("name", schema.Fields[1].Name);
    }

    [Fact]
    public void CollectionSchema_CanBeCreated_WithInitialization()
    {
        // Arrange
        var fields = new List<FieldDefinition>
        {
            new FieldDefinition { Name = "id", Type = FieldType.String, Nullable = false },
            new FieldDefinition { Name = "email", Type = FieldType.String, Nullable = true }
        };

        // Act
        var schema = new CollectionSchema
        {
            Name = "users",
            Fields = fields
        };

        // Assert
        Assert.Equal("users", schema.Name);
        Assert.NotNull(schema.Fields);
        Assert.Equal(2, schema.Fields.Count);
    }

    // ============================================
    // Task 2.2.5: CollectionSchema Edge Cases
    // ============================================

    [Fact]
    public void CollectionSchema_WithNullFields_HandlesGracefully()
    {
        // Arrange
        var schema = new CollectionSchema
        {
            Name = "test",
            Fields = null!
        };

        // Assert - Fields can be set to null (though not recommended)
        Assert.Equal("test", schema.Name);
        Assert.Null(schema.Fields);
    }

    [Fact]
    public void CollectionSchema_WithEmptyName_IsValid()
    {
        // Arrange
        var schema = new CollectionSchema
        {
            Name = string.Empty,
            Fields = new List<FieldDefinition> { new FieldDefinition { Name = "id", Type = FieldType.String } }
        };

        // Assert - Empty name is valid (default value)
        Assert.Equal(string.Empty, schema.Name);
        Assert.NotNull(schema.Fields);
        Assert.Single(schema.Fields);
    }

    [Fact]
    public void CollectionSchema_WithDuplicateFieldNames_IsValid()
    {
        // Arrange
        var schema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String },
                new FieldDefinition { Name = "id", Type = FieldType.Integer } // Duplicate name
            }
        };

        // Assert - Duplicate field names are allowed (validation should happen at service layer)
        Assert.Equal(2, schema.Fields.Count);
        Assert.Equal("id", schema.Fields[0].Name);
        Assert.Equal("id", schema.Fields[1].Name);
        Assert.Equal(FieldType.String, schema.Fields[0].Type);
        Assert.Equal(FieldType.Integer, schema.Fields[1].Type);
    }
}

