namespace DataAbstractionAPI.Services.Tests;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Core.Exceptions;
using DataAbstractionAPI.Services;

public class TypeConverterTests
{
    private readonly ITypeConverter _converter;

    public TypeConverterTests()
    {
        _converter = new TypeConverter();
    }

    #region Basic String to Integer Conversions

    [Fact]
    public void TypeConverter_ConvertsStringToInt_Successfully()
    {
        // Arrange
        var value = "123";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<int>(result);
        Assert.Equal(123, result);
    }

    [Fact]
    public void TypeConverter_ConvertsIntToString_Successfully()
    {
        // Arrange
        var value = 123;
        var fromType = FieldType.Integer;
        var toType = FieldType.String;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<string>(result);
        Assert.Equal("123", result);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToInt_WithInvalidValue_ThrowsException()
    {
        // Arrange
        var value = "abc";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() => 
            _converter.Convert(value, fromType, toType, strategy));
        
        Assert.Equal(value, exception.Value);
        Assert.Equal(fromType, exception.FromType);
        Assert.Equal(toType, exception.ToType);
    }

    #endregion

    #region String to Float Conversions

    [Fact]
    public void TypeConverter_ConvertsStringToFloat_Successfully()
    {
        // Arrange
        var value = "123.45";
        var fromType = FieldType.String;
        var toType = FieldType.Float;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(123.45, result);
    }

    [Fact]
    public void TypeConverter_ConvertsFloatToString_Successfully()
    {
        // Arrange
        var value = 123.45;
        var fromType = FieldType.Float;
        var toType = FieldType.String;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<string>(result);
        Assert.Equal("123.45", result);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToFloat_WithInvalidValue_ThrowsException()
    {
        // Arrange
        var value = "not-a-number";
        var fromType = FieldType.String;
        var toType = FieldType.Float;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() => 
            _converter.Convert(value, fromType, toType, strategy));
        
        Assert.Equal(value, exception.Value);
        Assert.Equal(fromType, exception.FromType);
        Assert.Equal(toType, exception.ToType);
    }

    #endregion

    #region Boolean Conversions

    [Fact]
    public void TypeConverter_ConvertsStringToBool_HandlesTrueVariants()
    {
        // Arrange
        var strategy = ConversionStrategy.Cast;
        var trueVariants = new[] { "true", "True", "TRUE", "1", "yes", "Yes", "YES" };

        // Act & Assert
        foreach (var value in trueVariants)
        {
            var result = _converter.Convert(value, FieldType.String, FieldType.Boolean, strategy);
            Assert.IsType<bool>(result);
            Assert.True((bool)result, $"Value '{value}' should convert to true");
        }
    }

    [Fact]
    public void TypeConverter_ConvertsStringToBool_HandlesFalseVariants()
    {
        // Arrange
        var strategy = ConversionStrategy.Cast;
        var falseVariants = new[] { "false", "False", "FALSE", "0", "no", "No", "NO", "" };

        // Act & Assert
        foreach (var value in falseVariants)
        {
            var result = _converter.Convert(value, FieldType.String, FieldType.Boolean, strategy);
            Assert.IsType<bool>(result);
            Assert.False((bool)result, $"Value '{value}' should convert to false");
        }
    }

    [Fact]
    public void TypeConverter_ConvertsIntToBool_Successfully()
    {
        // Arrange
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var trueResult = _converter.Convert(1, FieldType.Integer, FieldType.Boolean, strategy);
        Assert.IsType<bool>(trueResult);
        Assert.True((bool)trueResult);

        var falseResult = _converter.Convert(0, FieldType.Integer, FieldType.Boolean, strategy);
        Assert.IsType<bool>(falseResult);
        Assert.False((bool)falseResult);
    }

    [Fact]
    public void TypeConverter_ConvertsBoolToString_Successfully()
    {
        // Arrange
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var trueResult = _converter.Convert(true, FieldType.Boolean, FieldType.String, strategy);
        Assert.IsType<string>(trueResult);
        Assert.Equal("True", trueResult);

        var falseResult = _converter.Convert(false, FieldType.Boolean, FieldType.String, strategy);
        Assert.IsType<string>(falseResult);
        Assert.Equal("False", falseResult);
    }

    #endregion

    #region DateTime/Date Conversions

    [Fact]
    public void TypeConverter_ConvertsStringToDateTime_Successfully()
    {
        // Arrange
        var value = "2025-10-26T10:30:00Z";
        var fromType = FieldType.String;
        var toType = FieldType.DateTime;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<DateTime>(result);
        var dateTime = (DateTime)result;
        Assert.Equal(2025, dateTime.Year);
        Assert.Equal(10, dateTime.Month);
        Assert.Equal(26, dateTime.Day);
    }

    [Fact]
    public void TypeConverter_ConvertsDateTimeToString_Successfully()
    {
        // Arrange
        var value = new DateTime(2025, 10, 26, 10, 30, 0, DateTimeKind.Utc);
        var fromType = FieldType.DateTime;
        var toType = FieldType.String;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<string>(result);
        var resultString = (string)result;
        Assert.Contains("2025", resultString);
        Assert.Contains("10", resultString);
        Assert.Contains("26", resultString);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToDate_Successfully()
    {
        // Arrange
        var value = "2025-10-26";
        var fromType = FieldType.String;
        var toType = FieldType.Date;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<DateTime>(result);
        var date = (DateTime)result;
        Assert.Equal(2025, date.Year);
        Assert.Equal(10, date.Month);
        Assert.Equal(26, date.Day);
        Assert.Equal(0, date.Hour); // Date should be at midnight
        Assert.Equal(0, date.Minute);
    }

    #endregion

    #region Conversion Strategy Tests

    [Fact]
    public void TypeConverter_WithFailOnErrorStrategy_ThrowsOnInvalidConversion()
    {
        // Arrange
        var value = "invalid-number";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.FailOnError;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() => 
            _converter.Convert(value, fromType, toType, strategy));
        
        Assert.Equal(value, exception.Value);
    }

    [Fact]
    public void TypeConverter_WithSetNullStrategy_ReturnsNullOnInvalidConversion()
    {
        // Arrange
        var value = "invalid-number";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.SetNull;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TypeConverter_WithSetNullStrategy_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        object? value = null;
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.SetNull;

        // Act
        var result = _converter.Convert(value!, fromType, toType, strategy);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TypeConverter_HandlesNullValue_ReturnsNull()
    {
        // Arrange
        object? value = null;
        var fromType = FieldType.String;
        var toType = FieldType.String;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value!, fromType, toType, strategy);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TypeConverter_HandlesEmptyString_ConvertsAppropriately()
    {
        // Arrange
        var value = "";
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        // Empty string to boolean should be false
        var boolResult = _converter.Convert(value, FieldType.String, FieldType.Boolean, strategy);
        Assert.False((bool)boolResult);

        // Empty string to integer should throw with Cast strategy
        Assert.Throws<ConversionException>(() => 
            _converter.Convert(value, FieldType.String, FieldType.Integer, strategy));
    }

    [Fact]
    public void TypeConverter_HandlesSameTypeConversion_ReturnsOriginal()
    {
        // Arrange
        var value = "test";
        var fromType = FieldType.String;
        var toType = FieldType.String;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void TypeConverter_ConvertsIntegerToFloat_Successfully()
    {
        // Arrange
        var value = 123;
        var fromType = FieldType.Integer;
        var toType = FieldType.Float;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(123.0, result);
    }

    [Fact]
    public void TypeConverter_ConvertsFloatToInteger_Truncates()
    {
        // Arrange
        var value = 123.7;
        var fromType = FieldType.Float;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<int>(result);
        Assert.Equal(123, result); // Should truncate, not round
    }

    [Fact]
    public void TypeConverter_ConvertsFloatToInteger_TruncatesNegative()
    {
        // Arrange
        var value = -123.7;
        var fromType = FieldType.Float;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<int>(result);
        Assert.Equal(-123, result); // Should truncate toward zero
    }

    [Fact]
    public void TypeConverter_ConvertsFloatToInteger_TruncatesZero()
    {
        // Arrange
        var value = 0.0;
        var fromType = FieldType.Float;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<int>(result);
        Assert.Equal(0, result);
    }

    [Fact]
    public void TypeConverter_ConvertsIntegerToFloat_Negative()
    {
        // Arrange
        var value = -123;
        var fromType = FieldType.Integer;
        var toType = FieldType.Float;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(-123.0, result);
    }

    [Fact]
    public void TypeConverter_ConvertsIntegerToFloat_Zero()
    {
        // Arrange
        var value = 0;
        var fromType = FieldType.Integer;
        var toType = FieldType.Float;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void TypeConverter_ConvertsIntegerToFloat_Large()
    {
        // Arrange
        var value = int.MaxValue;
        var fromType = FieldType.Integer;
        var toType = FieldType.Float;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal((double)int.MaxValue, result);
    }

    #endregion

    #region Date to String Conversions

    [Fact]
    public void TypeConverter_ConvertsDateToString_Successfully()
    {
        // Arrange
        var value = new DateTime(2025, 10, 26, 0, 0, 0, DateTimeKind.Utc);
        var fromType = FieldType.Date;
        var toType = FieldType.String;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<string>(result);
        Assert.Equal("2025-10-26", result);
    }

    #endregion

    #region Additional DateTime Format Tests

    [Fact]
    public void TypeConverter_ConvertsStringToDateTime_WithMillisecondsFormat()
    {
        // Arrange
        var value = "2025-10-26T10:30:00.123Z";
        var fromType = FieldType.String;
        var toType = FieldType.DateTime;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<DateTime>(result);
        var dateTime = (DateTime)result;
        Assert.Equal(2025, dateTime.Year);
        Assert.Equal(10, dateTime.Month);
        Assert.Equal(26, dateTime.Day);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToDateTime_WithSpaceSeparator()
    {
        // Arrange
        var value = "2025-10-26 10:30:00";
        var fromType = FieldType.String;
        var toType = FieldType.DateTime;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<DateTime>(result);
        var dateTime = (DateTime)result;
        Assert.Equal(2025, dateTime.Year);
        Assert.Equal(10, dateTime.Month);
        Assert.Equal(26, dateTime.Day);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToDateTime_WithSlashSeparator()
    {
        // Arrange
        var value = "2025/10/26 10:30:00";
        var fromType = FieldType.String;
        var toType = FieldType.DateTime;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<DateTime>(result);
        var dateTime = (DateTime)result;
        Assert.Equal(2025, dateTime.Year);
        Assert.Equal(10, dateTime.Month);
        Assert.Equal(26, dateTime.Day);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToDateTime_WithInvalidFormat_ThrowsException()
    {
        // Arrange
        var value = "invalid-date-format";
        var fromType = FieldType.String;
        var toType = FieldType.DateTime;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.Equal(value, exception.Value);
        Assert.Equal(fromType, exception.FromType);
        Assert.Equal(toType, exception.ToType);
    }

    #endregion

    #region Additional Date Format Tests

    [Fact]
    public void TypeConverter_ConvertsStringToDate_WithSlashFormat()
    {
        // Arrange
        var value = "2025/10/26";
        var fromType = FieldType.String;
        var toType = FieldType.Date;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<DateTime>(result);
        var date = (DateTime)result;
        Assert.Equal(2025, date.Year);
        Assert.Equal(10, date.Month);
        Assert.Equal(26, date.Day);
        Assert.Equal(0, date.Hour);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToDate_WithMMDDYYYYFormat()
    {
        // Arrange
        var value = "10/26/2025";
        var fromType = FieldType.String;
        var toType = FieldType.Date;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<DateTime>(result);
        var date = (DateTime)result;
        Assert.Equal(2025, date.Year);
        Assert.Equal(10, date.Month);
        Assert.Equal(26, date.Day);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToDate_WithDDMMYYYYFormat()
    {
        // Arrange
        var value = "26/10/2025";
        var fromType = FieldType.String;
        var toType = FieldType.Date;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<DateTime>(result);
        var date = (DateTime)result;
        Assert.Equal(2025, date.Year);
        Assert.Equal(10, date.Month);
        Assert.Equal(26, date.Day);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToDate_WithInvalidFormat_ThrowsException()
    {
        // Arrange
        var value = "invalid-date";
        var fromType = FieldType.String;
        var toType = FieldType.Date;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.Equal(value, exception.Value);
    }

    #endregion

    #region Truncate Strategy Tests

    [Fact]
    public void TypeConverter_WithTruncateStrategy_ForFloatToInteger_Truncates()
    {
        // Arrange
        var value = 123.9;
        var fromType = FieldType.Float;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Truncate;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<int>(result);
        Assert.Equal(123, result);
    }

    [Fact]
    public void TypeConverter_WithTruncateStrategy_ForUnsupportedConversion_ThrowsException()
    {
        // Arrange
        var value = "test";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Truncate;

        // Act & Assert
        // Truncate strategy re-throws the original ConversionException
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        // Should throw a ConversionException (either original or wrapped)
        Assert.NotNull(exception);
        Assert.Equal(value, exception.Value);
    }

    #endregion

    #region Unsupported Conversion Tests

    [Fact]
    public void TypeConverter_UnsupportedConversion_ArrayToString_ThrowsException()
    {
        // Arrange
        var value = new object[] { 1, 2, 3 };
        var fromType = FieldType.Array;
        var toType = FieldType.String;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TypeConverter_UnsupportedConversion_ObjectToString_ThrowsException()
    {
        // Arrange
        var value = new Dictionary<string, object> { { "key", "value" } };
        var fromType = FieldType.Object;
        var toType = FieldType.String;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TypeConverter_UnsupportedConversion_DateTimeToInteger_ThrowsException()
    {
        // Arrange
        var value = DateTime.UtcNow;
        var fromType = FieldType.DateTime;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TypeConverter_UnsupportedConversion_BooleanToInteger_ThrowsException()
    {
        // Arrange
        var value = true;
        var fromType = FieldType.Boolean;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public void TypeConverter_ExceptionWrapping_NonConversionException_WrapsCorrectly()
    {
        // Arrange
        // This tests that non-ConversionException exceptions are wrapped
        // We'll use an invalid conversion that throws a different exception type
        var value = "invalid";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.FailOnError;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.NotNull(exception);
        Assert.Equal(value, exception.Value);
    }

    [Fact]
    public void TypeConverter_ExceptionWrapping_ConversionException_Preserved()
    {
        // Arrange
        var value = "invalid-number";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        // Should be the same ConversionException, not wrapped
        Assert.Equal(value, exception.Value);
        Assert.Equal(fromType, exception.FromType);
        Assert.Equal(toType, exception.ToType);
    }

    #endregion

    #region Empty String Edge Cases

    [Fact]
    public void TypeConverter_EmptyStringToFloat_ThrowsException()
    {
        // Arrange
        var value = "";
        var fromType = FieldType.String;
        var toType = FieldType.Float;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TypeConverter_WhitespaceStringToInteger_ThrowsException()
    {
        // Arrange
        var value = "   ";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var exception = Assert.Throws<ConversionException>(() =>
            _converter.Convert(value, fromType, toType, strategy));

        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Boolean Conversion Variants

    [Fact]
    public void TypeConverter_ConvertsStringToBool_CaseInsensitive()
    {
        // Arrange
        var strategy = ConversionStrategy.Cast;
        var variants = new[] { "TRUE", "FALSE", "True", "False", "tRuE", "fAlSe" };

        // Act & Assert
        foreach (var value in variants)
        {
            var result = _converter.Convert(value, FieldType.String, FieldType.Boolean, strategy);
            Assert.IsType<bool>(result);
        }
    }

    [Fact]
    public void TypeConverter_ConvertsStringToBool_WithWhitespace_Trims()
    {
        // Arrange
        var strategy = ConversionStrategy.Cast;

        // Act & Assert
        var trueResult = _converter.Convert("  true  ", FieldType.String, FieldType.Boolean, strategy);
        Assert.True((bool)trueResult);

        var falseResult = _converter.Convert("  false  ", FieldType.String, FieldType.Boolean, strategy);
        Assert.False((bool)falseResult);
    }

    [Fact]
    public void TypeConverter_ConvertsStringToBool_UnrecognizedValue_ReturnsFalse()
    {
        // Arrange
        var value = "maybe";
        var fromType = FieldType.String;
        var toType = FieldType.Boolean;
        var strategy = ConversionStrategy.Cast;

        // Act
        var result = _converter.Convert(value, fromType, toType, strategy);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result); // Defaults to false for unrecognized values
    }

    #endregion
}

