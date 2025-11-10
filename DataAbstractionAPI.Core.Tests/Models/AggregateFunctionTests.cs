namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class AggregateFunctionTests
{
    [Fact]
    public void AggregateFunction_Initializes_WithDefaults()
    {
        // Arrange & Act
        var aggregateFunction = new AggregateFunction();

        // Assert
        Assert.NotNull(aggregateFunction);
        Assert.Equal(string.Empty, aggregateFunction.Field);
        Assert.Equal(string.Empty, aggregateFunction.Function);
        Assert.Equal(string.Empty, aggregateFunction.Alias);
    }

    [Fact]
    public void AggregateFunction_CanSetAllProperties()
    {
        // Arrange
        var aggregateFunction = new AggregateFunction
        {
            Field = "price",
            Function = "sum",
            Alias = "total_price"
        };

        // Assert
        Assert.Equal("price", aggregateFunction.Field);
        Assert.Equal("sum", aggregateFunction.Function);
        Assert.Equal("total_price", aggregateFunction.Alias);
    }

    [Fact]
    public void AggregateFunction_WithAllFunctions_WorksCorrectly()
    {
        // Arrange & Act
        var countFunction = new AggregateFunction { Field = "id", Function = "count", Alias = "count" };
        var sumFunction = new AggregateFunction { Field = "price", Function = "sum", Alias = "total" };
        var avgFunction = new AggregateFunction { Field = "price", Function = "avg", Alias = "average" };
        var minFunction = new AggregateFunction { Field = "price", Function = "min", Alias = "minimum" };
        var maxFunction = new AggregateFunction { Field = "price", Function = "max", Alias = "maximum" };

        // Assert
        Assert.Equal("count", countFunction.Function);
        Assert.Equal("sum", sumFunction.Function);
        Assert.Equal("avg", avgFunction.Function);
        Assert.Equal("min", minFunction.Function);
        Assert.Equal("max", maxFunction.Function);
    }

    [Fact]
    public void AggregateFunction_WithEmptyStrings_IsValid()
    {
        // Arrange
        var aggregateFunction = new AggregateFunction
        {
            Field = string.Empty,
            Function = string.Empty,
            Alias = string.Empty
        };

        // Assert - Empty strings are valid (default values)
        Assert.Equal(string.Empty, aggregateFunction.Field);
        Assert.Equal(string.Empty, aggregateFunction.Function);
        Assert.Equal(string.Empty, aggregateFunction.Alias);
    }
}

