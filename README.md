# Data Abstraction API - C# Implementation

A .NET Core implementation of a unified data abstraction layer that provides a consistent interface for interacting with data across different storage backends.

**Status**: Phase 1 + 1.x + 2.0-2.1 + 3.0 Complete ✅  
**Last Updated**: November 2025

---

## Overview

This project implements a storage-agnostic data access API following TDD (Test-Driven Development) principles. The foundation phase includes:

- **Core domain models and interfaces**
- **CSV storage adapter** with full CRUD operations
- **File locking** for concurrency safety
- **Security validation** to prevent path traversal attacks
- **Full CRUD operations** (Create, Read, Update, Delete)
- **Schema operations** (Get schema, List collections)
- **Comprehensive test coverage** (78 tests passing)

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
│   └── CsvSchemaManager.cs                    # Schema file management
├── DataAbstractionAPI.Services/               # Business logic services (in progress)
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
- **4 security tests** + **4 concurrency tests** + **3 service injection tests**

---

## Test Results

```bash
Test Run Summary:
✓ Core.Tests: 39 tests passed
✓ Adapters.Tests: 39 tests passed
Total: 78 tests, 78 passed, 0 failed
```

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
- **Schema**: Get collection schema, list collections
- **Upload**: Upload CSV files to create or replace collections via Swagger UI or API
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
- **Schemas**: JSON-based schema management (`.schema` directory)

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

### Phase 2: Services Layer ✅ PARTIAL
- [x] Core types prepared (Step 2.0)
- [x] CsvAdapter refactored for injection (Step 2.05)
- [x] Services project created (Step 2.1)
- [x] DefaultGenerator service (Step 2.2) ✅ COMPLETE
- [x] TypeConverter service (Step 2.3) ✅ COMPLETE
- [ ] FilterEvaluator service (Step 2.4)
- [ ] ValidationService (Step 2.5)
- [ ] Integration tests (Step 2.6)

**Note**: DefaultGenerator service is implemented and ready for use, but not yet integrated into the API endpoints.

### Phase 3: REST API ✅ COMPLETE (Basic Implementation)
- [x] REST API with ASP.NET Core Web API
- [x] Swagger documentation
- [x] Basic CRUD endpoints (Create, Read, Update, Delete)
- [x] Collections listing endpoint
- [x] Schema endpoint
- [x] **CSV file upload endpoint** - Upload CSV files to create or replace collections
- [x] DI integration
- [x] HTTPS support
- Port: http://localhost:5012, https://localhost:7128

**Note**: The current API implementation is a basic version. The adapter supports filtering, sorting, pagination, and field selection via QueryOptions, but the REST API controller currently only exposes the `limit` query parameter. Full query parameter support can be added in future iterations.

### Upcoming Phases
- **Phase 4**: Management UI (Blazor Server)

---

## Security Features

### Path Traversal Prevention
Collection names are validated to prevent directory traversal attacks:
- ✅ Blocks: `../`, `/`, `\`, absolute paths
- ✅ Allows: Alphanumeric, hyphens, underscores

### File Locking
CSV operations use file locking to prevent concurrent access:
- Exclusive locks per file
- Automatic cleanup on dispose
- Process information recorded for debugging

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
- **API_USAGE.md** - REST API usage guide
- **QUICK_API_GUIDE.md** - Quick start for the API
- **data-abstraction-api.md** - API specification
- **TEST_COVERAGE_REPORT.md** - Test coverage status

### Archived Documentation
Historical and outdated documentation has been moved to the `archive/` folder for reference.

## Contributing

This is an active development project. See `IMPLEMENTATION_PLAN.md` for detailed implementation roadmap.

**Current Focus**: Phase 2 in progress - implementing services layer. Ready for DefaultGenerator implementation.
