namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class ListResultTests
{
    [Fact]
    public void ListResult_Initializes_WithDefaults()
    {
        // Arrange & Act
        var result = new ListResult();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Total);
        Assert.False(result.More);
        Assert.Empty(result.Data);
    }

    [Fact]
    public void ListResult_CanStoreDataList()
    {
        // Arrange
        var result = new ListResult
        {
            Data = new List<Record>
            {
                new Record { Id = "1", Data = new Dictionary<string, object> { { "name", "Alice" } } },
                new Record { Id = "2", Data = new Dictionary<string, object> { { "name", "Bob" } } }
            },
            Total = 2,
            More = false
        };

        // Assert
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Total);
        Assert.False(result.More);
        Assert.Equal("Alice", result.Data[0].Data["name"]);
    }

    // ============================================
    // Task 2.2.3: ListResult Edge Cases
    // ============================================

    [Fact]
    public void ListResult_WithEmptyData_IsValid()
    {
        // Arrange
        var result = new ListResult
        {
            Data = new List<Record>(),
            Total = 0,
            More = false
        };

        // Assert
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Total);
        Assert.False(result.More);
    }

    [Fact]
    public void ListResult_WithMoreFlagEdgeCases_CalculatesCorrectly()
    {
        // Arrange & Act - Test case 1: More should be true when offset + limit < total
        var result1 = new ListResult
        {
            Data = new List<Record> { new Record { Id = "1", Data = new Dictionary<string, object>() } },
            Total = 100,
            More = true
        };

        // Assert
        Assert.True(result1.More);
        Assert.Equal(100, result1.Total);
        Assert.Single(result1.Data);

        // Arrange & Act - Test case 2: More should be false when offset + limit >= total
        var result2 = new ListResult
        {
            Data = new List<Record>
            {
                new Record { Id = "1", Data = new Dictionary<string, object>() },
                new Record { Id = "2", Data = new Dictionary<string, object>() }
            },
            Total = 2,
            More = false
        };

        // Assert
        Assert.False(result2.More);
        Assert.Equal(2, result2.Total);
        Assert.Equal(2, result2.Data.Count);

        // Arrange & Act - Test case 3: More flag with offset scenario
        var result3 = new ListResult
        {
            Data = new List<Record> { new Record { Id = "10", Data = new Dictionary<string, object>() } },
            Total = 50,
            More = true // offset=9, limit=1, so 9+1=10 < 50, more should be true
        };

        // Assert
        Assert.True(result3.More);
        Assert.Equal(50, result3.Total);
    }

    [Fact]
    public void ListResult_WithTotalZero_IsValid()
    {
        // Arrange
        var result = new ListResult
        {
            Data = new List<Record>(),
            Total = 0,
            More = false
        };

        // Assert
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Data);
        Assert.False(result.More);
    }
}

