namespace DataAbstractionAPI.Core.Tests.Enums;

using DataAbstractionAPI.Core.Enums;

public class ConversionStrategyTests
{
    [Fact]
    public void ConversionStrategy_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)ConversionStrategy.Cast);
        Assert.Equal(1, (int)ConversionStrategy.Truncate);
        Assert.Equal(2, (int)ConversionStrategy.FailOnError);
        Assert.Equal(3, (int)ConversionStrategy.SetNull);
    }

    [Fact]
    public void ConversionStrategy_ToString_ReturnsEnumName()
    {
        // Arrange
        var cast = ConversionStrategy.Cast;
        var failOnError = ConversionStrategy.FailOnError;

        // Act
        var castString = cast.ToString();
        var failString = failOnError.ToString();

        // Assert
        Assert.Equal("Cast", castString);
        Assert.Equal("FailOnError", failString);
    }

    [Fact]
    public void ConversionStrategy_CanBeParsed_FromString()
    {
        // Arrange
        var truncateString = "Truncate";
        var setNullString = "SetNull";

        // Act
        var truncateParsed = Enum.Parse<ConversionStrategy>(truncateString);
        var setNullParsed = Enum.Parse<ConversionStrategy>(setNullString);

        // Assert
        Assert.Equal(ConversionStrategy.Truncate, truncateParsed);
        Assert.Equal(ConversionStrategy.SetNull, setNullParsed);
    }
}

