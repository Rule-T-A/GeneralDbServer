namespace DataAbstractionAPI.Core.Tests.Exceptions;

using DataAbstractionAPI.Core.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void ValidationException_Initializes_WithFieldName()
    {
        // Arrange
        var fieldName = "email";

        // Act
        var exception = new ValidationException(fieldName);

        // Assert
        Assert.Equal(fieldName, exception.FieldName);
        Assert.NotNull(exception.Message);
        Assert.Contains("email", exception.Message);
    }

    [Fact]
    public void ValidationException_Initializes_WithCustomMessage()
    {
        // Arrange
        var fieldName = "username";
        var customMessage = "Username must be at least 3 characters";

        // Act
        var exception = new ValidationException(fieldName, customMessage);

        // Assert
        Assert.Equal(fieldName, exception.FieldName);
        Assert.Equal(customMessage, exception.Message);
    }

    [Fact]
    public void ValidationException_CanWrap_InnerException()
    {
        // Arrange
        var fieldName = "password";
        var message = "Password validation failed";
        var innerException = new Exception("Inner validation error");

        // Act
        var exception = new ValidationException(fieldName, message, innerException);

        // Assert
        Assert.Equal(fieldName, exception.FieldName);
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void ValidationException_Message_ContainsFieldName()
    {
        // Arrange
        var fieldName = "age";

        // Act
        var exception = new ValidationException(fieldName);

        // Assert
        Assert.Contains(fieldName, exception.Message);
        Assert.Contains("Validation failed", exception.Message);
    }
}

