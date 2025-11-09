namespace DataAbstractionAPI.Services;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Core.Exceptions;
using System.Globalization;
using Microsoft.Extensions.Logging;

/// <summary>
/// Converts values between different types using various conversion strategies.
/// </summary>
public class TypeConverter : ITypeConverter
{
    private readonly ILogger<TypeConverter>? _logger;

    public TypeConverter(ILogger<TypeConverter>? logger = null)
    {
        _logger = logger;
    }

    public object Convert(object value, FieldType fromType, FieldType toType, ConversionStrategy strategy)
    {
        _logger?.LogDebug("Converting value '{Value}' from {FromType} to {ToType} using strategy {Strategy}", 
            value, fromType, toType, strategy);

        // Handle null values
        if (value == null)
        {
            _logger?.LogDebug("Value is null, returning null");
            return null!;
        }

        // Handle same-type conversion (no conversion needed)
        if (fromType == toType)
        {
            _logger?.LogDebug("Source and target types are the same, no conversion needed");
            return value;
        }

        try
        {
            var result = PerformConversion(value, fromType, toType);
            _logger?.LogDebug("Successfully converted value '{Value}' from {FromType} to {ToType}, result: '{Result}'", 
                value, fromType, toType, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Conversion failed for value '{Value}' from {FromType} to {ToType} using strategy {Strategy}", 
                value, fromType, toType, strategy);
            // Handle conversion failure based on strategy
            return HandleConversionFailure(value, fromType, toType, strategy, ex);
        }
    }

    private object PerformConversion(object value, FieldType fromType, FieldType toType)
    {
        // String conversions
        if (fromType == FieldType.String && toType == FieldType.Integer)
        {
            return ConvertStringToInteger((string)value);
        }

        if (fromType == FieldType.String && toType == FieldType.Float)
        {
            return ConvertStringToFloat((string)value);
        }

        if (fromType == FieldType.String && toType == FieldType.Boolean)
        {
            return ConvertStringToBoolean((string)value);
        }

        if (fromType == FieldType.String && toType == FieldType.DateTime)
        {
            return ConvertStringToDateTime((string)value);
        }

        if (fromType == FieldType.String && toType == FieldType.Date)
        {
            return ConvertStringToDate((string)value);
        }

        // Integer conversions
        if (fromType == FieldType.Integer && toType == FieldType.String)
        {
            return ((int)value).ToString(CultureInfo.InvariantCulture);
        }

        if (fromType == FieldType.Integer && toType == FieldType.Float)
        {
            return (double)(int)value;
        }

        if (fromType == FieldType.Integer && toType == FieldType.Boolean)
        {
            return (int)value != 0;
        }

        // Float conversions
        if (fromType == FieldType.Float && toType == FieldType.String)
        {
            return ((double)value).ToString(CultureInfo.InvariantCulture);
        }

        if (fromType == FieldType.Float && toType == FieldType.Integer)
        {
            return (int)(double)value; // Truncates
        }

        // Boolean conversions
        if (fromType == FieldType.Boolean && toType == FieldType.String)
        {
            return ((bool)value).ToString();
        }

        // DateTime conversions
        if (fromType == FieldType.DateTime && toType == FieldType.String)
        {
            var dt = (DateTime)value;
            return dt.ToString("O", CultureInfo.InvariantCulture); // ISO 8601 format
        }

        // Date conversions (same as DateTime internally)
        if (fromType == FieldType.Date && toType == FieldType.String)
        {
            var dt = (DateTime)value;
            return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        // Unsupported conversion - throw exception
        throw new ConversionException(
            string.Empty,
            value,
            fromType,
            toType,
            $"Conversion from {fromType} to {toType} is not supported");
    }

    private int ConvertStringToInteger(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ConversionException(
                string.Empty,
                value,
                FieldType.String,
                FieldType.Integer,
                "Cannot convert empty or whitespace string to integer");
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new ConversionException(
            string.Empty,
            value,
            FieldType.String,
            FieldType.Integer,
            $"Cannot convert string '{value}' to integer");
    }

    private double ConvertStringToFloat(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ConversionException(
                string.Empty,
                value,
                FieldType.String,
                FieldType.Float,
                "Cannot convert empty or whitespace string to float");
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new ConversionException(
            string.Empty,
            value,
            FieldType.String,
            FieldType.Float,
            $"Cannot convert string '{value}' to float");
    }

    private bool ConvertStringToBoolean(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var lowerValue = value.ToLowerInvariant().Trim();

        // True variants
        if (lowerValue == "true" || lowerValue == "1" || lowerValue == "yes")
        {
            return true;
        }

        // False variants
        if (lowerValue == "false" || lowerValue == "0" || lowerValue == "no" || lowerValue == "")
        {
            return false;
        }

        // Default to false for unrecognized values
        return false;
    }

    private DateTime ConvertStringToDateTime(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ConversionException(
                string.Empty,
                value,
                FieldType.String,
                FieldType.DateTime,
                "Cannot convert empty or whitespace string to DateTime");
        }

        // Try ISO 8601 format first
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
        {
            return dt;
        }

        // Try common formats
        var formats = new[]
        {
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
            {
                return dt;
            }
        }

        throw new ConversionException(
            string.Empty,
            value,
            FieldType.String,
            FieldType.DateTime,
            $"Cannot convert string '{value}' to DateTime");
    }

    private DateTime ConvertStringToDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ConversionException(
                string.Empty,
                value,
                FieldType.String,
                FieldType.Date,
                "Cannot convert empty or whitespace string to Date");
        }

        // Try date-only formats
        var formats = new[]
        {
            "yyyy-MM-dd",
            "yyyy/MM/dd",
            "MM/dd/yyyy",
            "dd/MM/yyyy"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt.Date; // Return date at midnight
            }
        }

        // Fall back to general parsing
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date.Date;
        }

        throw new ConversionException(
            string.Empty,
            value,
            FieldType.String,
            FieldType.Date,
            $"Cannot convert string '{value}' to Date");
    }

    private object HandleConversionFailure(
        object value,
        FieldType fromType,
        FieldType toType,
        ConversionStrategy strategy,
        Exception ex)
    {
        switch (strategy)
        {
            case ConversionStrategy.Cast:
            case ConversionStrategy.FailOnError:
                // Re-throw as ConversionException if it's not already one
                if (ex is ConversionException)
                {
                    throw ex;
                }

                throw new ConversionException(
                    string.Empty,
                    value,
                    fromType,
                    toType,
                    $"Failed to convert from {fromType} to {toType}",
                    ex);

            case ConversionStrategy.SetNull:
                _logger?.LogDebug("Conversion strategy is SetNull, returning null for failed conversion");
                return null!;

            case ConversionStrategy.Truncate:
                // For truncate, attempt to handle gracefully where possible
                // For most types, if we can't convert, we can't truncate either
                // So we'll fall through to SetNull behavior
                if (ex is ConversionException)
                {
                    throw ex;
                }

                throw new ConversionException(
                    string.Empty,
                    value,
                    fromType,
                    toType,
                    $"Cannot truncate value from {fromType} to {toType}",
                    ex);

            default:
                throw new ConversionException(
                    string.Empty,
                    value,
                    fromType,
                    toType,
                    $"Unknown conversion strategy: {strategy}");
        }
    }
}

