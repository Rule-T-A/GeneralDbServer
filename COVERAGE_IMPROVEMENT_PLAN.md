# Test Coverage Improvement Plan - High CRAP Score Methods

**Created**: December 2025  
**Target**: Improve test coverage for all methods with CRAP score > 30  
**Status**: 🔄 **IN PROGRESS** - Sections 1-5 Completed (December 2025)

---

## Quick Start for Agent

**Current Coverage Status:**
- Overall: **91%** line coverage, **75.5%** branch coverage
- **Target**: >90% line ✅, >85% branch (currently 75.5% - needs improvement)

**Focus Areas (CRAP Score > 30):**
1. **CsvAdapter.UpdateAsync()** - CRAP: 812, Complexity: 28 🔴 CRITICAL
2. **CsvAdapter.DeleteAsync()** - CRAP: 600, Complexity: 24 🔴 CRITICAL
3. **DataController.UploadCsvFile()** - CRAP: 420, Complexity: 20 🔴 CRITICAL
4. **CsvAdapter.SortRecords()** - CRAP: 210, Complexity: 14 🟡 HIGH
5. **CsvAdapter.InferFieldType()** - CRAP: 111, Complexity: 24 🟡 HIGH
6. **CsvAdapter.BulkOperationAsync()** - CRAP: 93, Complexity: 90 🟡 HIGH
7. **CsvAdapter.AggregateAsync()** - CRAP: 60, Complexity: 58 🟡 HIGH
8. **CsvAdapter.ConvertToNumeric()** - CRAP: 59, Complexity: 18 🟡 HIGH
9. **CsvAdapter.UpdateAsync() (overload)** - CRAP: 52, Complexity: 52 🟡 HIGH
10. **TypeConverter.PerformConversion()** - CRAP: 52, Complexity: 52 🟡 HIGH
11. **FilterEvaluator.CompareValues()** - CRAP: 50, Complexity: 50 🟡 HIGH
12. **TypeConverter.HandleConversionFailure()** - CRAP: 43, Complexity: 11 🟢 MEDIUM
13. **CsvAdapter.GetAsync()** - CRAP: 42, Complexity: 6 🟢 MEDIUM
14. **CsvAdapter.ValidateCollectionName()** - CRAP: 39, Complexity: 12 🟢 MEDIUM
15. **DefaultGenerator.GeneratePatternBasedDefault()** - CRAP: 32, Complexity: 32 🟢 MEDIUM

**Verification Command**: 
```bash
dotnet test /p:CollectCoverage=true
reportgenerator -reports:"**/coverage.json" -targetdir:"coverage/html" -reporttypes:"Html"
```

---

## Pre-Implementation Checklist

- [ ] Verify all test projects have `coverlet.msbuild` package (already configured)
- [ ] Run baseline coverage: `dotnet test /p:CollectCoverage=true`
- [ ] Review existing test patterns in test files
- [ ] Ensure test data files exist in `testdata/` directory

---

## 1. CsvAdapter.UpdateAsync() - CRAP: 812 🔴 CRITICAL

**Priority**: 🔴 CRITICAL  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 191

### Task 1.1: UpdateAsync Branch Coverage

**Status**: ✅ COMPLETED - December 2025

#### Step 1.1.1: Error Path Coverage ✅ COMPLETED
- [x] **Step 1**: Read `CsvAdapter.cs` lines 191-370 to understand all branches
- [x] **Step 2**: Add test: `CsvAdapter_UpdateAsync_WithCollectionNotFound_ThrowsFileNotFoundException`
  - Test when CSV file doesn't exist
- [x] **Step 3**: Add test: `CsvAdapter_UpdateAsync_WithRecordNotFound_ThrowsKeyNotFoundException`
  - Test when record ID doesn't exist
- [x] **Step 4**: Add test: `CsvAdapter_UpdateAsync_WithCancellationBeforeRead_ThrowsCancellationException`
  - Test cancellation at start
- [x] **Step 5**: Note: Cancellation during update loop is difficult to test without artificial delays
- [x] **Step 6**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "UpdateAsync"` ✅

#### Step 1.1.2: New Field Addition Branches ✅ COMPLETED
- [x] **Step 1**: Add test: `CsvAdapter_UpdateAsync_WithMultipleNewFields_AddsAllFields`
  - Test adding multiple new fields in one update
- [x] **Step 2**: Add test: `CsvAdapter_UpdateAsync_WithNewFieldAndExistingField_MixedUpdate`
  - Test updating existing field and adding new field simultaneously
- [x] **Step 3**: Note: Header already exists edge case is covered by duplicate field test in SchemaManager section
- [x] **Step 4**: Run tests and verify coverage improvement ✅

#### Step 1.1.3: DefaultGenerator Integration Branches ✅ COMPLETED
- [x] **Step 1**: Note: `CsvAdapter_UpdateAsync_WithDefaultGenerator_UsesDefaultGenerator` already exists
- [x] **Step 2**: Add test: `CsvAdapter_UpdateAsync_WithDefaultGenerator_NullValue_HandlesGracefully`
  - Test DefaultGenerator returning null
- [x] **Step 3**: Add test: `CsvAdapter_UpdateAsync_WithDefaultGenerator_TypeInference_WorksCorrectly`
  - Test type inference for new fields with various types
- [x] **Step 4**: Run tests and verify coverage improvement ✅

#### Step 1.1.4: SchemaManager Integration Branches ✅ COMPLETED
- [x] **Step 1**: Note: `CsvAdapter_UpdateAsync_WithSchemaManager_UpdatesSchemaFile` already exists
- [x] **Step 2**: Add test: `CsvAdapter_UpdateAsync_WithSchemaManager_NoExistingSchema_CreatesSchema`
  - Test schema file creation when no schema exists
- [x] **Step 3**: Add test: `CsvAdapter_UpdateAsync_WithSchemaManager_NullFieldsList_HandlesGracefully`
  - Test when schema exists but Fields is null
- [x] **Step 4**: Add test: `CsvAdapter_UpdateAsync_WithSchemaManager_DuplicateField_NoDuplicateInSchema`
  - Test that duplicate fields aren't added to schema
- [x] **Step 5**: Run tests and verify coverage improvement ✅

#### Step 1.1.5: File Write Error Paths
- [ ] **Step 1**: Add test: `CsvAdapter_UpdateAsync_WithFileLock_RetriesOperation`
  - Test retry logic on file lock (may require mocking) - Deferred (complex to test)
- [ ] **Step 2**: Add test: `CsvAdapter_UpdateAsync_WithRetryExhaustion_ThrowsException`
  - Test when all retries fail - Deferred (complex to test)
- [ ] **Step 3**: Note: File locking retry tests are complex and may require integration test setup

---

## 2. CsvAdapter.DeleteAsync() - CRAP: 600 🔴 CRITICAL

**Priority**: 🔴 CRITICAL  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 373

### Task 2.1: DeleteAsync Branch Coverage

**Status**: ✅ COMPLETED - December 2025

#### Step 2.1.1: Error Path Coverage ✅ COMPLETED
- [x] **Step 1**: Read `CsvAdapter.cs` lines 373-445 to understand all branches
- [x] **Step 2**: Add test: `CsvAdapter_DeleteAsync_WithCollectionNotFound_ThrowsFileNotFoundException`
  - Test when CSV file doesn't exist
- [x] **Step 3**: Add test: `CsvAdapter_DeleteAsync_WithRecordNotFound_ThrowsKeyNotFoundException`
  - Test when record ID doesn't exist
- [x] **Step 4**: Add test: `CsvAdapter_DeleteAsync_WithCancellationBeforeRead_ThrowsCancellationException`
  - Test cancellation at start
- [x] **Step 5**: Note: Cancellation during search/write loops is difficult to test without artificial delays
- [x] **Step 6**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "DeleteAsync"` ✅

#### Step 2.1.2: Edge Cases ✅ COMPLETED
- [x] **Step 1**: Add test: `CsvAdapter_DeleteAsync_WithLastRecord_FileStillExists`
  - Test deleting the last record (file should still exist with headers)
- [x] **Step 2**: Add test: `CsvAdapter_DeleteAsync_WithRecordWithoutId_HandlesGracefully`
  - Test deleting when record exists but has no "id" field
- [x] **Step 3**: Add test: `CsvAdapter_DeleteAsync_WithMultipleRecords_OnlyDeletesTarget`
  - Test that only the target record is deleted
- [x] **Step 4**: Run tests and verify coverage improvement ✅

#### Step 2.1.3: File Write Error Paths
- [ ] **Step 1**: Add test: `CsvAdapter_DeleteAsync_WithFileLock_RetriesOperation`
  - Test retry logic on file lock - Deferred (complex to test)
- [ ] **Step 2**: Add test: `CsvAdapter_DeleteAsync_WithRetryExhaustion_ThrowsException`
  - Test when all retries fail - Deferred (complex to test)
- [ ] **Step 3**: Note: File locking retry tests are complex and may require integration test setup

---

## 3. DataController.UploadCsvFile() - CRAP: 420 🔴 CRITICAL

**Priority**: 🔴 CRITICAL  
**Test Project**: `DataAbstractionAPI.API.Tests`  
**File to Modify**: `DataControllerTests.cs`  
**Method Location**: `DataController.cs` line 358

### Task 3.1: UploadCsvFile Branch Coverage

**Status**: ✅ COMPLETED - December 2025

#### Step 3.1.1: Validation Error Paths ✅ COMPLETED
- [x] **Step 1**: Read `DataController.cs` lines 358-404 to understand all branches
- [x] **Step 2**: Add test: `DataController_UploadCsvFile_WithNullRequest_ReturnsBadRequest`
  - Test null request handling
- [x] **Step 3**: Add test: `DataController_UploadCsvFile_WithEmptyCollectionName_ReturnsBadRequest`
  - Test empty collection name
- [x] **Step 4**: Add test: `DataController_UploadCsvFile_WithWhitespaceCollectionName_ReturnsBadRequest`
  - Test whitespace-only collection name
- [x] **Step 5**: Add test: `DataController_UploadCsvFile_WithNullFile_ReturnsBadRequest`
  - Test null file
- [x] **Step 6**: Add test: `DataController_UploadCsvFile_WithEmptyFile_ReturnsBadRequest`
  - Test empty file (Length == 0)
- [x] **Step 7**: Run tests: `dotnet test DataAbstractionAPI.API.Tests --filter "UploadCsvFile"` ✅

#### Step 3.1.2: File Extension Validation ✅ COMPLETED
- [x] **Step 1**: Add test: `DataController_UploadCsvFile_WithNonCsvExtension_ReturnsBadRequest`
  - Test .txt, .xlsx, .json, etc.
- [x] **Step 2**: Add test: `DataController_UploadCsvFile_WithUppercaseExtension_AcceptsFile`
  - Test .CSV (uppercase)
- [x] **Step 3**: Add test: `DataController_UploadCsvFile_WithMixedCaseExtension_AcceptsFile`
  - Test .Csv (mixed case)
- [x] **Step 4**: Run tests and verify coverage improvement ✅

#### Step 3.1.3: Collection Name Security Validation ✅ COMPLETED
- [x] **Step 1**: Add test: `DataController_UploadCsvFile_WithPathTraversal_ReturnsBadRequest`
  - Test collection name with ".."
- [x] **Step 2**: Add test: `DataController_UploadCsvFile_WithForwardSlash_ReturnsBadRequest`
  - Test collection name with "/"
- [x] **Step 3**: Add test: `DataController_UploadCsvFile_WithBackslash_ReturnsBadRequest`
  - Test collection name with "\"
- [x] **Step 4**: Add test: `DataController_UploadCsvFile_WithAbsolutePath_ReturnsBadRequest`
  - Test absolute path (Path.IsPathRooted)
- [x] **Step 5**: Run tests and verify coverage improvement ✅

#### Step 3.1.4: Directory and File Operations ✅ COMPLETED
- [x] **Step 1**: Note: Directory creation is tested implicitly in valid file tests
- [x] **Step 2**: Add test: `DataController_UploadCsvFile_WithExistingFile_OverwritesFile`
  - Test that existing CSV file is overwritten
- [x] **Step 3**: Add test: `DataController_UploadCsvFile_WithValidFile_ReturnsSuccess`
  - Test successful upload
- [x] **Step 4**: Add test: `DataController_UploadCsvFile_WithValidFile_ReturnsCorrectPath`
  - Test that response contains correct file path
- [x] **Step 5**: Run tests and verify coverage improvement ✅

---

## 4. CsvAdapter.SortRecords() - CRAP: 210 🟡 HIGH

**Priority**: 🟡 HIGH  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 612 (private method)

### Task 4.1: SortRecords Branch Coverage

**Status**: ✅ COMPLETED - December 2025

**Note**: This is a private method, so test through public methods that use it (ListAsync with Sort option).

#### Step 4.1.1: Sort Parsing Branches ✅ COMPLETED
- [x] **Step 1**: Read `CsvAdapter.cs` lines 612-634 to understand all branches
- [x] **Step 2**: Add test: `CsvAdapter_ListAsync_WithSortAscending_SortsCorrectly`
  - Test "field:asc" format
- [x] **Step 3**: Add test: `CsvAdapter_ListAsync_WithSortDescending_SortsCorrectly`
  - Test "field:desc" format
- [x] **Step 4**: Add test: `CsvAdapter_ListAsync_WithInvalidSortFormat_ReturnsUnsorted`
  - Test invalid formats: "field", "field:", ":asc", "field:invalid", etc.
- [x] **Step 5**: Add test: `CsvAdapter_ListAsync_WithSortMissingField_HandlesGracefully`
  - Test sorting by field that doesn't exist in records
- [x] **Step 6**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "Sort"` ✅

#### Step 4.1.2: Sort Edge Cases ✅ COMPLETED
- [x] **Step 1**: Add test: `CsvAdapter_ListAsync_WithSortNullValues_HandlesGracefully`
  - Test sorting when some records have null values for sort field
- [x] **Step 2**: Note: Numeric sorting as strings is covered by existing sort tests
- [x] **Step 3**: Add test: `CsvAdapter_ListAsync_WithSortEmptyString_HandlesGracefully`
  - Test sorting when field value is empty string
- [x] **Step 4**: Run tests and verify coverage improvement ✅

---

## 5. CsvAdapter.InferFieldType() - CRAP: 111 🟡 HIGH

**Priority**: 🟡 HIGH  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 699 (private method)

### Task 5.1: InferFieldType Branch Coverage

**Status**: ✅ COMPLETED - December 2025

**Note**: This is a private method, test through public methods that use it (GetSchemaAsync, UpdateAsync).

#### Step 5.1.1: Type Inference Branches ✅ COMPLETED
- [x] **Step 1**: Read `CsvAdapter.cs` lines 699-716 to understand all branches
- [x] **Step 2**: Add test: `CsvAdapter_GetSchemaAsync_WithStringValue_InfersStringType`
  - Test string type inference
- [x] **Step 3**: Add test: `CsvAdapter_GetSchemaAsync_WithIntegerValue_InfersIntegerType`
  - Test int, long, short type inference (tested through UpdateAsync with typed values)
- [x] **Step 4**: Note: Float type inference is tested through UpdateAsync with DefaultGenerator
- [x] **Step 5**: Note: Boolean type inference is tested through UpdateAsync with DefaultGenerator
- [x] **Step 6**: Note: DateTime type inference would require DateTime values in CSV (covered by UpdateAsync)
- [x] **Step 7**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "InferFieldType|GetSchemaAsync"` ✅

#### Step 5.1.2: Edge Cases ✅ COMPLETED
- [x] **Step 1**: Add test: `CsvAdapter_GetSchemaAsync_WithNullValue_ReturnsStringType`
  - Test null value defaults to String
- [x] **Step 2**: Note: Array/Object type inference would require complex test setup (deferred)
- [x] **Step 3**: Note: Object type inference (fallback) is implicit in other tests
- [x] **Step 4**: Add test: `CsvAdapter_UpdateAsync_WithMixedTypes_UsesFirstNonNull`
  - Test InferFieldTypeFromData with mixed types
- [x] **Step 5**: Run tests and verify coverage improvement ✅

---

## 6. CsvAdapter.BulkOperationAsync() - CRAP: 93 🟡 HIGH

**Priority**: 🟡 HIGH  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 736

### Task 6.1: BulkOperationAsync Branch Coverage

**Status**: ⏳ PENDING

#### Step 6.1.1: Validation Branches
- [ ] **Step 1**: Read `CsvAdapter.cs` lines 736-1012 to understand all branches
- [ ] **Step 2**: Add test: `CsvAdapter_BulkOperationAsync_WithInvalidAction_ThrowsArgumentException`
  - Test invalid action values
- [ ] **Step 3**: Add test: `CsvAdapter_BulkOperationAsync_WithNullRecords_ThrowsArgumentException`
  - Test null records list
- [ ] **Step 4**: Add test: `CsvAdapter_BulkOperationAsync_WithEmptyRecords_ThrowsArgumentException`
  - Test empty records list
- [ ] **Step 5**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "BulkOperationAsync"`

#### Step 6.1.2: Atomic Mode Additional Branches
- [ ] **Step 1**: Review existing atomic mode tests (from previous plan)
- [ ] **Step 2**: Add test: `CsvAdapter_BulkOperationAsync_Atomic_WithFileLock_RetriesOperation`
  - Test retry logic in atomic mode
- [ ] **Step 3**: Add test: `CsvAdapter_BulkOperationAsync_Atomic_WithCancellation_ThrowsException`
  - Test cancellation in atomic mode
- [ ] **Step 4**: Run tests and verify coverage improvement

#### Step 6.1.3: Best-Effort Mode Additional Branches
- [ ] **Step 1**: Review existing best-effort mode tests (from previous plan)
- [ ] **Step 2**: Add test: `CsvAdapter_BulkOperationAsync_BestEffort_WithFileLock_RetriesOperation`
  - Test retry logic in best-effort mode
- [ ] **Step 3**: Add test: `CsvAdapter_BulkOperationAsync_BestEffort_WithCancellation_ThrowsException`
  - Test cancellation in best-effort mode
- [ ] **Step 4**: Run tests and verify coverage improvement

---

## 7. CsvAdapter.AggregateAsync() - CRAP: 60 🟡 HIGH

**Priority**: 🟡 HIGH  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 1043

### Task 7.1: AggregateAsync Branch Coverage

**Status**: ⏳ PENDING

#### Step 7.1.1: Additional Error Paths
- [ ] **Step 1**: Review existing AggregateAsync tests (from previous plan)
- [ ] **Step 2**: Add test: `CsvAdapter_AggregateAsync_WithNullGroupBy_ThrowsException`
  - Test null groupBy field
- [ ] **Step 3**: Add test: `CsvAdapter_AggregateAsync_WithInvalidGroupBy_HandlesGracefully`
  - Test groupBy field that doesn't exist
- [ ] **Step 4**: Add test: `CsvAdapter_AggregateAsync_WithCancellation_ThrowsException`
  - Test cancellation at various points
- [ ] **Step 5**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "AggregateAsync"`

#### Step 7.1.2: Aggregate Function Edge Cases
- [ ] **Step 1**: Add test: `CsvAdapter_AggregateAsync_WithCountOnEmptyGroup_ReturnsZero`
  - Test count on empty groups
- [ ] **Step 2**: Add test: `CsvAdapter_AggregateAsync_WithSumOnNullValues_HandlesGracefully`
  - Test sum with null values
- [ ] **Step 3**: Add test: `CsvAdapter_AggregateAsync_WithAvgOnNullValues_HandlesGracefully`
  - Test average with null values
- [ ] **Step 4**: Run tests and verify coverage improvement

---

## 8. CsvAdapter.ConvertToNumeric() - CRAP: 59 🟡 HIGH

**Priority**: 🟡 HIGH  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 1199 (private method)

### Task 8.1: ConvertToNumeric Branch Coverage

**Status**: ⏳ PENDING

**Note**: This is a private method, test through AggregateAsync which uses it.

#### Step 8.1.1: Numeric Type Conversions
- [ ] **Step 1**: Read `CsvAdapter.cs` lines 1199-1213 to understand all branches
- [ ] **Step 2**: Add test: `CsvAdapter_AggregateAsync_WithIntValue_ConvertsToNumeric`
  - Test int conversion
- [ ] **Step 3**: Add test: `CsvAdapter_AggregateAsync_WithLongValue_ConvertsToNumeric`
  - Test long conversion
- [ ] **Step 4**: Add test: `CsvAdapter_AggregateAsync_WithShortValue_ConvertsToNumeric`
  - Test short conversion
- [ ] **Step 5**: Add test: `CsvAdapter_AggregateAsync_WithDoubleValue_ConvertsToNumeric`
  - Test double conversion
- [ ] **Step 6**: Add test: `CsvAdapter_AggregateAsync_WithFloatValue_ConvertsToNumeric`
  - Test float conversion
- [ ] **Step 7**: Add test: `CsvAdapter_AggregateAsync_WithDecimalValue_ConvertsToNumeric`
  - Test decimal conversion
- [ ] **Step 8**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "AggregateAsync"`

#### Step 8.1.2: String and Edge Cases
- [ ] **Step 1**: Add test: `CsvAdapter_AggregateAsync_WithNumericString_ConvertsToNumeric`
  - Test string that can be parsed as double
- [ ] **Step 2**: Add test: `CsvAdapter_AggregateAsync_WithNonNumericString_ReturnsNull`
  - Test string that cannot be parsed
- [ ] **Step 3**: Add test: `CsvAdapter_AggregateAsync_WithNullValue_ReturnsNull`
  - Test null value
- [ ] **Step 4**: Add test: `CsvAdapter_AggregateAsync_WithNonNumericType_ReturnsNull`
  - Test non-numeric types (bool, DateTime, etc.)
- [ ] **Step 5**: Run tests and verify coverage improvement

---

## 9. TypeConverter.PerformConversion() - CRAP: 52 🟡 HIGH

**Priority**: 🟡 HIGH  
**Test Project**: `DataAbstractionAPI.Services.Tests`  
**File to Modify**: `TypeConverterTests.cs`  
**Method Location**: `TypeConverter.cs` line 56 (private method)

### Task 9.1: PerformConversion Branch Coverage

**Status**: ⏳ PENDING

#### Step 9.1.1: Conversion Type Combinations
- [ ] **Step 1**: Read `TypeConverter.cs` lines 56-138 to understand all conversion branches
- [ ] **Step 2**: Add test: `TypeConverter_Convert_StringToInteger_ConvertsCorrectly`
  - Test String -> Integer
- [ ] **Step 3**: Add test: `TypeConverter_Convert_StringToFloat_ConvertsCorrectly`
  - Test String -> Float
- [ ] **Step 4**: Add test: `TypeConverter_Convert_StringToBoolean_ConvertsCorrectly`
  - Test String -> Boolean
- [ ] **Step 5**: Add test: `TypeConverter_Convert_StringToDateTime_ConvertsCorrectly`
  - Test String -> DateTime
- [ ] **Step 6**: Add test: `TypeConverter_Convert_StringToDate_ConvertsCorrectly`
  - Test String -> Date
- [ ] **Step 7**: Run tests: `dotnet test DataAbstractionAPI.Services.Tests --filter "TypeConverter"`

#### Step 9.1.2: Reverse Conversions
- [ ] **Step 1**: Add test: `TypeConverter_Convert_IntegerToString_ConvertsCorrectly`
  - Test Integer -> String
- [ ] **Step 2**: Add test: `TypeConverter_Convert_IntegerToFloat_ConvertsCorrectly`
  - Test Integer -> Float
- [ ] **Step 3**: Add test: `TypeConverter_Convert_IntegerToBoolean_ConvertsCorrectly`
  - Test Integer -> Boolean
- [ ] **Step 4**: Add test: `TypeConverter_Convert_FloatToString_ConvertsCorrectly`
  - Test Float -> String
- [ ] **Step 5**: Add test: `TypeConverter_Convert_FloatToInteger_ConvertsCorrectly`
  - Test Float -> Integer (truncates)
- [ ] **Step 6**: Run tests and verify coverage improvement

#### Step 9.1.3: Unsupported Conversions
- [ ] **Step 1**: Add test: `TypeConverter_Convert_UnsupportedConversion_ThrowsException`
  - Test unsupported conversion combinations
- [ ] **Step 2**: Run tests and verify coverage improvement

---

## 10. FilterEvaluator.CompareValues() - CRAP: 50 🟡 HIGH

**Priority**: 🟡 HIGH  
**Test Project**: `DataAbstractionAPI.Services.Tests`  
**File to Modify**: `FilterEvaluatorTests.cs`  
**Method Location**: `FilterEvaluator.cs` line 156 (private method)

### Task 10.1: CompareValues Branch Coverage

**Status**: ⏳ PENDING

#### Step 10.1.1: Operator Coverage
- [ ] **Step 1**: Read `FilterEvaluator.cs` lines 156-173 to understand all operators
- [ ] **Step 2**: Add test: `FilterEvaluator_Evaluate_WithEqOperator_ComparesCorrectly`
  - Test "eq" operator
- [ ] **Step 3**: Add test: `FilterEvaluator_Evaluate_WithNeOperator_ComparesCorrectly`
  - Test "ne" operator
- [ ] **Step 4**: Add test: `FilterEvaluator_Evaluate_WithGtOperator_ComparesCorrectly`
  - Test "gt" operator
- [ ] **Step 5**: Add test: `FilterEvaluator_Evaluate_WithGteOperator_ComparesCorrectly`
  - Test "gte" operator
- [ ] **Step 6**: Add test: `FilterEvaluator_Evaluate_WithLtOperator_ComparesCorrectly`
  - Test "lt" operator
- [ ] **Step 7**: Add test: `FilterEvaluator_Evaluate_WithLteOperator_ComparesCorrectly`
  - Test "lte" operator
- [ ] **Step 8**: Run tests: `dotnet test DataAbstractionAPI.Services.Tests --filter "FilterEvaluator"`

#### Step 10.1.2: Array and String Operators
- [ ] **Step 1**: Add test: `FilterEvaluator_Evaluate_WithInOperator_ComparesCorrectly`
  - Test "in" operator
- [ ] **Step 2**: Add test: `FilterEvaluator_Evaluate_WithNinOperator_ComparesCorrectly`
  - Test "nin" operator
- [ ] **Step 3**: Add test: `FilterEvaluator_Evaluate_WithContainsOperator_ComparesCorrectly`
  - Test "contains" operator
- [ ] **Step 4**: Add test: `FilterEvaluator_Evaluate_WithStartsWithOperator_ComparesCorrectly`
  - Test "startswith" operator
- [ ] **Step 5**: Add test: `FilterEvaluator_Evaluate_WithEndsWithOperator_ComparesCorrectly`
  - Test "endswith" operator
- [ ] **Step 6**: Add test: `FilterEvaluator_Evaluate_WithUnsupportedOperator_ThrowsException`
  - Test unsupported operator
- [ ] **Step 7**: Run tests and verify coverage improvement

---

## 11. TypeConverter.HandleConversionFailure() - CRAP: 43 🟢 MEDIUM

**Priority**: 🟢 MEDIUM  
**Test Project**: `DataAbstractionAPI.Services.Tests`  
**File to Modify**: `TypeConverterTests.cs`  
**Method Location**: `TypeConverter.cs` line 301 (private method)

### Task 11.1: HandleConversionFailure Branch Coverage

**Status**: ⏳ PENDING

#### Step 11.1.1: Conversion Strategy Branches
- [ ] **Step 1**: Read `TypeConverter.cs` lines 301-356 to understand all strategy branches
- [ ] **Step 2**: Add test: `TypeConverter_Convert_WithFailOnErrorStrategy_ThrowsException`
  - Test FailOnError strategy
- [ ] **Step 3**: Add test: `TypeConverter_Convert_WithCastStrategy_ThrowsException`
  - Test Cast strategy (same as FailOnError)
- [ ] **Step 4**: Add test: `TypeConverter_Convert_WithSetNullStrategy_ReturnsNull`
  - Test SetNull strategy
- [ ] **Step 5**: Add test: `TypeConverter_Convert_WithTruncateStrategy_ThrowsException`
  - Test Truncate strategy
- [ ] **Step 6**: Run tests: `dotnet test DataAbstractionAPI.Services.Tests --filter "TypeConverter"`

#### Step 11.1.2: Exception Handling
- [ ] **Step 1**: Add test: `TypeConverter_Convert_WithConversionException_PreservesException`
  - Test that ConversionException is preserved
- [ ] **Step 2**: Add test: `TypeConverter_Convert_WithOtherException_WrapsInConversionException`
  - Test that other exceptions are wrapped
- [ ] **Step 3**: Run tests and verify coverage improvement

---

## 12. CsvAdapter.GetAsync() - CRAP: 42 🟢 MEDIUM

**Priority**: 🟢 MEDIUM  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 110

### Task 12.1: GetAsync Branch Coverage

**Status**: ⏳ PENDING

#### Step 12.1.1: Additional Edge Cases
- [ ] **Step 1**: Review existing GetAsync tests
- [ ] **Step 2**: Add test: `CsvAdapter_GetAsync_WithRecordWithoutId_ThrowsNotFoundException`
  - Test when record exists but has no "id" field
- [ ] **Step 3**: Add test: `CsvAdapter_GetAsync_WithNullIdValue_HandlesGracefully`
  - Test when record has "id" field but value is null
- [ ] **Step 4**: Add test: `CsvAdapter_GetAsync_WithCancellationDuringRead_ThrowsCancellationException`
  - Test cancellation during file read
- [ ] **Step 5**: Add test: `CsvAdapter_GetAsync_WithCancellationDuringSearch_ThrowsCancellationException`
  - Test cancellation during record search
- [ ] **Step 6**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "GetAsync"`

---

## 13. CsvAdapter.ValidateCollectionName() - CRAP: 39 🟢 MEDIUM

**Priority**: 🟢 MEDIUM  
**Test Project**: `DataAbstractionAPI.Adapters.Tests`  
**File to Modify**: `CsvAdapterTests.cs`  
**Method Location**: `CsvAdapter.cs` line 563

### Task 13.1: ValidateCollectionName Branch Coverage

**Status**: ⏳ PENDING

#### Step 13.1.1: Additional Edge Cases
- [ ] **Step 1**: Review existing ValidateCollectionName tests (from previous plan)
- [ ] **Step 2**: Add test: `CsvAdapter_WithCollectionNameContainingSpecialChars_ThrowsArgumentException`
  - Test special characters that might cause issues
- [ ] **Step 3**: Add test: `CsvAdapter_WithVeryLongCollectionName_ThrowsArgumentException`
  - Test extremely long collection names
- [ ] **Step 4**: Add test: `CsvAdapter_WithCollectionNameContainingUnicode_HandlesGracefully`
  - Test Unicode characters (should be allowed or rejected consistently)
- [ ] **Step 5**: Run tests: `dotnet test DataAbstractionAPI.Adapters.Tests --filter "ValidateCollectionName"`

---

## 14. DefaultGenerator.GeneratePatternBasedDefault() - CRAP: 32 🟢 MEDIUM

**Priority**: 🟢 MEDIUM  
**Test Project**: `DataAbstractionAPI.Services.Tests`  
**File to Modify**: `DefaultGeneratorTests.cs`  
**Method Location**: `DefaultGenerator.cs` line 93 (private method)

### Task 14.1: GeneratePatternBasedDefault Branch Coverage

**Status**: ⏳ PENDING

#### Step 14.1.1: Pattern Matching Branches
- [ ] **Step 1**: Read `DefaultGenerator.cs` lines 93-125 to understand all pattern branches
- [ ] **Step 2**: Add test: `DefaultGenerator_GenerateDefault_WithIsPrefix_ReturnsFalse`
  - Test "is_" prefix for boolean
- [ ] **Step 3**: Add test: `DefaultGenerator_GenerateDefault_WithHasPrefix_ReturnsFalse`
  - Test "has_" prefix for boolean
- [ ] **Step 4**: Add test: `DefaultGenerator_GenerateDefault_WithCanPrefix_ReturnsFalse`
  - Test "can_" prefix for boolean
- [ ] **Step 5**: Add test: `DefaultGenerator_GenerateDefault_WithAtSuffix_ReturnsDateTime`
  - Test "_at" suffix for DateTime
- [ ] **Step 6**: Add test: `DefaultGenerator_GenerateDefault_WithDateSuffix_ReturnsDateTime`
  - Test "_date" suffix for DateTime
- [ ] **Step 7**: Run tests: `dotnet test DataAbstractionAPI.Services.Tests --filter "DefaultGenerator"`

#### Step 14.1.2: Additional Patterns
- [ ] **Step 1**: Add test: `DefaultGenerator_GenerateDefault_WithCreatedPrefix_ReturnsDateTime`
  - Test "created_" prefix
- [ ] **Step 2**: Add test: `DefaultGenerator_GenerateDefault_WithUpdatedPrefix_ReturnsDateTime`
  - Test "updated_" prefix
- [ ] **Step 3**: Add test: `DefaultGenerator_GenerateDefault_WithDeletedPrefix_ReturnsDateTime`
  - Test "deleted_" prefix
- [ ] **Step 4**: Add test: `DefaultGenerator_GenerateDefault_WithIdSuffix_ReturnsNull`
  - Test "_id" suffix
- [ ] **Step 5**: Add test: `DefaultGenerator_GenerateDefault_WithKeySuffix_ReturnsNull`
  - Test "_key" suffix
- [ ] **Step 6**: Add test: `DefaultGenerator_GenerateDefault_WithCountSuffix_ReturnsZero`
  - Test "_count" suffix
- [ ] **Step 7**: Add test: `DefaultGenerator_GenerateDefault_WithTotalSuffix_ReturnsZero`
  - Test "_total" suffix
- [ ] **Step 8**: Add test: `DefaultGenerator_GenerateDefault_WithNumPrefix_ReturnsZero`
  - Test "num_" prefix
- [ ] **Step 9**: Run tests and verify coverage improvement

---

## Progress Tracking

### Overall Progress
- [x] Section 1: CsvAdapter.UpdateAsync() - 100% complete ✅ (10 tests added)
- [x] Section 2: CsvAdapter.DeleteAsync() - 100% complete ✅ (6 tests added)
- [x] Section 3: DataController.UploadCsvFile() - 100% complete ✅ (14 tests added)
- [x] Section 4: CsvAdapter.SortRecords() - 100% complete ✅ (6 tests added)
- [x] Section 5: CsvAdapter.InferFieldType() - 100% complete ✅ (4 tests added)
- [ ] Section 6: CsvAdapter.BulkOperationAsync() - 0% complete
- [ ] Section 7: CsvAdapter.AggregateAsync() - 0% complete
- [ ] Section 8: CsvAdapter.ConvertToNumeric() - 0% complete
- [ ] Section 9: TypeConverter.PerformConversion() - 0% complete
- [ ] Section 10: FilterEvaluator.CompareValues() - 0% complete
- [ ] Section 11: TypeConverter.HandleConversionFailure() - 0% complete
- [ ] Section 12: CsvAdapter.GetAsync() - 0% complete
- [ ] Section 13: CsvAdapter.ValidateCollectionName() - 0% complete
- [ ] Section 14: DefaultGenerator.GeneratePatternBasedDefault() - 0% complete

### Test Count Estimate
- **Estimated New Tests**: ~150-200 tests
- **Completed**: ~40 tests (Sections 1-5)
- **Remaining**: ~110-160 tests (Sections 6-14)

### Coverage Progress
- **Current**: 91% line, 75.5% branch
- **Target**: >90% line ✅, >85% branch
- **Branch Coverage Gap**: 9.5% to reach target

---

## Key Commands Reference

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test filter
dotnet test DataAbstractionAPI.Adapters.Tests --filter "UpdateAsync"
dotnet test DataAbstractionAPI.API.Tests --filter "UploadCsvFile"
dotnet test DataAbstractionAPI.Services.Tests --filter "TypeConverter"

# Generate HTML coverage report
reportgenerator -reports:"**/coverage.json" -targetdir:"coverage/html" -reporttypes:"Html"

# View HTML report
Start-Process "coverage\html\index.html"
```

---

## Testing Best Practices

1. **Follow existing patterns** - Look at existing tests for style
2. **Use Arrange-Act-Assert** structure
3. **Name tests descriptively** - MethodName_Scenario_ExpectedResult
4. **Test one thing per test** - Keep tests focused
5. **Clean up resources** - Use Dispose pattern for file operations
6. **Isolate tests** - Each test should be independent
7. **Use mocks for dependencies** - Mock services when testing branches
8. **Test error paths** - Focus on exception scenarios and edge cases
9. **Test private methods indirectly** - Test through public methods
10. **Cover all branches** - Use coverage report to identify missed branches

---

## Notes and Considerations

- **CRAP Score**: Change Risk Anti-Patterns score indicates code complexity and test coverage. Higher scores mean more risk.
- **Private Methods**: Test through public methods that use them
- **File Locking**: May require complex scenarios or mocking to test retry logic
- **Cancellation Tokens**: Test at various points in async operations
- **Error Paths**: Focus on exception handling and edge cases
- **Branch Coverage**: The main goal is to improve branch coverage from 75.5% to >85%

---

**Document Version**: 2.1 (High CRAP Score Methods Focus)  
**Last Updated**: December 2025  
**Status**: 🔄 **IN PROGRESS** - Sections 1-5 Completed (40 tests added)
