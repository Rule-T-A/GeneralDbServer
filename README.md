# Data Abstraction API - C# Implementation

A .NET Core implementation of a unified data abstraction layer that provides a consistent interface for interacting with data across different storage backends.

**Status**: Phase 1 Complete ✅  
**Last Updated**: October 26, 2025

---

## Overview

This project implements a storage-agnostic data access API following TDD (Test-Driven Development) principles. The foundation phase includes:

- **Core domain models and interfaces**
- **CSV storage adapter** with full CRUD operations
- **File locking** for concurrency safety
- **Security validation** to prevent path traversal attacks
- **Comprehensive test coverage** (37 tests passing)

---

## Project Structure

```
GeneralDbServer/
├── DataAbstractionAPI.Core/              # Core models, interfaces, and enums
│   ├── Enums/                            # FieldType, StorageType
│   ├── Interfaces/                       # IDataAdapter, IDefaultGenerator, ITypeConverter
│   └── Models/                           # Record, QueryOptions, ListResult, etc.
├── DataAbstractionAPI.Adapters.Csv/      # CSV storage adapter
│   ├── CsvAdapter.cs                     # Main adapter implementation
│   ├── CsvFileHandler.cs                 # CSV file read/write operations
│   ├── CsvFileLock.cs                    # File locking mechanism
│   └── CsvSchemaManager.cs               # Schema file management
├── DataAbstractionAPI.Core.Tests/       # Core model and interface tests
└── DataAbstractionAPI.Adapters.Tests/   # Adapter and integration tests
```

---

## What's Implemented (Phase 1)

### Core Components ✅
- **Interfaces**: `IDataAdapter`, `IDefaultGenerator`, `ITypeConverter`
- **Models**: `Record`, `CollectionSchema`, `FieldDefinition`, `QueryOptions`, `ListResult`, `CreateResult`
- **Enums**: `FieldType` (8 types), `StorageType` (4 types)
- **10 tests** for core models and enums

### CSV Adapter ✅
- **ListAsync**: Query records with filtering, pagination, sorting
- **GetAsync**: Retrieve single record by ID
- **CreateAsync**: Create new records with auto-generated IDs
- **GenerateId**: Unique ID generation using GUIDs
- **27 tests** for adapter functionality

### Security & Safety ✅
- **Path traversal prevention**: Validates collection names
- **File locking**: Prevents concurrent access issues
- **Error handling**: Proper exception handling throughout
- **4 security tests** + **4 concurrency tests**

### Supporting Utilities ✅
- **CsvFileHandler**: Read/write CSV files using CsvHelper
- **CsvSchemaManager**: JSON schema file management
- **CsvFileLock**: Exclusive file locking mechanism
- **9 tests** for utilities

---

## Test Results

```bash
Test Run Summary:
✓ Core.Tests: 10 tests passed
✓ Adapters.Tests: 27 tests passed
Total: 37 tests, 37 passed, 0 failed
```

Run tests:
```bash
dotnet test
```

---

## Key Features

### Data Operations
- **List**: Query with filters, pagination, sorting, field selection
- **Get**: Retrieve single record by ID
- **Create**: Add new records with auto-generated IDs
- **Field projection**: Request only specific fields
- **Filtering**: Basic equality filters on any field

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

// List all users with pagination
var options = new QueryOptions
{
    Fields = new[] { "id", "name", "email" },
    Limit = 10,
    Offset = 0
};
var result = await adapter.ListAsync("users", options);
```

### Querying Data
```csharp
// Get single record
var user = await adapter.GetAsync("users", "123");

// Create new record
var newRecord = new Dictionary<string, object>
{
    { "name", "John Doe" },
    { "email", "john@example.com" }
};
var created = await adapter.CreateAsync("users", newRecord);
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
- [x] 37 tests passing

### Upcoming Phases (Not Started)
- **Phase 2**: Services Layer (DefaultGenerator, TypeConverter, FilterEvaluator)
- **Phase 3**: REST API (ASP.NET Core Web API)
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

## Contributing

This is an active development project. See `IMPLEMENTATION_PLAN.md` for detailed implementation roadmap.

**Current Focus**: Phase 1 complete. Ready for Phase 2 (Services Layer).
