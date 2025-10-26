namespace DataAbstractionAPI.Core.Exceptions;

/// <summary>
/// Exception thrown when type conversion fails.
/// </summary>
public class ConversionException : Exception
{
    /// <summary>
    /// The name of the field being converted.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// The value that failed to convert.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// The source type.
    /// </summary>
    public Enums.FieldType FromType { get; }

    /// <summary>
    /// The target type.
    /// </summary>
    public Enums.FieldType ToType { get; }

    public ConversionException(string fieldName, object value, Enums.FieldType fromType, Enums.FieldType toType)
        : this(fieldName, value, fromType, toType, $"Failed to convert field '{fieldName}' from {fromType} to {toType}. Value: {value}")
    {
    }

    public ConversionException(string fieldName, object value, Enums.FieldType fromType, Enums.FieldType toType, string message)
        : base(message)
    {
        FieldName = fieldName;
        Value = value;
        FromType = fromType;
        ToType = toType;
    }

    public ConversionException(string fieldName, object value, Enums.FieldType fromType, Enums.FieldType toType, string message, Exception innerException)
        : base(message, innerException)
    {
        FieldName = fieldName;
        Value = value;
        FromType = fromType;
        ToType = toType;
    }
}

