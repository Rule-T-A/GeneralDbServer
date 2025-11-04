# Test Coverage Gaps Analysis

**Generated**: December 2025  
**Current Test Count**: 78 tests (29 Core + 39 Adapter + 10 Services)

---

## Critical Gaps (High Priority)

### 1. ❌ **No API Controller Tests** (CRITICAL)

**Issue**: The entire REST API layer (`DataController`) has **zero tests**.

**Missing Coverage**:
- ❌ Endpoint routing and parameter binding
- ❌ HTTP status code returns (200, 201, 204, 404, etc.)
- ❌ Exception handling and error responses
- ❌ Request validation
- ❌ Response serialization
- ❌ Dependency injection

**Recommended Tests**:
```csharp
// Integration tests for DataController
- DataController_GetCollection_Returns200_WithListResult
- DataController_GetCollection_WithInvalidCollection_Returns404
- DataController_GetRecord_Returns200_WithRecord
- DataController_GetRecord_WithInvalidId_Returns404
- DataController_CreateRecord_Returns201_WithLocationHeader
- DataController_UpdateRecord_Returns204_OnSuccess
- DataController_UpdateRecord_WithInvalidId_Returns404
- DataController_DeleteRecord_Returns204_OnSuccess
- DataController_GetSchema_Returns200_WithSchema
- DataController_ListCollections_Returns200_WithArray
```

**Priority**: 🔴 **CRITICAL** - API layer is untested

---

### 2. ⚠️ **Filtering Edge Cases** (High Priority)

**Current**: Basic filtering is tested, but edge cases are missing.

**Missing Tests**:
- ❌ Filtering with null values
- ❌ Filtering with empty string values
- ❌ Filtering on non-existent fields
- ❌ Filtering with multiple conditions (AND logic)
- ❌ Filtering with special characters in values
- ❌ Filtering with numeric vs string comparison (e.g., "25" vs 25)
- ❌ Filtering with case sensitivity

**Example Missing Test**:
```csharp
[Fact]
public async Task CsvAdapter_ListAsync_FilterWithNullValue_HandlesCorrectly()
{
    // Test filtering when field value is null
}

[Fact]
public async Task CsvAdapter_ListAsync_FilterWithNonExistentField_ReturnsEmpty()
{
    // Test filtering on field that doesn't exist
}
```

---

### 3. ⚠️ **Sorting Edge Cases** (High Priority)

**Current**: Basic sorting exists in code but no tests.

**Missing Tests**:
- ❌ Sorting with null values
- ❌ Sorting with missing fields
- ❌ Sorting with invalid sort format (e.g., "name" without direction)
- ❌ Sorting with case sensitivity
- ❌ Sorting numeric values as strings
- ❌ Sorting multiple fields
- ❌ Sorting with empty result set

**Example Missing Test**:
```csharp
[Fact]
public async Task CsvAdapter_ListAsync_SortWithNullValues_HandlesCorrectly()
{
    // Test sorting when some records have null values
}

[Fact]
public async Task CsvAdapter_ListAsync_SortWithInvalidFormat_IgnoresSort()
{
    // Test that invalid sort strings are ignored
}
```

---

### 4. ⚠️ **Field Selection Edge Cases** (High Priority)

**Current**: Field selection exists but only basic test.

**Missing Tests**:
- ❌ Field selection with non-existent fields
- ❌ Field selection with empty array
- ❌ Field selection with null array
- ❌ Field selection with duplicate fields
- ❌ Field selection preserving ID field

**Example Missing Test**:
```csharp
[Fact]
public async Task CsvAdapter_ListAsync_SelectFields_WithNonExistentFields_IgnoresThem()
{
    // Test that requesting non-existent fields doesn't break
}

[Fact]
public async Task CsvAdapter_ListAsync_SelectFields_AlwaysIncludesId()
{
    // Test that ID is always included even if not requested
}
```

---

### 5. ⚠️ **CSV Special Characters** (High Priority)

**Current**: No tests for CSV edge cases with special characters.

**Missing Tests**:
- ❌ Values containing commas
- ❌ Values containing quotes (escaped and unescaped)
- ❌ Values containing newlines
- ❌ Values containing tabs
- ❌ Empty values
- ❌ Very long field values
- ❌ Unicode characters (emojis, non-ASCII)

**Example Missing Test**:
```csharp
[Fact]
public async Task CsvAdapter_CreateAsync_WithCommaInValue_HandlesCorrectly()
{
    var record = new Dictionary<string, object>
    {
        { "name", "Smith, John" },
        { "description", "Value with, comma" }
    };
    // Test that commas are properly escaped
}

[Fact]
public async Task CsvAdapter_CreateAsync_WithNewlineInValue_HandlesCorrectly()
{
    // Test multiline values
}
```

---

### 6. ⚠️ **Concurrency & File Locking** (High Priority)

**Current**: Basic lock tests exist, but concurrent scenarios are missing.

**Missing Tests**:
- ❌ Concurrent reads (should be allowed)
- ❌ Concurrent writes (one should wait)
- ❌ Read during write operation
- ❌ Write during read operation
- ❌ Lock timeout scenarios
- ❌ Lock file cleanup on process crash (simulated)

**Example Missing Test**:
```csharp
[Fact]
public async Task CsvAdapter_ConcurrentWrites_SerializeCorrectly()
{
    // Test multiple threads writing simultaneously
    // Should not corrupt data
}

[Fact]
public async Task CsvAdapter_ReadDuringWrite_HandlesGracefully()
{
    // Test reading while another operation is writing
}
```

---

## Medium Priority Gaps

### 7. ⚠️ **UpdateAsync Edge Cases**

**Missing Tests**:
- ❌ Update with empty dictionary (should be no-op or error?)
- ❌ Update adding new fields not in schema
- ❌ Update with null values
- ❌ Update preserving existing fields not in update dictionary
- ❌ Update with very large values

### 8. ⚠️ **CreateAsync Edge Cases**

**Missing Tests**:
- ❌ Create with null values
- ❌ Create with missing required fields (if any)
- ❌ Create with fields not in schema
- ❌ Create with very large record
- ❌ Create when file is locked by another process

### 9. ⚠️ **CsvFileHandler Edge Cases**

**Missing Tests**:
- ❌ CSV with BOM (Byte Order Mark)
- ❌ CSV with different encodings (UTF-8, UTF-16)
- ❌ CSV with missing trailing newline
- ❌ CSV with extra blank lines
- ❌ CSV with inconsistent column counts
- ❌ CSV with only headers (no data rows)
- ❌ CSV with malformed quotes

### 10. ⚠️ **Error Handling & Validation**

**Missing Tests**:
- ❌ Empty collection name
- ❌ Null collection name
- ❌ Empty/null ID in GetAsync/UpdateAsync/DeleteAsync
- ❌ Very long collection names
- ❌ Collection names with special characters (allowed? edge cases?)
- ❌ Invalid QueryOptions (negative limit, negative offset)
- ❌ Exception propagation from adapter to API controller

### 11. ⚠️ **Cancellation Token Support**

**Missing Tests**:
- ❌ All async methods accept CancellationToken but it's never tested
- ❌ Operation cancellation during long-running operations
- ❌ Cancellation during file I/O

**Example Missing Test**:
```csharp
[Fact]
public async Task CsvAdapter_ListAsync_WithCancellation_ThrowsOperationCanceledException()
{
    var cts = new CancellationTokenSource();
    cts.Cancel();
    
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _adapter.ListAsync("users", new QueryOptions(), cts.Token)
    );
}
```

---

## Low Priority Gaps

### 12. ⚠️ **Service Integration Tests**

**Current**: DefaultGenerator is tested in isolation.

**Missing Tests**:
- ❌ DefaultGenerator integration with CsvAdapter (when used)
- ❌ TypeConverter integration (when implemented)
- ❌ Service injection scenarios

### 13. ⚠️ **Performance & Load Tests**

**Missing Tests**:
- ❌ Large file handling (10,000+ records)
- ❌ Many concurrent operations
- ❌ Memory usage with large datasets
- ❌ Query performance with large offset values

### 14. ⚠️ **CsvFileHandler.Write Operations**

**Current**: AppendRecord is only tested via CreateAsync.

**Missing Tests**:
- ❌ Direct AppendRecord tests
- ❌ AppendRecord with special characters
- ❌ AppendRecord when file doesn't exist
- ❌ Write operations preserving header order

### 15. ⚠️ **ListCollectionsAsync Edge Cases**

**Missing Tests**:
- ❌ Empty directory
- ❌ Directory with only non-CSV files
- ❌ Directory with subdirectories
- ❌ Very long collection names

---

## Summary by Category

### Test Coverage by Component

| Component | Coverage | Status |
|-----------|----------|--------|
| Core Models | ✅ Excellent (100%) | Good |
| Core Enums | ✅ Excellent (100%) | Good |
| Core Exceptions | ✅ Excellent (100%) | Good |
| CsvAdapter (CRUD) | ✅ Good (90%) | Good |
| CsvAdapter (Query) | ⚠️ Fair (60%) | Missing edge cases |
| CsvFileHandler | ⚠️ Fair (70%) | Missing special chars |
| CsvFileLock | ✅ Good (85%) | Missing concurrency |
| DefaultGenerator | ✅ Good (90%) | Good |
| **API Controller** | ❌ **NONE (0%)** | **CRITICAL GAP** |

### Recommended Test Additions

**Immediate (Critical)**:
1. API Controller integration tests (15-20 tests)
2. Filtering edge cases (5-8 tests)
3. Sorting edge cases (5-8 tests)
4. Field selection edge cases (4-6 tests)

**Short Term (High Priority)**:
5. CSV special characters (6-8 tests)
6. Concurrency scenarios (5-7 tests)
7. UpdateAsync edge cases (4-6 tests)
8. Error handling validation (5-7 tests)

**Medium Term (Medium Priority)**:
9. CreateAsync edge cases (4-6 tests)
10. CsvFileHandler edge cases (6-8 tests)
11. Cancellation token tests (5-7 tests)
12. Service integration tests (3-5 tests)

**Long Term (Low Priority)**:
13. Performance tests (3-5 tests)
14. Load tests (2-3 tests)
15. Additional edge cases as discovered

---

## Estimated Test Count

**Current**: 78 tests  
**Recommended Additions**: ~80-100 additional tests  
**Target Total**: ~160-180 tests

**Priority Breakdown**:
- Critical: ~30-40 tests
- High: ~30-40 tests
- Medium: ~20-30 tests
- Low: ~10-15 tests

---

## Notes

1. **API Tests are Critical**: The REST API is the primary interface, yet it's completely untested. This should be the #1 priority.

2. **Edge Cases Matter**: While happy path is well-tested, edge cases and error scenarios are where bugs hide in production.

3. **Integration vs Unit**: Consider adding integration tests that test the full stack (API → Adapter → File System) in addition to unit tests.

4. **Performance Baseline**: Consider adding performance benchmarks to detect regressions as the codebase grows.

---

## Next Steps

1. **Create API.Tests project** and add controller tests
2. **Add filtering/sorting/field selection edge case tests**
3. **Add CSV special character handling tests**
4. **Add concurrency test scenarios**
5. **Review and prioritize remaining gaps**

