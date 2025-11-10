# Phase 3 Enhancement Plans

This document contains detailed implementation plans for the remaining Phase 3 items:

1. Advanced Data Endpoints (Bulk, Summary, Aggregate)
2. Advanced Schema Endpoints (Create, Rename, Delete, AddField, ModifyField, DeleteField)
3. DTOs with [JsonPropertyName] Attributes
4. CORS Configuration

---

## 1. Advanced Data Endpoints Plan

### Overview
Implement three advanced data operation endpoints:
- **Bulk Operations** (`POST /api/data/{collection}/bulk`) - Batch create/update/delete
- **Summary** (`GET /api/data/{collection}/summary`) - Simple field aggregation
- **Aggregate** (`POST /api/data/{collection}/aggregate`) - Complex aggregations with grouping

### Prerequisites
- [X] Review `data-abstraction-api.md` specification (lines 227-353)
- [X] Understand current `IDataAdapter` interface limitations
- [X] Review existing `CsvAdapter` implementation patterns

### Step 1.1: Extend Core Interface (IDataAdapter)
- [X] Complete

**Location**: `DataAbstractionAPI.Core/Interfaces/IDataAdapter.cs`

**Changes**:
- [X] Add method signatures for bulk operations, summary, and aggregate
- [X] Define new result models (BulkResult, SummaryResult, AggregateResult)
- [X] Consider backward compatibility (make methods optional or add new interface)

**New Methods to Add**:
```csharp
Task<BulkResult> BulkOperationAsync(string collection, BulkOperationRequest request, CancellationToken ct = default);
Task<SummaryResult> GetSummaryAsync(string collection, string field, CancellationToken ct = default);
Task<AggregateResult> AggregateAsync(string collection, AggregateRequest request, CancellationToken ct = default);
```

**Decision Point**: 
- Option A: Add to existing `IDataAdapter` (breaking change for other adapters)
- Option B: Create `IAdvancedDataAdapter` interface extending `IDataAdapter`
- Option C: Add methods with default implementations (C# 8.0+)

**Recommendation**: Option A (add to existing interface) since CSV is the only adapter currently

### Step 1.2: Create Core Models
- [X] Complete

**Location**: `DataAbstractionAPI.Core/Models/`

**New Models to Create**:

1. **BulkOperationRequest.cs**
   - [ ] Create file
   - `string Action` (create, update, delete)
   - `bool Atomic` (default: false)
   - `List<Dictionary<string, object>> Records`
   - `Dictionary<string, object>? UpdateData` (for update operations)

2. **BulkResult.cs**
   - [ ] Create file
   - `bool Success`
   - `int Succeeded`
   - `int Failed`
   - `List<BulkOperationItemResult> Results` (for best-effort mode)
   - `List<string>? Ids` (for atomic create mode)
   - `string? Error` (for atomic failure)
   - `int? FailedIndex` (for atomic failure)

3. **BulkOperationItemResult.cs**
   - [ ] Create file
   - `int Index`
   - `string? Id` (for successful creates)
   - `bool Success`
   - `string? Error`

4. **SummaryResult.cs**
   - [ ] Create file
   - `Dictionary<string, int> Counts` (field value → count mapping)

5. **AggregateRequest.cs**
   - [ ] Create file
   - `string[]? GroupBy` (fields to group by)
   - `List<AggregateFunction> Aggregates` (field, function, alias)
   - `Dictionary<string, object>? Filter` (optional filter)

6. **AggregateFunction.cs**
   - [ ] Create file
   - `string Field`
   - `string Function` (count, sum, avg, min, max)
   - `string Alias`

7. **AggregateResult.cs**
   - [ ] Create file
   - `List<Dictionary<string, object>> Data` (grouped and aggregated results)

### Step 1.3: Implement CsvAdapter Methods
- [X] Complete

**Location**: `DataAbstractionAPI.Adapters.Csv/CsvAdapter.cs`

#### 1.3.1: BulkOperationAsync Implementation
- [ ] Complete

**Logic**:
- [ ] Validate action (create, update, delete)
- [ ] If atomic=true:
  - [ ] Use file locking for entire operation
  - [ ] Process all records, collect errors
  - [ ] If any error, rollback (don't write file)
  - [ ] Return success with all IDs or failure with error
- [ ] If atomic=false:
  - [ ] Process each record individually
  - [ ] Collect successes and failures
  - [ ] Return best-effort result with per-item results

**Implementation Notes**:
- [ ] Use existing `CreateAsync`, `UpdateAsync`, `DeleteAsync` methods internally
- [ ] For atomic mode, read all records, modify in memory, write once
- [ ] Handle cancellation tokens properly
- [ ] Use retry logic for file operations

**Error Handling**:
- [ ] Validation errors per record
- [ ] File I/O errors
- [ ] Cancellation

#### 1.3.2: GetSummaryAsync Implementation
- [ ] Complete

**Logic**:
- [ ] Read all records from CSV
- [ ] Filter if needed (optional filter parameter)
- [ ] Group by specified field value
- [ ] Count occurrences of each value
- [ ] Return dictionary mapping value → count

**Implementation Notes**:
- [ ] Use LINQ `GroupBy` for efficiency
- [ ] Handle null/empty values
- [ ] Support string, integer, boolean field types
- [ ] Consider memory for large datasets

#### 1.3.3: AggregateAsync Implementation
- [ ] Complete

**Logic**:
- [ ] Read all records from CSV
- [ ] Apply filter if provided (use FilterEvaluator if available)
- [ ] Group by specified fields
- [ ] Apply aggregate functions (count, sum, avg, min, max)
- [ ] Return grouped results with aggregates

**Implementation Notes**:
- [ ] Use LINQ for grouping and aggregation
- [ ] Handle type conversions (sum/avg for numeric fields)
- [ ] Support multiple group-by fields
- [ ] Support multiple aggregate functions per request
- [ ] Handle null values appropriately

**Aggregate Functions**:
- [ ] `count`: Count records (works on any field)
- [ ] `sum`: Sum numeric values
- [ ] `avg`: Average numeric values
- [ ] `min`: Minimum value
- [ ] `max`: Maximum value

### Step 1.4: Create API Controller Endpoints
- [X] Complete

**Location**: `DataAbstractionAPI.API/Controllers/DataController.cs`

#### 1.4.1: Bulk Operation Endpoint
- [ ] Complete

**Route**: `POST /api/data/{collection}/bulk`

**Implementation**:
- [ ] Accept `BulkOperationRequest` in request body
- [ ] Validate collection name
- [ ] Validate request (action, records not empty)
- [ ] Call `_adapter.BulkOperationAsync`
- [ ] Return appropriate status code:
  - [ ] 200 OK for best-effort mode
  - [ ] 201 Created for atomic create (all succeed)
  - [ ] 400 Bad Request for atomic failure
  - [ ] 400 Bad Request for validation errors

**Response Format**:
- [ ] Match specification in `data-abstraction-api.md` (lines 250-281)

#### 1.4.2: Summary Endpoint
- [ ] Complete

**Route**: `GET /api/data/{collection}/summary?field={fieldName}`

**Implementation**:
- [ ] Extract `field` query parameter (required)
- [ ] Validate collection exists
- [ ] Validate field exists in collection
- [ ] Call `_adapter.GetSummaryAsync`
- [ ] Return 200 OK with `SummaryResult`

**Response Format**:
```json
{
  "active": 45,
  "inactive": 12,
  "pending": 8
}
```

#### 1.4.3: Aggregate Endpoint
- [ ] Complete

**Route**: `POST /api/data/{collection}/aggregate`

**Implementation**:
- [ ] Accept `AggregateRequest` in request body
- [ ] Validate collection exists
- [ ] Validate group-by fields exist
- [ ] Validate aggregate fields exist
- [ ] Call `_adapter.AggregateAsync`
- [ ] Return 200 OK with `AggregateResult`

**Response Format**:
```json
{
  "data": [
    {
      "category": "Electronics",
      "status": "active",
      "avg_price": 299.99,
      "total_qty": 150,
      "count": 25
    }
  ]
}
```

### Step 1.5: Write Tests
- [X] Complete

**Location**: `DataAbstractionAPI.Adapters.Tests/` and `DataAbstractionAPI.API.Tests/`

#### Adapter Tests:
- [X] `CsvAdapter_BulkOperationAsync_Create_Atomic_Success`
- [X] `CsvAdapter_BulkOperationAsync_Create_Atomic_Failure`
- [X] `CsvAdapter_BulkOperationAsync_Create_BestEffort`
- [X] `CsvAdapter_BulkOperationAsync_Update_Atomic`
- [X] `CsvAdapter_BulkOperationAsync_Delete_Atomic`
- [X] `CsvAdapter_BulkOperationAsync_HandlesCancellation`
- [X] `CsvAdapter_GetSummaryAsync_ReturnsFieldCounts`
- [X] `CsvAdapter_GetSummaryAsync_HandlesNullValues`
- [X] `CsvAdapter_GetSummaryAsync_WithFilter`
- [X] `CsvAdapter_AggregateAsync_SimpleGroupBy`
- [X] `CsvAdapter_AggregateAsync_MultipleGroupBy`
- [X] `CsvAdapter_AggregateAsync_MultipleAggregates`
- [X] `CsvAdapter_AggregateAsync_WithFilter`

#### API Tests:
- [X] `DataController_BulkOperation_Create_Returns201`
- [X] `DataController_BulkOperation_AtomicFailure_Returns400`
- [X] `DataController_BulkOperation_BestEffort_Returns200`
- [X] `DataController_GetSummary_ReturnsCounts`
- [X] `DataController_GetSummary_InvalidField_Returns400`
- [X] `DataController_Aggregate_ReturnsGroupedData`
- [X] `DataController_Aggregate_InvalidRequest_Returns400`

### Step 1.6: Update Swagger Documentation
- [X] Complete

**Location**: `DataAbstractionAPI.API/Program.cs` and controller attributes

- [X] Add XML comments to controller methods
- [X] Ensure Swagger shows request/response examples
- [X] Document error responses

### Estimated Effort
- **Step 1.1**: 2 hours (interface design, models)
- **Step 1.2**: 4 hours (create all models)
- **Step 1.3**: 12 hours (implement three methods)
- **Step 1.4**: 4 hours (three endpoints)
- **Step 1.5**: 8 hours (comprehensive tests)
- **Step 1.6**: 2 hours (documentation)

**Total**: ~32 hours (4 days)

---

## 2. Advanced Schema Endpoints Plan

### Overview
Implement schema management endpoints:
- **Create Collection** (`POST /api/schema`)
- **Rename Collection** (`PATCH /api/schema/{collection}`)
- **Delete Collection** (`DELETE /api/schema/{collection}`)
- **Add Field** (`POST /api/schema/{collection}/fields`)
- **Modify Field** (`PATCH /api/schema/{collection}/fields/{fieldName}`)
- **Delete Field** (`DELETE /api/schema/{collection}/fields/{fieldName}`)

### Prerequisites
- [ ] Review `data-abstraction-api.md` specification (lines 357-644)
- [ ] Understand `CsvSchemaManager` capabilities
- [ ] Review existing schema update logic in `CsvAdapter.UpdateAsync`

### Step 2.1: Extend Core Interface (IDataAdapter)
- [ ] Complete

**Location**: `DataAbstractionAPI.Core/Interfaces/IDataAdapter.cs`

**New Methods to Add**:
```csharp
Task<SchemaResult> CreateCollectionAsync(string collection, CollectionSchema schema, CancellationToken ct = default);
Task<SchemaResult> RenameCollectionAsync(string oldName, string newName, CancellationToken ct = default);
Task<SchemaResult> DeleteCollectionAsync(string collection, bool dryRun = false, CancellationToken ct = default);
Task<SchemaResult> AddFieldAsync(string collection, FieldDefinition field, CancellationToken ct = default);
Task<SchemaResult> ModifyFieldAsync(string collection, string fieldName, FieldModificationRequest request, CancellationToken ct = default);
Task<SchemaResult> DeleteFieldAsync(string collection, string fieldName, bool dryRun = false, CancellationToken ct = default);
```

### Step 2.2: Create Core Models
- [ ] Complete

**Location**: `DataAbstractionAPI.Core/Models/`

**New Models to Create**:

1. **SchemaResult.cs**
   - [ ] Create file
   - `bool Success`
   - `string? Collection` (collection name)
   - `string? Field` (field name for field operations)
   - `int? Fields` (number of fields for create collection)
   - `int? RecordsAffected` (for delete operations)
   - `string? OldName` (for rename)
   - `string? NewName` (for rename)
   - `string? OldType` (for modify field)
   - `string? NewType` (for modify field)
   - `int? ConversionErrors` (for modify field type changes)
   - `List<ConversionError>? Errors` (detailed conversion errors)
   - `bool? DryRun` (for dry-run operations)
   - `int? RecordsAffected` (for dry-run preview)
   - `List<string>? Warnings`

2. **FieldModificationRequest.cs**
   - [ ] Create file
   - `string? Name` (new name for rename)
   - `FieldType? Type` (new type)
   - `bool? Nullable` (new nullability)
   - `object? Default` (new default value)
   - `ConversionStrategy? ConversionStrategy` (for type changes)

3. **ConversionError.cs**
   - [ ] Create file
   - `string RecordId`
   - `object? Value`
   - `string Error`

4. **CreateCollectionRequest.cs**
   - [ ] Create file
   - `string Name`
   - `List<FieldDefinition> Fields`

5. **RenameCollectionRequest.cs**
   - [ ] Create file
   - `string Name` (new name)

### Step 2.3: Implement CsvAdapter Methods
- [ ] Complete

**Location**: `DataAbstractionAPI.Adapters.Csv/CsvAdapter.cs`

#### 2.3.1: CreateCollectionAsync
- [ ] Complete

**Logic**:
- [ ] Validate collection name (security check)
- [ ] Check if collection already exists
- [ ] Create empty CSV file with headers from schema
- [ ] Create schema file if fields have metadata
- [ ] Return success with field count

**Implementation Notes**:
- [ ] Use `CsvFileHandler` to write headers
- [ ] Use `CsvSchemaManager` to save schema
- [ ] Handle file locking
- [ ] Apply intelligent defaults if DefaultGenerator available

#### 2.3.2: RenameCollectionAsync
- [ ] Complete

**Logic**:
- [ ] Validate old and new collection names
- [ ] Check old collection exists
- [ ] Check new collection doesn't exist
- [ ] Rename CSV file
- [ ] Rename schema file if exists
- [ ] Return success with old/new names

**Implementation Notes**:
- [ ] Use `File.Move` for atomic rename
- [ ] Handle schema file rename
- [ ] Use file locking during operation

#### 2.3.3: DeleteCollectionAsync
- [ ] Complete

**Logic**:
- [ ] Validate collection name
- [ ] Check collection exists
- [ ] If dryRun=true:
  - [ ] Count records
  - [ ] Return preview with warnings
- [ ] If dryRun=false:
  - [ ] Delete CSV file
  - [ ] Delete schema file if exists
  - [ ] Return success with record count

**Implementation Notes**:
- [ ] Count records before deletion for dry-run
- [ ] Use file locking to prevent concurrent access
- [ ] Return appropriate warnings

#### 2.3.4: AddFieldAsync
- [ ] Complete

**Logic**:
- [ ] Validate collection exists
- [ ] Validate field name (not duplicate)
- [ ] Determine default value:
  - [ ] Use provided default if specified
  - [ ] Use DefaultGenerator if available
  - [ ] Use empty string as fallback
- [ ] Read all records
- [ ] Add new column to headers
- [ ] Apply default value to all existing records
- [ ] Write updated CSV
- [ ] Update schema file
- [ ] Return success with applied count

**Implementation Notes**:
- [ ] Reuse logic from `UpdateAsync` new field handling
- [ ] Use DefaultGenerator for intelligent defaults
- [ ] Handle file locking
- [ ] Update schema file metadata

#### 2.3.5: ModifyFieldAsync
- [ ] Complete

**Logic**:
- [ ] Validate collection exists
- [ ] Validate field exists
- [ ] Read all records
- [ ] Apply modifications:
  - [ ] **Rename**: Update header, update all records
  - [ ] **Type change**: Use TypeConverter with specified strategy
  - [ ] **Nullable change**: Validate existing nulls if making non-nullable
  - [ ] **Default change**: Update schema only (doesn't affect existing records)
- [ ] Write updated CSV
- [ ] Update schema file
- [ ] Return success with conversion errors if any

**Implementation Notes**:
- [ ] Use TypeConverter for type conversions
- [ ] Support all ConversionStrategy options (cast, truncate, fail_on_error, set_null)
- [ ] Collect conversion errors for best-effort mode
- [ ] Handle file locking
- [ ] Update schema file

**Type Conversion Logic**:
- [ ] If strategy = `fail_on_error`: Fail entire operation on first error
- [ ] If strategy = `cast`: Attempt conversion, set null on failure (if nullable)
- [ ] If strategy = `truncate`: Truncate strings, cast numbers
- [ ] If strategy = `set_null`: Set unconvertible values to null (requires nullable=true)

#### 2.3.6: DeleteFieldAsync
- [ ] Complete

**Logic**:
- [ ] Validate collection exists
- [ ] Validate field exists
- [ ] If dryRun=true:
  - [ ] Return preview with record count
- [ ] If dryRun=false:
  - [ ] Read all records
  - [ ] Remove column from headers
  - [ ] Remove field from all records
  - [ ] Write updated CSV
  - [ ] Update schema file
  - [ ] Return success with record count

**Implementation Notes**:
- [ ] Use file locking
- [ ] Update schema file
- [ ] Handle cancellation

### Step 2.4: Create Schema Controller
- [ ] Complete

**Location**: `DataAbstractionAPI.API/Controllers/SchemaController.cs` (NEW FILE)

**Create new controller** for schema operations (separate from DataController for better organization)

**Endpoints**:

1. [ ] `POST /api/schema` - CreateCollection
2. [ ] `PATCH /api/schema/{collection}` - RenameCollection
3. [ ] `DELETE /api/schema/{collection}` - DeleteCollection
4. [ ] `POST /api/schema/{collection}/fields` - AddField
5. [ ] `PATCH /api/schema/{collection}/fields/{fieldName}` - ModifyField
6. [ ] `DELETE /api/schema/{collection}/fields/{fieldName}` - DeleteField

**Implementation Notes**:
- [ ] Use same authentication/error handling as DataController
- [ ] Validate collection names (security)
- [ ] Return appropriate HTTP status codes
- [ ] Handle dry-run query parameter for delete operations

### Step 2.5: Write Tests
- [ ] Complete

**Location**: `DataAbstractionAPI.Adapters.Tests/` and `DataAbstractionAPI.API.Tests/`

#### Adapter Tests:
- [ ] `CsvAdapter_CreateCollectionAsync_CreatesEmptyCsv`
- [ ] `CsvAdapter_CreateCollectionAsync_CreatesSchemaFile`
- [ ] `CsvAdapter_CreateCollectionAsync_DuplicateName_ThrowsException`
- [ ] `CsvAdapter_RenameCollectionAsync_RenamesFiles`
- [ ] `CsvAdapter_RenameCollectionAsync_NewNameExists_ThrowsException`
- [ ] `CsvAdapter_DeleteCollectionAsync_DeletesFiles`
- [ ] `CsvAdapter_DeleteCollectionAsync_DryRun_ReturnsPreview`
- [ ] `CsvAdapter_AddFieldAsync_AddsColumnToCsv`
- [ ] `CsvAdapter_AddFieldAsync_UsesIntelligentDefaults`
- [ ] `CsvAdapter_AddFieldAsync_UpdatesSchemaFile`
- [ ] `CsvAdapter_ModifyFieldAsync_Rename_UpdatesHeaders`
- [ ] `CsvAdapter_ModifyFieldAsync_TypeChange_ConvertsValues`
- [ ] `CsvAdapter_ModifyFieldAsync_TypeChange_FailOnError_ThrowsOnFailure`
- [ ] `CsvAdapter_ModifyFieldAsync_TypeChange_CollectsErrors`
- [ ] `CsvAdapter_DeleteFieldAsync_RemovesColumn`
- [ ] `CsvAdapter_DeleteFieldAsync_DryRun_ReturnsPreview`

#### API Tests:
- [ ] `SchemaController_CreateCollection_Returns201`
- [ ] `SchemaController_CreateCollection_InvalidName_Returns400`
- [ ] `SchemaController_RenameCollection_Returns200`
- [ ] `SchemaController_DeleteCollection_Returns200`
- [ ] `SchemaController_DeleteCollection_DryRun_Returns200`
- [ ] `SchemaController_AddField_Returns200`
- [ ] `SchemaController_ModifyField_Returns200`
- [ ] `SchemaController_DeleteField_Returns200`

### Step 2.6: Update Swagger Documentation
- [ ] Complete

- [ ] Add XML comments to all controller methods
- [ ] Document request/response formats
- [ ] Document dry-run behavior
- [ ] Document conversion strategies

### Estimated Effort
- **Step 2.1**: 1 hour (interface design)
- **Step 2.2**: 3 hours (create models)
- **Step 2.3**: 16 hours (implement 6 methods)
- **Step 2.4**: 4 hours (create controller, 6 endpoints)
- **Step 2.5**: 10 hours (comprehensive tests)
- **Step 2.6**: 2 hours (documentation)

**Total**: ~36 hours (4.5 days)

---

## 3. DTOs with [JsonPropertyName] Attributes Plan

### Overview
Create Data Transfer Objects (DTOs) with proper JSON property naming for API responses, replacing direct use of Core models. This improves API contract clarity and allows for API-specific optimizations.

### Prerequisites
- [X] Review current API responses (using Core models directly)
- [X] Understand JSON serialization in ASP.NET Core
- [X] Review `data-abstraction-api.md` for response format requirements

### Step 3.1: Analyze Current API Response Models
- [X] Complete

**Current State**:
- `ListResult` - used directly in API
- `Record` - used directly in API
- `CreateResult` - used directly in API
- `CollectionSchema` - used directly in API
- `FieldDefinition` - used directly in API

**Issues**:
- Property names match C# naming (PascalCase) but spec may want camelCase
- No API-specific optimizations (e.g., compact keys like "d", "t", "more")
- Cannot evolve API independently from Core models

### Step 3.2: Design DTO Strategy
- [X] Complete

**Decision Point**: 
- [X] Option A: Create separate DTOs for all API responses (full separation) - **SELECTED**
- [ ] Option B: Add [JsonPropertyName] to Core models (quick fix, but couples API to Core)
- [ ] Option C: Create DTOs only for responses that need different structure (hybrid)

**Recommendation**: Option A (separate DTOs) for better separation of concerns

**Location**: `DataAbstractionAPI.API/Models/DTOs/` (NEW DIRECTORY)

### Step 3.3: Create Response DTOs
- [X] Complete

**Location**: `DataAbstractionAPI.API/Models/DTOs/`

#### 3.3.1: ListResponseDto.cs
- [X] Create file

**Purpose**: Replace `ListResult` in API responses

**Properties**:
```csharp
[JsonPropertyName("d")]
public List<RecordDto> Data { get; set; }

[JsonPropertyName("t")]
public int Total { get; set; }

[JsonPropertyName("more")]
public bool More { get; set; }

[JsonPropertyName("cursor")]
public string? Cursor { get; set; } // For future pagination
```

**Mapping**: Create extension method or mapper to convert `ListResult` → `ListResponseDto`

#### 3.3.2: RecordDto.cs
- [X] Create file

**Purpose**: Replace `Record` in API responses

**Properties**:
```csharp
[JsonPropertyName("id")]
public string Id { get; set; }

[JsonPropertyName("d")]
public Dictionary<string, object> Data { get; set; }
```

**Note**: Consider if compact "d" key is needed, or use full property names

#### 3.3.3: CreateResponseDto.cs
- [X] Create file

**Purpose**: Replace `CreateResult` in API responses

**Properties**:
```csharp
[JsonPropertyName("d")]
public RecordDto Record { get; set; }

[JsonPropertyName("id")]
public string Id { get; set; }
```

#### 3.3.4: UpdateResponseDto.cs
- [X] Create file

**Purpose**: New DTO for update responses (currently returns NoContent) - **IMPLEMENTED**

**Properties**:
```csharp
[JsonPropertyName("d")]
public Dictionary<string, object> UpdatedFields { get; set; }

[JsonPropertyName("success")]
public bool Success { get; set; }
```

**Note**: Currently UpdateAsync returns NoContent, but spec shows response with updated fields

#### 3.3.5: DeleteResponseDto.cs
- [X] Create file

**Purpose**: New DTO for delete responses (currently returns NoContent) - **IMPLEMENTED**

**Properties**:
```csharp
[JsonPropertyName("success")]
public bool Success { get; set; }

[JsonPropertyName("id")]
public string Id { get; set; }
```

#### 3.3.6: SchemaResponseDto.cs
- [X] Create file

**Purpose**: Replace `CollectionSchema` in API responses

**Properties**:
```csharp
[JsonPropertyName("name")]
public string Name { get; set; }

[JsonPropertyName("fields")]
public List<FieldDefinitionDto> Fields { get; set; }
```

#### 3.3.7: FieldDefinitionDto.cs
- [X] Create file

**Purpose**: Replace `FieldDefinition` in API responses

**Properties**:
```csharp
[JsonPropertyName("name")]
public string Name { get; set; }

[JsonPropertyName("type")]
public string Type { get; set; } // Serialize enum as string

[JsonPropertyName("nullable")]
public bool Nullable { get; set; }

[JsonPropertyName("default")]
public object? Default { get; set; }
```

#### 3.3.8: BulkResponseDto.cs
- [ ] Create file (Deferred - will create when implementing Advanced Data Endpoints)

**Purpose**: For bulk operation responses (when implementing Step 1)

**Properties**: (See Step 1.2 for BulkResult structure)

#### 3.3.9: SummaryResponseDto.cs
- [ ] Create file (Deferred - will create when implementing Advanced Data Endpoints)

**Purpose**: For summary responses (when implementing Step 1)

**Properties**: Dictionary<string, int> (no special naming needed)

#### 3.3.10: AggregateResponseDto.cs
- [ ] Create file (Deferred - will create when implementing Advanced Data Endpoints)

**Purpose**: For aggregate responses (when implementing Step 1)

**Properties**:
```csharp
[JsonPropertyName("d")]
public List<Dictionary<string, object>> Data { get; set; }
```

### Step 3.4: Create Request DTOs
- [X] Complete

**Location**: `DataAbstractionAPI.API/Models/DTOs/`

#### 3.4.1: CreateRecordRequestDto.cs
- [X] Decision: Keep as `Dictionary<string, object>` (no separate DTO needed)

**Purpose**: Request DTO for creating records (currently uses `Dictionary<string, object>`)

**Properties**:
- [X] Keep as `Dictionary<string, object>` or create strongly-typed DTO?
- **Decision**: Keep flexible with Dictionary for schema flexibility

#### 3.4.2: UpdateRecordRequestDto.cs
- [X] Decision: Keep as `Dictionary<string, object>` (no separate DTO needed)

**Purpose**: Request DTO for updating records

**Properties**:
- [X] Keep as `Dictionary<string, object>` for partial updates

#### 3.4.3: QueryRequestDto.cs
- [X] Create file

**Purpose**: For POST /data/{collection}/query endpoint (if implementing)

**Properties**:
```csharp
[JsonPropertyName("fields")]
public string[]? Fields { get; set; }

[JsonPropertyName("filter")]
public Dictionary<string, object>? Filter { get; set; }

[JsonPropertyName("limit")]
public int Limit { get; set; }

[JsonPropertyName("offset")]
public int Offset { get; set; }

[JsonPropertyName("sort")]
public string? Sort { get; set; }
```

### Step 3.5: Create Mapping Logic
- [X] Complete

**Location**: `DataAbstractionAPI.API/Mapping/` (NEW DIRECTORY)

**Options**:
- [X] Option A: Extension methods (simple, no dependencies) - **SELECTED**
- [ ] Option B: AutoMapper (powerful, but adds dependency)
- [ ] Option C: Manual mapper class (explicit, no magic)

**Recommendation**: Option A (extension methods) for simplicity

**Files to Create**:
- [X] `ListResultExtensions.cs` - `ToDto()` extension
- [X] `RecordExtensions.cs` - `ToDto()` extension
- [X] `CreateResultExtensions.cs` - `ToDto()` extension
- [X] `CollectionSchemaExtensions.cs` - `ToDto()` extension
- [X] `FieldDefinitionExtensions.cs` - `ToDto()` extension

**Example**:
```csharp
public static class ListResultExtensions
{
    public static ListResponseDto ToDto(this ListResult result)
    {
        return new ListResponseDto
        {
            Data = result.Data.Select(r => r.ToDto()).ToList(),
            Total = result.Total,
            More = result.More
        };
    }
}
```

### Step 3.6: Update Controllers
- [X] Complete

**Location**: `DataAbstractionAPI.API/Controllers/DataController.cs`

**Changes**:
- [X] Update return types from Core models to DTOs
- [X] Add `.ToDto()` calls after adapter methods
- [X] Update XML comments
- [X] Ensure Swagger shows DTOs
- [X] Update UpdateRecord to return UpdateResponseDto (was NoContent)
- [X] Update DeleteRecord to return DeleteResponseDto (was NoContent)

**Example**:
```csharp
[HttpGet("{collection}")]
public async Task<ActionResult<ListResponseDto>> GetCollection(...)
{
    var result = await _adapter.ListAsync(collection, options, cancellationToken);
    return Ok(result.ToDto());
}
```

### Step 3.7: Configure JSON Serialization
- [X] Complete

**Location**: `DataAbstractionAPI.API/Program.cs`

**Changes**:
- [X] Ensure System.Text.Json is configured for camelCase (if desired)
- [X] Or rely on [JsonPropertyName] attributes - **SELECTED**
- [X] Configure enum serialization (as strings)
- [X] Set PropertyNamingPolicy to null (use [JsonPropertyName] attributes)
- [X] Add JsonStringEnumConverter for enum serialization

### Step 3.8: Update Tests
- [X] Complete

**Location**: `DataAbstractionAPI.API.Tests/`

**Changes**:
- [X] Update test assertions to check DTO properties
- [X] Verify JSON property names in integration tests
- [X] Test mapping logic
- [X] Created MappingTests.cs (8 tests for mapping logic)
- [X] Created DtoIntegrationTests.cs (6 tests for JSON serialization)
- [X] Updated DataControllerTests.cs (15 tests updated for DTOs)

### Step 3.9: Update Swagger
- [X] Complete

- [X] Verify Swagger shows correct property names (via [ProducesResponseType] attributes)
- [X] Add examples to DTOs (XML comments added to all DTOs)
- [X] Document response formats (XML comments on controller methods)

### Estimated Effort
- **Step 3.1**: 1 hour (analysis)
- **Step 3.2**: 1 hour (design decision)
- **Step 3.3**: 4 hours (create 10+ DTOs)
- **Step 3.4**: 2 hours (create request DTOs)
- **Step 3.5**: 3 hours (create mapping logic)
- **Step 3.6**: 4 hours (update controllers)
- **Step 3.7**: 1 hour (JSON configuration)
- **Step 3.8**: 4 hours (update tests)
- **Step 3.9**: 2 hours (Swagger updates)

**Total**: ~22 hours (2.75 days)

---

## 4. CORS Configuration Plan

### Overview
Configure Cross-Origin Resource Sharing (CORS) to allow the API to be accessed from web browsers running on different origins (e.g., Blazor UI on different port, or external web applications).

### Prerequisites
- [X] Understand CORS requirements
- [X] Identify allowed origins (development, production)
- [X] Review ASP.NET Core CORS documentation

### Step 4.1: Determine CORS Requirements
- [X] Complete

**Questions to Answer**:
1. [X] Which origins should be allowed?
   - Development: `http://localhost:5001` (Blazor UI)
   - Production: TBD (configurable)
2. [X] Which HTTP methods should be allowed?
   - GET, POST, PUT, PATCH, DELETE (all current methods)
3. [X] Which headers should be allowed?
   - `Content-Type`
   - `X-API-Key` (for API key authentication)
   - `Authorization` (if adding Bearer tokens later)
4. [X] Should credentials be allowed?
   - Probably not needed for API key authentication
5. [X] Should preflight requests be cached?
   - Default (24 hours) is usually fine

### Step 4.2: Add CORS Configuration to appsettings.json
- [X] Complete

**Location**: `DataAbstractionAPI.API/appsettings.json` and `appsettings.Development.json`

**Configuration Structure**:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5001",
      "https://localhost:5002"
    ],
    "AllowedMethods": [
      "GET",
      "POST",
      "PUT",
      "PATCH",
      "DELETE",
      "OPTIONS"
    ],
    "AllowedHeaders": [
      "Content-Type",
      "X-API-Key",
      "Authorization"
    ],
    "AllowCredentials": false,
    "PreflightMaxAge": 86400
  }
}
```

**Development vs Production**:
- Development: Allow localhost origins
- Production: Configure specific production origins

### Step 4.3: Create CORS Configuration Model
- [X] Complete

**Location**: `DataAbstractionAPI.API/Configuration/CorsOptions.cs` (NEW FILE)

**Properties**:
```csharp
public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; } = false;
    public int? PreflightMaxAge { get; set; } = 86400;
}
```

### Step 4.4: Configure CORS in Program.cs
- [X] Complete

**Location**: `DataAbstractionAPI.API/Program.cs`

**Changes**:
1. [X] Bind CORS configuration from appsettings
2. [X] Add CORS service with named policy
3. [X] Use CORS middleware in pipeline (before UseAuthorization)

**Implementation**:
```csharp
// Configure CORS
builder.Services.Configure<CorsOptions>(
    builder.Configuration.GetSection("Cors"));

var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptions>() 
    ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        if (corsOptions.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsOptions.AllowedOrigins);
        }
        else
        {
            // Fallback: allow all origins in development
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin();
            }
        }

        if (corsOptions.AllowedMethods.Length > 0)
        {
            policy.WithMethods(corsOptions.AllowedMethods);
        }
        else
        {
            policy.WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS");
        }

        if (corsOptions.AllowedHeaders.Length > 0)
        {
            policy.WithHeaders(corsOptions.AllowedHeaders);
        }
        else
        {
            policy.WithHeaders("Content-Type", "X-API-Key", "Authorization");
        }

        if (corsOptions.AllowCredentials)
        {
            policy.AllowCredentials();
        }

        if (corsOptions.PreflightMaxAge.HasValue)
        {
            policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsOptions.PreflightMaxAge.Value));
        }
    });
});

// In app configuration:
app.UseCors("DefaultPolicy");
```

**Middleware Order**:
```
app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");  // Before UseAuthorization
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseAuthorization();
app.MapControllers();
```

### Step 4.5: Add CORS to Swagger (if needed)
- [X] Complete (Not needed - Swagger runs on same origin)

**Location**: `DataAbstractionAPI.API/Program.cs`

**Note**: Swagger UI typically doesn't need CORS (same origin), but if hosting Swagger separately, may need it.

### Step 4.6: Write Tests
- [X] Complete

**Location**: `DataAbstractionAPI.API.Tests/`

**Tests to Create**:
- [X] `CorsMiddleware_AllowsConfiguredOrigins_ReturnsCorsHeaders`
- [X] `CorsMiddleware_HandlesPreflightRequest_Returns200`
- [X] `CorsMiddleware_IncludesRequiredHeaders_InResponse`
- [X] `CorsMiddleware_WithPostRequest_IncludesCorsHeaders`
- [X] `CorsMiddleware_AllowsApiKeyHeader_InPreflight`

**Implementation Notes**:
- Use `TestServer` to test CORS headers
- Verify `Access-Control-Allow-Origin` header
- Verify `Access-Control-Allow-Methods` header
- Verify `Access-Control-Allow-Headers` header
- Test preflight OPTIONS requests

### Step 4.7: Update Documentation
- [X] Complete

**Location**: README.md or API documentation

**Documentation to Add**:
- [X] CORS configuration instructions
- [X] How to configure allowed origins
- [X] Development vs production settings
- [X] Troubleshooting CORS issues

### Step 4.8: Security Considerations
- [X] Complete

**Important Notes**:
- [X] **Never use `AllowAnyOrigin()` in production** (security risk) - Implemented with environment check
- [X] Always specify exact origins in production - Configuration supports this
- [X] Consider using environment variables for production origins - Documented
- [X] Review CORS policy regularly - Documented in README

### Estimated Effort
- **Step 4.1**: 1 hour (requirements analysis)
- **Step 4.2**: 1 hour (configuration files)
- **Step 4.3**: 1 hour (create model)
- **Step 4.4**: 2 hours (configure middleware)
- **Step 4.5**: 0.5 hours (Swagger if needed)
- **Step 4.6**: 3 hours (write tests)
- **Step 4.7**: 1 hour (documentation)
- **Step 4.8**: 0.5 hours (security review)

**Total**: ~10 hours (1.25 days)

---

## 5. Agent Discovery Endpoint Plan

### Overview
Create a discovery endpoint (`GET /api/help` or `GET /api/docs`) that provides machine-readable information to help agents (LLMs, automated clients) understand how to use the API. This endpoint will be available in both Development and Production environments, unlike Swagger which is development-only.

### Prerequisites
- [ ] Understand current API structure and endpoints
- [ ] Review `data-abstraction-api.md` specification for agent usage patterns
- [ ] Consider what information agents need to discover and use the API

### Step 5.1: Design Discovery Endpoint Response

**Location**: New model in `DataAbstractionAPI.API/Models/DTOs/`

**Decision Point**: What information should the discovery endpoint provide?

**Recommended Response Structure**:
```json
{
  "api_version": "v1",
  "base_url": "http://localhost:5012/api",
  "documentation": {
    "openapi_json": "http://localhost:5012/swagger/v1/swagger.json",
    "swagger_ui": "http://localhost:5012/swagger",
    "available": true
  },
  "authentication": {
    "type": "api_key",
    "header_name": "X-API-Key",
    "required": false,
    "description": "Optional API key authentication. Configured via appsettings.json"
  },
  "endpoints": {
    "collections": {
      "list": "GET /api/data",
      "description": "List all available collections"
    },
    "data": {
      "list": "GET /api/data/{collection}",
      "get": "GET /api/data/{collection}/{id}",
      "create": "POST /api/data/{collection}",
      "update": "PUT /api/data/{collection}/{id}",
      "delete": "DELETE /api/data/{collection}/{id}",
      "summary": "GET /api/data/{collection}/summary?field={fieldName}",
      "bulk": "POST /api/data/{collection}/bulk",
      "aggregate": "POST /api/data/{collection}/aggregate"
    },
    "schema": {
      "get": "GET /api/data/{collection}/schema",
      "description": "Get collection schema (field definitions)"
    },
    "upload": {
      "upload": "POST /api/data/upload",
      "description": "Upload CSV file to create or replace a collection",
      "content_type": "multipart/form-data"
    }
  },
  "common_patterns": {
    "discover_collections": "1. GET /api/data to list collections",
    "discover_schema": "2. GET /api/data/{collection}/schema to get field definitions",
    "query_data": "3. GET /api/data/{collection}?limit=10 to query records",
    "field_projection": "Note: Field projection, filtering, and sorting available in adapter layer but not yet exposed via REST API"
  },
  "response_formats": {
    "list_response": {
      "data_key": "d",
      "total_key": "t",
      "more_key": "more"
    },
    "compact_keys": true,
    "description": "Responses use compact keys (d, t, more) for token efficiency"
  }
}
```

**Alternative Simpler Version** (if full structure is too complex):
```json
{
  "api_version": "v1",
  "base_url": "http://localhost:5012/api",
  "openapi_spec": "http://localhost:5012/swagger/v1/swagger.json",
  "authentication": {
    "type": "api_key",
    "header": "X-API-Key",
    "required": false
  },
  "quick_start": [
    "GET /api/data - List collections",
    "GET /api/data/{collection}/schema - Get schema",
    "GET /api/data/{collection} - List records",
    "GET /api/data/{collection}/summary?field={field} - Get field counts"
  ]
}
```

**Recommendation**: Start with simpler version, can expand later

### Step 5.2: Create Discovery Response DTO

**Location**: `DataAbstractionAPI.API/Models/DTOs/DiscoveryResponseDto.cs` (NEW FILE)

**Properties**:
```csharp
public class DiscoveryResponseDto
{
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; } = "v1";
    
    [JsonPropertyName("base_url")]
    public string BaseUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("openapi_spec")]
    public string? OpenApiSpec { get; set; }
    
    [JsonPropertyName("swagger_ui")]
    public string? SwaggerUi { get; set; }
    
    [JsonPropertyName("openapi_available")]
    public bool OpenApiAvailable { get; set; }
    
    [JsonPropertyName("authentication")]
    public AuthenticationInfoDto Authentication { get; set; } = new();
    
    [JsonPropertyName("quick_start")]
    public List<string> QuickStart { get; set; } = new();
    
    [JsonPropertyName("endpoints")]
    public EndpointsInfoDto? Endpoints { get; set; }
}

public class AuthenticationInfoDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "api_key";
    
    [JsonPropertyName("header")]
    public string Header { get; set; } = "X-API-Key";
    
    [JsonPropertyName("required")]
    public bool Required { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class EndpointsInfoDto
{
    [JsonPropertyName("collections")]
    public EndpointInfoDto? Collections { get; set; }
    
    [JsonPropertyName("data")]
    public DataEndpointsInfoDto? Data { get; set; }
    
    [JsonPropertyName("schema")]
    public EndpointInfoDto? Schema { get; set; }
    
    [JsonPropertyName("upload")]
    public EndpointInfoDto? Upload { get; set; }
}

public class DataEndpointsInfoDto
{
    [JsonPropertyName("list")]
    public string List { get; set; } = "GET /api/data/{collection}";
    
    [JsonPropertyName("get")]
    public string Get { get; set; } = "GET /api/data/{collection}/{id}";
    
    [JsonPropertyName("create")]
    public string Create { get; set; } = "POST /api/data/{collection}";
    
    [JsonPropertyName("update")]
    public string Update { get; set; } = "PUT /api/data/{collection}/{id}";
    
    [JsonPropertyName("delete")]
    public string Delete { get; set; } = "DELETE /api/data/{collection}/{id}";
    
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
    
    [JsonPropertyName("bulk")]
    public string? Bulk { get; set; }
    
    [JsonPropertyName("aggregate")]
    public string? Aggregate { get; set; }
}

public class EndpointInfoDto
{
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
```

### Step 5.3: Create Discovery Controller Endpoint

**Location**: `DataAbstractionAPI.API/Controllers/DataController.cs` (add new method) OR create `DiscoveryController.cs` (NEW FILE)

**Decision Point**: 
- Option A: Add to existing `DataController.cs` (simpler, fewer files)
- Option B: Create separate `DiscoveryController.cs` (better organization)

**Recommendation**: Option A (add to DataController) for simplicity

**Route**: `GET /api/help` or `GET /api/docs`

**Implementation**:
```csharp
/// <summary>
/// Discovery endpoint for agents and automated clients.
/// Provides machine-readable information about the API structure and usage.
/// </summary>
/// <param name="request">HTTP request (for determining base URL)</param>
/// <returns>Discovery information including endpoints, authentication, and quick start guide</returns>
[HttpGet("help")]
[ProducesResponseType(typeof(DiscoveryResponseDto), StatusCodes.Status200OK)]
public ActionResult<DiscoveryResponseDto> GetHelp([FromServices] IWebHostEnvironment environment)
{
    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api";
    var isDevelopment = environment.IsDevelopment();
    
    var response = new DiscoveryResponseDto
    {
        ApiVersion = "v1",
        BaseUrl = baseUrl,
        OpenApiSpec = isDevelopment ? $"{Request.Scheme}://{Request.Host}{Request.PathBase}/swagger/v1/swagger.json" : null,
        SwaggerUi = isDevelopment ? $"{Request.Scheme}://{Request.Host}{Request.PathBase}/swagger" : null,
        OpenApiAvailable = isDevelopment,
        Authentication = new AuthenticationInfoDto
        {
            Type = "api_key",
            Header = "X-API-Key",
            Required = false, // Check from configuration
            Description = "Optional API key authentication. Configured via appsettings.json"
        },
        QuickStart = new List<string>
        {
            "GET /api/data - List all available collections",
            "GET /api/data/{collection}/schema - Get collection schema (field definitions)",
            "GET /api/data/{collection} - List records in a collection (supports ?limit parameter)",
            "GET /api/data/{collection}/{id} - Get a single record by ID",
            "POST /api/data/{collection} - Create a new record",
            "PUT /api/data/{collection}/{id} - Update an existing record",
            "DELETE /api/data/{collection}/{id} - Delete a record",
            "GET /api/data/{collection}/summary?field={fieldName} - Get count of values for a field",
            "POST /api/data/{collection}/bulk - Perform bulk operations (create/update/delete)",
            "POST /api/data/{collection}/aggregate - Perform complex aggregations with grouping"
        }
    };
    
    // Optionally populate detailed endpoints structure
    // Can be expanded later if needed
    
    return Ok(response);
}
```

**Notes**:
- Use `Request.Scheme`, `Request.Host`, `Request.PathBase` to build URLs dynamically
- Check `IWebHostEnvironment.IsDevelopment()` to determine if OpenAPI is available
- Check API key configuration to determine if authentication is required
- Keep response simple initially, can expand later

### Step 5.4: Configure API Key Requirement Detection

**Location**: `DataAbstractionAPI.API/Controllers/DataController.cs` or helper method

**Implementation**:
- Read `ApiKeyAuthenticationOptions` from configuration
- Check if API key authentication is enabled
- Set `Authentication.Required` accordingly

**Code**:
```csharp
private bool IsApiKeyRequired()
{
    var apiKeyOptions = _configuration.GetSection("ApiKeyAuthentication").Get<ApiKeyAuthenticationOptions>();
    return apiKeyOptions?.Enabled == true && 
           apiKeyOptions?.ValidKeys != null && 
           apiKeyOptions.ValidKeys.Length > 0;
}
```

### Step 5.5: Write Tests

**Location**: `DataAbstractionAPI.API.Tests/Integration/DiscoveryEndpointTests.cs` (NEW FILE)

**Tests to Create**:
- [ ] `DiscoveryEndpoint_ReturnsValidJson_WithCorrectStructure`
- [ ] `DiscoveryEndpoint_IncludesBaseUrl_FromRequest`
- [ ] `DiscoveryEndpoint_InDevelopment_IncludesOpenApiLinks`
- [ ] `DiscoveryEndpoint_InProduction_ExcludesOpenApiLinks`
- [ ] `DiscoveryEndpoint_ReflectsApiKeyConfiguration_WhenEnabled`
- [ ] `DiscoveryEndpoint_ReflectsApiKeyConfiguration_WhenDisabled`
- [ ] `DiscoveryEndpoint_IncludesAllQuickStartEndpoints`
- [ ] `DiscoveryEndpoint_Returns200StatusCode`

**Implementation Notes**:
- Use `TestServer` for integration tests
- Test both Development and Production environments
- Verify JSON structure matches DTO
- Test with API key enabled/disabled configurations

### Step 5.6: Update Swagger Documentation

**Location**: `DataAbstractionAPI.API/Controllers/DataController.cs`

- [ ] Add XML comments to discovery endpoint method
- [ ] Document that this endpoint is always available (unlike Swagger)
- [ ] Add example response to Swagger
- [ ] Mark as important for agent/automated client usage

### Step 5.7: Update API Documentation

**Location**: `data-abstraction-api.md` and `README.md`

**Documentation to Add**:
- [ ] Document `/api/help` endpoint in API specification
- [ ] Explain that this is the recommended entry point for agents
- [ ] Add example discovery endpoint response
- [ ] Update "System Prompt for LLM" section to mention discovery endpoint
- [ ] Add to README.md as a feature

### Step 5.8: Consider Additional Features (Optional)

**Future Enhancements** (not in initial implementation):
- [ ] Include current collections list in response (could be expensive, make optional)
- [ ] Include schema examples
- [ ] Include rate limiting information
- [ ] Include API versioning information
- [ ] Support different response formats (JSON-LD, etc.)
- [ ] Include links to related resources (HATEOAS-style)

### Estimated Effort
- **Step 5.1**: 1 hour (design response structure)
- **Step 5.2**: 2 hours (create DTOs)
- **Step 5.3**: 2 hours (implement controller endpoint)
- **Step 5.4**: 1 hour (configure API key detection)
- **Step 5.5**: 3 hours (write tests)
- **Step 5.6**: 1 hour (update Swagger)
- **Step 5.7**: 1 hour (update documentation)

**Total**: ~11 hours (1.4 days)

### Benefits

1. **Agent-Friendly**: Provides machine-readable API information
2. **Always Available**: Works in Production (unlike Swagger)
3. **Self-Documenting**: API can explain itself to clients
4. **Standard Pattern**: Common convention (`/api/help`, `/api/docs`)
5. **Extensible**: Can grow to include more information over time

### Dependencies

- None - This is independent and can be implemented at any time
- Can be done before or after other Phase 3 enhancements
- Recommended: Implement early as it helps with testing other features

---

## Summary

### Total Estimated Effort

1. **Advanced Data Endpoints**: ~32 hours (4 days)
2. **Advanced Schema Endpoints**: ~36 hours (4.5 days)
3. **DTOs with [JsonPropertyName]**: ~22 hours (2.75 days)
4. **CORS Configuration**: ~10 hours (1.25 days)
5. **Agent Discovery Endpoint**: ~11 hours (1.4 days)

**Grand Total**: ~111 hours (~13.9 days)

### Recommended Implementation Order

1. **CORS Configuration** (easiest, quick win)
2. **Agent Discovery Endpoint** (helps with testing and agent integration, independent)
3. **DTOs with [JsonPropertyName]** (foundation for other endpoints)
4. **Advanced Data Endpoints** (Bulk, Summary, Aggregate)
5. **Advanced Schema Endpoints** (most complex, depends on DTOs)

### Dependencies

- DTOs should be done before Advanced Data/Schema endpoints (cleaner API responses)
- CORS is independent and can be done anytime
- Agent Discovery Endpoint is independent and can be done anytime (recommended early)
- Advanced Data and Schema endpoints are independent of each other

### Testing Strategy

- Write tests for each feature as implemented
- Integration tests for all endpoints
- Verify backward compatibility where applicable
- Test error scenarios thoroughly

### Notes

- All plans assume TDD approach (write tests first)
- Consider breaking large steps into smaller PRs
- Review and adjust estimates based on actual implementation complexity
- Update IMPLEMENTATION_PLAN.md as each item is completed

