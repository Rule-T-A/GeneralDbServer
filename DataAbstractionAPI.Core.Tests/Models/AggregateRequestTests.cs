namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class AggregateRequestTests
{
    [Fact]
    public void AggregateRequest_Initializes_WithDefaults()
    {
        // Arrange & Act
        var request = new AggregateRequest();

        // Assert
        Assert.NotNull(request);
        Assert.Null(request.GroupBy);
        Assert.NotNull(request.Aggregates);
        Assert.Empty(request.Aggregates);
        Assert.Null(request.Filter);
    }

    [Fact]
    public void AggregateRequest_CanSetGroupByArray()
    {
        // Arrange
        var request = new AggregateRequest
        {
            GroupBy = new[] { "category", "status" }
        };

        // Assert
        Assert.NotNull(request.GroupBy);
        Assert.Equal(2, request.GroupBy.Length);
        Assert.Equal("category", request.GroupBy[0]);
        Assert.Equal("status", request.GroupBy[1]);
    }

    [Fact]
    public void AggregateRequest_CanSetAggregatesList()
    {
        // Arrange
        var request = new AggregateRequest
        {
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "price", Function = "sum", Alias = "total" },
                new() { Field = "price", Function = "avg", Alias = "average" }
            }
        };

        // Assert
        Assert.NotNull(request.Aggregates);
        Assert.Equal(2, request.Aggregates.Count);
        Assert.Equal("sum", request.Aggregates[0].Function);
        Assert.Equal("avg", request.Aggregates[1].Function);
    }

    [Fact]
    public void AggregateRequest_CanSetFilterDictionary()
    {
        // Arrange
        var request = new AggregateRequest
        {
            Filter = new Dictionary<string, object>
            {
                { "status", "active" },
                { "category", "Electronics" }
            }
        };

        // Assert
        Assert.NotNull(request.Filter);
        Assert.Equal(2, request.Filter.Count);
        Assert.Equal("active", request.Filter["status"]);
        Assert.Equal("Electronics", request.Filter["category"]);
    }

    [Fact]
    public void AggregateRequest_WithAllProperties_WorksCorrectly()
    {
        // Arrange
        var request = new AggregateRequest
        {
            GroupBy = new[] { "category" },
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "price", Function = "sum", Alias = "total_price" },
                new() { Field = "id", Function = "count", Alias = "count" }
            },
            Filter = new Dictionary<string, object> { { "status", "active" } }
        };

        // Assert
        Assert.NotNull(request.GroupBy);
        Assert.Single(request.GroupBy);
        Assert.Equal(2, request.Aggregates.Count);
        Assert.NotNull(request.Filter);
        Assert.Single(request.Filter);
    }
}

