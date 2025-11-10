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

    // ============================================
    // Task 2.2.1: Record Edge Cases
    // ============================================

    [Fact]
    public void Record_WithNullData_HandlesGracefully()
    {
        // Arrange
        var record = new Record
        {
            Id = "test-id",
            Data = null!
        };

        // Assert - Data can be set to null (though not recommended)
        Assert.Equal("test-id", record.Id);
        Assert.Null(record.Data);
    }

    [Fact]
    public void Record_WithEmptyId_IsValid()
    {
        // Arrange
        var record = new Record
        {
            Id = string.Empty,
            Data = new Dictionary<string, object> { { "name", "Test" } }
        };

        // Assert - Empty ID is valid (default value)
        Assert.Equal(string.Empty, record.Id);
        Assert.NotNull(record.Data);
        Assert.Equal("Test", record.Data["name"]);
    }

    [Fact]
    public void Record_WithSpecialCharactersInId_IsValid()
    {
        // Arrange
        var record = new Record
        {
            Id = "id-with-special-chars-123!@#$%^&*()",
            Data = new Dictionary<string, object> { { "name", "Test" } }
        };

        // Assert - Special characters in ID are valid
        Assert.Equal("id-with-special-chars-123!@#$%^&*()", record.Id);
        Assert.NotNull(record.Data);
    }
}

