namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class BulkOperationItemResultTests
{
    [Fact]
    public void BulkOperationItemResult_Initializes_WithDefaults()
    {
        // Arrange & Act
        var result = new BulkOperationItemResult();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Index);
        Assert.Null(result.Id);
        Assert.False(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public void BulkOperationItemResult_CanSetSuccessFlag()
    {
        // Arrange
        var result = new BulkOperationItemResult
        {
            Success = true
        };

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void BulkOperationItemResult_CanSetId()
    {
        // Arrange
        var result = new BulkOperationItemResult
        {
            Id = "12345"
        };

        // Assert
        Assert.Equal("12345", result.Id);
    }

    [Fact]
    public void BulkOperationItemResult_CanSetErrorMessage()
    {
        // Arrange
        var result = new BulkOperationItemResult
        {
            Error = "Record not found"
        };

        // Assert
        Assert.Equal("Record not found", result.Error);
    }

    [Fact]
    public void BulkOperationItemResult_WithAllProperties_WorksCorrectly()
    {
        // Arrange
        var successResult = new BulkOperationItemResult
        {
            Index = 0,
            Id = "new-id-123",
            Success = true,
            Error = null
        };

        var failureResult = new BulkOperationItemResult
        {
            Index = 1,
            Id = null,
            Success = false,
            Error = "Validation failed"
        };

        // Assert - Success case
        Assert.Equal(0, successResult.Index);
        Assert.Equal("new-id-123", successResult.Id);
        Assert.True(successResult.Success);
        Assert.Null(successResult.Error);

        // Assert - Failure case
        Assert.Equal(1, failureResult.Index);
        Assert.Null(failureResult.Id);
        Assert.False(failureResult.Success);
        Assert.Equal("Validation failed", failureResult.Error);
    }
}

