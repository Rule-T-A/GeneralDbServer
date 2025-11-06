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

    #endregion
}

