namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class SummaryResultTests
{
    [Fact]
    public void SummaryResult_Initializes_WithDefaults()
    {
        // Arrange & Act
        var result = new SummaryResult();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Counts);
        Assert.Empty(result.Counts);
    }

    [Fact]
    public void SummaryResult_CanSetCountsDictionary()
    {
        // Arrange
        var result = new SummaryResult
        {
            Counts = new Dictionary<string, int>
            {
                { "active", 10 },
                { "inactive", 5 }
            }
        };

        // Assert
        Assert.NotNull(result.Counts);
        Assert.Equal(2, result.Counts.Count);
        Assert.Equal(10, result.Counts["active"]);
        Assert.Equal(5, result.Counts["inactive"]);
    }

    [Fact]
    public void SummaryResult_WithComplexCounts_WorksCorrectly()
    {
        // Arrange
        var result = new SummaryResult
        {
            Counts = new Dictionary<string, int>
            {
                { "Electronics", 25 },
                { "Books", 15 },
                { "Clothing", 30 },
                { "null", 5 },
                { "", 2 }
            }
        };

        // Assert
        Assert.NotNull(result.Counts);
        Assert.Equal(5, result.Counts.Count);
        Assert.Equal(25, result.Counts["Electronics"]);
        Assert.Equal(15, result.Counts["Books"]);
        Assert.Equal(30, result.Counts["Clothing"]);
        Assert.Equal(5, result.Counts["null"]);
        Assert.Equal(2, result.Counts[""]);
    }

    [Fact]
    public void SummaryResult_WithZeroCounts_IsValid()
    {
        // Arrange
        var result = new SummaryResult
        {
            Counts = new Dictionary<string, int>
            {
                { "category1", 0 },
                { "category2", 0 }
            }
        };

        // Assert
        Assert.NotNull(result.Counts);
        Assert.Equal(2, result.Counts.Count);
        Assert.Equal(0, result.Counts["category1"]);
        Assert.Equal(0, result.Counts["category2"]);
    }
}

