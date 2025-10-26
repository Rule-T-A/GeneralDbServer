# Data Abstraction API - TDD Implementation Plan

**Status**: Phase 1 COMPLETE ✅ (All core functionality implemented with security validation)
**Last Updated**: 2025-10-26

## Overview

This plan follows Test-Driven Development (TDD) principles:

1. Write failing test
2. Write minimal code to pass test
3. Refactor
4. Repeat

**Phase Gate Policy**: Do NOT proceed to next phase without explicit discussion with team.

---

## Implementation Status & Known Gaps

### Current Status (October 26, 2025)
- ✅ **Phase 1**: COMPLETE - Basic CRUD operations working (37 tests passing)
- ⏸️ **Phase 2**: READY - Plan updated with prerequisites
- ⏸️ **Phase 3**: DEPENDS ON PHASE 2 + missing adapter methods
- ⏸️ **Phase 4**: WAITING FOR PHASE 3
- ⏸️ **Phase 5**: WAITING FOR PHASE 4

### Known Scope Limitations

**Simplified Interface Approach:**
The current `IDataAdapter` interface is simplified to ~60% of the full specification to enable faster MVP delivery.

**Implemented (Phase 1):**
- ✅ `ListAsync` - Query with filtering, pagination, sorting
- ✅ `GetAsync` - Get single record by ID
- ✅ `CreateAsync` - Create new records
- ⚠️ `UpdateAsync` - Signature present, not implemented (needs Phase 1.x or Phase 3)
- ⚠️ `DeleteAsync` - Signature present, not implemented (needs Phase 1.x or Phase 3)
- ⚠️ `GetSchemaAsync` - Signature present, not implemented (needs Phase 1.x or Phase 3)
- ✅ `ListCollectionsAsync` - Signature present, not implemented (needs Phase 1.x or Phase 3)

**Not in Current Interface (Available as needed):**
- Schema operations: AddFieldAsync, ModifyFieldAsync, DeleteFieldAsync, etc.
- Bulk operations: BulkOperationAsync, GetSummaryAsync, AggregateAsync
- Result models: UpdateResult, DeleteResult, BulkResult, AggregateResult, SchemaResult

**Strategic Decision:**
- Ship MVP with current scope
- Add advanced features incrementally as needed
- Focus on working end-to-end over comprehensive spec compliance

---

## Phase 1: Core Foundation (Weeks 1-2)

**Goal**: Create solution structure, core interfaces, basic CSV adapter with TDD validation

### Prerequisites

- [X] .NET 8.0 SDK installed
- [X] IDE configured (VS Code / Visual Studio)
- [X] Git repository initialized

---

### Step 1.1: Create Solution Structure (Day 1) ✅ COMPLETE

#### Setup

- [X] Create solution file: `dotnet new sln -n DataAbstractionAPI`
- [X] Create Core project: `dotnet new classlib -n DataAbstractionAPI.Core -f net8.0`
- [X] Add project to solution: `dotnet sln add DataAbstractionAPI.Core`
- [X] Create Core test project: `dotnet new xunit -n DataAbstractionAPI.Core.Tests -f net8.0`
- [X] Add Core test to solution: `dotnet sln add DataAbstractionAPI.Core.Tests`
- [X] Add reference from Core test to Core: `dotnet add DataAbstractionAPI.Core.Tests reference DataAbstractionAPI.Core`
- [X] Create Adapter project: `dotnet new classlib -n DataAbstractionAPI.Adapters.Csv -f net8.0`
- [X] Add adapter to solution: `dotnet sln add DataAbstractionAPI.Adapters.Csv`
- [X] Create Adapter test project: `dotnet new xunit -n DataAbstractionAPI.Adapters.Tests -f net8.0`
- [X] Add test project to solution: `dotnet sln add DataAbstractionAPI.Adapters.Tests`
- [X] Add project references from adapter test to adapter and Core: 
  - `dotnet add DataAbstractionAPI.Adapters.Tests reference DataAbstractionAPI.Adapters.Csv`
  - `dotnet add DataAbstractionAPI.Adapters.Tests reference DataAbstractionAPI.Core`
- [X] Add CsvHelper package: `dotnet add DataAbstractionAPI.Adapters.Csv package CsvHelper -v 33.0.1`
- [X] Verify solution builds: `dotnet build`

**Validation**: Solution compiles, all projects added, NuGet packages restored, all tests pass ✅

---

### Step 1.2: Define Core Interfaces (TDD Day 2-3) ✅ COMPLETE

#### Test First: Create Interface Contracts

- [X] Create `IDataAdapter` interface in Core/Interfaces:
  ```csharp
  public interface IDataAdapter
  {
      Task<ListResult> ListAsync(string collection, QueryOptions options, CancellationToken ct = default);
      Task<Record> GetAsync(string collection, string id, CancellationToken ct = default);
      Task<CreateResult> CreateAsync(string collection, Dictionary<string, object> data, CancellationToken ct = default);
  }
  ```
- [X] Create `IDefaultGenerator` interface
- [X] Create `ITypeConverter` interface
- [X] Create models in Core/Models:
  - [X] `Record` (id, data dict)
  - [X] `CollectionSchema` (name, fields list)
  - [X] `FieldDefinition` (name, type, nullable, default)
  - [X] `QueryOptions` (fields, filter, limit, offset)
  - [X] `ListResult` (data list, total, more)
  - [X] `CreateResult` (record, id)
- [X] Create enums:
  - [X] `FieldType` (String, Integer, Float, Boolean, DateTime, Date, Array, Object)
  - [X] `StorageType` (Csv, Sql, NoSql, InMemory)
- [X] Create basic tests in Core.Tests for models (validation, serialization, equality)
  - [X] Test Record model (id, data dictionary)
  - [X] Test FieldDefinition model (name, type, nullable, default)
  - [X] Test QueryOptions model (fields, filter, limit, offset)
  - [X] Test ListResult model (data list, total, more flag)
  - [X] Test enum values and serialization

**Validation**: All interfaces compile without implementation, all 10 model tests pass ✅

---

### Step 1.3: Implement CSV Schema Manager (TDD Day 4-5) ✅ COMPLETE

#### Test: Schema File Operations

- [X] Write test: `CsvSchemaManager_SavesSchema_ToJsonFile`
  - Creates temp directory
  - Saves schema to `.schema/{collection}.json`
  - Verifies file exists
  - Verifies content is valid JSON
- [X] Write test: `CsvSchemaManager_LoadsSchema_FromJsonFile`
- [X] Implement `CsvSchemaManager` class
- [X] Make test pass
- [X] Write test: `CsvSchemaManager_SavesAndLoads_SchemaRoundtrip` (integration test)
- [X] Write test: `CsvSchemaManager_LoadsSchema_WhenFileDoesNotExist_ReturnsNull`
- [X] Refactor if needed
- [X] Verify all tests pass: `dotnet test --filter "CsvSchemaManager"`

**Validation**: Schema manager can save/load JSON schemas, all 5 tests pass ✅

---

### Step 1.4: Implement Basic CSV Reading (TDD Day 6-7) ✅ COMPLETE

#### Test: CSV File Reading

- [X] Create sample CSV: `testdata/users.csv` with headers and 3 rows
- [X] Write test: `CsvFileHandler_ReadsHeaders_FromCsvFile`
  - Verifies headers array returned
- [X] Write test: `CsvFileHandler_ReadsRecords_AsDictionary`
  - Verifies records returned as `List<Dictionary<string, object>>`
- [X] Implement `CsvFileHandler` class using CsvHelper
- [X] Make tests pass
- [X] Write test: `CsvFileHandler_HandlesEmptyFile_Gracefully`
- [X] Write test: `CsvFileHandler_HandlesMissingFile_ThrowsException`
- [X] Refactor
- [X] Verify all tests pass: `dotnet test --filter "CsvFileHandler"`

**Validation**: Can read CSV files into dictionaries, all 4 tests pass ✅

---

### Step 1.5: Implement CsvAdapter - ListAsync (TDD Day 8-9) ✅ COMPLETE

#### Test: List Records

- [X] Add project reference: `dotnet add DataAbstractionAPI.Adapters.Csv reference DataAbstractionAPI.Core`
- [X] Create mock in-memory CSV for testing
- [X] Write test: `CsvAdapter_ListAsync_ReturnsAllRecords_WithoutFilter`
  - Creates test CSV with 5 records
  - Calls ListAsync with no filter
  - Verifies 5 records returned
- [X] Write test: `CsvAdapter_ListAsync_ReturnsCorrectTotal_Count`
  - Verifies `ListResult.Total` equals correct count
- [X] Implement `CsvAdapter` class
- [X] Make tests pass
- [X] Write test: `CsvAdapter_ListAsync_HandlesMissingCollection_ThrowsException`
- [X] Write test: `CsvAdapter_ListAsync_RespectsLimit`
- [X] Write test: `CsvAdapter_ListAsync_RespectsOffset`
- [X] Refactor
- [X] Verify all tests pass: `dotnet test --filter "CsvAdapter_ListAsync"`

**Validation**: Can list records from CSV with filtering, pagination, and sorting ✅

---

### Step 1.6: Implement CsvAdapter - GetAsync (TDD Day 10) ✅ COMPLETE

#### Test: Get Single Record

- [X] Write test: `CsvAdapter_GetAsync_ReturnsRecord_WithMatchingId`
  - Creates test CSV with known IDs
  - Calls GetAsync with known ID
  - Verifies correct record returned
- [X] Write test: `CsvAdapter_GetAsync_WithInvalidId_ThrowsNotFoundException`
- [X] Implement GetAsync method
- [X] Make tests pass
- [X] Refactor
- [X] Verify all tests pass: `dotnet test --filter "CsvAdapter_GetAsync"`

**Validation**: Can get single record by ID, throws exception if not found ✅

---

### Step 1.7: Implement ID Generation (TDD Day 11) ✅ COMPLETE

#### Test: Generate Unique IDs

- [X] Write test: `CsvAdapter_GenerateId_ReturnsUniqueGuids`
  - Calls GenerateId() 100 times
  - Verifies all IDs are unique
- [X] Implement ID generation (use GUID for simplicity)
- [X] Make test pass
- [X] Refactor to use IIdGenerator interface if needed
- [X] Verify test passes

**Validation**: Can generate unique IDs for new records (GUID format without hyphens) ✅

---

### Step 1.8: Implement CsvAdapter - CreateAsync (TDD Day 12) ✅ COMPLETE

#### Test: Create Record

- [X] Write test: `CsvAdapter_CreateAsync_AddsRecord_ToCsvFile`
  - Creates test CSV
  - Calls CreateAsync with new record data
  - Verifies record appears in CSV file
  - Verifies ID is returned
- [X] Write test: `CsvAdapter_CreateAsync_SetsDefaultValues_ForMissingFields`
- [X] Implement CreateAsync method
- [X] Make tests pass
- [X] Write test: `CsvAdapter_CreateAsync_AppendsToExistingFile`
- [X] Refactor
- [X] Verify all tests pass: `dotnet test --filter "CsvAdapter_CreateAsync"`

**Validation**: Can create records, writes to file, returns new ID ✅

---

### Step 1.9: Add File Locking (TDD Day 13) ✅ COMPLETE

#### Test: Concurrency Safety

- [X] Write test: `CsvFileLock_AcquiresLock_OnCreation`
  - Creates lock file
  - Verifies .lock file exists
- [X] Write test: `CsvFileLock_ReleasesLock_OnDispose`
  - Uses `using` block
  - Verifies lock file deleted after dispose
- [X] Implement `CsvFileLock` class
- [X] Make tests pass
- [X] Write test: `CsvFileLock_PreventsMultipleLocks_OnSameFile`
- [X] Write test: `CsvFileLock_AllowsLock_AfterPreviousLockReleased`
- [X] Refactor
- [X] Verify all tests pass

**Validation**: File locking prevents concurrent access issues ✅

---

### Step 1.10: Security Validation (Day 14) ✅ COMPLETE

#### Test: Path Traversal Prevention

- [X] Write test: `CsvAdapter_SecureCollection_RejectsPathTraversal_WithDotDot`
  - Calls ListAsync with "../"
  - Verifies exception thrown
- [X] Write test: `CsvAdapter_SecureCollection_RejectsPathTraversal_WithBackslash`
- [X] Write test: `CsvAdapter_SecureCollection_RejectsPathTraversal_WithForwardSlash`
- [X] Write test: `CsvAdapter_SecureCollection_AcceptsValidCollectionName`
- [X] Implement ValidateCollectionName with security checks
- [X] Make tests pass
- [X] Verify all security tests pass: `dotnet test --filter "Secure"`

**Validation**: Path traversal attacks prevented ✅

---

### Phase 1 Complete Checklist ✅

**Code**

- [X] All interfaces defined in Core
- [X] All models defined in Core with proper validation
- [X] CsvAdapter implements IDataAdapter
- [X] Can read/write CSV files
- [X] Can save/load schemas
- [X] File locking works
- [X] Security validation works

**Tests**

- [X] All Core tests pass: `dotnet test DataAbstractionAPI.Core.Tests`
- [X] All unit tests pass: `dotnet test DataAbstractionAPI.Adapters.Tests`
- [X] Test coverage > 80% for CsvAdapter
- [X] Integration tests pass
- [X] Security tests pass
- [X] Model validation tests pass in Core.Tests

**Documentation**

- [X] README updated with Phase 1 results
- [X] Code comments added
- [X] Sample CSV files created in testdata/

**Validation Command:**

```bash
# Run all Core tests
dotnet test DataAbstractionAPI.Core.Tests

# Run all Adapter tests
dotnet test --filter "FullyQualifiedName~CsvAdapter"

# Or run all tests
dotnet test
```

**Phase Gate**: ✅ PHASE 1 COMPLETE - Do NOT proceed to Phase 2 without discussion.

---

## Phase 1.x: Complete CsvAdapter Implementation (Optional)

**Status**: Not Started 🟥  
**Goal**: Implement remaining IDataAdapter methods for full CRUD support

### Optional but Recommended

These methods are needed for the API (Phase 3), but aren't critical for Services (Phase 2). You can implement them either:
- Before Phase 2 (more complete foundation)
- During Phase 3 (as API endpoints are built)

### Step 1.11: Implement CsvAdapter - UpdateAsync (Optional)

#### Test: Update Record

- [ ] Write test: `CsvAdapter_UpdateAsync_ModifiesRecord_InPlace`
- [ ] Write test: `CsvAdapter_UpdateAsync_WithInvalidId_ThrowsException`
- [ ] Implement UpdateAsync method
- [ ] Handle partial updates (only specified fields)
- [ ] Verify all tests pass

**Validation**: Can update records in place

---

### Step 1.12: Implement CsvAdapter - DeleteAsync (Optional)

#### Test: Delete Record

- [ ] Write test: `CsvAdapter_DeleteAsync_RemovesRecord_FromFile`
- [ ] Write test: `CsvAdapter_DeleteAsync_WithInvalidId_ThrowsException`
- [ ] Implement DeleteAsync method
- [ ] Verify all tests pass

**Validation**: Can delete records

---

### Step 1.13: Implement CsvAdapter - Schema Operations (Optional)

#### Test: Schema Methods

- [ ] Write test: `CsvAdapter_GetSchemaAsync_ReturnsSchema`
- [ ] Write test: `CsvAdapter_ListCollectionsAsync_ReturnsCollectionNames`
- [ ] Implement GetSchemaAsync method
- [ ] Implement ListCollectionsAsync method
- [ ] Verify all tests pass

**Validation**: Can access schema information

**Note**: These are particularly needed for Phase 3 API schema endpoints

---

## Phase 2: Services Layer (Week 3)

**⚠️ DO NOT START PHASE 2 WITHOUT DISCUSSION**

**Goal**: Implement business logic services (DefaultGenerator, TypeConverter, FilterEvaluator, ValidationService)

### ⚠️ Issues Addressed Before Starting Phase 2

**These changes address gaps between Phase 1 implementation and Phase 2 requirements:**

1. **Missing Core Types**: Added ConversionStrategy enum, ConversionException, ValidationException
2. **Interface Updates**: Updated ITypeConverter and IDefaultGenerator signatures to match specifications
3. **Architecture Gap**: Added Step 2.0 and 2.05 to prepare Core and refactor CsvAdapter for service injection
4. **Context Support**: Added DefaultGenerationContext for context-aware default generation
5. **Dependency Injection**: CsvAdapter will be updated to accept services via constructor

### Architecture Overview

The Services layer provides reusable business logic that adapters can use:

```
┌─────────────────────────────────────────┐
│         Adapters (CsvAdapter)           │
│  - Injects Services via constructor     │
│  - Delegates filtering to FilterEvaluator│
│  - Uses DefaultGenerator for fields     │
│  - Uses TypeConverter for conversions   │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│           Services Layer                 │
│  - FilterEvaluator (filter logic)       │
│  - DefaultGenerator (smart defaults)    │
│  - TypeConverter (type conversions)     │
│  - ValidationService (data validation)   │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│              Core (Interfaces)           │
│  - IDataAdapter                         │
│  - IDefaultGenerator                    │
│  - ITypeConverter                       │
└─────────────────────────────────────────┘
```

**Key Integration Points:**
- `FilterEvaluator`: Replaces inline filtering in CsvAdapter
- `DefaultGenerator`: Used when adding new fields without defaults
- `TypeConverter`: Used when modifying field types
- `ValidationService`: Validates records before create/update

### Important Notes

**Breaking Changes in Phase 2:**
- Core interfaces updated with new signatures (ITypeConverter, IDefaultGenerator)
- New Core types required: ConversionStrategy, DefaultGenerationStrategy, exceptions
- CsvAdapter will need refactoring to use FilterEvaluator service
- All Phase 1 tests should still pass after Step 2.0 and 2.05

**Updated Service Signature:**
- `ITypeConverter.Convert()` now includes `ConversionStrategy` parameter
- `IDefaultGenerator.GenerateDefault()` now includes `DefaultGenerationContext` parameter
- These changes maintain backward compatibility with optional parameters

### Discuss Before Proceeding:

- [ ] Review Phase 1 results with team
- [ ] Confirm Phase 1 acceptance criteria met
- [ ] Get explicit approval to proceed
- [ ] Address any technical debt from Phase 1

### Prerequisites

- [ ] Phase 1 fully complete
- [ ] All Phase 1 tests passing
- [ ] Phase 1 code reviewed
- [ ] Core interfaces updated with missing types
- [ ] CsvAdapter refactored to support service injection

---

### Step 2.0: Prepare Core for Services (Pre-requisite)

**⚠️ CRITICAL: Complete this before starting Services implementation**

#### Update Core Interfaces and Add Missing Types

- [ ] Add `ConversionStrategy` enum to Core/Enums:
  ```csharp
  public enum ConversionStrategy
  {
      Cast = 0,
      Truncate = 1,
      FailOnError = 2,
      SetNull = 3
  }
  ```

- [ ] Add `ConversionException` to Core/Exceptions:
  ```csharp
  public class ConversionException : Exception
  {
      public string FieldName { get; }
      public object Value { get; }
      // Constructor with field name and value
  }
  ```

- [ ] Add `ValidationException` to Core/Exceptions:
  ```csharp
  public class ValidationException : Exception
  {
      public string FieldName { get; }
      // Constructor with field name
  }
  ```

- [ ] Update `ITypeConverter` signature to include strategy:
  ```csharp
  public interface ITypeConverter
  {
      object Convert(object value, FieldType fromType, FieldType toType, ConversionStrategy strategy);
  }
  ```

- [ ] Add `DefaultGenerationContext` model to Core/Models:
  ```csharp
  public class DefaultGenerationContext
  {
      public string CollectionName { get; set; }
      public List<Record> ExistingRecords { get; set; }
  }
  ```

- [ ] Update `IDefaultGenerator` signature:
  ```csharp
  public interface IDefaultGenerator
  {
      object GenerateDefault(string fieldName, FieldType fieldType, DefaultGenerationContext context);
      DefaultGenerationStrategy DetermineStrategy(string fieldName, FieldType fieldType);
  }
  ```

- [ ] Add `DefaultGenerationStrategy` enum to Core/Enums:
  ```csharp
  public enum DefaultGenerationStrategy
  {
      UserSpecified = 0,
      PatternMatch = 1,
      ContextAnalysis = 2,
      TypeBased = 3
  }
  ```

**Validation**: All Core types compile, no breaking changes

---

### Step 2.05: Refactor CsvAdapter for Service Injection (Pre-requisite)

**⚠️ CRITICAL: Update CsvAdapter before implementing Services**

#### Prepare CsvAdapter for Dependency Injection

- [ ] Add constructor overload to accept services:
  ```csharp
  public CsvAdapter(
      string baseDirectory,
      IDefaultGenerator? defaultGenerator = null,
      ITypeConverter? typeConverter = null,
      FilterEvaluator? filterEvaluator = null)
  ```

- [ ] Update `FilterRecords` method to use FilterEvaluator if available:
  ```csharp
  private List<Record> FilterRecords(List<Record> records, Dictionary<string, object> filter)
  {
      if (_filterEvaluator != null)
      {
          return _filterEvaluator.Evaluate(records, filter).ToList();
      }
      // Fall back to simple equality for backward compatibility
      return records.Where(...).ToList();
  }
  ```

- [ ] Update existing tests to ensure they still pass
- [ ] Add tests for CsvAdapter with injected services (optional)
- [ ] Verify all Phase 1 tests still pass

**Validation**: CsvAdapter refactored, all 27 tests still pass, ready for service injection

---

### Step 2.1: Create Services Project (TDD)

#### Setup

- [ ] Create project: `dotnet new classlib -n DataAbstractionAPI.Services -f net8.0`
- [ ] Add to solution: `dotnet sln add DataAbstractionAPI.Services`
- [ ] Create test project: `dotnet new xunit -n DataAbstractionAPI.Services.Tests -f net8.0`
- [ ] Add test to solution: `dotnet sln add DataAbstractionAPI.Services.Tests`
- [ ] Add references:
  - `dotnet add DataAbstractionAPI.Services reference DataAbstractionAPI.Core`
  - `dotnet add DataAbstractionAPI.Services.Tests reference DataAbstractionAPI.Services`
- [ ] Install packages: `dotnet add DataAbstractionAPI.Services package Microsoft.Extensions.Logging.Abstractions`
- [ ] Verify builds

**Validation**: Services project compiles

---

### Step 2.2: Implement DefaultGenerator (TDD)

#### Test: Pattern-Based Defaults

- [ ] Write test: `DefaultGenerator_ForBooleanFields_WithIsPrefix_ReturnsFalse`
  - Field name: "is_active"
  - Verifies default is false
- [ ] Write test: `DefaultGenerator_ForDateTimeFields_WithAtSuffix_ReturnsCurrentTimestamp`
  - Field name: "created_at"
  - Verifies default is DateTime.UtcNow
- [ ] Write test: `DefaultGenerator_ForIdFields_ReturnsNull`
  - Field name: "user_id"
  - Verifies default is null
- [ ] Implement DefaultGenerator.DetermineStrategy()
- [ ] Implement DefaultGenerator.GenerateDefault()
- [ ] Make tests pass
- [ ] Write tests for remaining patterns (see spec lines 629-648)
- [ ] Refactor
- [ ] Verify all tests pass: `dotnet test --filter "DefaultGenerator"`

**Validation**: Pattern-based defaults work for all patterns

---

### Step 2.3: Implement TypeConverter (TDD)

#### Test: Type Conversions

- [ ] Write test: `TypeConverter_ConvertsStringToInt_Successfully`
  - Value: "123"
  - Converts to Integer
  - Verifies result is 123
- [ ] Write test: `TypeConverter_ConvertsStringToInt_WithInvalidValue_ThrowsException`
  - Value: "abc"
  - Verifies ConversionException thrown
- [ ] Write test: `TypeConverter_ConvertsStringToBool_HandlesVariousFormats`
  - Tests "true", "True", "1", "yes" → true
- [ ] Implement TypeConverter.Convert()
- [ ] Implement all conversion strategies (Cast, Truncate, FailOnError, SetNull)
- [ ] Make tests pass
- [ ] Write comprehensive test suite for all type combinations
- [ ] Refactor
- [ ] Verify all tests pass: `dotnet test --filter "TypeConverter"`

**Validation**: All type conversions work, error handling correct

---

### Step 2.4: Implement FilterEvaluator (TDD)

#### Test: Filter Evaluation

- [ ] Write test: `FilterEvaluator_SimpleFilter_Equals_ReturnsMatches`
  - Filter: { "status": "active" }
  - Records: 3 records with different statuses
  - Verifies only "active" returned
- [ ] Write test: `FilterEvaluator_OperatorFilter_GreaterThan_Works`
  - Filter: { "field": "age", "operator": "gt", "value": 18 }
  - Records: ages 15, 20, 25, 30
  - Verifies only 20, 25, 30 returned
- [ ] Implement FilterEvaluator.Evaluate()
- [ ] Implement all operators: eq, ne, gt, gte, lt, lte, in, contains
- [ ] Make tests pass
- [ ] Write test: `FilterEvaluator_CompoundFilter_AndOr_Works`
- [ ] Refactor
- [ ] Verify all tests pass: `dotnet test --filter "FilterEvaluator"`

**Validation**: All filter operators work, compound filters work

---

### Step 2.5: Implement ValidationService (TDD)

#### Test: Data Validation

- [ ] Write test: `ValidationService_ValidatesRecord_AgainstSchema`
  - Schema: { name: string required }
  - Record: { name: "Alice" }
  - Verifies valid
- [ ] Write test: `ValidationService_RejectsRecord_WithMissingRequiredField`
  - Schema: { name: string required }
  - Record: { }
  - Verifies throws ValidationException
- [ ] Implement ValidationService.ValidateField()
- [ ] Make tests pass
- [ ] Write test: `ValidationService_ValidatesType_MatchesSchema`
- [ ] Write test: `ValidationService_AllowsNullableFields_ToBeNull`
- [ ] Refactor
- [ ] Verify all tests pass: `dotnet test --filter "ValidationService"`

**Validation**: Data validation works for all field types and constraints

---

### Phase 2 Complete Checklist

**Code**

- [ ] DefaultGenerator implements IDefaultGenerator
- [ ] TypeConverter implements ITypeConverter
- [ ] FilterEvaluator works for all operators
- [ ] ValidationService validates records
- [ ] All services have logging

**Tests**

- [ ] All unit tests pass: `dotnet test DataAbstractionAPI.Services.Tests`
- [ ] Test coverage > 85% for services
- [ ] Integration tests for service composition pass

**Documentation**

- [ ] Services documented
- [ ] Patterns documented
- [ ] Examples added

**Validation Command:**

```bash
dotnet test DataAbstractionAPI.Services.Tests
```

**Phase Gate**: ✅ PHASE 2 COMPLETE - Do NOT proceed to Phase 3 without discussion.

---

## Phase 3: REST API (Week 4)

**⚠️ DO NOT START PHASE 3 WITHOUT DISCUSSION**

**Goal**: Create REST API endpoints with DTOs, authentication, error handling

### Discuss Before Proceeding:

- [ ] Review Phase 2 results
- [ ] Confirm services working correctly
- [ ] Get approval to proceed
- [ ] Define API versioning strategy

---

### Step 3.1: Create API Project (TDD)

#### Setup

- [ ] Create project: `dotnet new webapi -n DataAbstractionAPI.API -f net8.0`
- [ ] Add to solution
- [ ] Remove WeatherForecast example
- [ ] Create test project: `dotnet new xunit -n DataAbstractionAPI.API.Tests -f net8.0`
- [ ] Add references from API to Core, Services, Adapters.Csv
- [ ] Install Swagger: `dotnet add package Swashbuckle.AspNetCore`
- [ ] Configure Program.cs (see spec lines 588-688)
- [ ] Verify API starts on localhost:5000

**Validation**: API starts, Swagger UI accessible

---

### Step 3.2: Implement DataController - GET (TDD)

#### Test: List Endpoint

- [ ] Write integration test: `DataController_GetCollection_ReturnsRecords`
  - Arrange: Mock CsvAdapter with test data
  - Act: GET /api/data/users
  - Assert: Returns ListResponseDto with records
- [ ] Write test: `DataController_GetCollection_WithFields_ReturnsFilteredFields`
- [ ] Implement DataController.ListRecords()
- [ ] Make tests pass
- [ ] Write test: `DataController_GetCollection_WithInvalidCollection_Returns404`
- [ ] Refactor
- [ ] Verify all tests pass

**Validation**: GET endpoints work, DTOs serialize correctly

---

### Step 3.3: Implement API Key Authentication (TDD)

#### Test: Authentication

- [ ] Write test: `ApiKeyMiddleware_WithValidKey_AllowsRequest`
  - Sends X-API-Key header
  - Verifies 200 response
- [ ] Write test: `ApiKeyMiddleware_WithoutKey_Returns401`
- [ ] Write test: `ApiKeyMiddleware_WithInvalidKey_Returns401`
- [ ] Implement ApiKeyAuthenticationMiddleware
- [ ] Configure in appsettings.json
- [ ] Make tests pass
- [ ] Verify Swagger UI includes API key input
- [ ] Refactor

**Validation**: Authentication works, middleware logs attempts

---

### Step 3.4: Implement Error Handling (TDD)

#### Test: Error Responses

- [ ] Write test: `ErrorHandler_CatchesNotFoundException_Returns404`
- [ ] Write test: `ErrorHandler_CatchesValidationException_Returns400`
- [ ] Implement GlobalExceptionHandler
- [ ] Make tests pass
- [ ] Write tests for all custom exceptions
- [ ] Refactor
- [ ] Verify consistent error format

**Validation**: All exceptions handled, consistent error responses

---

### Phase 3 Complete Checklist

**Code**

- [ ] All data endpoints implemented (GET, POST, PATCH, DELETE, Bulk, Summary, Aggregate)
- [ ] All schema endpoints implemented (List, Get, Create, Rename, Delete, AddField, ModifyField, DeleteField)
- [ ] DTOs with [JsonPropertyName] attributes
- [ ] API key authentication works
- [ ] Error handling middleware works
- [ ] CORS configured
- [ ] Swagger documents all endpoints

**Tests**

- [ ] All API integration tests pass
- [ ] Authentication tests pass
- [ ] Error handling tests pass
- [ ] Test coverage > 75%

**Documentation**

- [ ] Swagger shows all endpoints
- [ ] Examples added to Swagger
- [ ] Postman collection created

**Validation Command:**

```bash
dotnet test DataAbstractionAPI.API.Tests
```

**Manual Testing:**

- [ ] Test via Swagger UI
- [ ] Test via curl with API key
- [ ] Test error scenarios

**Missing Implementations:**
- ⚠️ CsvAdapter.UpdateAsync() not implemented (needed for PATCH)
- ⚠️ CsvAdapter.DeleteAsync() not implemented (needed for DELETE)
- ⚠️ CsvAdapter.GetSchemaAsync() not implemented (needed for schema GET)
- ⚠️ CsvAdapter.ListCollectionsAsync() not implemented (needed for /schema)
- ⚠️ IDataAdapter interface missing additional methods from spec:
  - BulkOperationAsync(), GetSummaryAsync(), AggregateAsync()
  - AddFieldAsync(), ModifyFieldAsync(), DeleteFieldAsync()
- These need to be implemented before API can be fully functional

**Phase Gate**: ✅ PHASE 3 COMPLETE - Do NOT proceed to Phase 4 without discussion.

---

## Phase 4: Management UI (Weeks 5-6)

**⚠️ DO NOT START PHASE 4 WITHOUT DISCUSSION**

**Goal**: Create Blazor Server UI for management and debugging

### Discuss Before Proceeding:

- [ ] Review API functionality
- [ ] Confirm API ready for UI consumption
- [ ] Get approval to proceed
- [ ] Define UI requirements

---

### Step 4.1: Create UI Project

#### Setup

- [ ] Create project: `dotnet new blazorserver -n DataAbstractionAPI.UI -f net8.0`
- [ ] Add to solution
- [ ] **CRITICAL: Do NOT add references to Core/Services/Adapters**
- [ ] Install MudBlazor: `dotnet add package MudBlazor -v 6.11.0`
- [ ] Install Blazored.LocalStorage: `dotnet add package Blazored.LocalStorage`
- [ ] Configure MudBlazor theme
- [ ] Configure HttpClient in Program.cs
- [ ] Create ApiClientService (using HttpClient)
- [ ] Verify UI starts on localhost:5001

**Validation**: UI starts, connects to API

---

### Step 4.2: Implement Dashboard Page

#### Test: UI Components

- [ ] Create Dashboard.razor
- [ ] Implement connection status cards
- [ ] Add stats display
- [ ] Test UI manually:
  - [ ] Displays when API connected
  - [ ] Shows error when API offline
- [ ] Refactor
- [ ] Verify responsive design

**Validation**: Dashboard displays connection status

---

### Step 4.3: Implement Data Browser

#### Test: Data Display

- [ ] Create DataBrowser.razor
- [ ] Implement DataGrid component with MudTable
- [ ] Test pagination
- [ ] Test sorting
- [ ] Test field selection
- [ ] Add loading states
- [ ] Add error handling UI
- [ ] Verify displays API data

**Validation**: Data browser displays records, pagination works

---

### Step 4.4: Implement Schema Editor

#### Test: Schema Management

- [ ] Create SchemaEditor.razor
- [ ] Implement field list display
- [ ] Add field definitions editor
- [ ] Test creating new collections
- [ ] Test modifying fields
- [ ] Add validation UI
- [ ] Verify schema operations work

**Validation**: Can view/edit schemas via UI

---

### Phase 4 Complete Checklist

**Code**

- [ ] All pages implemented
- [ ] Components reusable
- [ ] UI communicates only via HTTP (validate!)
- [ ] Responsive design
- [ ] Error handling in UI

**Tests**

- [ ] Manual testing complete
- [ ] All API calls use ApiClientService
- [ ] UI works with API running
- [ ] UI shows errors gracefully

**Documentation**

- [ ] UI screenshots added
- [ ] User guide written

**Validation Command:**

```bash
# Verify UI has NO references to backend
dotnet list DataAbstractionAPI.UI reference
# Should show: NO references to Core, Services, or Adapters
```

**Phase Gate**: ✅ PHASE 4 COMPLETE - Do NOT proceed to Phase 5 without discussion.

---

## Phase 5: Polish & Documentation (Week 7)

**⚠️ DO NOT START PHASE 5 WITHOUT DISCUSSION**

**Goal**: Performance optimization, documentation, deployment prep

### Prerequisites

- [X] Phase 1 complete ✅
- [X] All Phase 1 tests passing (37 tests) ✅
- [ ] CsvAdapter methods still NOT implemented:
  - ⚠️ UpdateAsync() - needed for PATCH endpoints
  - ⚠️ DeleteAsync() - needed for DELETE endpoints
  - ⚠️ GetSchemaAsync() - needed for schema GET endpoints
  - ⚠️ ListCollectionsAsync() - needed for /schema endpoint
- [ ] These should be implemented in Phase 1.x before starting API

---

### Step 5.1: Performance Optimization

- [ ] Add response caching
- [ ] Optimize CSV queries
- [ ] Add indexes for large files
- [ ] Profile memory usage
- [ ] Verify performance targets met (< 200ms)

**Validation**: Performance improved, tests still pass

---

### Step 5.2: Documentation

- [ ] Write README.md
- [ ] Document CSV file format
- [ ] Create deployment guide
- [ ] Add API examples
- [ ] Create sample datasets

**Validation**: Documentation complete and reviewed

---

### Step 5.3: Deployment

- [ ] Create Dockerfile for API
- [ ] Create Dockerfile for UI
- [ ] Create docker-compose.yml
- [ ] Test deployment
- [ ] Document deployment steps

**Validation**: Can deploy via Docker

---

### Phase 5 Complete Checklist

**Code**

- [ ] Performance optimized
- [ ] Error handling improved
- [ ] Logging enhanced

**Tests**

- [ ] All tests pass
- [ ] Performance tests pass

**Documentation**

- [ ] Complete and reviewed

**Phase Gate**: ✅ PROJECT COMPLETE

---

## Quick Validation Commands

```bash
# Run all tests
dotnet test

# Check UI has no backend references
dotnet list DataAbstractionAPI.UI reference

# Build all projects
dotnet build

# Test specific phase
dotnet test --filter "FullyQualifiedName~Phase1"

# Check code coverage
dotnet test /p:CollectCoverage=true
```

---

## Important Notes

1. **Phase Gates**: Always discuss before starting next phase
2. **TDD First**: Write tests before implementation
3. **Refactor Often**: Keep code clean after making tests pass
4. **Validate Separation**: UI must never reference backend directly
5. **Test Coverage**: Aim for >80% coverage
6. **Documentation**: Update as you go

### ⚠️ Known Gaps in Current Implementation

**Core Interface Simplification:**
- Current IDataAdapter is a simplified version (covers MVP functionality)
- Missing from current interface: BulkOperationAsync, GetSummaryAsync, AggregateAsync, AddFieldAsync, ModifyFieldAsync, DeleteFieldAsync, CreateCollectionAsync, RenameCollectionAsync, DeleteCollectionAsync
- Missing result models: UpdateResult, DeleteResult, BulkResult, AggregateResult, SchemaResult
- Missing enum: ReturnMode
- **Impact**: These are needed for full Phase 3 API implementation
- **Resolution**: Add as needed in Phase 2.6 or during Phase 3

**Current Coverage**: ~60% of full specification (sufficient for MVP)

**Status Tracking**: Update checkboxes as you complete each step.
