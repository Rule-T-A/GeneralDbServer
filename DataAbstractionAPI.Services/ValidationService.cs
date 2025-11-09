namespace DataAbstractionAPI.Services;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Core.Exceptions;
using System.Globalization;

/// <summary>
/// Validates records against collection schemas, checking required fields and type compatibility.
/// </summary>
public class ValidationService : IValidationService
{
    public void Validate(Dictionary<string, object> record, CollectionSchema schema)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (schema == null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        // Validate each field in the schema
        foreach (var field in schema.Fields)
        {
            ValidateField(record, field);
        }
    }

    private void ValidateField(Dictionary<string, object> record, FieldDefinition field)
    {
        // Check if field exists in record
        var hasField = record.ContainsKey(field.Name);
        var fieldValue = hasField ? record[field.Name] : null;

        // Check required field (nullable = false)
        if (!field.Nullable)
        {
            if (!hasField || fieldValue == null)
            {
                throw new ValidationException(
                    field.Name,
                    $"Required field '{field.Name}' is missing or null");
            }
        }

        // If field is present and not null, validate type
        if (hasField && fieldValue != null)
        {
            ValidateFieldType(fieldValue, field);
        }
    }

    private void ValidateFieldType(object value, FieldDefinition field)
    {
        var actualType = GetValueType(value);
        var expectedType = field.Type;

        // If types match, validation passes
        if (IsTypeCompatible(actualType, expectedType, value))
        {
            return;
        }

        // Try type coercion for string values
        if (actualType == FieldType.String && CanCoerceToType(value, expectedType))
        {
            return;
        }

        // Type mismatch
        throw new ValidationException(
            field.Name,
            $"Field '{field.Name}' has invalid type. Expected {expectedType}, but got {actualType} (value: {value})");
    }

    private FieldType GetValueType(object value)
    {
        return value switch
        {
            string => FieldType.String,
            int => FieldType.Integer,
            long => FieldType.Integer,
            short => FieldType.Integer,
            double => FieldType.Float,
            float => FieldType.Float,
            decimal => FieldType.Float,
            bool => FieldType.Boolean,
            DateTime => FieldType.DateTime,
            System.Collections.IEnumerable enumerable when !(value is string) => FieldType.Array,
            _ => FieldType.Object
        };
    }

    private bool IsTypeCompatible(FieldType actualType, FieldType expectedType, object value)
    {
        // Exact match
        if (actualType == expectedType)
        {
            return true;
        }

        // Integer and Float are compatible for numeric operations
        if ((actualType == FieldType.Integer && expectedType == FieldType.Float) ||
            (actualType == FieldType.Float && expectedType == FieldType.Integer))
        {
            return true;
        }

        // DateTime and Date are compatible
        if ((actualType == FieldType.DateTime && expectedType == FieldType.Date) ||
            (actualType == FieldType.Date && expectedType == FieldType.DateTime))
        {
            return true;
        }

        return false;
    }

    private bool CanCoerceToType(object value, FieldType targetType)
    {
        if (value is not string stringValue)
        {
            return false;
        }

        return targetType switch
        {
            FieldType.Integer => int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            FieldType.Float => double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out _),
            FieldType.Boolean => bool.TryParse(stringValue, out _) ||
                                 stringValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                 stringValue.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                                 stringValue == "1" || stringValue == "0",
            FieldType.DateTime => DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
            FieldType.Date => DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
            _ => false
        };
    }
}

