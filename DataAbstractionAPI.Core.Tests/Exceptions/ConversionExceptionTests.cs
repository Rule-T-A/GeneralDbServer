namespace DataAbstractionAPI.Core.Tests.Exceptions;

using DataAbstractionAPI.Core.Exceptions;
using DataAbstractionAPI.Core.Enums;

public class ConversionExceptionTests
{
    [Fact]
    public void ConversionException_Initializes_WithFieldNameAndValue()
    {
        // Arrange
        var fieldName = "age";
        var value = "not-a-number";
        var fromType = FieldType.String;
        var toType = FieldType.Integer;

        // Act
        var exception = new ConversionException(fieldName, value, fromType, toType);

        // Assert
        Assert.Equal(fieldName, exception.FieldName);
        Assert.Equal(value, exception.Value);
        Assert.Equal(fromType, exception.FromType);
        Assert.Equal(toType, exception.ToType);
        Assert.NotNull(exception.Message);
        Assert.Contains("age", exception.Message);
        Assert.Contains("String", exception.Message);
        Assert.Contains("Integer", exception.Message);
    }

    [Fact]
    public void ConversionException_Initializes_WithCustomMessage()
    {
        // Arrange
        var fieldName = "price";
        var value = "invalid";
        var fromType = FieldType.String;
        var toType = FieldType.Float;
        var customMessage = "Custom conversion error message";

        // Act
        var exception = new ConversionException(fieldName, value, fromType, toType, customMessage);

        // Assert
        Assert.Equal(fieldName, exception.FieldName);
        Assert.Equal(value, exception.Value);
        Assert.Equal(customMessage, exception.Message);
    }

    [Fact]
    public void ConversionException_CanWrap_InnerException()
    {
        // Arrange
        var fieldName = "id";
        var value = 123;
        var fromType = FieldType.Integer;
        var toType = FieldType.String;
        var innerException = new Exception("Inner error");

        // Act
        var exception = new ConversionException(fieldName, value, fromType, toType, "Outer error", innerException);

        // Assert
        Assert.Equal(innerException, exception.InnerException);
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public void ConversionException_Message_ContainsValue()
    {
        // Arrange
        var fieldName = "count";
        var value = "abc";

        // Act
        var exception = new ConversionException(fieldName, value, FieldType.String, FieldType.Integer);

        // Assert
        Assert.Contains(value.ToString(), exception.Message);
    }
}

