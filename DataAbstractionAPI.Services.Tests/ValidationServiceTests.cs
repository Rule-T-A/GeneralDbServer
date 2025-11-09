namespace DataAbstractionAPI.Services.Tests;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Core.Exceptions;
using DataAbstractionAPI.Services;

public class ValidationServiceTests
{
    private readonly IValidationService _validator;

    public ValidationServiceTests()
    {
        _validator = new ValidationService();
    }

    private CollectionSchema CreateSchema(string name, List<FieldDefinition> fields)
    {
        return new CollectionSchema { Name = name, Fields = fields };
    }

    private FieldDefinition CreateField(string name, FieldType type, bool nullable = true, object? defaultValue = null)
    {
        return new FieldDefinition
        {
            Name = name,
            Type = type,
            Nullable = nullable,
            Default = defaultValue
        };
    }

    #region Required Field Validation

    [Fact]
    public void ValidationService_ValidatesRecord_AgainstSchema()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false)
        });
        var record = new Dictionary<string, object> { { "name", "Alice" } };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_RejectsRecord_WithMissingRequiredField()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false)
        });
        var record = new Dictionary<string, object>();

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validator.Validate(record, schema));
        Assert.Equal("name", exception.FieldName);
        Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidationService_RejectsRecord_WithNullRequiredField()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false)
        });
        var record = new Dictionary<string, object> { { "name", null! } };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validator.Validate(record, schema));
        Assert.Equal("name", exception.FieldName);
    }

    [Fact]
    public void ValidationService_AllowsNullableFields_ToBeNull()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false),
            CreateField("email", FieldType.String, nullable: true)
        });
        var record = new Dictionary<string, object> 
        { 
            { "name", "Alice" },
            { "email", null! }
        };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_AllowsNullableFields_ToBeMissing()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false),
            CreateField("email", FieldType.String, nullable: true)
        });
        var record = new Dictionary<string, object> { { "name", "Alice" } };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    #endregion

    #region Type Validation

    [Fact]
    public void ValidationService_ValidatesType_MatchesSchema_String()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false)
        });
        var record = new Dictionary<string, object> { { "name", "Alice" } };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_ValidatesType_MatchesSchema_Integer()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("age", FieldType.Integer, nullable: false)
        });
        var record = new Dictionary<string, object> { { "age", 25 } };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_ValidatesType_MatchesSchema_Float()
    {
        // Arrange
        var schema = CreateSchema("products", new List<FieldDefinition>
        {
            CreateField("price", FieldType.Float, nullable: false)
        });
        var record = new Dictionary<string, object> { { "price", 19.99 } };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_ValidatesType_MatchesSchema_Boolean()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("is_active", FieldType.Boolean, nullable: false)
        });
        var record = new Dictionary<string, object> { { "is_active", true } };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_ValidatesType_MatchesSchema_DateTime()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("created_at", FieldType.DateTime, nullable: false)
        });
        var record = new Dictionary<string, object> { { "created_at", DateTime.UtcNow } };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_AllowsTypeCoercion_StringToInteger()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("age", FieldType.Integer, nullable: false)
        });
        var record = new Dictionary<string, object> { { "age", "25" } }; // String that can be parsed as int

        // Act & Assert - should not throw (type coercion allowed)
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_AllowsTypeCoercion_StringToFloat()
    {
        // Arrange
        var schema = CreateSchema("products", new List<FieldDefinition>
        {
            CreateField("price", FieldType.Float, nullable: false)
        });
        var record = new Dictionary<string, object> { { "price", "19.99" } }; // String that can be parsed as float

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_RejectsInvalidType_WhenCoercionFails()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("age", FieldType.Integer, nullable: false)
        });
        var record = new Dictionary<string, object> { { "age", "not-a-number" } };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validator.Validate(record, schema));
        Assert.Equal("age", exception.FieldName);
        Assert.Contains("type", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Multiple Fields Validation

    [Fact]
    public void ValidationService_ValidatesMultipleRequiredFields()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false),
            CreateField("email", FieldType.String, nullable: false),
            CreateField("age", FieldType.Integer, nullable: false)
        });
        var record = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "email", "alice@example.com" },
            { "age", 25 }
        };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_RejectsRecord_WithMultipleMissingRequiredFields()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false),
            CreateField("email", FieldType.String, nullable: false)
        });
        var record = new Dictionary<string, object> { { "name", "Alice" } }; // Missing email

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validator.Validate(record, schema));
        Assert.Equal("email", exception.FieldName);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ValidationService_AllowsExtraFields_NotInSchema()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false)
        });
        var record = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "extra_field", "value" } // Not in schema, but allowed
        };

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_HandlesEmptySchema()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>());
        var record = new Dictionary<string, object> { { "any_field", "any_value" } };

        // Act & Assert - should not throw (no fields to validate)
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_HandlesEmptyRecord()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: true) // All fields nullable
        });
        var record = new Dictionary<string, object>();

        // Act & Assert - should not throw
        _validator.Validate(record, schema);
    }

    [Fact]
    public void ValidationService_ValidatesEmptyString_ForRequiredStringField()
    {
        // Arrange
        var schema = CreateSchema("users", new List<FieldDefinition>
        {
            CreateField("name", FieldType.String, nullable: false)
        });
        var record = new Dictionary<string, object> { { "name", "" } }; // Empty string

        // Act & Assert - empty string is valid (not null)
        _validator.Validate(record, schema);
    }

    #endregion
}

