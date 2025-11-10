namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class BulkOperationRequestTests
{
    [Fact]
    public void BulkOperationRequest_Initializes_WithDefaults()
    {
        // Arrange & Act
        var request = new BulkOperationRequest();

        // Assert
        Assert.NotNull(request);
        Assert.Equal(string.Empty, request.Action);
        Assert.False(request.Atomic);
        Assert.NotNull(request.Records);
        Assert.Empty(request.Records);
        Assert.Null(request.UpdateData);
    }

    [Fact]
    public void BulkOperationRequest_CanSetActionEnum()
    {
        // Arrange & Act
        var createRequest = new BulkOperationRequest { Action = "create" };
        var updateRequest = new BulkOperationRequest { Action = "update" };
        var deleteRequest = new BulkOperationRequest { Action = "delete" };

        // Assert
        Assert.Equal("create", createRequest.Action);
        Assert.Equal("update", updateRequest.Action);
        Assert.Equal("delete", deleteRequest.Action);
    }

    [Fact]
    public void BulkOperationRequest_CanSetAtomicFlag()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            Atomic = true
        };

        // Assert
        Assert.True(request.Atomic);
    }

    [Fact]
    public void BulkOperationRequest_CanSetRecordsList()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            Records = new List<Dictionary<string, object>>
            {
                new() { { "name", "Item 1" }, { "price", 100 } },
                new() { { "name", "Item 2" }, { "price", 200 } }
            }
        };

        // Assert
        Assert.NotNull(request.Records);
        Assert.Equal(2, request.Records.Count);
        Assert.Equal("Item 1", request.Records[0]["name"]);
        Assert.Equal("Item 2", request.Records[1]["name"]);
    }

    [Fact]
    public void BulkOperationRequest_CanSetUpdateData()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            UpdateData = new Dictionary<string, object>
            {
                { "status", "updated" },
                { "modified", DateTime.UtcNow }
            }
        };

        // Assert
        Assert.NotNull(request.UpdateData);
        Assert.Equal(2, request.UpdateData.Count);
        Assert.Equal("updated", request.UpdateData["status"]);
    }

    [Fact]
    public void BulkOperationRequest_WithAllProperties_WorksCorrectly()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            Action = "update",
            Atomic = true,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "id", "1" }, { "name", "Updated Item" } }
            },
            UpdateData = new Dictionary<string, object> { { "status", "active" } }
        };

        // Assert
        Assert.Equal("update", request.Action);
        Assert.True(request.Atomic);
        Assert.Single(request.Records);
        Assert.NotNull(request.UpdateData);
        Assert.Single(request.UpdateData);
    }
}

