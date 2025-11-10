namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class QueryOptionsTests
{
    [Fact]
    public void QueryOptions_Initializes_WithDefaults()
    {
        // Arrange & Act
        var options = new QueryOptions();

        // Assert
        Assert.Null(options.Fields);
        Assert.Null(options.Filter);
        Assert.Equal(10, options.Limit);
        Assert.Equal(0, options.Offset);
        Assert.Null(options.Sort);
    }

    [Fact]
    public void QueryOptions_CanSetAllProperties()
    {
        // Arrange
        var options = new QueryOptions
        {
            Fields = new[] { "id", "name" },
            Filter = new Dictionary<string, object> { { "status", "active" } },
            Limit = 5,
            Offset = 10,
            Sort = "name:asc"
        };

        // Assert
        Assert.Equal(new[] { "id", "name" }, options.Fields);
        Assert.NotNull(options.Filter);
        Assert.Equal("active", options.Filter["status"]);
        Assert.Equal(5, options.Limit);
        Assert.Equal(10, options.Offset);
        Assert.Equal("name:asc", options.Sort);
    }

    // ============================================
    // Task 2.2.2: QueryOptions Edge Cases
    // ============================================

    [Fact]
    public void QueryOptions_WithNegativeLimit_HandlesGracefully()
    {
        // Arrange
        var options = new QueryOptions
        {
            Limit = -5
        };

        // Assert - Negative limit is allowed (validation should happen at service layer)
        Assert.Equal(-5, options.Limit);
    }

    [Fact]
    public void QueryOptions_WithNegativeOffset_HandlesGracefully()
    {
        // Arrange
        var options = new QueryOptions
        {
            Offset = -10
        };

        // Assert - Negative offset is allowed (validation should happen at service layer)
        Assert.Equal(-10, options.Offset);
    }

    [Fact]
    public void QueryOptions_WithEmptyFieldsArray_IsValid()
    {
        // Arrange
        var options = new QueryOptions
        {
            Fields = Array.Empty<string>()
        };

        // Assert - Empty fields array is valid
        Assert.NotNull(options.Fields);
        Assert.Empty(options.Fields);
    }

    [Fact]
    public void QueryOptions_WithNullSortString_IsValid()
    {
        // Arrange
        var options = new QueryOptions
        {
            Sort = null
        };

        // Assert - Null sort string is valid (default value)
        Assert.Null(options.Sort);
    }
}

