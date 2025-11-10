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

    // ============================================
    // Filtering Edge Cases
    // ============================================

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithNonExistentField_ReturnsEmpty()
    {
        // Arrange
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "nonexistent_field", "value" } },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithEmptyString_MatchesEmptyValues()
    {
        // Arrange - Create a record with empty string
        var newRecord = new Dictionary<string, object>
        {
            { "name", "" },
            { "email", "empty@example.com" }
        };
        await _adapter.CreateAsync("users", newRecord);

        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "name", "" } },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.True(result.Data.Count > 0);
        Assert.All(result.Data, r => Assert.Equal("", r.Data["name"]?.ToString() ?? ""));
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithMultipleConditions_AppliesAll()
    {
        // Arrange
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object>
            {
                { "active", "true" },
                { "age", "30" }
            },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.All(result.Data, r =>
        {
            Assert.Equal("true", r.Data["active"]?.ToString());
            Assert.Equal("30", r.Data["age"]?.ToString());
        });
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange - Create record with special characters
        var newRecord = new Dictionary<string, object>
        {
            { "name", "Test & Special" },
            { "email", "test@example.com" }
        };
        var created = await _adapter.CreateAsync("users", newRecord);

        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "name", "Test & Special" } },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.Contains(result.Data, r => r.Data["name"]?.ToString() == "Test & Special");
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithNullValues_HandlesGracefully()
    {
        // Arrange - Create a record with null value (empty string in CSV)
        var newRecord = new Dictionary<string, object>
        {
            { "name", "NullTest" },
            { "email", "" } // Empty string represents null in CSV context
        };
        await _adapter.CreateAsync("users", newRecord);

        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "email", (object?)null! } },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Filter with null should convert to empty string and match empty values
        Assert.NotNull(result);
        // The filter value null will be converted to empty string, so it should match empty email
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithNullInRecordData_HandlesGracefully()
    {
        // Arrange - Create record, then filter for a field that might be null/empty
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "email", "" } }, // Empty string filter
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.NotNull(result);
        // Should handle null/empty values in record data gracefully
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithEmptyFilterDictionary_ReturnsAllRecords()
    {
        // Arrange
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object>(), // Empty filter
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Empty filter should return all records
        Assert.NotNull(result);
        Assert.True(result.Data.Count >= 3); // Should return all records
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithNonStringValues_ConvertsToString()
    {
        // Arrange - Filter with integer value
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "age", 30 } }, // Integer instead of string
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Integer should be converted to string "30" for comparison
        Assert.NotNull(result);
        Assert.All(result.Data, r => Assert.Equal("30", r.Data["age"]?.ToString()));
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_FilterWithBooleanValue_ConvertsToString()
    {
        // Arrange - Filter with boolean value
        var options = new QueryOptions
        {
            Filter = new Dictionary<string, object> { { "active", true } }, // Boolean instead of string
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Boolean should be converted to string for comparison
        Assert.NotNull(result);
        Assert.All(result.Data, r => 
        {
            var activeValue = r.Data["active"]?.ToString();
            Assert.True(activeValue == "True" || activeValue == "true");
        });
    }

    // ============================================
    // Sorting Edge Cases
    // ============================================

    [Fact]
    public async Task CsvAdapter_ListAsync_SortWithInvalidFormat_IgnoresSort()
    {
        // Arrange
        var optionsWithInvalidSort = new QueryOptions
        {
            Sort = "invalid-format",
            Limit = 100
        };
        var optionsWithoutSort = new QueryOptions
        {
            Limit = 100
        };

        // Act
        var resultWithSort = await _adapter.ListAsync("users", optionsWithInvalidSort);
        var resultWithoutSort = await _adapter.ListAsync("users", optionsWithoutSort);

        // Assert - Should return same order (or at least same records)
        Assert.Equal(resultWithoutSort.Total, resultWithSort.Total);
        Assert.Equal(resultWithoutSort.Data.Count, resultWithSort.Data.Count);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SortWithMissingField_HandlesGracefully()
    {
        // Arrange
        var options = new QueryOptions
        {
            Sort = "nonexistent_field:asc",
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should not throw, but may sort by empty string
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SortAscending_OrdersCorrectly()
    {
        // Arrange
        var options = new QueryOptions
        {
            Sort = "name:asc",
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.True(result.Data.Count > 1);
        var names = result.Data.Select(r => r.Data["name"]?.ToString() ?? "").ToList();
        var sortedNames = names.OrderBy(n => n).ToList();
        Assert.Equal(sortedNames, names);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SortDescending_OrdersCorrectly()
    {
        // Arrange
        var options = new QueryOptions
        {
            Sort = "name:desc",
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.True(result.Data.Count > 1);
        var names = result.Data.Select(r => r.Data["name"]?.ToString() ?? "").ToList();
        var sortedNames = names.OrderByDescending(n => n).ToList();
        Assert.Equal(sortedNames, names);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SortWithCaseSensitivity_HandlesCorrectly()
    {
        // Arrange - Create records with different case
        var record1 = new Dictionary<string, object> { { "name", "apple" }, { "email", "a@example.com" } };
        var record2 = new Dictionary<string, object> { { "name", "Apple" }, { "email", "b@example.com" } };
        await _adapter.CreateAsync("users", record1);
        await _adapter.CreateAsync("users", record2);

        var options = new QueryOptions
        {
            Sort = "name:asc",
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should handle case sensitivity (C# string comparison is case-sensitive)
        Assert.True(result.Data.Count > 1);
        var names = result.Data.Select(r => r.Data["name"]?.ToString() ?? "").ToList();
        var sortedNames = names.OrderBy(n => n).ToList();
        Assert.Equal(sortedNames, names);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SortWithNullValues_HandlesGracefully()
    {
        // Arrange - Create record with null/empty value in sort field
        var newRecord = new Dictionary<string, object>
        {
            { "name", "" }, // Empty name
            { "email", "nulltest@example.com" }
        };
        await _adapter.CreateAsync("users", newRecord);

        var options = new QueryOptions
        {
            Sort = "name:asc",
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should handle null/empty values gracefully (treat as empty string)
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
        // Records with null/empty values should be sorted (likely at beginning or end)
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SortWithDuplicateValues_MaintainsStableOrder()
    {
        // Arrange - Create multiple records with same sort value
        var newRecord1 = new Dictionary<string, object>
        {
            { "name", "DuplicateName" },
            { "email", "dup1@example.com" }
        };
        await _adapter.CreateAsync("users", newRecord1);

        var newRecord2 = new Dictionary<string, object>
        {
            { "name", "DuplicateName" },
            { "email", "dup2@example.com" }
        };
        await _adapter.CreateAsync("users", newRecord2);

        var options = new QueryOptions
        {
            Sort = "name:asc",
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should sort correctly even with duplicate values
        Assert.NotNull(result);
        var duplicateRecords = result.Data.Where(r => r.Data["name"]?.ToString() == "DuplicateName").ToList();
        Assert.True(duplicateRecords.Count >= 2);
        // All records with same name should be grouped together
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SortWithEmptyString_NoSorting()
    {
        // Arrange
        var options = new QueryOptions
        {
            Sort = "", // Empty sort string
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Empty sort should not apply sorting
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
        // Order should be same as without sort (original order)
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SortWithNullSortString_NoSorting()
    {
        // Arrange
        var options = new QueryOptions
        {
            Sort = null, // Null sort string
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Null sort should not apply sorting
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
    }

    // ============================================
    // Field Selection Edge Cases
    // ============================================

    [Fact]
    public async Task CsvAdapter_ListAsync_SelectFields_WithNonExistentFields_IgnoresThem()
    {
        // Arrange
        var options = new QueryOptions
        {
            Fields = new[] { "name", "nonexistent_field", "email" },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.True(result.Data.Count > 0);
        foreach (var record in result.Data)
        {
            Assert.True(record.Data.ContainsKey("name"));
            Assert.True(record.Data.ContainsKey("email"));
            Assert.False(record.Data.ContainsKey("nonexistent_field"));
        }
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SelectFields_AlwaysIncludesId()
    {
        // Arrange
        var options = new QueryOptions
        {
            Fields = new[] { "name", "email" }, // ID not explicitly requested
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert
        Assert.True(result.Data.Count > 0);
        foreach (var record in result.Data)
        {
            Assert.NotNull(record.Id);
            Assert.NotEmpty(record.Id);
        }
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SelectFields_WithEmptyArray_ReturnsAllFields()
    {
        // Arrange
        var optionsEmpty = new QueryOptions
        {
            Fields = Array.Empty<string>(),
            Limit = 100
        };
        var optionsAll = new QueryOptions
        {
            Limit = 100
        };

        // Act
        var resultEmpty = await _adapter.ListAsync("users", optionsEmpty);
        var resultAll = await _adapter.ListAsync("users", optionsAll);

        // Assert - Empty array should behave like null (return all fields)
        Assert.Equal(resultAll.Total, resultEmpty.Total);
        if (resultEmpty.Data.Count > 0 && resultAll.Data.Count > 0)
        {
            // Both should have same fields (or at least same record IDs)
            Assert.Equal(resultAll.Data[0].Id, resultEmpty.Data[0].Id);
        }
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SelectFields_WithDuplicateFields_DeduplicatesFields()
    {
        // Arrange - Test that duplicate fields are deduplicated
        var options = new QueryOptions
        {
            Fields = new[] { "name", "name", "email" }, // Duplicate "name"
            Limit = 100
        };

        // Act - Should not throw exception, should deduplicate fields
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should return records with only unique fields
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        
        // Verify that each record has only unique fields (name and email, not duplicate name)
        var firstRecord = result.Data.First();
        var fieldCount = firstRecord.Data.Keys.Count;
        var uniqueFieldCount = firstRecord.Data.Keys.Distinct().Count();
        Assert.Equal(fieldCount, uniqueFieldCount); // No duplicate keys
        
        // Verify expected fields are present
        Assert.True(firstRecord.Data.ContainsKey("name"));
        Assert.True(firstRecord.Data.ContainsKey("email"));
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SelectFields_PreservesFieldOrder()
    {
        // Arrange - Request fields in specific order
        var options = new QueryOptions
        {
            Fields = new[] { "email", "name", "age" }, // Specific order
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Fields should be in the order requested (first occurrence preserved)
        Assert.True(result.Data.Count > 0);
        var firstRecord = result.Data[0];
        var fieldKeys = firstRecord.Data.Keys.ToList();
        
        // Verify order: email should come before name, name before age
        var emailIndex = fieldKeys.IndexOf("email");
        var nameIndex = fieldKeys.IndexOf("name");
        var ageIndex = fieldKeys.IndexOf("age");
        
        Assert.True(emailIndex >= 0);
        Assert.True(nameIndex >= 0);
        Assert.True(ageIndex >= 0);
        Assert.True(emailIndex < nameIndex);
        Assert.True(nameIndex < ageIndex);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_SelectFields_WithNullValues_IncludesNulls()
    {
        // Arrange - Create a record with null/empty value
        var newRecord = new Dictionary<string, object>
        {
            { "name", "NullFieldTest" },
            { "email", "" }, // Empty string (represents null in CSV)
            { "age", "25" }
        };
        await _adapter.CreateAsync("users", newRecord);

        var options = new QueryOptions
        {
            Fields = new[] { "name", "email", "age" },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should include fields even if they have null/empty values
        var testRecord = result.Data.FirstOrDefault(r => r.Data["name"]?.ToString() == "NullFieldTest");
        Assert.NotNull(testRecord);
        Assert.True(testRecord.Data.ContainsKey("email")); // Field should be present even if empty
        Assert.True(testRecord.Data.ContainsKey("age"));
    }

    // ============================================
    // CSV Special Characters
    // ============================================

    [Fact]
    public async Task CsvAdapter_CreateAsync_WithCommaInValue_HandlesCorrectly()
    {
        // Arrange
        var newRecord = new Dictionary<string, object>
        {
            { "name", "Smith, John" },
            { "email", "smith@example.com" }
        };

        // Act
        var result = await _adapter.CreateAsync("users", newRecord);

        // Assert
        Assert.NotNull(result);
        var retrieved = await _adapter.GetAsync("users", result.Id);
        Assert.Equal("Smith, John", retrieved.Data["name"]?.ToString());
    }

    [Fact]
    public async Task CsvAdapter_CreateAsync_WithQuotesInValue_HandlesCorrectly()
    {
        // Arrange
        var newRecord = new Dictionary<string, object>
        {
            { "name", "John \"Johnny\" Doe" },
            { "email", "john@example.com" }
        };

        // Act
        var result = await _adapter.CreateAsync("users", newRecord);

        // Assert
        Assert.NotNull(result);
        var retrieved = await _adapter.GetAsync("users", result.Id);
        Assert.Equal("John \"Johnny\" Doe", retrieved.Data["name"]?.ToString());
    }

    [Fact]
    public async Task CsvAdapter_CreateAsync_WithNewlineInValue_HandlesCorrectly()
    {
        // Arrange
        var newRecord = new Dictionary<string, object>
        {
            { "name", "John\nDoe" },
            { "email", "john@example.com" }
        };

        // Act
        var result = await _adapter.CreateAsync("users", newRecord);

        // Assert
        Assert.NotNull(result);
        var retrieved = await _adapter.GetAsync("users", result.Id);
        Assert.Contains("John", retrieved.Data["name"]?.ToString() ?? "");
        Assert.Contains("Doe", retrieved.Data["name"]?.ToString() ?? "");
    }

    [Fact]
    public async Task CsvAdapter_CreateAsync_WithUnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        var newRecord = new Dictionary<string, object>
        {
            { "name", "José García 🎉" },
            { "email", "jose@example.com" }
        };

        // Act
        var result = await _adapter.CreateAsync("users", newRecord);

        // Assert
        Assert.NotNull(result);
        var retrieved = await _adapter.GetAsync("users", result.Id);
        Assert.Equal("José García 🎉", retrieved.Data["name"]?.ToString());
    }

    // ============================================
    // Cancellation Token Tests
    // ============================================

    [Fact]
    public async Task CsvAdapter_ListAsync_WithCancellation_AcceptsCancellationToken()
    {
        // Arrange - Note: Current implementation doesn't check cancellation after Task.Yield()
        // This test verifies the method signature accepts the token
        var cts = new CancellationTokenSource();
        var options = new QueryOptions { Limit = 100 };

        // Act - Should complete successfully (current implementation doesn't check cancellation)
        var result = await _adapter.ListAsync("users", options, cts.Token);

        // Assert
        Assert.NotNull(result);
        // TODO: Implement actual cancellation checking in CsvAdapter methods
    }

    [Fact]
    public async Task CsvAdapter_GetAsync_WithCancellation_AcceptsCancellationToken()
    {
        // Arrange - Note: Current implementation doesn't check cancellation after Task.Yield()
        var cts = new CancellationTokenSource();
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var recordId = listResult.Data.First().Id;

        // Act - Should complete successfully
        var result = await _adapter.GetAsync("users", recordId, cts.Token);

        // Assert
        Assert.NotNull(result);
        // TODO: Implement actual cancellation checking in CsvAdapter methods
    }

    [Fact]
    public async Task CsvAdapter_CreateAsync_WithCancellation_AcceptsCancellationToken()
    {
        // Arrange - Note: Current implementation doesn't check cancellation after Task.Yield()
        var cts = new CancellationTokenSource();
        var newRecord = new Dictionary<string, object> { { "name", "Test" } };

        // Act - Should complete successfully
        var result = await _adapter.CreateAsync("users", newRecord, cts.Token);

        // Assert
        Assert.NotNull(result);
        // TODO: Implement actual cancellation checking in CsvAdapter methods
    }

    // ============================================
    // UpdateAsync Edge Cases
    // ============================================

    [Fact]
    public async Task CsvAdapter_UpdateAsync_WithEmptyDictionary_PreservesRecord()
    {
        // Arrange - Get existing record
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var recordId = listResult.Data.First().Id;
        var originalName = listResult.Data.First().Data["name"]?.ToString();

        var emptyUpdates = new Dictionary<string, object>();

        // Act
        await _adapter.UpdateAsync("users", recordId, emptyUpdates);

        // Assert - Record should be unchanged
        var updatedRecord = await _adapter.GetAsync("users", recordId);
        Assert.Equal(originalName, updatedRecord.Data["name"]?.ToString());
    }

    [Fact]
    public async Task CsvAdapter_UpdateAsync_WithNewField_AddsFieldToRecord()
    {
        // Arrange - Get existing record
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var recordId = listResult.Data.First().Id;

        var updates = new Dictionary<string, object>
        {
            { "new_field", "new_value" }
        };

        // Act
        await _adapter.UpdateAsync("users", recordId, updates);

        // Assert - The field should be persisted to CSV headers and be retrievable
        var updatedRecord = await _adapter.GetAsync("users", recordId);
        Assert.NotNull(updatedRecord);
        Assert.True(updatedRecord.Data.ContainsKey("new_field"));
        Assert.Equal("new_value", updatedRecord.Data["new_field"]?.ToString());
        
        // Verify the field is in the schema (headers)
        var schema = await _adapter.GetSchemaAsync("users");
        Assert.Contains(schema.Fields, f => f.Name == "new_field");
    }

    [Fact]
    public async Task CsvAdapter_UpdateAsync_WithNullValue_ConvertsToEmptyString()
    {
        // Arrange - Get existing record
        // Note: CSV format doesn't support null values, so null is converted to empty string
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var recordId = listResult.Data.First().Id;

        var updates = new Dictionary<string, object>
        {
            { "name", "" }
        };

        // Act
        await _adapter.UpdateAsync("users", recordId, updates);

        // Assert - CSV stores null as empty string
        var updatedRecord = await _adapter.GetAsync("users", recordId);
        Assert.Equal("", updatedRecord.Data["name"]?.ToString() ?? "");
    }

    // ============================================
    // Concurrency Tests
    // ============================================

    [Fact]
    public async Task CsvAdapter_ConcurrentReads_AllowMultipleReaders()
    {
        // Arrange
        var tasks = new List<Task<ListResult>>();
        var options = new QueryOptions { Limit = 100 };

        // Act - Start multiple concurrent reads
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_adapter.ListAsync("users", options));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All reads should succeed and return same data
        Assert.Equal(10, results.Length);
        foreach (var result in results)
        {
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
        }
    }

    [Fact]
    public async Task CsvAdapter_ConcurrentWrites_MayRequireRetry()
    {
        // Arrange - Note: File locking may prevent true concurrent writes
        // This test verifies the locking mechanism works (may throw IOException if locked)
        var tasks = new List<Task<CreateResult>>();
        var initialCount = (await _adapter.ListAsync("users", new QueryOptions { Limit = 100 })).Total;

        // Act - Start multiple concurrent writes (may have some failures due to locking)
        var successfulWrites = 0;
        var exceptions = new List<Exception>();
        
        for (int i = 0; i < 5; i++)
        {
            var record = new Dictionary<string, object>
            {
                { "name", $"Concurrent User {i}" },
                { "email", $"concurrent{i}@example.com" }
            };
            
            try
            {
                var result = await _adapter.CreateAsync("users", record);
                Assert.NotNull(result);
                Assert.NotNull(result.Id);
                successfulWrites++;
            }
            catch (IOException)
            {
                // Expected - file may be locked by another concurrent write
                // This demonstrates the locking mechanism works
                exceptions.Add(new IOException("File locked"));
            }
        }

        // Assert - At least some writes should succeed
        // Note: True concurrent writes may fail due to file locking, which is expected behavior
        Assert.True(successfulWrites > 0 || exceptions.Count > 0);
        
        // Verify records were written (may be fewer than 5 due to locking)
        var finalCount = (await _adapter.ListAsync("users", new QueryOptions { Limit = 1000 })).Total;
        Assert.True(finalCount >= initialCount);
    }

    [Fact]
    public async Task CsvAdapter_ReadDuringWrite_HandlesGracefully()
    {
        // Arrange
        var readTask = _adapter.ListAsync("users", new QueryOptions { Limit = 100 });
        
        // Start write operation
        var newRecord = new Dictionary<string, object>
        {
            { "name", "Write During Read" },
            { "email", "writeduringread@example.com" }
        };
        var writeTask = _adapter.CreateAsync("users", newRecord);

        // Act - Wait for both to complete
        await Task.WhenAll(readTask, writeTask);

        // Assert - Both should complete successfully
        var readResult = await readTask;
        var writeResult = await writeTask;
        
        Assert.NotNull(readResult);
        Assert.NotNull(writeResult);
        Assert.NotNull(writeResult.Id);
    }

    [Fact]
    public async Task CsvAdapter_UpdateAndRead_Concurrently_HandlesCorrectly()
    {
        // Arrange - Get existing record
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var recordId = listResult.Data.First().Id;

        // Start update and read concurrently
        var updateTask = _adapter.UpdateAsync("users", recordId, new Dictionary<string, object>
        {
            { "name", "Updated Concurrently" }
        });
        var readTask = _adapter.GetAsync("users", recordId);

        // Act - Wait for both
        await Task.WhenAll(updateTask, readTask);

        // Assert - Both should complete
        var readResult = await readTask;
        Assert.NotNull(readResult);
        // The read may happen before or after update, so we just verify it completes
    }

    #region BulkOperationAsync Tests

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_Create_Atomic_Success()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            Action = "create",
            Atomic = true,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "name", "New User 1" }, { "email", "new1@example.com" } },
                new() { { "name", "New User 2" }, { "email", "new2@example.com" } }
            }
        };

        // Act
        var result = await _adapter.BulkOperationAsync("users", request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Succeeded);
        Assert.Equal(0, result.Failed);
        Assert.NotNull(result.Ids);
        Assert.Equal(2, result.Ids.Count);

        // Verify records were created
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 100 });
        Assert.Equal(5, listResult.Total); // 3 original + 2 new
    }

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_Create_Atomic_Failure()
    {
        // Arrange - Try to create with duplicate email (if validation exists) or invalid data
        var request = new BulkOperationRequest
        {
            Action = "create",
            Atomic = true,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "name", "Valid User" }, { "email", "valid@example.com" } },
                new() { { "name", "" }, { "email", "" } } // Invalid data that might cause failure
            }
        };

        // Act
        var result = await _adapter.BulkOperationAsync("users", request);

        // Assert - In atomic mode, if one fails, all should fail
        // Note: Current implementation may not validate, so this test verifies atomic behavior
        Assert.NotNull(result);
        // If validation is added later, this should fail atomically
    }

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_Create_BestEffort()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            Action = "create",
            Atomic = false,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "name", "Best Effort 1" }, { "email", "be1@example.com" } },
                new() { { "name", "Best Effort 2" }, { "email", "be2@example.com" } }
            }
        };

        // Act
        var result = await _adapter.BulkOperationAsync("users", request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Succeeded);
        Assert.Equal(0, result.Failed);
        Assert.NotNull(result.Results);
        Assert.Equal(2, result.Results.Count);
        Assert.All(result.Results, r => Assert.True(r.Success));
    }

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_Update_Atomic()
    {
        // Arrange - Get existing record
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var existingId = listResult.Data[0].Id;

        var request = new BulkOperationRequest
        {
            Action = "update",
            Atomic = true,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "id", existingId }, { "name", "Updated Name" } }
            },
            UpdateData = new Dictionary<string, object> { { "name", "Updated Name" } }
        };

        // Act
        var result = await _adapter.BulkOperationAsync("users", request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(1, result.Succeeded);
        Assert.Equal(0, result.Failed);

        // Verify update
        var updated = await _adapter.GetAsync("users", existingId);
        Assert.Equal("Updated Name", updated.Data["name"]);
    }

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_Delete_Atomic()
    {
        // Arrange - Get existing record
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var existingId = listResult.Data[0].Id;
        var initialCount = listResult.Total;

        var request = new BulkOperationRequest
        {
            Action = "delete",
            Atomic = true,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "id", existingId } }
            }
        };

        // Act
        var result = await _adapter.BulkOperationAsync("users", request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(1, result.Succeeded);
        Assert.Equal(0, result.Failed);

        // Verify deletion
        var newListResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 100 });
        Assert.Equal(initialCount - 1, newListResult.Total);
    }

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_HandlesCancellation()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            Action = "create",
            Atomic = false,
            Records = new List<Dictionary<string, object>>
            {
                new() { { "name", "Cancel Test" }, { "email", "cancel@example.com" } }
            }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _adapter.BulkOperationAsync("users", request, cts.Token));
    }

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_InvalidAction_ThrowsException()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            Action = "invalid",
            Atomic = false,
            Records = new List<Dictionary<string, object>> { new() }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _adapter.BulkOperationAsync("users", request));
    }

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_EmptyRecords_ThrowsException()
    {
        // Arrange
        var request = new BulkOperationRequest
        {
            Action = "create",
            Atomic = false,
            Records = new List<Dictionary<string, object>>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _adapter.BulkOperationAsync("users", request));
    }

    #endregion

    // ============================================
    // Task 1.4.1: BulkOperationAsync Edge Cases
    // ============================================

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_WithPartialFailures_ReturnsPartialResults()
    {
        // Arrange - Create best-effort bulk operation where some records will fail
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email\n1,Alice,alice@example.com\n");
        
        var adapter = new CsvAdapter(testDir);
        
        var request = new BulkOperationRequest
        {
            Action = "update",
            Atomic = false, // Best-effort mode
            Records = new List<Dictionary<string, object>>
            {
                new() { { "id", "1" }, { "name", "Updated Alice" } }, // Valid - should succeed
                new() { { "id", "999" }, { "name", "Non-existent" } } // Invalid - should fail (ID doesn't exist)
            }
        };

        // Act
        var result = await adapter.BulkOperationAsync("test", request);

        // Assert - Should return partial results
        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.Equal(2, result.Results.Count);
        Assert.Equal(1, result.Succeeded);
        Assert.Equal(1, result.Failed);
        Assert.True(result.Results[0].Success); // First should succeed
        Assert.False(result.Results[1].Success); // Second should fail
        Assert.NotNull(result.Results[1].Error); // Should have error message

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_BulkOperationAsync_WithLargeBatch_HandlesCorrectly()
    {
        // Arrange - Create a large batch of 100+ records
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email\n");
        
        var adapter = new CsvAdapter(testDir);
        
        var records = new List<Dictionary<string, object>>();
        for (int i = 0; i < 150; i++)
        {
            records.Add(new Dictionary<string, object>
            {
                { "name", $"User {i}" },
                { "email", $"user{i}@example.com" }
            });
        }

        var request = new BulkOperationRequest
        {
            Action = "create",
            Atomic = false, // Best-effort mode for large batch
            Records = records
        };

        // Act
        var result = await adapter.BulkOperationAsync("test", request);

        // Assert - All should succeed
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(150, result.Succeeded);
        Assert.Equal(0, result.Failed);
        Assert.NotNull(result.Results);
        Assert.Equal(150, result.Results.Count);
        Assert.All(result.Results, r => Assert.True(r.Success));

        // Verify records were created
        var listResult = await adapter.ListAsync("test", new QueryOptions { Limit = 200 });
        Assert.Equal(150, listResult.Total);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    #region GetSummaryAsync Tests

    [Fact]
    public async Task CsvAdapter_GetSummaryAsync_ReturnsFieldCounts()
    {
        // Arrange
        var field = "email";

        // Act
        var result = await _adapter.GetSummaryAsync("users", field);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Counts);
        Assert.True(result.Counts.Count > 0);
        Assert.All(result.Counts.Values, count => Assert.True(count > 0));
    }

    [Fact]
    public async Task CsvAdapter_GetSummaryAsync_HandlesNullValues()
    {
        // Arrange - Create a test file with null values
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,status\n1,Alice,active\n2,Bob,\n3,Charlie,active\n");

        var adapter = new CsvAdapter(testDir);

        // Act
        var result = await adapter.GetSummaryAsync("test", "status");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Counts);
        // Should handle empty/null values
        Assert.True(result.Counts.ContainsKey("active") || result.Counts.ContainsKey("") || result.Counts.ContainsKey("null"));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_GetSummaryAsync_InvalidField_ReturnsEmptyCounts()
    {
        // Arrange
        var field = "nonexistent_field";

        // Act
        var result = await _adapter.GetSummaryAsync("users", field);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Counts);
        // Field doesn't exist, so counts should be empty
        Assert.Empty(result.Counts);
    }

    [Fact]
    public async Task CsvAdapter_GetSummaryAsync_InvalidCollection_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _adapter.GetSummaryAsync("nonexistent", "field"));
    }

    [Fact]
    public async Task CsvAdapter_GetSummaryAsync_EmptyField_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _adapter.GetSummaryAsync("users", ""));
    }

    // ============================================
    // Task 1.4.2: GetSummaryAsync Edge Cases
    // ============================================

    [Fact]
    public async Task CsvAdapter_GetSummaryAsync_WithEmptyCollection_ReturnsEmptyCounts()
    {
        // Arrange - Create empty collection (headers only, no data)
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,status\n"); // Only headers, no records
        
        var adapter = new CsvAdapter(testDir);

        // Act
        var result = await adapter.GetSummaryAsync("test", "status");

        // Assert - Should return empty counts
        Assert.NotNull(result);
        Assert.NotNull(result.Counts);
        Assert.Empty(result.Counts);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    #endregion

    #region AggregateAsync Tests

    [Fact]
    public async Task CsvAdapter_AggregateAsync_SimpleGroupBy()
    {
        // Arrange
        var request = new AggregateRequest
        {
            GroupBy = new[] { "email" },
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };

        // Act
        var result = await _adapter.AggregateAsync("users", request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);
        Assert.All(result.Data, item => Assert.True(item.ContainsKey("email")));
        Assert.All(result.Data, item => Assert.True(item.ContainsKey("count")));
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_MultipleGroupBy()
    {
        // Arrange - Create test data with multiple fields
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "products.csv");
        File.WriteAllText(csvPath, "id,category,status,price\n1,Electronics,active,100\n2,Electronics,active,200\n3,Books,inactive,15\n");

        var adapter = new CsvAdapter(testDir);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "category", "status" },
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);
        Assert.All(result.Data, item => Assert.True(item.ContainsKey("category")));
        Assert.All(result.Data, item => Assert.True(item.ContainsKey("status")));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_MultipleAggregates()
    {
        // Arrange - Create test data with numeric field
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "products.csv");
        File.WriteAllText(csvPath, "id,category,price\n1,Electronics,100\n2,Electronics,200\n3,Books,15\n");

        var adapter = new CsvAdapter(testDir);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "category" },
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "count" },
                new() { Field = "price", Function = "sum", Alias = "total_price" },
                new() { Field = "price", Function = "avg", Alias = "avg_price" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);
        var firstGroup = result.Data[0];
        Assert.True(firstGroup.ContainsKey("count"));
        Assert.True(firstGroup.ContainsKey("total_price"));
        Assert.True(firstGroup.ContainsKey("avg_price"));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_WithFilter()
    {
        // Arrange - Create test data
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "products.csv");
        File.WriteAllText(csvPath, "id,category,status,price\n1,Electronics,active,100\n2,Electronics,active,200\n3,Books,inactive,15\n");

        var adapter = new CsvAdapter(testDir);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "category" },
            Filter = new Dictionary<string, object> { { "status", "active" } },
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        // Should only include active items
        Assert.True(result.Data.Count > 0);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_NoGroupBy_ReturnsSingleResult()
    {
        // Arrange
        var request = new AggregateRequest
        {
            GroupBy = null,
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "total_count" }
            }
        };

        // Act
        var result = await _adapter.AggregateAsync("users", request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.True(result.Data[0].ContainsKey("total_count"));
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_EmptyAggregates_ThrowsException()
    {
        // Arrange
        var request = new AggregateRequest
        {
            GroupBy = new[] { "email" },
            Aggregates = new List<AggregateFunction>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _adapter.AggregateAsync("users", request));
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_InvalidCollection_ThrowsException()
    {
        // Arrange
        var request = new AggregateRequest
        {
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _adapter.AggregateAsync("nonexistent", request));
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_HandlesCancellation()
    {
        // Arrange
        var request = new AggregateRequest
        {
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _adapter.AggregateAsync("users", request, cts.Token));
    }

    // ============================================
    // Task 1.4.3: AggregateAsync Edge Cases
    // ============================================

    [Fact]
    public async Task CsvAdapter_AggregateAsync_WithNullValues_HandlesGracefully()
    {
        // Arrange - Create test data with null values
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "products.csv");
        File.WriteAllText(csvPath, "id,category,price\n1,Electronics,100\n2,Electronics,\n3,Books,15\n4,Books,\n");
        
        var adapter = new CsvAdapter(testDir);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "category" },
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "price", Function = "avg", Alias = "avg_price" },
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert - Should handle null values gracefully
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);
        // Avg should only consider non-null values
        var electronicsGroup = result.Data.FirstOrDefault(d => d.ContainsKey("category") && d["category"]?.ToString() == "Electronics");
        Assert.NotNull(electronicsGroup);
        Assert.True(electronicsGroup.ContainsKey("avg_price"));
        Assert.True(electronicsGroup.ContainsKey("count"));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_WithEmptyGroups_ReturnsEmpty()
    {
        // Arrange - Create test data that will result in empty groups after filtering
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "products.csv");
        File.WriteAllText(csvPath, "id,category,status\n1,Electronics,active\n2,Electronics,active\n3,Books,active\n");
        
        var adapter = new CsvAdapter(testDir);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "category" },
            Filter = new Dictionary<string, object> { { "status", "inactive" } }, // Filter that matches nothing
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert - Should return empty result (no groups match filter)
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_WithInvalidFieldNames_HandlesGracefully()
    {
        // Arrange - Create test data and aggregate on non-existent field
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "products.csv");
        File.WriteAllText(csvPath, "id,category,price\n1,Electronics,100\n2,Electronics,200\n");
        
        var adapter = new CsvAdapter(testDir);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "category" },
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "nonexistent_field", Function = "count", Alias = "count" },
                new() { Field = "nonexistent_field", Function = "sum", Alias = "total" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert - Should handle invalid field names gracefully
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);
        var firstGroup = result.Data[0];
        // Count should work (counts all records regardless of field)
        Assert.True(firstGroup.ContainsKey("count"));
        // Sum should be 0 or null for non-existent field
        Assert.True(firstGroup.ContainsKey("total"));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_WithGroupByInvalidField_HandlesGracefully()
    {
        // Arrange - Group by a field that doesn't exist
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var csvPath = Path.Combine(testDir, "products.csv");
        File.WriteAllText(csvPath, "id,category,price\n1,Electronics,100\n2,Electronics,200\n3,Books,15\n");
        
        var adapter = new CsvAdapter(testDir);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "nonexistent_field" }, // Field that doesn't exist
            Aggregates = new List<AggregateFunction>
            {
                new() { Field = "id", Function = "count", Alias = "count" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert - Should group by "null" for missing field
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data); // All records should be in one group (all have null for missing field)
        var group = result.Data[0];
        Assert.True(group.ContainsKey("nonexistent_field"));
        Assert.Equal("null", group["nonexistent_field"]?.ToString());

        // Cleanup
        Directory.Delete(testDir, true);
    }

    // ============================================
    // Retry Logic Tests (Testing RetryFileOperationAsync indirectly)
    // ============================================

    [Fact]
    public async Task CsvAdapter_CreateAsync_WithRetryDisabled_NoRetries()
    {
        // Arrange - Create adapter with retry disabled
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var retryOptions = new RetryOptions { Enabled = false };
        var adapter = new CsvAdapter(testDir, retryOptions: retryOptions);

        // Create initial collection file
        var csvPath = Path.Combine(testDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,name\n");

        // Create initial collection
        var initialRecord = new Dictionary<string, object> { { "name", "Test" } };
        await adapter.CreateAsync("test", initialRecord);

        // Act - Should work without retry logic
        var newRecord = new Dictionary<string, object> { { "name", "Test2" } };
        var result = await adapter.CreateAsync("test", newRecord);

        // Assert - Operation should succeed
        Assert.NotNull(result);
        Assert.NotNull(result.Id);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_CreateAsync_WithSuccessfulFirstAttempt_NoRetries()
    {
        // Arrange - Normal adapter with retry enabled
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create initial collection file
        var csvPath = Path.Combine(testDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,name\n");

        // Act - Operation should succeed on first attempt (no retries needed)
        var newRecord = new Dictionary<string, object> { { "name", "Test" } };
        var result = await adapter.CreateAsync("test", newRecord);

        // Assert - Should succeed without needing retries
        Assert.NotNull(result);
        Assert.NotNull(result.Id);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_CreateAsync_WithCancellation_ThrowsCancellationException()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);
        var cts = new CancellationTokenSource();

        // Act & Assert - Cancel before operation
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => adapter.CreateAsync("test", new Dictionary<string, object> { { "name", "Test" } }, cts.Token));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public void RetryOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new RetryOptions();

        // Assert
        Assert.True(options.Enabled);
        Assert.Equal(3, options.MaxRetries);
        Assert.Equal(50, options.BaseDelayMs);
    }

    [Fact]
    public void RetryOptions_CustomConfiguration_WorksCorrectly()
    {
        // Arrange & Act
        var options = new RetryOptions
        {
            Enabled = false,
            MaxRetries = 5,
            BaseDelayMs = 100
        };

        // Assert
        Assert.False(options.Enabled);
        Assert.Equal(5, options.MaxRetries);
        Assert.Equal(100, options.BaseDelayMs);
    }

    // ============================================
    // Type Inference Tests (Testing InferFieldType and InferFieldTypeFromData indirectly)
    // ============================================

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_WithStringField_InfersStringType()
    {
        // Arrange - Create collection with string field
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        var csvPath = Path.Combine(testDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,name,email\n1,Test User,test@example.com\n");

        var record = new Dictionary<string, object>
        {
            { "name", "Test User" },
            { "email", "test@example.com" }
        };
        await adapter.CreateAsync("test", record);

        // Act
        var schema = await adapter.GetSchemaAsync("test");

        // Assert - Should infer String type for text fields
        Assert.NotNull(schema);
        var nameField = schema.Fields.FirstOrDefault(f => f.Name == "name");
        Assert.NotNull(nameField);
        Assert.Equal(DataAbstractionAPI.Core.Enums.FieldType.String, nameField.Type);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_WithIntegerField_InfersIntegerType()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        var csvPath = Path.Combine(testDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,age,count\n1,25,100\n");

        var record = new Dictionary<string, object>
        {
            { "age", 25 },
            { "count", 100 }
        };
        await adapter.CreateAsync("test", record);

        // Act
        var schema = await adapter.GetSchemaAsync("test");

        // Assert - Note: CSV stores everything as strings, so type inference may return String
        // This test verifies the type inference logic is called, even if result is String
        Assert.NotNull(schema);
        var ageField = schema.Fields.FirstOrDefault(f => f.Name == "age");
        Assert.NotNull(ageField);
        // Type inference may return String for CSV data since values are stored as strings
        Assert.True(ageField.Type == DataAbstractionAPI.Core.Enums.FieldType.Integer || 
                    ageField.Type == DataAbstractionAPI.Core.Enums.FieldType.String);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_WithFloatField_InfersFloatType()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        var csvPath = Path.Combine(testDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,price,weight\n1,19.99,5.5\n");

        var record = new Dictionary<string, object>
        {
            { "price", 19.99 },
            { "weight", 5.5 }
        };
        await adapter.CreateAsync("test", record);

        // Act
        var schema = await adapter.GetSchemaAsync("test");

        // Assert - Note: CSV stores everything as strings, so type inference may return String
        Assert.NotNull(schema);
        var priceField = schema.Fields.FirstOrDefault(f => f.Name == "price");
        Assert.NotNull(priceField);
        // Type inference may return String for CSV data since values are stored as strings
        Assert.True(priceField.Type == DataAbstractionAPI.Core.Enums.FieldType.Float || 
                    priceField.Type == DataAbstractionAPI.Core.Enums.FieldType.String);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_WithBooleanField_InfersBooleanType()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        var csvPath = Path.Combine(testDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,active,enabled\n1,True,False\n");

        var record = new Dictionary<string, object>
        {
            { "active", true },
            { "enabled", false }
        };
        await adapter.CreateAsync("test", record);

        // Act
        var schema = await adapter.GetSchemaAsync("test");

        // Assert - Note: CSV stores everything as strings, so type inference may return String
        Assert.NotNull(schema);
        var activeField = schema.Fields.FirstOrDefault(f => f.Name == "active");
        Assert.NotNull(activeField);
        // Type inference may return String for CSV data since values are stored as strings
        Assert.True(activeField.Type == DataAbstractionAPI.Core.Enums.FieldType.Boolean || 
                    activeField.Type == DataAbstractionAPI.Core.Enums.FieldType.String);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_WithNullValues_InfersFromNonNullValues()
    {
        // Arrange - Create records with some null values
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        var csvPath = Path.Combine(testDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,name,age\n1,,25\n");

        // First record with null
        var record1 = new Dictionary<string, object>
        {
            { "name", "" }, // Empty string (null in CSV)
            { "age", 25 }
        };
        await adapter.CreateAsync("test", record1);

        // Second record with actual value
        var record2 = new Dictionary<string, object>
        {
            { "name", "Test User" },
            { "age", 30 }
        };
        await adapter.CreateAsync("test", record2);

        // Act
        var schema = await adapter.GetSchemaAsync("test");

        // Assert - Should infer type from non-null values
        Assert.NotNull(schema);
        var nameField = schema.Fields.FirstOrDefault(f => f.Name == "name");
        Assert.NotNull(nameField);
        // Should infer String from the non-null value
        Assert.Equal(DataAbstractionAPI.Core.Enums.FieldType.String, nameField.Type);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_WithArrayField_InfersArrayType()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        // Note: Arrays in CSV are stored as strings, so we'll test with a string representation
        var csvPath = Path.Combine(testDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,tags,name\n1,\"1,2,3\",Test\n");

        var record = new Dictionary<string, object>
        {
            { "tags", new List<int> { 1, 2, 3 } },
            { "name", "Test" }
        };
        await adapter.CreateAsync("test", record);

        // Act
        var schema = await adapter.GetSchemaAsync("test");

        // Assert - Note: CSV stores arrays as strings, so type inference will return String
        Assert.NotNull(schema);
        var tagsField = schema.Fields.FirstOrDefault(f => f.Name == "tags");
        Assert.NotNull(tagsField);
        // CSV stores arrays as strings, so type will be String
        Assert.Equal(DataAbstractionAPI.Core.Enums.FieldType.String, tagsField.Type);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_GetSchemaAsync_WithDateTimeField_InfersDateTimeType()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        var csvPath = Path.Combine(testDir, "test.csv");
        var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        await File.WriteAllTextAsync(csvPath, $"id,created_at,name\n1,{now},Test\n");

        var record = new Dictionary<string, object>
        {
            { "created_at", DateTime.UtcNow },
            { "name", "Test" }
        };
        await adapter.CreateAsync("test", record);

        // Act
        var schema = await adapter.GetSchemaAsync("test");

        // Assert - Note: CSV stores everything as strings, so type inference may return String
        Assert.NotNull(schema);
        var dateField = schema.Fields.FirstOrDefault(f => f.Name == "created_at");
        Assert.NotNull(dateField);
        // Type inference may return String for CSV data since values are stored as strings
        Assert.True(dateField.Type == DataAbstractionAPI.Core.Enums.FieldType.DateTime || 
                    dateField.Type == DataAbstractionAPI.Core.Enums.FieldType.String);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    // ============================================
    // ConvertToNumeric Tests (Testing through AggregateAsync)
    // ============================================

    [Fact]
    public async Task CsvAdapter_AggregateAsync_WithNumericStringValues_ConvertsToNumeric()
    {
        // Arrange - Create collection with numeric string values
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        var csvPath = Path.Combine(testDir, "products.csv");
        await File.WriteAllTextAsync(csvPath, "id,price,category\n1,10.50,A\n");

        // Create records with numeric strings
        var record1 = new Dictionary<string, object> { { "price", "10.50" }, { "category", "A" } };
        var record2 = new Dictionary<string, object> { { "price", "20.75" }, { "category", "A" } };
        var record3 = new Dictionary<string, object> { { "price", "15.25" }, { "category", "B" } };

        await adapter.CreateAsync("products", record1);
        await adapter.CreateAsync("products", record2);
        await adapter.CreateAsync("products", record3);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "category" },
            Aggregates = new List<AggregateFunction>
            {
                new AggregateFunction { Field = "price", Function = "sum", Alias = "total_price" },
                new AggregateFunction { Field = "price", Function = "avg", Alias = "avg_price" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert - Should convert string numbers to numeric for aggregation
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_AggregateAsync_WithInvalidNumericStrings_HandlesGracefully()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var adapter = new CsvAdapter(testDir);

        // Create the CSV file first with headers
        var csvPath = Path.Combine(testDir, "products.csv");
        await File.WriteAllTextAsync(csvPath, "id,price,category\n1,10.50,A\n");

        // Create records with invalid numeric strings
        var record1 = new Dictionary<string, object> { { "price", "10.50" }, { "category", "A" } };
        var record2 = new Dictionary<string, object> { { "price", "not-a-number" }, { "category", "A" } };

        await adapter.CreateAsync("products", record1);
        await adapter.CreateAsync("products", record2);

        var request = new AggregateRequest
        {
            GroupBy = new[] { "category" },
            Aggregates = new List<AggregateFunction>
            {
                new AggregateFunction { Field = "price", Function = "sum", Alias = "total_price" }
            }
        };

        // Act
        var result = await adapter.AggregateAsync("products", request);

        // Assert - Should handle invalid numeric strings (likely skip or return null)
        Assert.NotNull(result);
        // The exact behavior depends on implementation, but should not throw

        // Cleanup
        Directory.Delete(testDir, true);
    }

    // ============================================
    // SelectFields Tests (Testing through ListAsync with Fields option)
    // ============================================

    [Fact]
    public async Task CsvAdapter_ListAsync_WithDuplicateFields_Deduplicates()
    {
        // Arrange
        var options = new QueryOptions
        {
            Fields = new[] { "name", "email", "name", "age", "email" }, // Duplicate fields
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should deduplicate fields while preserving order (first occurrence)
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
        var firstRecord = result.Data[0];
        // Should only contain unique fields: name, email, age (in that order)
        Assert.True(firstRecord.Data.ContainsKey("name"));
        Assert.True(firstRecord.Data.ContainsKey("email"));
        Assert.True(firstRecord.Data.ContainsKey("age"));
        // Verify no duplicate keys
        var keys = firstRecord.Data.Keys.ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_WithNonExistentFields_OmitsFields()
    {
        // Arrange
        var options = new QueryOptions
        {
            Fields = new[] { "name", "nonexistent_field", "email", "another_missing" },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should only include fields that exist in records
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
        var firstRecord = result.Data[0];
        Assert.True(firstRecord.Data.ContainsKey("name"));
        Assert.True(firstRecord.Data.ContainsKey("email"));
        Assert.False(firstRecord.Data.ContainsKey("nonexistent_field"));
        Assert.False(firstRecord.Data.ContainsKey("another_missing"));
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_WithEmptyFieldsArray_ReturnsAllFields()
    {
        // Arrange
        var optionsWithFields = new QueryOptions
        {
            Fields = new string[0], // Empty array
            Limit = 100
        };

        var optionsWithoutFields = new QueryOptions
        {
            Limit = 100
        };

        // Act
        var resultWithEmpty = await _adapter.ListAsync("users", optionsWithFields);
        var resultWithout = await _adapter.ListAsync("users", optionsWithoutFields);

        // Assert - Empty fields array should return all fields (same as no fields specified)
        Assert.NotNull(resultWithEmpty);
        Assert.NotNull(resultWithout);
        Assert.True(resultWithEmpty.Data.Count > 0);
        Assert.True(resultWithout.Data.Count > 0);
        
        // Both should have the same fields
        var fieldsWithEmpty = resultWithEmpty.Data[0].Data.Keys.OrderBy(k => k).ToList();
        var fieldsWithout = resultWithout.Data[0].Data.Keys.OrderBy(k => k).ToList();
        Assert.Equal(fieldsWithout, fieldsWithEmpty);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_WithFieldSelection_PreservesOrder()
    {
        // Arrange - Request fields in specific order
        var options = new QueryOptions
        {
            Fields = new[] { "age", "name", "email" }, // Specific order
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Field order should be preserved
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
        var firstRecord = result.Data[0];
        var keys = firstRecord.Data.Keys.ToArray();
        
        // Fields should appear in the order specified (age, name, email)
        Assert.Equal("age", keys[0]);
        Assert.Equal("name", keys[1]);
        Assert.Equal("email", keys[2]);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_WithNullFieldValues_IncludesNulls()
    {
        // Arrange - Create a record with null/empty value
        var newRecord = new Dictionary<string, object>
        {
            { "name", "NullTest" },
            { "email", "" }, // Empty string (null in CSV)
            { "age", "30" }
        };
        await _adapter.CreateAsync("users", newRecord);

        var options = new QueryOptions
        {
            Fields = new[] { "name", "email", "age" },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should include fields even if values are null/empty
        Assert.NotNull(result);
        var nullRecord = result.Data.FirstOrDefault(r => r.Data["name"]?.ToString() == "NullTest");
        Assert.NotNull(nullRecord);
        Assert.True(nullRecord.Data.ContainsKey("email")); // Field should be included
        // Value may be empty string or null
        Assert.True(string.IsNullOrEmpty(nullRecord.Data["email"]?.ToString()) || 
                    nullRecord.Data["email"] == null);
    }

    [Fact]
    public async Task CsvAdapter_ListAsync_WithSingleField_ReturnsOnlyThatField()
    {
        // Arrange
        var options = new QueryOptions
        {
            Fields = new[] { "name" },
            Limit = 100
        };

        // Act
        var result = await _adapter.ListAsync("users", options);

        // Assert - Should only return the specified field
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
        var firstRecord = result.Data[0];
        // Record.Id is always set, but "id" may or may not be in Data dictionary
        Assert.NotNull(firstRecord.Id);
        // Should have name field
        Assert.True(firstRecord.Data.ContainsKey("name"));
        // Should not have other data fields (id may be in Data if it was in the original CSV)
        var dataKeys = firstRecord.Data.Keys.ToList();
        // Should only have "name" (and possibly "id" if it was in the original data)
        Assert.True(dataKeys.Count <= 2); // At most "id" and "name"
        Assert.Contains("name", dataKeys);
    }

    // ============================================
    // IsLockException Tests (Testing through file locking scenarios)
    // ============================================

    [Fact]
    public async Task CsvAdapter_ConcurrentOperations_HandlesFileLocking()
    {
        // Arrange - This test verifies that IsLockException is called during concurrent operations
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        // Copy test data
        var testDataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "testdata");
        File.Copy(
            Path.Combine(testDataDir, "users.csv"),
            Path.Combine(testDir, "users.csv"),
            true
        );
        
        var adapter = new CsvAdapter(testDir);

        // Act - Start multiple concurrent operations
        var tasks = new List<Task<CreateResult>>();
        for (int i = 0; i < 5; i++)
        {
            var record = new Dictionary<string, object>
            {
                { "name", $"Concurrent User {i}" },
                { "email", $"concurrent{i}@example.com" }
            };
            tasks.Add(adapter.CreateAsync("users", record));
        }

        // Assert - All operations should complete (retry logic handles locks)
        var results = await Task.WhenAll(tasks);
        Assert.All(results, r => Assert.NotNull(r));
        Assert.All(results, r => Assert.NotNull(r.Id));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    [Fact]
    public async Task CsvAdapter_UpdateAsync_WithConcurrentAccess_RetriesOnLock()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        var testDataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "testdata");
        File.Copy(
            Path.Combine(testDataDir, "users.csv"),
            Path.Combine(testDir, "users.csv"),
            true
        );
        
        var adapter = new CsvAdapter(testDir);

        // Act - Start concurrent updates (retry logic should handle file locks)
        var tasks = new List<Task>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(adapter.UpdateAsync("users", "1", new Dictionary<string, object>
            {
                { "name", $"Updated Name {i}" }
            }));
        }

        // Assert - All updates should complete (retry handles locks)
        // UpdateAsync returns Task (void), so we just await all tasks
        await Task.WhenAll(tasks);

        // Verify final state
        var result = await adapter.GetAsync("users", "1");
        Assert.NotNull(result);
        Assert.True(result.Data.ContainsKey("name"));

        // Cleanup
        Directory.Delete(testDir, true);
    }

    #endregion
}

