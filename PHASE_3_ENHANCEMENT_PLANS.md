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
- [ ] Review `data-abstraction-api.md` specification (lines 227-353)
- [ ] Understand current `IDataAdapter` interface limitations
- [ ] Review existing `CsvAdapter` implementation patterns

### Step 1.1: Extend Core Interface (IDataAdapter)
- [ ] Complete

**Location**: `DataAbstractionAPI.Core/Interfaces/IDataAdapter.cs`

**Changes**:
- [ ] Add method signatures for bulk operations, summary, and aggregate
- [ ] Define new result models (BulkResult, SummaryResult, AggregateResult)
- [ ] Consider backward compatibility (make methods optional or add new interface)

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
- [ ] Complete

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
- [ ] Complete

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
- [ ] Complete

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
- [ ] Complete

**Location**: `DataAbstractionAPI.Adapters.Tests/` and `DataAbstractionAPI.API.Tests/`

#### Adapter Tests:
- [ ] `CsvAdapter_BulkOperationAsync_Create_Atomic_Success`
- [ ] `CsvAdapter_BulkOperationAsync_Create_Atomic_Failure`
- [ ] `CsvAdapter_BulkOperationAsync_Create_BestEffort`
- [ ] `CsvAdapter_BulkOperationAsync_Update_Atomic`
- [ ] `CsvAdapter_BulkOperationAsync_Delete_Atomic`
- [ ] `CsvAdapter_BulkOperationAsync_HandlesCancellation`
- [ ] `CsvAdapter_GetSummaryAsync_ReturnsFieldCounts`
- [ ] `CsvAdapter_GetSummaryAsync_HandlesNullValues`
- [ ] `CsvAdapter_GetSummaryAsync_WithFilter`
- [ ] `CsvAdapter_AggregateAsync_SimpleGroupBy`
- [ ] `CsvAdapter_AggregateAsync_MultipleGroupBy`
- [ ] `CsvAdapter_AggregateAsync_MultipleAggregates`
- [ ] `CsvAdapter_AggregateAsync_WithFilter`

#### API Tests:
- [ ] `DataController_BulkOperation_Create_Returns201`
- [ ] `DataController_BulkOperation_AtomicFailure_Returns400`
- [ ] `DataController_BulkOperation_BestEffort_Returns200`
- [ ] `DataController_GetSummary_ReturnsCounts`
- [ ] `DataController_GetSummary_InvalidField_Returns400`
- [ ] `DataController_Aggregate_ReturnsGroupedData`
- [ ] `DataController_Aggregate_InvalidRequest_Returns400`

### Step 1.6: Update Swagger Documentation
- [ ] Complete

**Location**: `DataAbstractionAPI.API/Program.cs` and controller attributes

- [ ] Add XML comments to controller methods
- [ ] Ensure Swagger shows request/response examples
- [ ] Document error responses

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

## Summary

### Total Estimated Effort

1. **Advanced Data Endpoints**: ~32 hours (4 days)
2. **Advanced Schema Endpoints**: ~36 hours (4.5 days)
3. **DTOs with [JsonPropertyName]**: ~22 hours (2.75 days)
4. **CORS Configuration**: ~10 hours (1.25 days)

**Grand Total**: ~100 hours (~12.5 days)

### Recommended Implementation Order

1. **CORS Configuration** (easiest, quick win)
2. **DTOs with [JsonPropertyName]** (foundation for other endpoints)
3. **Advanced Data Endpoints** (Bulk, Summary, Aggregate)
4. **Advanced Schema Endpoints** (most complex, depends on DTOs)

### Dependencies

- DTOs should be done before Advanced Data/Schema endpoints (cleaner API responses)
- CORS is independent and can be done anytime
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

