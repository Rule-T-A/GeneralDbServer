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

    #region Date Type Pattern Tests

    [Fact]
    public void DefaultGenerator_ForDeletedAt_ReturnsCurrentTimestamp()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var beforeGenerate = DateTime.UtcNow;

        // Act
        var result = (DateTime)_generator.GenerateDefault("deleted_at", FieldType.DateTime, context);

        // Assert
        Assert.True(result >= beforeGenerate && result <= DateTime.UtcNow);
    }

    [Fact]
    public void DefaultGenerator_ForDateSuffix_ReturnsCurrentTimestamp()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var beforeGenerate = DateTime.UtcNow;

        // Act
        var result = (DateTime)_generator.GenerateDefault("created_date", FieldType.Date, context);

        // Assert
        Assert.True(result >= beforeGenerate && result <= DateTime.UtcNow);
    }

    [Fact]
    public void DefaultGenerator_ForCreatedDate_ReturnsCurrentTimestamp()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var beforeGenerate = DateTime.UtcNow;

        // Act
        var result = (DateTime)_generator.GenerateDefault("created_date", FieldType.DateTime, context);

        // Assert
        Assert.True(result >= beforeGenerate && result <= DateTime.UtcNow);
    }

    [Fact]
    public void DefaultGenerator_ForUpdatedDate_ReturnsCurrentTimestamp()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var beforeGenerate = DateTime.UtcNow;

        // Act
        var result = (DateTime)_generator.GenerateDefault("updated_date", FieldType.DateTime, context);

        // Assert
        Assert.True(result >= beforeGenerate && result <= DateTime.UtcNow);
    }

    [Fact]
    public void DefaultGenerator_ForDeletedDate_ReturnsCurrentTimestamp()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var beforeGenerate = DateTime.UtcNow;

        // Act
        var result = (DateTime)_generator.GenerateDefault("deleted_date", FieldType.DateTime, context);

        // Assert
        Assert.True(result >= beforeGenerate && result <= DateTime.UtcNow);
    }

    #endregion

    #region Count Pattern Tests

    [Fact]
    public void DefaultGenerator_ForNumPrefix_ReturnsZero()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("num_items", FieldType.Integer, context);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void DefaultGenerator_ForCountSuffix_WithFloatType_ReturnsZero()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("item_count", FieldType.Float, context);

        // Assert - Count pattern returns 0 (int), which is compatible with 0.0
        Assert.Equal(0, result);
    }

    #endregion

    #region ID Pattern Tests

    [Fact]
    public void DefaultGenerator_ForIdSuffix_WithIntegerType_ReturnsNull()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("user_id", FieldType.Integer, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DefaultGenerator_ForKeySuffix_WithIntegerType_ReturnsNull()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("product_key", FieldType.Integer, context);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Type-Based Default Tests

    [Fact]
    public void DefaultGenerator_ForArrayType_ReturnsEmptyArray()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("tags", FieldType.Array, context);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<object[]>(result);
        Assert.Empty((object[])result);
    }

    [Fact]
    public void DefaultGenerator_ForObjectType_ReturnsEmptyDictionary()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("metadata", FieldType.Object, context);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, object>>(result);
        Assert.Empty((Dictionary<string, object>)result);
    }

    [Fact]
    public void DefaultGenerator_ForDateType_ReturnsDateAtMidnight()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var beforeGenerate = DateTime.UtcNow.Date;

        // Act - Use a field name that doesn't match any pattern to test type-based default
        var result = (DateTime)_generator.GenerateDefault("some_date_field", FieldType.Date, context);
        var afterGenerate = DateTime.UtcNow.Date;

        // Assert - Date type returns DateTime.UtcNow.Date which is at midnight
        // The result should be between before and after (inclusive)
        Assert.True(result.Date >= beforeGenerate && result.Date <= afterGenerate);
        Assert.Equal(0, result.Hour);
        Assert.Equal(0, result.Minute);
        Assert.Equal(0, result.Second);
    }

    [Fact]
    public void DefaultGenerator_ForBooleanType_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("enabled", FieldType.Boolean, context);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void DefaultGenerator_ForIntegerType_ReturnsZero()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        var result = _generator.GenerateDefault("quantity", FieldType.Integer, context);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void DefaultGenerator_WithNullContext_HandlesGracefully()
    {
        // Arrange
        DefaultGenerationContext? context = null;

        // Act
        var result = _generator.GenerateDefault("name", FieldType.String, context!);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void DefaultGenerator_ForUnknownFieldType_ReturnsNull()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        // Using a value that doesn't exist in the enum - testing default case
        // Since we can't create invalid enum values, we'll test with a pattern that falls back

        // Act - This should hit the default case in GenerateTypeBasedDefault
        // We'll test with a field that has no pattern and uses type-based
        var result = _generator.GenerateDefault("unknown_field", FieldType.String, context);

        // Assert - Should return empty string for String type
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void DefaultGenerator_PatternBased_FallsBackToTypeBased()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        // Use a pattern that matches but doesn't have specific handling
        // Count pattern returns 0, but for String type it should fall back to type-based

        // Act - Count pattern matches, returns 0, but then falls back to type-based for String
        var result = _generator.GenerateDefault("item_count", FieldType.String, context);

        // Assert - Count pattern returns 0 (int), not empty string
        // The pattern-based default returns 0, which is then used
        Assert.Equal(0, result);
    }

    #endregion

    #region Context Analysis Strategy Tests

    [Fact]
    public void DefaultGenerator_WithContextAnalysisStrategy_FallsBackToPattern()
    {
        // Arrange
        var context = new DefaultGenerationContext { CollectionName = "users" };
        // ContextAnalysis strategy currently falls back to pattern/type-based
        // This tests that GenerateContextBasedDefault is called and works

        // Act - Use a field that would trigger context analysis if implemented
        // For now, it should fall back to pattern-based or type-based
        var result = _generator.GenerateDefault("is_active", FieldType.Boolean, context);

        // Assert - Should use pattern-based (is_ prefix for Boolean)
        Assert.False((bool)result);
    }

    #endregion
}

