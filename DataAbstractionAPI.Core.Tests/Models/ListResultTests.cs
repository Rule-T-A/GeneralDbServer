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
}

