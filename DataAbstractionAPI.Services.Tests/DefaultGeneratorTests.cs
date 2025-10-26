namespace DataAbstractionAPI.Services.Tests;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;
using DataAbstractionAPI.Services;

public class DefaultGeneratorTests
{
    private readonly IDefaultGenerator _generator;

    public DefaultGeneratorTests()
    {
        _generator = new DefaultGenerator();
    }

    [Fact]
    public void DefaultGenerator_ForBooleanFields_WithIsPrefix_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("is_active", FieldType.Boolean, context);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void DefaultGenerator_ForDateTimeFields_WithAtSuffix_ReturnsCurrentTimestamp()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var beforeGenerate = DateTime.UtcNow;

        // Act
        var result = (DateTime)_generator.GenerateDefault("created_at", FieldType.DateTime, context);
        var afterGenerate = DateTime.UtcNow;

        // Assert
        Assert.True(result >= beforeGenerate && result <= afterGenerate);
        Assert.NotEqual(default(DateTime), result);
    }

    [Fact]
    public void DefaultGenerator_ForIdFields_ReturnsNull()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("user_id", FieldType.String, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DefaultGenerator_ForCountFields_ReturnsZero()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("total_count", FieldType.Integer, context);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void DefaultGenerator_DetermineStrategy_ForPatternMatch_ReturnsPatternMatch()
    {
        // Arrange & Act
        var strategy1 = _generator.DetermineStrategy("is_enabled", FieldType.Boolean);
        var strategy2 = _generator.DetermineStrategy("created_at", FieldType.DateTime);
        var strategy3 = _generator.DetermineStrategy("user_id", FieldType.String);

        // Assert
        Assert.Equal(DefaultGenerationStrategy.PatternMatch, strategy1);
        Assert.Equal(DefaultGenerationStrategy.PatternMatch, strategy2);
        Assert.Equal(DefaultGenerationStrategy.PatternMatch, strategy3);
    }

    [Fact]
    public void DefaultGenerator_DetermineStrategy_ForTypeBased_ReturnsTypeBased()
    {
        // Arrange & Act
        var strategy1 = _generator.DetermineStrategy("name", FieldType.String);
        var strategy2 = _generator.DetermineStrategy("age", FieldType.Integer);
        var strategy3 = _generator.DetermineStrategy("price", FieldType.Float);

        // Assert
        Assert.Equal(DefaultGenerationStrategy.TypeBased, strategy1);
        Assert.Equal(DefaultGenerationStrategy.TypeBased, strategy2);
        Assert.Equal(DefaultGenerationStrategy.TypeBased, strategy3);
    }

    [Fact]
    public void DefaultGenerator_ForHasPrefix_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("has_permission", FieldType.Boolean, context);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void DefaultGenerator_ForCanPrefix_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("can_edit", FieldType.Boolean, context);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void DefaultGenerator_ForUpdatedAt_ReturnsCurrentTimestamp()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var beforeGenerate = DateTime.UtcNow;

        // Act
        var result = (DateTime)_generator.GenerateDefault("updated_at", FieldType.DateTime, context);

        // Assert
        Assert.True(result >= beforeGenerate && result <= DateTime.UtcNow);
    }

    [Fact]
    public void DefaultGenerator_ForTotalField_ReturnsZero()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("grand_total", FieldType.Integer, context);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void DefaultGenerator_ForPrimaryKey_ReturnsNull()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("product_key", FieldType.String, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DefaultGenerator_ForGenericStringField_ReturnsEmptyString()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("name", FieldType.String, context);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void DefaultGenerator_ForGenericFloatField_ReturnsZero()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("price", FieldType.Float, context);

        // Assert
        Assert.Equal(0.0, result);
    }
}

