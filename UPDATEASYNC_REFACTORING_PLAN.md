# UpdateAsync Method Extraction Plan

## Current State
- **Cyclomatic Complexity:** 28
- **CRAP Score:** 812 (with 0% coverage on some paths)
- **Lines of Code:** ~188 lines
- **Responsibilities:** 8+ distinct operations

## Goal
Reduce cyclomatic complexity from 28 to <10 per method by extracting logical units into separate, testable methods.

---

## Extraction Plan

### Phase 1: Record Finding and Validation

#### 1.1 Extract: `FindRecordIndexById`
**Location:** Lines 210-224  
**Purpose:** Find the index of a record by ID in the records list  
**Signature:**
```csharp
private int FindRecordIndexById(List<Dictionary<string, object>> records, string id, string collection)
```
**Returns:** Record index, throws `KeyNotFoundException` if not found  
**Complexity Reduction:** -3 (removes loop + condition + exception)  
**Testability:** Easy - pure function with clear inputs/outputs

---

### Phase 2: Record Update Logic

#### 2.1 Extract: `ApplyUpdatesToRecord`
**Location:** Lines 227-248  
**Purpose:** Apply updates to a record and identify which fields are new  
**Signature:**
```csharp
private (Dictionary<string, object> updatedRecord, List<string> newFields) ApplyUpdatesToRecord(
    Dictionary<string, object> existingRecord, 
    Dictionary<string, object> updates, 
    string[] headers,
    CancellationToken ct)
```
**Returns:** Tuple of updated record and list of new field names  
**Complexity Reduction:** -4 (removes foreach + if/else + Contains check)  
**Testability:** High - pure transformation logic

---

### Phase 3: Header Management

#### 3.1 Extract: `AddNewFieldsToHeaders`
**Location:** Lines 253-262  
**Purpose:** Add new fields to the headers array  
**Signature:**
```csharp
private string[] AddNewFieldsToHeaders(string[] existingHeaders, List<string> newFields)
```
**Returns:** Updated headers array with new fields appended  
**Complexity Reduction:** -3 (removes if + foreach + Contains check)  
**Testability:** Very high - pure function, no side effects

---

### Phase 4: Default Value Generation

#### 4.1 Extract: `GenerateDefaultValueForField`
**Location:** Lines 267-287  
**Purpose:** Generate a default value for a new field based on its type  
**Signature:**
```csharp
private object GenerateDefaultValueForField(
    string fieldName, 
    object? sampleValue, 
    string collection)
```
**Returns:** Default value (string.Empty if no generator, or generated value)  
**Complexity Reduction:** -3 (removes if/else + null check + type inference)  
**Testability:** High - can test with/without DefaultGenerator

#### 4.2 Extract: `ApplyDefaultsToExistingRecords`
**Location:** Lines 265-297  
**Purpose:** Apply default values to all existing records for new fields  
**Signature:**
```csharp
private void ApplyDefaultsToExistingRecords(
    List<Dictionary<string, object>> allRecords,
    Dictionary<string, object> updatedRecord,
    List<string> newFields,
    string collection,
    CancellationToken ct)
```
**Returns:** void (modifies records in place)  
**Complexity Reduction:** -5 (removes nested foreach + if + default generation logic)  
**Testability:** Medium - tests side effects on records list

---

### Phase 5: Schema Management

#### 5.1 Extract: `CreateFieldDefinition`
**Location:** Lines 318-329  
**Purpose:** Create a FieldDefinition for a new field  
**Signature:**
```csharp
private FieldDefinition CreateFieldDefinition(
    string fieldName, 
    object? sampleValue, 
    string collection)
```
**Returns:** FieldDefinition with inferred type and default value  
**Complexity Reduction:** -2 (removes type inference + default generation)  
**Testability:** High - pure function

#### 5.2 Extract: `UpdateSchemaForNewFields`
**Location:** Lines 300-344  
**Purpose:** Update or create schema file with new field definitions  
**Signature:**
```csharp
private void UpdateSchemaForNewFields(
    string collection,
    List<string> newFields,
    Dictionary<string, object> updatedRecord,
    CancellationToken ct)
```
**Returns:** void  
**Complexity Reduction:** -8 (removes if + null check + foreach + Contains check + schema creation)  
**Testability:** Medium - requires schema manager, tests file operations

---

### Phase 6: CSV Writing

#### 6.1 Extract: `WriteRecordsToCsvFile`
**Location:** Lines 350-378 (the lambda inside RetryFileOperationAsync)  
**Purpose:** Write headers and records to CSV file  
**Signature:**
```csharp
private void WriteRecordsToCsvFile(
    string csvPath,
    string[] headers,
    List<Dictionary<string, object>> records,
    CancellationToken ct)
```
**Returns:** void  
**Complexity Reduction:** -4 (removes nested foreach loops + Contains check)  
**Testability:** Medium - requires file system, but isolated from retry logic

**Note:** This method will be called from within `RetryFileOperationAsync`, so the signature might need adjustment to work with the retry mechanism.

---

## Refactored UpdateAsync Structure

After extraction, `UpdateAsync` will look like:

```csharp
public async Task UpdateAsync(string collection, string id, Dictionary<string, object> data, CancellationToken ct = default)
{
    await Task.Yield();
    ct.ThrowIfCancellationRequested();

    var csvPath = GetCsvPath(collection);
    
    if (!File.Exists(csvPath))
    {
        throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
    }

    ct.ThrowIfCancellationRequested();
    var handler = new CsvFileHandler(csvPath);
    var headers = handler.ReadHeaders();
    var allRecords = handler.ReadRecords();

    ct.ThrowIfCancellationRequested();
    var recordIndex = FindRecordIndexById(allRecords, id, collection);
    var existingRecord = allRecords[recordIndex];

    ct.ThrowIfCancellationRequested();
    var (updatedRecord, newFields) = ApplyUpdatesToRecord(existingRecord, data, headers, ct);
    allRecords[recordIndex] = updatedRecord;

    if (newFields.Count > 0)
    {
        headers = AddNewFieldsToHeaders(headers, newFields);
        ApplyDefaultsToExistingRecords(allRecords, updatedRecord, newFields, collection, ct);
        UpdateSchemaForNewFields(collection, newFields, updatedRecord, ct);
    }

    ct.ThrowIfCancellationRequested();
    var lockPath = csvPath + ".lock";
    await RetryFileOperationAsync(async () =>
    {
        using (var fileLock = new CsvFileLock(lockPath))
        {
            WriteRecordsToCsvFile(csvPath, headers, allRecords, ct);
        }
    }, ct);
}
```

**Estimated Complexity:** ~6-8 (down from 28)  
**Estimated Lines:** ~35-40 (down from 188)

---

## Implementation Order

### Step 1: Low-Risk Extractions (Pure Functions)
1. `AddNewFieldsToHeaders` - No dependencies, pure function
2. `GenerateDefaultValueForField` - Simple logic, easy to test
3. `CreateFieldDefinition` - Uses existing helper, straightforward

### Step 2: Medium-Risk Extractions (Side Effects)
4. `FindRecordIndexById` - Simple but throws exception
5. `ApplyUpdatesToRecord` - Modifies record but isolated
6. `ApplyDefaultsToExistingRecords` - Modifies list but clear purpose

### Step 3: Higher-Risk Extractions (File I/O)
7. `UpdateSchemaForNewFields` - File operations, requires schema manager
8. `WriteRecordsToCsvFile` - File operations, needs to work with retry mechanism

---

## Testing Strategy

### Unit Tests for Each Extracted Method
1. **FindRecordIndexById**
   - Record found at various positions
   - Record not found (exception)
   - Multiple records with same ID (first match)
   - Empty records list

2. **ApplyUpdatesToRecord**
   - Update existing fields only
   - Add new fields only
   - Mix of existing and new fields
   - Empty updates dictionary
   - Fields already in headers vs. not in headers

3. **AddNewFieldsToHeaders**
   - Single new field
   - Multiple new fields
   - Duplicate field names (should not add twice)
   - Empty newFields list

4. **GenerateDefaultValueForField**
   - With DefaultGenerator (various types)
   - Without DefaultGenerator (returns empty string)
   - Null sample value
   - Various field types (string, int, bool, etc.)

5. **ApplyDefaultsToExistingRecords**
   - Single new field
   - Multiple new fields
   - Records already containing field (should skip)
   - Empty records list
   - With/without DefaultGenerator

6. **CreateFieldDefinition**
   - Various field types
   - With/without DefaultGenerator
   - Null sample value

7. **UpdateSchemaForNewFields**
   - Schema exists, add fields
   - Schema doesn't exist, create new
   - Schema exists but Fields is null
   - With/without SchemaManager
   - Duplicate field names (should not add twice)

8. **WriteRecordsToCsvFile**
   - Normal write operation
   - Empty records list
   - Records with missing fields (should use empty string)
   - Cancellation during write

---

## Expected Improvements

### Complexity Reduction
- **Before:** 28 (single method)
- **After:** 
  - `UpdateAsync`: ~6-8
  - Extracted methods: 1-4 each
  - **Total reduction:** ~70% in main method

### CRAP Score Reduction
- **Before:** 812 (with 0% coverage)
- **After:** 
  - With 90%+ coverage on all methods: ~10-20 per method
  - Main method: ~5-10
  - **Total improvement:** ~95% reduction

### Maintainability
- Each method has single responsibility
- Easier to understand and modify
- Easier to test in isolation
- Better code reusability

### Test Coverage
- Each extracted method can be tested independently
- Edge cases easier to cover
- Mock dependencies more easily
- Integration tests can focus on coordination

---

## Risk Assessment

### Low Risk
- `AddNewFieldsToHeaders`
- `GenerateDefaultValueForField`
- `CreateFieldDefinition`
- `FindRecordIndexById`

### Medium Risk
- `ApplyUpdatesToRecord`
- `ApplyDefaultsToExistingRecords`

### Higher Risk
- `UpdateSchemaForNewFields` (file I/O, schema manager dependency)
- `WriteRecordsToCsvFile` (file I/O, retry mechanism integration)

### Mitigation
- Implement in phases (low risk first)
- Maintain existing tests throughout
- Add new tests for extracted methods before removing old code
- Use feature flags if needed for gradual rollout

---

## Notes

1. **Cancellation Tokens:** Most extracted methods should accept `CancellationToken` to maintain cancellation support throughout the operation.

2. **Error Handling:** Consider whether extracted methods should throw exceptions or return error results. Current approach (throwing exceptions) is fine for most cases.

3. **Performance:** Extraction should not significantly impact performance. Most methods are simple transformations. File I/O operations remain the same.

4. **Backward Compatibility:** This refactoring should not change the public API or behavior of `UpdateAsync`. All existing tests should continue to pass.

5. **Code Duplication:** Some logic (like default value generation) appears in multiple places. After extraction, we can ensure consistency.

