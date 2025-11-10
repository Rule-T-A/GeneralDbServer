namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class AggregateResultTests
{
    [Fact]
    public void AggregateResult_Initializes_WithDefaults()
    {
        // Arrange & Act
        var result = new AggregateResult();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public void AggregateResult_CanSetDataDictionary()
    {
        // Arrange
        var result = new AggregateResult
        {
            Data = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "category", "Electronics" },
                    { "total_price", 1000 },
                    { "count", 5 }
                }
            }
        };

        // Assert
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Electronics", result.Data[0]["category"]);
        Assert.Equal(1000, result.Data[0]["total_price"]);
        Assert.Equal(5, result.Data[0]["count"]);
    }

    [Fact]
    public void AggregateResult_WithComplexData_WorksCorrectly()
    {
        // Arrange
        var result = new AggregateResult
        {
            Data = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "category", "Electronics" },
                    { "status", "active" },
                    { "total_price", 1000.50 },
                    { "avg_price", 200.10 },
                    { "count", 5 }
                },
                new Dictionary<string, object>
                {
                    { "category", "Books" },
                    { "status", "active" },
                    { "total_price", 250.75 },
                    { "avg_price", 50.15 },
                    { "count", 5 }
                }
            }
        };

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("Electronics", result.Data[0]["category"]);
        Assert.Equal("Books", result.Data[1]["category"]);
        Assert.Equal(1000.50, result.Data[0]["total_price"]);
        Assert.Equal(250.75, result.Data[1]["total_price"]);
    }
}

