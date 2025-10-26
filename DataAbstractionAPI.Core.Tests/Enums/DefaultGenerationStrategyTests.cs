namespace DataAbstractionAPI.Core.Tests.Enums;

using DataAbstractionAPI.Core.Enums;

public class DefaultGenerationStrategyTests
{
    [Fact]
    public void DefaultGenerationStrategy_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)DefaultGenerationStrategy.UserSpecified);
        Assert.Equal(1, (int)DefaultGenerationStrategy.PatternMatch);
        Assert.Equal(2, (int)DefaultGenerationStrategy.ContextAnalysis);
        Assert.Equal(3, (int)DefaultGenerationStrategy.TypeBased);
    }

    [Fact]
    public void DefaultGenerationStrategy_ToString_ReturnsEnumName()
    {
        // Arrange
        var userSpecified = DefaultGenerationStrategy.UserSpecified;
        var contextAnalysis = DefaultGenerationStrategy.ContextAnalysis;

        // Act
        var userString = userSpecified.ToString();
        var contextString = contextAnalysis.ToString();

        // Assert
        Assert.Equal("UserSpecified", userString);
        Assert.Equal("ContextAnalysis", contextString);
    }

    [Fact]
    public void DefaultGenerationStrategy_CanBeParsed_FromString()
    {
        // Arrange
        var patternMatchString = "PatternMatch";
        var typeBasedString = "TypeBased";

        // Act
        var patternParsed = Enum.Parse<DefaultGenerationStrategy>(patternMatchString);
        var typeParsed = Enum.Parse<DefaultGenerationStrategy>(typeBasedString);

        // Assert
        Assert.Equal(DefaultGenerationStrategy.PatternMatch, patternParsed);
        Assert.Equal(DefaultGenerationStrategy.TypeBased, typeParsed);
    }
}

