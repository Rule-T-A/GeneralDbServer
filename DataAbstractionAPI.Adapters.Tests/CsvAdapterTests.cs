namespace DataAbstractionAPI.Adapters.Tests;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Adapters.Csv;

public class CsvAdapterTests : IDisposable
{
    private readonly string _testDataDir;
    private readonly string _tempTestDir;
    private readonly CsvAdapter _adapter;

    public CsvAdapterTests()
    {
        _testDataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "testdata");
        _tempTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestDir);
        
        // Copy test CSV to temp directory
        File.Copy(
            Path.Combine(_testDataDir, "users.csv"),
            Path.Combine(_tempTestDir, "users.csv"),
            true
        );

        _adapter = new CsvAdapter(_tempTestDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempTestDir, true);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_ReturnsAllRecords_WithoutFilter()
    {
        // Arrange
        var options = new QueryOptions
        {
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Data.Count);
        Assert.Equal(3, result.Total);
        Assert.False(result.More);
        
        // Verify first record
        Assert.Equal("1", result.Data[0].Id);
        Assert.Equal("Alice Johnson", result.Data[0].Data["name"]);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_ReturnsCorrectTotal_Count()
    {
        // Arrange
        var options = new QueryOptions
        {
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.Equal(3, result.Total);
        Assert.Equal(3, result.Data.Count);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_HandlesMissingCollection_ThrowsException()
    {
        // Arrange
        var options = new QueryOptions();

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _adapter.ListAsync("nonexistent", options)
        );
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_RespectsLimit()
    {
        // Arrange
        var options = new QueryOptions
        {
            Limit = 2
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(3, result.Total);
        Assert.True(result.More);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_RespectsOffset()
    {
        // Arrange
        var options = new QueryOptions
        {
            Limit = 10,
            Offset = 1
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(3, result.Total);
        Assert.Equal("2", result.Data[0].Id); // Should start from second record
    }

    [Fact]
    public async Task CsvAdapter_GetAsync_ReturnsRecord_WithMatchingId()
    {
        // Arrange
        var id = "2";

        // Act
        var record = await _adapter.GetAsync("users", id);

        // Assert
        Assert.NotNull(record);
        Assert.Equal("2", record.Id);
        Assert.Equal("Bob Smith", record.Data["name"]);
        Assert.Equal("bob@example.com", record.Data["email"]);
        Assert.Equal("25", record.Data["age"]);
    }

    [Fact]
    public async Task CsvAdapter_GetAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _adapter.GetAsync("users", "999")
        );
    }

    [Fact]
    public void CsvAdapter_GenerateId_ReturnsUniqueGuids()
    {
        // Arrange
        var ids = new HashSet<string>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var id = _adapter.GenerateId();
            ids.Add(id);
        }

        // Assert
        Assert.Equal(100, ids.Count); // All IDs should be unique
    }

    [Fact]
    public async Task CsvAdapter_CreateAsync_AddsRecord_ToCsvFile()
    {
        // Arrange
        var newRecord = new Dictionary<string, object>
        {
            { "name", "David Lee" },
            { "email", "david@example.com" },
            { "age", "35" },
            { "active", "true" }
        };

        // Act
        var result = await _adapter.CreateAsync("users", newRecord);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal("David Lee", result.Record.Data["name"]);
        Assert.Equal("david@example.com", result.Record.Data["email"]);

        // Verify record was written to file
        var allRecords = await _adapter.ListAsync("users", new QueryOptions { Limit = 100 });
        Assert.Equal(4, allRecords.Total); // Should now have 4 records (original 3 + 1 new)
    }

    [Fact]
    public async Task CsvAdapter_CreateAsync_AppendsToExistingFile()
    {
        // Arrange
        var initialCount = (await _adapter.ListAsync("users", new QueryOptions { Limit = 100 })).Total;
        var newRecord = new Dictionary<string, object>
        {
            { "name", "Eva Martin" },
            { "email", "eva@example.com" },
            { "age", "28" }
        };

        // Act
        await _adapter.CreateAsync("users", newRecord);

        // Assert
        var finalCount = (await _adapter.ListAsync("users", new QueryOptions { Limit = 100 })).Total;
        Assert.Equal(initialCount + 1, finalCount);
    }
}

