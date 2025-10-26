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
}

