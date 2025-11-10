namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;

public class FieldDefinitionTests
{
    [Fact]
    public void FieldDefinition_Initializes_WithDefaults()
    {
        // Arrange & Act
        var field = new FieldDefinition();

        // Assert
        Assert.NotNull(field);
        Assert.Equal(string.Empty, field.Name);
        Assert.Equal(FieldType.String, field.Type);
        Assert.True(field.Nullable);
        Assert.Null(field.Default);
    }

    [Fact]
    public void FieldDefinition_CanSetAllProperties()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "email",
            Type = FieldType.String,
            Nullable = false,
            Default = "user@example.com"
        };

        // Assert
        Assert.Equal("email", field.Name);
        Assert.Equal(FieldType.String, field.Type);
        Assert.False(field.Nullable);
        Assert.Equal("user@example.com", field.Default);
    }

    // ============================================
    // Task 2.2.6: FieldDefinition Edge Cases
    // ============================================

    [Fact]
    public void FieldDefinition_WithNullDefault_IsValid()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "optional_field",
            Type = FieldType.String,
            Nullable = true,
            Default = null
        };

        // Assert - Null default is valid (default value)
        Assert.Equal("optional_field", field.Name);
        Assert.True(field.Nullable);
        Assert.Null(field.Default);
    }

    [Fact]
    public void FieldDefinition_WithAllFieldTypes_WorksCorrectly()
    {
        // Arrange & Act - Test all FieldType enum values
        var stringField = new FieldDefinition { Name = "str", Type = FieldType.String };
        var intField = new FieldDefinition { Name = "int", Type = FieldType.Integer };
        var floatField = new FieldDefinition { Name = "float", Type = FieldType.Float };
        var boolField = new FieldDefinition { Name = "bool", Type = FieldType.Boolean };
        var dateTimeField = new FieldDefinition { Name = "datetime", Type = FieldType.DateTime };
        var dateField = new FieldDefinition { Name = "date", Type = FieldType.Date };
        var arrayField = new FieldDefinition { Name = "array", Type = FieldType.Array };
        var objectField = new FieldDefinition { Name = "object", Type = FieldType.Object };

        // Assert
        Assert.Equal(FieldType.String, stringField.Type);
        Assert.Equal(FieldType.Integer, intField.Type);
        Assert.Equal(FieldType.Float, floatField.Type);
        Assert.Equal(FieldType.Boolean, boolField.Type);
        Assert.Equal(FieldType.DateTime, dateTimeField.Type);
        Assert.Equal(FieldType.Date, dateField.Type);
        Assert.Equal(FieldType.Array, arrayField.Type);
        Assert.Equal(FieldType.Object, objectField.Type);
    }

    [Fact]
    public void FieldDefinition_WithSpecialCharactersInName_IsValid()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "field-with-special_chars.123",
            Type = FieldType.String
        };

        // Assert - Special characters in name are valid
        Assert.Equal("field-with-special_chars.123", field.Name);
        Assert.Equal(FieldType.String, field.Type);
    }
}

