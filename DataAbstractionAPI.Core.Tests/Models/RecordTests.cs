namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class RecordTests
{
    [Fact]
    public void Record_Initializes_WithEmptyData()
    {
        // Arrange & Act
        var record = new Record();

        // Assert
        Assert.NotNull(record);
        Assert.NotNull(record.Data);
        Assert.Equal(string.Empty, record.Id);
        Assert.Empty(record.Data);
    }

    [Fact]
    public void Record_CanStoreData_Dictionary()
    {
        // Arrange
        var record = new Record
        {
            Id = "123",
            Data = new Dictionary<string, object>
            {
                { "name", "Alice" },
                { "age", 30 }
            }
        };

        // Assert
        Assert.Equal("123", record.Id);
        Assert.Equal("Alice", record.Data["name"]);
        Assert.Equal(30, record.Data["age"]);
    }
}

