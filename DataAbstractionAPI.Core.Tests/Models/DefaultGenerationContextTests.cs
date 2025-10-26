namespace DataAbstractionAPI.Core.Tests.Models;

using DataAbstractionAPI.Core.Models;

public class DefaultGenerationContextTests
{
    [Fact]
    public void DefaultGenerationContext_Initializes_WithDefaults()
    {
        // Arrange & Act
        var context = new DefaultGenerationContext();

        // Assert
        Assert.Null(context.CollectionName);
        Assert.Null(context.ExistingRecords);
    }

    [Fact]
    public void DefaultGenerationContext_CanSetCollectionName()
    {
        // Arrange
        var context = new DefaultGenerationContext();

        // Act
        context.CollectionName = "users";

        // Assert
        Assert.Equal("users", context.CollectionName);
    }

    [Fact]
    public void DefaultGenerationContext_CanSetExistingRecords()
    {
        // Arrange
        var context = new DefaultGenerationContext();
        var records = new List<Record>
        {
            new Record { Id = "1", Data = new Dictionary<string, object> { { "name", "Test1" } } },
            new Record { Id = "2", Data = new Dictionary<string, object> { { "name", "Test2" } } }
        };

        // Act
        context.ExistingRecords = records;

        // Assert
        Assert.NotNull(context.ExistingRecords);
        Assert.Equal(2, context.ExistingRecords.Count);
    }

    [Fact]
    public void DefaultGenerationContext_CanBeCreated_WithInitialization()
    {
        // Arrange
        var records = new List<Record>
        {
            new Record { Id = "user-1", Data = new Dictionary<string, object>() }
        };

        // Act
        var context = new DefaultGenerationContext
        {
            CollectionName = "products",
            ExistingRecords = records
        };

        // Assert
        Assert.Equal("products", context.CollectionName);
        Assert.NotNull(context.ExistingRecords);
        Assert.Single(context.ExistingRecords);
    }
}

