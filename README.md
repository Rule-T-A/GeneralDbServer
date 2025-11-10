# Data Abstraction API - C# Implementation

A .NET Core implementation of a unified data abstraction layer that provides a consistent interface for interacting with data across different storage backends.

**Status**: Phase 1 + 1.x + 2 + 3 + 3.1 + 3.2 + Coverage Improvement Complete ✅  
**Last Updated**: December 2025  
**Test Coverage**: 
- **Core**: 100% line, 100% branch, 100% method ✅
- **Services**: 91.27% line, 86.03% branch, 97.43% method ✅
- **Adapters.Csv**: 85.01% line, 68.04% branch, 86.3% method
- **API**: 89.22% line, 66.66% branch, 90.24% method
**Total Tests**: 556 passing (94 Core + 154 Adapter + 185 Services + 123 API)

---

## Overview

This project implements a storage-agnostic data access API following TDD (Test-Driven Development) principles. The foundation phase includes:

- **Core domain models and interfaces**
- **CSV storage adapter** with full CRUD operations
- **File locking** for concurrency safety
- **Security validation** to prevent path traversal attacks
- **Full CRUD operations** (Create, Read, Update, Delete)
- **Advanced Data Endpoints** (Bulk Operations, Summary, Aggregate)
- **Schema operations** (Get schema, List collections)
- **Services layer** (DefaultGenerator, TypeConverter, FilterEvaluator, ValidationService)
- **REST API** with Swagger documentation
- **Limitations remediation** (cancellation tokens, retry logic, field persistence, schema consistency)
- **Comprehensive test coverage** (556 tests passing, 100% Core coverage, 91.27% Services coverage, 85.01% Adapters.Csv line coverage, 89.22% API line coverage)

---

## Project Structure

```
GeneralDbServer/
├── DataAbstractionAPI.Core/                    # Core models, interfaces, and enums
│   ├── Enums/                                  # FieldType, StorageType, ConversionStrategy, DefaultGenerationStrategy
│   ├── Exceptions/                             # ConversionException, ValidationException
│   ├── Interfaces/                             # IDataAdapter, IDefaultGenerator, ITypeConverter
│   └── Models/                                  # Record, QueryOptions, ListResult, CollectionSchema, etc.
├── DataAbstractionAPI.Adapters.Csv/           # CSV storage adapter
│   ├── CsvAdapter.cs                          # Main adapter implementation
│   ├── CsvFileHandler.cs                      # CSV file read/write operations
│   ├── CsvFileLock.cs                         # File locking mechanism
│   ├── CsvSchemaManager.cs                    # Schema file management
│   └── RetryOptions.cs                        # Retry configuration for concurrent writes
├── DataAbstractionAPI.Services/               # Business logic services
├── DataAbstractionAPI.API/                    # REST API with Swagger
├── DataAbstractionAPI.Core.Tests/             # Core model and interface tests
├── DataAbstractionAPI.Adapters.Tests/         # Adapter and integration tests
└── DataAbstractionAPI.Services.Tests/         # Service tests (in progress)
```

---

## What's Implemented (Phase 1)

### Core Components ✅
- **Interfaces**: `IDataAdapter`, `IDefaultGenerator`, `ITypeConverter`
- **Models**: `Record`, `CollectionSchema`, `FieldDefinition`, `QueryOptions`, `ListResult`, `CreateResult`, `DefaultGenerationContext`
- **Enums**: `FieldType`, `StorageType`, `ConversionStrategy`, `DefaultGenerationStrategy`
- **Exceptions**: `ConversionException`, `ValidationException`
- **29 tests** for core models, enums, and exceptions

### CSV Adapter ✅
- **ListAsync**: Query records with filtering, pagination, sorting
- **GetAsync**: Retrieve single record by ID
- **CreateAsync**: Create new records with auto-generated IDs
- **UpdateAsync**: Update existing records with partial updates
- **DeleteAsync**: Delete records by ID
- **GetSchemaAsync**: Retrieve collection schema from CSV headers
- **ListCollectionsAsync**: List all available collections
- **GenerateId**: Unique ID generation using GUIDs
- **39 tests** for adapter functionality

### Security & Safety ✅
- **Path traversal prevention**: Validates collection names
- **File locking**: Prevents concurrent access issues
- **Error handling**: Proper exception handling throughout
- **Service injection support**: Ready for dependency injection
- **Cancellation token support**: Full cancellation support throughout
- **Retry logic**: Exponential backoff for concurrent write operations
- **4 security tests** + **4 concurrency tests** + **3 service injection tests** + **3 cancellation tests**

---

## Service Patterns and Examples

### DefaultGenerator Patterns

The `DefaultGenerator` service automatically generates intelligent default values based on field naming patterns:

#### Boolean Patterns
Fields starting with `is_`, `has_`, or `can_` default to `false`:
- `is_active` → `false`
- `has_permission` → `false`
- `can_edit` → `false`

#### DateTime/Date Patterns
Fields ending with `_at` or `_date`, or starting with `created_`, `updated_`, or `deleted_` default to current UTC timestamp:
- `created_at` → `DateTime.UtcNow`
- `updated_at` → `DateTime.UtcNow`
- `deleted_at` → `DateTime.UtcNow`
- `purchase_date` → `DateTime.UtcNow`

#### ID Patterns
Fields ending with `_id` or `_key` default to `null`:
- `user_id` → `null`
- `order_key` → `null`

#### Count Patterns
Fields ending with `_count` or `_total`, or starting with `num_` default to `0`:
- `item_count` → `0`
- `total_amount` → `0`
- `num_items` → `0`

#### Type-Based Defaults
If no pattern matches, defaults are based on field type:
- `String` → `""` (empty string)
- `Integer` → `0`
- `Float` → `0.0`
- `Boolean` → `false`
- `DateTime` → `DateTime.UtcNow`
- `Date` → `DateTime.UtcNow.Date`
- `Array` → `[]` (empty array)
- `Object` → `{}` (empty dictionary)

### TypeConverter Examples

The `TypeConverter` service converts values between types using different strategies:

#### Conversion Strategies

**Cast** (default): Attempts direct conversion, throws exception on failure
```csharp
// String to Integer
"123" → 123 (success)
"abc" → ConversionException (failure)
```

**Truncate**: Truncates values when possible
```csharp
// Float to Integer
3.7 → 3 (truncated)
```

**FailOnError**: Explicitly throws exception on failure (same as Cast)
```csharp
// String to Integer
"123" → 123 (success)
"abc" → ConversionException (failure)
```

**SetNull**: Returns null on conversion failure
```csharp
// String to Integer
"123" → 123 (success)
"abc" → null (failure, returns null)
```

#### Supported Conversions

- String ↔ Integer, Float, Boolean, DateTime, Date
- Integer ↔ String, Float, Boolean
- Float ↔ String, Integer (truncates)
- Boolean ↔ String, Integer (0/1)
- DateTime ↔ String (ISO 8601 format)
- Date ↔ String (yyyy-MM-dd format)

### FilterEvaluator Examples

The `FilterEvaluator` service supports three types of filters:

#### Simple Filters (AND logic)
All conditions must match:
```json
{
  "status": "active",
  "age": 25
}
```
Matches records where `status == "active"` AND `age == 25`

#### Operator-Based Filters
Single condition with operator:
```json
{
  "field": "age",
  "operator": "gte",
  "value": 18
}
```
Matches records where `age >= 18`

**Supported Operators:**
- `eq` - equals
- `ne` - not equals
- `gt` - greater than
- `gte` - greater than or equal
- `lt` - less than
- `lte` - less than or equal
- `in` - value in array
- `nin` - value not in array
- `contains` - string contains substring
- `startswith` - string starts with
- `endswith` - string ends with

#### Compound Filters (AND/OR)
```json
{
  "and": [
    { "status": "active" },
    {
      "or": [
        { "field": "age", "operator": "gte", "value": 18 },
        { "field": "age", "operator": "lt", "value": 65 }
      ]
    }
  ]
}
```
Matches records where `status == "active"` AND (`age >= 18` OR `age < 65`)

### ValidationService Examples

The `ValidationService` validates records against schemas:

#### Required Fields
Fields with `nullable: false` must be present and non-null:
```csharp
// Schema
{
  "name": "users",
  "fields": [
    { "name": "email", "type": "String", "nullable": false }
  ]
}

// Valid record
{ "email": "user@example.com" }

// Invalid record (missing required field)
{ } // throws ValidationException
```

#### Type Validation
Field values must match the schema type:
```csharp
// Schema
{
  "fields": [
    { "name": "age", "type": "Integer", "nullable": true }
  ]
}

// Valid records
{ "age": 25 }
{ "age": "25" } // String can be coerced to Integer
{ } // nullable field can be omitted

// Invalid record
{ "age": "not-a-number" } // throws ValidationException
```

#### Type Compatibility
Some types are compatible:
- `Integer` ↔ `Float` (numeric compatibility)
- `DateTime` ↔ `Date` (date compatibility)
- `String` can be coerced to `Integer`, `Float`, `Boolean`, `DateTime`, `Date` if parseable

### Service Integration Example

All services work together when injected into `CsvAdapter`:

```csharp
// Setup with dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var defaultGenerator = new DefaultGenerator(loggerFactory.CreateLogger<DefaultGenerator>());
var typeConverter = new TypeConverter(loggerFactory.CreateLogger<TypeConverter>());
var filterEvaluator = new FilterEvaluator(loggerFactory.CreateLogger<FilterEvaluator>());
var validationService = new ValidationService(loggerFactory.CreateLogger<ValidationService>());

var adapter = new CsvAdapter(
    baseDirectory: "./data",
    defaultGenerator: defaultGenerator,
    typeConverter: typeConverter,
    filterEvaluator: filterEvaluator
);

// Create record - DefaultGenerator provides defaults for missing fields
var record = await adapter.CreateAsync("users", new Dictionary<string, object>
{
    { "name", "John Doe" }
    // is_active will default to false
    // created_at will default to DateTime.UtcNow
});

// List with filter - FilterEvaluator handles complex filtering
var options = new QueryOptions
{
    Filter = new Dictionary<string, object>
    {
        { "field", "age" },
        { "operator", "gte" },
        { "value", 18 }
    }
};
var results = await adapter.ListAsync("users", options);
```

---

## Test Results

```bash
Test Run Summary (End of Advanced Data Endpoints):
✓ Core.Tests: 39 tests passed
✓ Adapters.Tests: 87 tests passed (21 new tests for Advanced Data Endpoints)
✓ Services.Tests: 185 tests passed
✓ API.Tests: 92 tests passed (66 new tests including Advanced Data Endpoints integration tests)
Total: 403 tests, 403 passed, 0 failed
```

### Test Coverage (End of Phase 2)

**Services Layer Coverage** (independently verified November 2025):
- **Overall Services Package**: 91.27% line coverage, 86.03% branch coverage ✅
- **DefaultGenerator**: 87.50% line, 80.45% branch ✅
- **TypeConverter**: 82.80% line, 88.88% branch ✅
- **FilterEvaluator**: 99.47% line, 84.65% branch ✅
- **ValidationService**: 97.95% line, 90.90% branch ✅

All services exceed the >85% line coverage target requirement.

**Verification**: Coverage percentages have been independently verified using coverlet and ReportGenerator tools. See `TEST_COVERAGE_VERIFICATION_REPORT.md` for detailed verification results.

Run tests:
```bash
dotnet test
```

---

## Key Features

### Data Operations
- **List**: Query records with pagination (limit parameter via API; adapter supports filtering, sorting, field selection)
- **Get**: Retrieve single record by ID
- **Create**: Add new records with auto-generated GUID IDs
- **Update**: Modify existing records (partial updates supported)
- **Delete**: Remove records by ID
- **Bulk Operations**: Batch create/update/delete with atomic or best-effort mode
- **Summary**: Get field value counts for simple aggregations
- **Aggregate**: Complex aggregations with grouping and multiple functions (count, sum, avg, min, max)
- **Schema**: Get collection schema, list collections
- **Upload**: Upload CSV files to create or replace collections via Swagger UI or API
- **Discovery**: Agent discovery endpoint (`GET /api/data/help`) for machine-readable API information
- **Field projection**: Available in adapter layer (not yet exposed via REST API)
- **Filtering**: Available in adapter layer (not yet exposed via REST API)
- **Sorting**: Available in adapter layer (not yet exposed via REST API)

### Security
- **Path traversal protection**: Collection names validated
- **Directory isolation**: All operations scoped to base directory
- **Concurrent safety**: File locking prevents race conditions

### CSV Support
- **Read**: Parse CSV files to dictionaries
- **Write**: Append records maintaining column order
- **Headers**: Automatic header detection and preservation
- **Header updates**: Headers automatically updated when new fields added
- **Schemas**: JSON-based schema management (`.schema` directory)
- **Schema consistency**: CSV headers are source of truth, schema files optional metadata
- **Type inference**: Automatic type inference from data

---

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Build
```bash
dotnet build
```

### Run Tests
```bash
# Run all tests
dotnet test

# Run specific project
dotnet test DataAbstractionAPI.Core.Tests
dotnet test DataAbstractionAPI.Adapters.Tests

# Run specific test filter
dotnet test --filter "CsvAdapter"
```

### Project Structure
```bash
# View solution structure
dotnet sln list
```

---

## Example Usage

### Creating an Adapter
```csharp
var adapter = new CsvAdapter(@"C:\data\csv");

// List all users with pagination and filtering
var options = new QueryOptions
{
    Fields = new[] { "id", "name", "email" },  // Field projection
    Filter = new Dictionary<string, object> { { "status", "active" } },  // Filtering
    Limit = 10,
    Offset = 0,
    Sort = "name:asc"  // Sorting
};
var result = await adapter.ListAsync("users", options);
```

### Querying Data
```csharp
// Get single record
var user = await adapter.GetAsync("users", "123");

// Create new record (ID is auto-generated)
var newRecord = new Dictionary<string, object>
{
    { "name", "John Doe" },
    { "email", "john@example.com" }
};
var created = await adapter.CreateAsync("users", newRecord);
// created.Id contains the generated GUID
```

### Advanced Data Operations

#### Bulk Operations
```csharp
// Bulk create with atomic mode (all succeed or all fail)
var bulkRequest = new BulkOperationRequest
{
    Action = "create",
    Atomic = true,
    Records = new List<Dictionary<string, object>>
    {
        new() { { "name", "Alice" }, { "email", "alice@example.com" } },
        new() { { "name", "Bob" }, { "email", "bob@example.com" } }
    }
};
var bulkResult = await adapter.BulkOperationAsync("users", bulkRequest);
// bulkResult.Success indicates if all operations succeeded
// bulkResult.Ids contains all created IDs

// Bulk update with best-effort mode (partial success allowed)
var updateRequest = new BulkOperationRequest
{
    Action = "update",
    Atomic = false,
    Records = new List<Dictionary<string, object>>
    {
        new() { { "id", "123" }, { "status", "active" } },
        new() { { "id", "456" }, { "status", "inactive" } }
    }
};
var updateResult = await adapter.BulkOperationAsync("users", updateRequest);
// updateResult.Results contains per-item success/failure details
```

#### Summary (Field Value Counts)
```csharp
// Get count of values for a specific field
var summary = await adapter.GetSummaryAsync("users", "status");
// summary.Counts contains: { "active": 45, "inactive": 12, "pending": 8 }
```

#### Aggregate Operations
```csharp
// Group by category and calculate aggregates
var aggregateRequest = new AggregateRequest
{
    GroupBy = new[] { "category" },
    Aggregates = new List<AggregateFunction>
    {
        new() { Field = "id", Function = "count", Alias = "count" },
        new() { Field = "price", Function = "sum", Alias = "total_price" },
        new() { Field = "price", Function = "avg", Alias = "avg_price" }
    },
    Filter = new Dictionary<string, object> { { "status", "active" } }
};
var aggregateResult = await adapter.AggregateAsync("products", aggregateRequest);
// aggregateResult.Data contains grouped and aggregated results
```

### Discovery Endpoint

The API provides a discovery endpoint for agents and automated clients:

```csharp
// GET /api/data/help
// Returns machine-readable API information including:
// - Available endpoints
// - Authentication requirements
// - Quick start guide
// - OpenAPI spec location (in development)
```

This endpoint is always available (unlike Swagger which is development-only) and provides a standard way for automated clients to discover API capabilities.

---

## Architecture

### Data Flow
```
CsvAdapter (implements IDataAdapter)
    ↓
CsvFileHandler (reads/writes CSV files)
    ↓
CsvFileLock (ensures exclusive access)
    ↓
CSV Files (storage)
```

### Test Architecture
```
CsvAdapterTests
├── ListAsync tests (5 tests)
├── GetAsync tests (2 tests)
├── CreateAsync tests (2 tests)
├── Security tests (4 tests)
└── ID Generation test (1 test)
```

---

## Current Status

### Phase 1: Core Foundation ✅ COMPLETE
- [x] Solution structure
- [x] Core interfaces and models
- [x] CSV adapter with CRUD operations
- [x] File locking
- [x] Security validation
- [x] 78 tests passing

### Phase 1.x: Complete CRUD ✅ COMPLETE
- [x] UpdateAsync implementation
- [x] DeleteAsync implementation
- [x] GetSchemaAsync implementation
- [x] ListCollectionsAsync implementation
- [x] Additional test coverage

### Phase 2: Services Layer ✅ COMPLETE
- [x] Core types prepared (Step 2.0)
- [x] CsvAdapter refactored for injection (Step 2.05)
- [x] Services project created (Step 2.1)
- [x] DefaultGenerator service (Step 2.2) ✅ COMPLETE
- [x] TypeConverter service (Step 2.3) ✅ COMPLETE
- [x] FilterEvaluator service (Step 2.4) ✅ COMPLETE
- [x] ValidationService (Step 2.5) ✅ COMPLETE
- [x] Integration tests (Step 2.6) ✅ COMPLETE

**Note**: DefaultGenerator service is implemented and ready for use, but not yet integrated into the API endpoints.

### Phase 3: REST API ✅ COMPLETE (Basic Implementation + Advanced Data Endpoints)
- [x] REST API with ASP.NET Core Web API
- [x] Swagger documentation
- [x] Basic CRUD endpoints (Create, Read, Update, Delete)
- [x] **Advanced Data Endpoints**:
  - [x] **Bulk Operations** (`POST /api/data/{collection}/bulk`) - Batch create/update/delete with atomic or best-effort mode
  - [x] **Summary** (`GET /api/data/{collection}/summary?field={fieldName}`) - Field value counts
  - [x] **Aggregate** (`POST /api/data/{collection}/aggregate`) - Complex aggregations with grouping and multiple functions
- [x] Collections listing endpoint
- [x] Schema endpoint
- [x] **CSV file upload endpoint** - Upload CSV files to create or replace collections
- [x] **Agent Discovery Endpoint** (`GET /api/data/help`) - Machine-readable API information
- [x] API key authentication (optional, configurable)
- [x] DI integration
- [x] HTTPS support
- [x] Cancellation token support
- [x] **CORS configuration** - Cross-Origin Resource Sharing support for web clients
- Port: http://localhost:5012, https://localhost:7128

**Note**: The current API implementation includes advanced data operations. The adapter supports filtering, sorting, pagination, and field selection via QueryOptions, but the REST API controller currently only exposes the `limit` query parameter. Full query parameter support can be added in future iterations.

#### CORS Configuration

The API supports Cross-Origin Resource Sharing (CORS) to allow web browsers to access the API from different origins. CORS is configured via `appsettings.json`:

**Development Configuration** (`appsettings.Development.json`):
- Allows origins: `http://localhost:5001`, `https://localhost:5002`
- Allows methods: GET, POST, PUT, PATCH, DELETE, OPTIONS
- Allows headers: Content-Type, X-API-Key, Authorization

**Production Configuration** (`appsettings.json`):
- Specify exact allowed origins (empty array disables CORS in production)
- Defaults to allowing all origins in development if not configured

⚠️ **Security**: Never use `AllowAnyOrigin()` in production. Always specify exact origins.

For detailed CORS configuration options, see `DataAbstractionAPI.API/Configuration/CorsOptions.cs`.

### Phase 3.1: Limitations Remediation ✅ COMPLETE
- [x] **Cancellation token support**: Full cancellation support in all async methods
- [x] **Duplicate field handling**: Automatic deduplication in field selection
- [x] **Concurrent write retry logic**: Exponential backoff retry mechanism
- [x] **New field persistence**: Headers updated when new fields added via UpdateAsync
- [x] **Intelligent defaults**: DefaultGenerator integration for new field defaults
- [x] **Schema file consistency**: CSV headers as source of truth, schema files as optional metadata

### Phase 3.2: Advanced Data Endpoints ✅ COMPLETE
- [x] **Bulk Operations**: Batch create/update/delete operations with atomic or best-effort mode
- [x] **Summary Endpoint**: Simple field aggregation (value counts)
- [x] **Aggregate Endpoint**: Complex aggregations with grouping and multiple functions (count, sum, avg, min, max)
- [x] **Comprehensive test coverage**: 22 new adapter tests + 66 new API integration tests
- [x] **Swagger documentation**: All endpoints fully documented with XML comments

### Upcoming Phases
- **Phase 4**: Management UI (Blazor Server) - Ready to start

---

## Security Features

### Path Traversal Prevention
Collection names are validated to prevent directory traversal attacks:
- ✅ Blocks: `../`, `/`, `\`, absolute paths
- ✅ Allows: Alphanumeric, hyphens, underscores

### File Locking & Concurrency
CSV operations use file locking to prevent concurrent access:
- Exclusive locks per file
- Automatic cleanup on dispose
- Process information recorded for debugging
- **Retry logic**: Exponential backoff retry for concurrent write operations
- **Configurable retry**: RetryOptions class with customizable retry parameters

---

## Development Approach

### Test-Driven Development
- Tests written before implementation
- Red-Green-Refactor cycle followed
- 100% of features have corresponding tests

### Code Quality
- Comprehensive error handling
- XML documentation comments
- Clean separation of concerns
- Security best practices

---

## License

MIT License - see LICENSE file for details

---

## Documentation

### Active Documentation
- **README.md** - This file, project overview
- **IMPLEMENTATION_PLAN.md** - Detailed TDD implementation plan
- **DOCUMENTATION_REVIEW_REPORT.md** - Documentation accuracy review (December 2025)
- **data-abstraction-api.md** - API specification
- **PHASE_3_ENHANCEMENT_PLANS.md** - Detailed plans for Phase 3 enhancements

### Archived Documentation
Historical and outdated documentation has been moved to the `archive/` folder for reference. This includes earlier test reports, implementation summaries, and planning documents that have been superseded.

## Contributing

This is an active development project. See `IMPLEMENTATION_PLAN.md` for detailed implementation roadmap.

**Current Focus**: Phase 3.2 complete - Advanced Data Endpoints implemented with full test coverage. Ready for Phase 4 (Management UI).
