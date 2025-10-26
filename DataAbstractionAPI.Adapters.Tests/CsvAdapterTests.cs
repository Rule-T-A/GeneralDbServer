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

    [Fact]
    public async Task CsvAdapter_SecureCollection_RejectsPathTraversal_WithDotDot()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _adapter.ListAsync("../etc", new QueryOptions())
        );
    }

    [Fact]
    public async Task CsvAdapter_SecureCollection_RejectsPathTraversal_WithBackslash()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _adapter.ListAsync("collection\\name", new QueryOptions())
        );
    }

    [Fact]
    public async Task CsvAdapter_SecureCollection_RejectsPathTraversal_WithForwardSlash()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _adapter.ListAsync("collection/name", new QueryOptions())
        );
    }

    [Fact]
    public async Task CsvAdapter_SecureCollection_AcceptsValidCollectionName()
    {
        // Arrange
        var options = new QueryOptions { Limit = 10 };

        // Act & Assert - Should throw FileNotFoundException (collection doesn't exist)
        // but NOT ArgumentException (security validation passed)
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => _adapter.ListAsync("valid-collection-name", options)
        );
        
        Assert.NotNull(exception);
        Assert.DoesNotContain("ArgumentException", exception.GetType().Name);
    }

    [Fact]
    public async Task CsvAdapter_UpdateAsync_ModifiesRecord_InPlace()
    {
        // Arrange - Get existing record
        var queryOptions = new QueryOptions { Limit = 100 };
        var initialRecords = await _adapter.ListAsync("users", queryOptions);
        var recordToUpdate = initialRecords.Data.First();
        var originalEmail = recordToUpdate.Data["email"].ToString();

        var updates = new Dictionary<string, object>
        {
            { "email", "updated@example.com" },
            { "name", "Updated Name" }
        };

        // Act
        await _adapter.UpdateAsync("users", recordToUpdate.Id, updates);

        // Assert - Verify the record was updated
        var updatedRecord = await _adapter.GetAsync("users", recordToUpdate.Id);
        Assert.NotNull(updatedRecord);
        Assert.Equal("updated@example.com", updatedRecord.Data["email"].ToString());
        Assert.Equal("Updated Name", updatedRecord.Data["name"].ToString());
        
        // Verify other fields unchanged
        Assert.Equal(recordToUpdate.Data["id"], updatedRecord.Data["id"]);
        
        // Verify total record count unchanged
        var finalRecords = await _adapter.ListAsync("users", queryOptions);
        Assert.Equal(initialRecords.Total, finalRecords.Total);
    }

    [Fact]
    public async Task CsvAdapter_UpdateAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var invalidId = Guid.NewGuid().ToString();
        var updates = new Dictionary<string, object>
        {
            { "name", "Test" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _adapter.UpdateAsync("users", invalidId, updates)
        );
    }

    [Fact]
    public async Task CsvAdapter_UpdateAsync_HandlesPartialUpdates()
    {
        // Arrange - Get existing record
        var queryOptions = new QueryOptions { Limit = 100 };
        var initialRecords = await _adapter.ListAsync("users", queryOptions);
        var recordToUpdate = initialRecords.Data.First();
        var originalName = recordToUpdate.Data["name"].ToString();
        var originalAge = recordToUpdate.Data["age"].ToString();

        // Only update email
        var updates = new Dictionary<string, object>
        {
            { "email", "partial-update@example.com" }
        };

        // Act
        await _adapter.UpdateAsync("users", recordToUpdate.Id, updates);

        // Assert - Verify only email was updated
        var updatedRecord = await _adapter.GetAsync("users", recordToUpdate.Id);
        Assert.Equal("partial-update@example.com", updatedRecord.Data["email"].ToString());
        Assert.Equal(originalName, updatedRecord.Data["name"].ToString());
        Assert.Equal(originalAge, updatedRecord.Data["age"].ToString());
    }

    [Fact]
    public async Task CsvAdapter_DeleteAsync_RemovesRecord_FromFile()
    {
        // Arrange - Get existing records
        var queryOptions = new QueryOptions { Limit = 100 };
        var initialRecords = await _adapter.ListAsync("users", queryOptions);
        var recordToDelete = initialRecords.Data.First();
        var initialCount = initialRecords.Total;

        // Act
        await _adapter.DeleteAsync("users", recordToDelete.Id);

        // Assert - Verify record count decreased
        var finalRecords = await _adapter.ListAsync("users", queryOptions);
        Assert.Equal(initialCount - 1, finalRecords.Total);
        
        // Verify the deleted record is not found
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _adapter.GetAsync("users", recordToDelete.Id)
        );
    }

    [Fact]
    public async Task CsvAdapter_DeleteAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var invalidId = Guid.NewGuid().ToString();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _adapter.DeleteAsync("users", invalidId)
        );
    }

    [Fact]
    public async Task CsvAdapter_DeleteAsync_RemainingRecords_Intact()
    {
        // Arrange
        var queryOptions = new QueryOptions { Limit = 100 };
        var allRecords = await _adapter.ListAsync("users", queryOptions);
        var recordToDelete = allRecords.Data.First();
        var recordToKeep = allRecords.Data.Skip(1).First();

        // Act
        await _adapter.DeleteAsync("users", recordToDelete.Id);

        // Assert - Verify other records still exist
        var keptRecord = await _adapter.GetAsync("users", recordToKeep.Id);
        Assert.NotNull(keptRecord);
        Assert.Equal(recordToKeep.Data["name"], keptRecord.Data["name"]);
    }

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_ReturnsSchema()
    {
        // Arrange & Act
        var schema = await _adapter.GetSchemaAsync("users");

        // Assert
        Assert.NotNull(schema);
        Assert.Equal("users", schema.Name);
        Assert.True(schema.Fields.Count > 0);
        
        // Verify schema contains expected fields from users.csv
        var fieldNames = schema.Fields.Select(f => f.Name).ToList();
        Assert.Contains("id", fieldNames);
        Assert.Contains("name", fieldNames);
        Assert.Contains("email", fieldNames);
    }

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_WithNonExistentCollection_ThrowsException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _adapter.GetSchemaAsync("nonexistent")
        );
    }

    [Fact]
    public async Task CsvAdapter_ListCollectionsAsync_ReturnsCollectionNames()
    {
        // Arrange - Ensure at least users collection exists
        var expectedCollections = new List<string> { "users" };

        // Act
        var collections = await _adapter.ListCollectionsAsync();

        // Assert
        Assert.NotNull(collections);
        Assert.Contains("users", collections);
        Assert.True(collections.Length >= 1);
    }

    [Fact]
    public async Task CsvAdapter_ListCollectionsAsync_OnlyReturnsCsvFiles()
    {
        // Arrange - Create a non-CSV file in the test directory
        var txtFilePath = Path.Combine(_tempTestDir, "noncsv.txt");
        await File.WriteAllTextAsync(txtFilePath, "this is not a csv");

        // Act
        var collections = await _adapter.ListCollectionsAsync();

        // Assert
        Assert.DoesNotContain("noncsv", collections);
        foreach (var collection in collections)
        {
            Assert.True(collection.EndsWith(".csv") || !collection.Contains("."));
        }
    }

    [Fact]
    public void CsvAdapter_WithOptionalServices_AcceptsNullServices()
    {
        // Arrange
        var adapter = new CsvAdapter(_tempTestDir, defaultGenerator: null, typeConverter: null);

        // Act & Assert - No exception should be thrown
        Assert.NotNull(adapter);
    }

    [Fact]
    public async Task CsvAdapter_WithOptionalServices_MaintainsBackwardCompatibility()
    {
        // Arrange
        var adapter = new CsvAdapter(_tempTestDir);

        // Act - Use adapter with default behavior
        var result = await adapter.ListAsync("users", new QueryOptions { Limit = 10 });

        // Assert - Should work exactly as before
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
    }

    [Fact]
    public void CsvAdapter_Constructor_BackwardCompatible_WithoutServices()
    {
        // This test ensures the original constructor signature still works
        // Arrange & Act - Should not throw
        var adapter = new CsvAdapter(_tempTestDir);

        // Assert
        Assert.NotNull(adapter);
        
        // Also test single parameter call works
        var adapter2 = new CsvAdapter(baseDirectory: _tempTestDir);
        Assert.NotNull(adapter2);
    }
}

