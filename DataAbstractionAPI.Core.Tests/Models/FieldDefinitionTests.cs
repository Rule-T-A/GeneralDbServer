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
}

