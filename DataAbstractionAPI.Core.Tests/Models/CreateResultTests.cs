namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class CreateResultTests
{
    [Fact]
    public void CreateResult_Initializes_WithDefaults()
    {
        // Arrange & Act
        var result = new CreateResult();

        // Assert
        Assert.NotNull(result.Record);
        Assert.Equal(string.Empty, result.Id);
    }

    [Fact]
    public void CreateResult_CanSetRecord_Property()
    {
        // Arrange
        var result = new CreateResult();
        var record = new Record
        {
            Id = "test-123",
            Data = new Dictionary<string, object> { { "name", "Test" } }
        };

        // Act
        result.Record = record;

        // Assert
        Assert.NotNull(result.Record);
        Assert.Equal("test-123", result.Record.Id);
    }

    [Fact]
    public void CreateResult_CanSetId_Property()
    {
        // Arrange
        var result = new CreateResult();

        // Act
        result.Id = "new-id-456";

        // Assert
        Assert.Equal("new-id-456", result.Id);
    }

    [Fact]
    public void CreateResult_CanBeCreated_WithInitialization()
    {
        // Arrange
        var record = new Record
        {
            Id = "initial-id",
            Data = new Dictionary<string, object> { { "key", "value" } }
        };

        // Act
        var result = new CreateResult
        {
            Record = record,
            Id = "initial-id"
        };

        // Assert
        Assert.NotNull(result.Record);
        Assert.Equal("initial-id", result.Id);
        Assert.Equal("initial-id", result.Record.Id);
    }

    // ============================================
    // Task 2.2.4: CreateResult Edge Cases
    // ============================================

    [Fact]
    public void CreateResult_WithNullRecord_HandlesGracefully()
    {
        // Arrange
        var result = new CreateResult
        {
            Record = null!,
            Id = "test-id"
        };

        // Assert - Record can be set to null (though not recommended)
        Assert.Null(result.Record);
        Assert.Equal("test-id", result.Id);
    }

    [Fact]
    public void CreateResult_WithEmptyId_IsValid()
    {
        // Arrange
        var result = new CreateResult
        {
            Record = new Record { Id = "record-id", Data = new Dictionary<string, object>() },
            Id = string.Empty
        };

        // Assert - Empty ID is valid (default value)
        Assert.NotNull(result.Record);
        Assert.Equal(string.Empty, result.Id);
    }
}

