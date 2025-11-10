namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class BulkResultTests
{
    [Fact]
    public void BulkResult_Initializes_WithDefaults()
    {
        // Arrange & Act
        var result = new BulkResult();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(0, result.Succeeded);
        Assert.Equal(0, result.Failed);
        Assert.NotNull(result.Results);
        Assert.Empty(result.Results);
        Assert.Null(result.Ids);
        Assert.Null(result.Error);
        Assert.Null(result.FailedIndex);
        Assert.Null(result.FailedError);
    }

    [Fact]
    public void BulkResult_CanSetSuccessFlag()
    {
        // Arrange
        var result = new BulkResult
        {
            Success = true
        };

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void BulkResult_CanSetIdsArray()
    {
        // Arrange
        var result = new BulkResult
        {
            Ids = new List<string> { "id1", "id2", "id3" }
        };

        // Assert
        Assert.NotNull(result.Ids);
        Assert.Equal(3, result.Ids.Count);
        Assert.Equal("id1", result.Ids[0]);
        Assert.Equal("id2", result.Ids[1]);
        Assert.Equal("id3", result.Ids[2]);
    }

    [Fact]
    public void BulkResult_CanSetResultsList()
    {
        // Arrange
        var result = new BulkResult
        {
            Results = new List<BulkOperationItemResult>
            {
                new() { Index = 0, Success = true, Id = "id1" },
                new() { Index = 1, Success = false, Error = "Error message" }
            }
        };

        // Assert
        Assert.NotNull(result.Results);
        Assert.Equal(2, result.Results.Count);
        Assert.True(result.Results[0].Success);
        Assert.False(result.Results[1].Success);
    }

    [Fact]
    public void BulkResult_CanSetErrorMessage()
    {
        // Arrange
        var result = new BulkResult
        {
            Error = "Transaction failed"
        };

        // Assert
        Assert.Equal("Transaction failed", result.Error);
    }

    [Fact]
    public void BulkResult_WithAtomicSuccess_WorksCorrectly()
    {
        // Arrange
        var result = new BulkResult
        {
            Success = true,
            Succeeded = 3,
            Failed = 0,
            Ids = new List<string> { "id1", "id2", "id3" }
        };

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.Succeeded);
        Assert.Equal(0, result.Failed);
        Assert.NotNull(result.Ids);
        Assert.Equal(3, result.Ids.Count);
    }

    [Fact]
    public void BulkResult_WithAtomicFailure_WorksCorrectly()
    {
        // Arrange
        var result = new BulkResult
        {
            Success = false,
            Succeeded = 0,
            Failed = 3,
            Error = "Transaction rolled back",
            FailedIndex = 2,
            FailedError = "Record validation failed"
        };

        // Assert
        Assert.False(result.Success);
        Assert.Equal(0, result.Succeeded);
        Assert.Equal(3, result.Failed);
        Assert.Equal("Transaction rolled back", result.Error);
        Assert.Equal(2, result.FailedIndex);
        Assert.Equal("Record validation failed", result.FailedError);
    }

    [Fact]
    public void BulkResult_WithBestEffortMode_WorksCorrectly()
    {
        // Arrange
        var result = new BulkResult
        {
            Success = true, // At least one succeeded
            Succeeded = 2,
            Failed = 1,
            Results = new List<BulkOperationItemResult>
            {
                new() { Index = 0, Success = true, Id = "id1" },
                new() { Index = 1, Success = true, Id = "id2" },
                new() { Index = 2, Success = false, Error = "Failed" }
            }
        };

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Succeeded);
        Assert.Equal(1, result.Failed);
        Assert.Equal(3, result.Results.Count);
    }
}

