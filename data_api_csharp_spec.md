# Data Abstraction REST API - C# .NET Core Implementation Specification

## Project Overview

A .NET Core implementation of the Data Abstraction REST API with initial CSV backend support and a companion management UI for configuration, troubleshooting, and connection management.

**Version:** 1.0  
**Target Framework:** .NET 8.0 (LTS)  
**Language:** C# 12

---

## Solution Structure

```
DataAbstractionAPI/
├── src/
│   ├── DataAbstractionAPI.Core/           # Core domain models and interfaces
│   ├── DataAbstractionAPI.API/            # REST API (ASP.NET Core Web API)
│   ├── DataAbstractionAPI.Adapters/       # Storage adapter implementations
│   │   ├── DataAbstractionAPI.Adapters.Csv/
│   │   ├── DataAbstractionAPI.Adapters.Sql/      # (Future)
│   │   └── DataAbstractionAPI.Adapters.NoSql/    # (Future)
│   ├── DataAbstractionAPI.Services/       # Business logic and services
│   └── DataAbstractionAPI.UI/             # Management UI (Blazor Server)
├── tests/
│   ├── DataAbstractionAPI.Core.Tests/
│   ├── DataAbstractionAPI.Services.Tests/
│   ├── DataAbstractionAPI.API.Tests/
│   ├── DataAbstractionAPI.Adapters.Tests/
│   └── DataAbstractionAPI.Integration.Tests/
├── docs/
└── samples/
```

---

## Project Details

### 1. DataAbstractionAPI.Core

**Type:** Class Library (.NET 8.0)  
**Purpose:** Core domain models, interfaces, and contracts

#### Key Components

**Interfaces:**
```csharp
namespace DataAbstractionAPI.Core.Interfaces
{
    public interface IDataAdapter
    {
        // Data Operations
        Task<ListResult> ListAsync(string collection, QueryOptions options, CancellationToken ct = default);
        Task<Record> GetAsync(string collection, string id, string[] fields = null, CancellationToken ct = default);
        Task<CreateResult> CreateAsync(string collection, Dictionary<string, object> data, CancellationToken ct = default);
        Task<UpdateResult> UpdateAsync(string collection, string id, Dictionary<string, object> data, ReturnMode returnMode = ReturnMode.Minimal, CancellationToken ct = default);
        Task<DeleteResult> DeleteAsync(string collection, string id, CancellationToken ct = default);
        Task<BulkResult> BulkOperationAsync(string collection, BulkRequest request, CancellationToken ct = default);
        Task<Dictionary<string, int>> GetSummaryAsync(string collection, string field, CancellationToken ct = default);
        Task<AggregateResult> AggregateAsync(string collection, AggregateRequest request, CancellationToken ct = default);
        
        // Schema Operations
        Task<CollectionSchema> GetSchemaAsync(string collection, CancellationToken ct = default);
        Task<SchemaResult> AddFieldAsync(string collection, FieldDefinition field, CancellationToken ct = default);
        Task<SchemaResult> ModifyFieldAsync(string collection, string fieldName, FieldModification modification, CancellationToken ct = default);
        Task<SchemaResult> DeleteFieldAsync(string collection, string fieldName, bool dryRun = false, CancellationToken ct = default);
        Task<CollectionResult> CreateCollectionAsync(string name, List<FieldDefinition> fields, CancellationToken ct = default);
        Task<CollectionResult> RenameCollectionAsync(string oldName, string newName, CancellationToken ct = default);
        Task<CollectionResult> DeleteCollectionAsync(string collection, bool dryRun = false, CancellationToken ct = default);
        Task<string[]> ListCollectionsAsync(CancellationToken ct = default);
        
        // Utility
        object GenerateDefault(string fieldName, FieldType fieldType, DefaultGenerationContext context);
        object ConvertType(object value, FieldType fromType, FieldType toType, ConversionStrategy strategy);
    }

    public interface IConnectionManager
    {
        Task<bool> TestConnectionAsync(ConnectionConfig config, CancellationToken ct = default);
        Task<IDataAdapter> GetAdapterAsync(string connectionId, CancellationToken ct = default);
        Task<ConnectionConfig> GetConnectionAsync(string connectionId, CancellationToken ct = default);
        Task<string> CreateConnectionAsync(ConnectionConfig config, CancellationToken ct = default);
        Task UpdateConnectionAsync(string connectionId, ConnectionConfig config, CancellationToken ct = default);
        Task DeleteConnectionAsync(string connectionId, CancellationToken ct = default);
        Task<List<ConnectionConfig>> ListConnectionsAsync(CancellationToken ct = default);
    }

    public interface IDefaultGenerator
    {
        object GenerateDefault(string fieldName, FieldType fieldType, DefaultGenerationContext context);
        DefaultGenerationStrategy DetermineStrategy(string fieldName, FieldType fieldType);
    }

    public interface ITypeConverter
    {
        object Convert(object value, FieldType fromType, FieldType toType, ConversionStrategy strategy);
        bool CanConvert(FieldType fromType, FieldType toType);
    }
}
```

**Models:**
```csharp
namespace DataAbstractionAPI.Core.Models
{
    // Domain Models
    public record Record(string Id, Dictionary<string, object> Data);
    
    public record CollectionSchema(
        string Name,
        List<FieldDefinition> Fields
    );
    
    public record FieldDefinition(
        string Name,
        FieldType Type,
        bool Nullable = true,
        object Default = null,
        bool IsComputed = false,
        string ComputedExpression = null,
        List<string> AllowedValues = null
    );

    public record FieldModification(
        string NewName = null,
        FieldType? NewType = null,
        bool? Nullable = null,
        object Default = null,
        ConversionStrategy ConversionStrategy = ConversionStrategy.Cast
    );

    public record QueryOptions(
        string[] Fields = null,
        Filter Filter = null,
        int Limit = 10,
        int Offset = 0,
        string Cursor = null,
        string Sort = null,
        int? Sample = null
    );

    public record AggregateRequest(
        string[] GroupBy,
        List<AggregateFunction> Aggregates,
        Filter Filter = null
    );

    public record AggregateFunction(
        string Field,
        AggregateFunctionType Function,
        string Alias
    );

    public record BulkRequest(
        BulkAction Action,
        bool Atomic,
        List<Dictionary<string, object>> Records
    );

    // Results
    public record ListResult(
        List<Record> Data,
        int Total,
        bool More,
        string Cursor = null
    );

    public record CreateResult(
        Record Data,
        string Id
    );

    public record UpdateResult(
        Dictionary<string, object> Data,
        bool Success
    );

    public record DeleteResult(
        bool Success,
        string Id
    );

    public record BulkResult(
        bool Success,
        int Succeeded = 0,
        int Failed = 0,
        List<BulkItemResult> Results = null,
        int? Created = null,
        List<string> Ids = null,
        string Error = null,
        int? FailedIndex = null,
        string FailedError = null
    );

    public record BulkItemResult(
        int Index,
        string Id = null,
        bool Success = true,
        string Error = null
    );

    public record SchemaResult(
        bool Success,
        string Field,
        FieldType Type,
        object Default,
        bool DefaultGenerated,
        string DefaultStrategy = null,
        int AppliedToRecords = 0,
        string OldName = null,
        string NewName = null,
        FieldType? OldType = null,
        FieldType? NewType = null,
        int ConversionErrors = 0,
        List<ConversionError> Errors = null
    );

    public record ConversionError(
        string RecordId,
        object Value,
        string Error
    );

    public record CollectionResult(
        bool Success,
        string Collection,
        int? Fields = null,
        int? RecordsDeleted = null,
        string OldName = null,
        string NewName = null,
        bool DryRun = false,
        int? RecordsAffected = null,
        List<string> Warnings = null
    );

    public record AggregateResult(
        List<Dictionary<string, object>> Data
    );

    // Configuration
    public record ConnectionConfig(
        string Id,
        string Name,
        StorageType Type,
        Dictionary<string, string> Settings,
        bool IsActive = true,
        DateTime CreatedAt = default,
        DateTime? UpdatedAt = null
    );

    public record DefaultGenerationContext(
        string CollectionName,
        List<FieldDefinition> ExistingFields,
        int RecordCount,
        Dictionary<string, object> SampleData = null
    );

    // Enums
    public enum FieldType
    {
        String,
        Integer,
        Float,
        Boolean,
        Date,
        DateTime,
        Array,
        Object
    }

    public enum StorageType
    {
        Csv,
        Sql,
        NoSql,
        InMemory
    }

    public enum ReturnMode
    {
        Minimal,
        Full
    }

    public enum BulkAction
    {
        Create,
        Update,
        Delete
    }

    public enum ConversionStrategy
    {
        Cast,
        Truncate,
        FailOnError,
        SetNull
    }

    public enum DefaultGenerationStrategy
    {
        UserSpecified,
        PatternMatch,
        ContextAware,
        TypeBased
    }

    public enum AggregateFunctionType
    {
        Count,
        Sum,
        Avg,
        Min,
        Max
    }

    // Filter Models
    public abstract record Filter;
    
    public record SimpleFilter(string Field, object Value) : Filter;
    
    public record OperatorFilter(
        string Field,
        FilterOperator Operator,
        object Value
    ) : Filter;
    
    public record CompoundFilter(
        LogicalOperator Operator,
        List<Filter> Filters
    ) : Filter;

    public enum FilterOperator
    {
        Eq, Ne, Gt, Gte, Lt, Lte, In, Nin, Contains, StartsWith, EndsWith
    }

    public enum LogicalOperator
    {
        And, Or
    }
}
```

**Dependencies:**
- None (pure domain layer)

---

### 2. DataAbstractionAPI.Adapters.Csv

**Type:** Class Library (.NET 8.0)  
**Purpose:** CSV storage implementation

#### Key Components

**CsvAdapter:**
```csharp
namespace DataAbstractionAPI.Adapters.Csv
{
    public class CsvAdapter : IDataAdapter
    {
        private readonly CsvAdapterConfig _config;
        private readonly IDefaultGenerator _defaultGenerator;
        private readonly ITypeConverter _typeConverter;
        private readonly ILogger<CsvAdapter> _logger;

        public CsvAdapter(
            CsvAdapterConfig config,
            IDefaultGenerator defaultGenerator,
            ITypeConverter typeConverter,
            ILogger<CsvAdapter> logger)
        {
            _config = config;
            _defaultGenerator = defaultGenerator;
            _typeConverter = typeConverter;
            _logger = logger;
        }

        // Implementation of IDataAdapter interface
        // CSV files stored in: {BaseDirectory}/{collection}.csv
        // Schema files stored in: {BaseDirectory}/.schema/{collection}.json
    }

    public record CsvAdapterConfig(
        string BaseDirectory,
        string Delimiter = ",",
        bool HasHeaders = true,
        string Encoding = "UTF-8",
        int BufferSize = 4096,
        bool EnableCaching = true,
        TimeSpan CacheDuration = default
    );

    public class CsvSchemaManager
    {
        // Manages .schema/{collection}.json files
        // Stores CollectionSchema as JSON
    }

    public class CsvFileHandler
    {
        // Low-level CSV read/write operations
        // Uses CsvHelper library
    }

    public class CsvIndexManager
    {
        // Optional: Maintains indexes for faster queries
        // Stores in .index/{collection}/ directory
    }
}
```

**Dependencies:**
- CsvHelper (33.0.1)
- System.Text.Json
- Microsoft.Extensions.Logging.Abstractions

**CSV File Structure:**
```
data/
├── users.csv              # Data files
├── orders.csv
├── products.csv
├── .schema/               # Schema definitions
│   ├── users.json
│   ├── orders.json
│   └── products.json
└── .index/                # Optional indexes
    ├── users/
    └── orders/
```

---

### 3. DataAbstractionAPI.Services

**Type:** Class Library (.NET 8.0)  
**Purpose:** Business logic and orchestration

#### Key Components

```csharp
namespace DataAbstractionAPI.Services
{
    public class DefaultGenerator : IDefaultGenerator
    {
        // Pattern-based default generation
        // Context-aware analysis
        // Type-based fallbacks
    }

    public class TypeConverter : ITypeConverter
    {
        // Type conversion with multiple strategies
        // Error handling and reporting
    }

    public class ConnectionManager : IConnectionManager
    {
        // Manages multiple data source connections
        // Connection pooling
        // Configuration persistence
    }

    public class FilterParser
    {
        // Parses JSON filter strings to Filter objects
        // Validates filter syntax
    }

    public class FilterEvaluator
    {
        // Evaluates filters against records
        // Used by CSV adapter for in-memory filtering
    }

    public class ValidationService
    {
        // Validates data against schema
        // Type checking
        // Required field validation
    }
}
```

**Dependencies:**
- DataAbstractionAPI.Core
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging.Abstractions

---

### 4. DataAbstractionAPI.API

**Type:** ASP.NET Core Web API (.NET 8.0)  
**Purpose:** REST API endpoints

#### Project Configuration

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Data Abstraction API",
        Version = "v1",
        Description = "Unified interface for data across storage backends"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register services
builder.Services.AddSingleton<IDefaultGenerator, DefaultGenerator>();
builder.Services.AddSingleton<ITypeConverter, TypeConverter>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddScoped<FilterParser>();
builder.Services.AddScoped<FilterEvaluator>();
builder.Services.AddScoped<ValidationService>();

// Configure CSV adapter
builder.Services.Configure<CsvAdapterConfig>(
    builder.Configuration.GetSection("CsvAdapter"));

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Controllers:**
```csharp
namespace DataAbstractionAPI.API.Controllers
{
    [ApiController]
    [Route("api/data")]
    public class DataController : ControllerBase
    {
        [HttpGet("{collection}")]
        public async Task<ActionResult<ListResponseDto>> ListRecords(
            [FromRoute] string collection,
            [FromQuery] string fields,
            [FromQuery] string filter,
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0,
            [FromQuery] string cursor = null,
            [FromQuery] string sort = null,
            [FromQuery] int? sample = null,
            CancellationToken ct = default)
        { }

        [HttpPost("{collection}/query")]
        public async Task<ActionResult<ListResponseDto>> ComplexQuery(
            [FromRoute] string collection,
            [FromBody] ComplexQueryDto request,
            CancellationToken ct = default)
        { }

        [HttpGet("{collection}/{id}")]
        public async Task<ActionResult<GetResponseDto>> GetRecord(
            [FromRoute] string collection,
            [FromRoute] string id,
            [FromQuery] string fields = null,
            CancellationToken ct = default)
        { }

        [HttpPost("{collection}")]
        public async Task<ActionResult<CreateResponseDto>> CreateRecord(
            [FromRoute] string collection,
            [FromBody] Dictionary<string, object> data,
            CancellationToken ct = default)
        { }

        [HttpPatch("{collection}/{id}")]
        public async Task<ActionResult<UpdateResponseDto>> UpdateRecord(
            [FromRoute] string collection,
            [FromRoute] string id,
            [FromBody] Dictionary<string, object> data,
            [FromQuery] string returnMode = "minimal",
            CancellationToken ct = default)
        { }

        [HttpDelete("{collection}/{id}")]
        public async Task<ActionResult<DeleteResponseDto>> DeleteRecord(
            [FromRoute] string collection,
            [FromRoute] string id,
            CancellationToken ct = default)
        { }

        [HttpPost("{collection}/bulk")]
        public async Task<ActionResult<BulkResponseDto>> BulkOperation(
            [FromRoute] string collection,
            [FromBody] BulkRequestDto request,
            CancellationToken ct = default)
        { }

        [HttpGet("{collection}/summary")]
        public async Task<ActionResult<Dictionary<string, int>>> GetSummary(
            [FromRoute] string collection,
            [FromQuery] string field,
            CancellationToken ct = default)
        { }

        [HttpPost("{collection}/aggregate")]
        public async Task<ActionResult<AggregateResponseDto>> Aggregate(
            [FromRoute] string collection,
            [FromBody] AggregateRequestDto request,
            CancellationToken ct = default)
        { }
    }

    [ApiController]
    [Route("api/schema")]
    public class SchemaController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<CollectionListDto>> ListCollections(
            CancellationToken ct = default)
        { }

        [HttpGet("{collection}")]
        public async Task<ActionResult<CollectionSchemaDto>> GetSchema(
            [FromRoute] string collection,
            CancellationToken ct = default)
        { }

        [HttpPost]
        public async Task<ActionResult<CollectionResultDto>> CreateCollection(
            [FromBody] CreateCollectionDto request,
            CancellationToken ct = default)
        { }

        [HttpPatch("{collection}")]
        public async Task<ActionResult<CollectionResultDto>> RenameCollection(
            [FromRoute] string collection,
            [FromBody] RenameCollectionDto request,
            CancellationToken ct = default)
        { }

        [HttpDelete("{collection}")]
        public async Task<ActionResult<CollectionResultDto>> DeleteCollection(
            [FromRoute] string collection,
            [FromQuery] bool dryRun = false,
            CancellationToken ct = default)
        { }

        [HttpPost("{collection}/fields")]
        public async Task<ActionResult<SchemaResultDto>> AddField(
            [FromRoute] string collection,
            [FromBody] AddFieldDto request,
            CancellationToken ct = default)
        { }

        [HttpPatch("{collection}/fields/{fieldName}")]
        public async Task<ActionResult<SchemaResultDto>> ModifyField(
            [FromRoute] string collection,
            [FromRoute] string fieldName,
            [FromBody] ModifyFieldDto request,
            CancellationToken ct = default)
        { }

        [HttpDelete("{collection}/fields/{fieldName}")]
        public async Task<ActionResult<SchemaResultDto>> DeleteField(
            [FromRoute] string collection,
            [FromRoute] string fieldName,
            [FromQuery] bool dryRun = false,
            CancellationToken ct = default)
        { }
    }
}
```

**DTOs:**
```csharp
namespace DataAbstractionAPI.API.Dtos
{
    // Response DTOs use compact keys as per spec
    public record ListResponseDto(
        [JsonPropertyName("d")] List<Dictionary<string, object>> Data,
        [JsonPropertyName("t")] int Total,
        [JsonPropertyName("more")] bool More,
        [JsonPropertyName("cursor")] string Cursor = null
    );

    public record GetResponseDto(
        [JsonPropertyName("d")] Dictionary<string, object> Data
    );

    public record CreateResponseDto(
        [JsonPropertyName("d")] Dictionary<string, object> Data,
        [JsonPropertyName("id")] string Id
    );

    public record UpdateResponseDto(
        [JsonPropertyName("d")] Dictionary<string, object> Data,
        [JsonPropertyName("success")] bool Success
    );

    public record DeleteResponseDto(
        [JsonPropertyName("success")] bool Success,
        [JsonPropertyName("id")] string Id
    );

    public record BulkResponseDto(
        [JsonPropertyName("success")] bool Success,
        [JsonPropertyName("succeeded")] int? Succeeded = null,
        [JsonPropertyName("failed")] int? Failed = null,
        [JsonPropertyName("results")] List<BulkItemResultDto> Results = null,
        [JsonPropertyName("created")] int? Created = null,
        [JsonPropertyName("ids")] List<string> Ids = null,
        [JsonPropertyName("error")] string Error = null,
        [JsonPropertyName("failed_index")] int? FailedIndex = null,
        [JsonPropertyName("failed_error")] string FailedError = null
    );

    // Additional DTOs for requests and responses...
}
```

**appsettings.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "CsvAdapter": {
    "BaseDirectory": "./data",
    "Delimiter": ",",
    "HasHeaders": true,
    "Encoding": "UTF-8",
    "EnableCaching": true,
    "CacheDuration": "00:05:00"
  },
  "Connections": {
    "StoragePath": "./config/connections.json"
  }
}
```

**Dependencies:**
- DataAbstractionAPI.Core
- DataAbstractionAPI.Services
- DataAbstractionAPI.Adapters.Csv
- Microsoft.AspNetCore.OpenApi
- Swashbuckle.AspNetCore

---

### 5. DataAbstractionAPI.UI

**Type:** Blazor Server App (.NET 8.0)  
**Purpose:** Management interface for configuration and troubleshooting

#### Features

**Pages:**

1. **Dashboard** (`/`)
   - Connection status overview
   - Quick stats (collections, records, storage size)
   - Recent activity log
   - Health check indicators

2. **Connections** (`/connections`)
   - List all configured connections
   - Add/Edit/Delete connections
   - Test connection button
   - Connection type selector (CSV, SQL, NoSQL)
   - Connection-specific settings form

3. **Collections** (`/collections`)
   - Browse collections across connections
   - View collection details (record count, size, schema)
   - Create new collection wizard
   - Delete collection with confirmation
   - Export/Import collection data

4. **Schema Browser** (`/schema/{connection}/{collection}`)
   - View full schema definition
   - Add/Modify/Delete fields
   - Preview data with schema overlay
   - Schema validation warnings

5. **Data Browser** (`/data/{connection}/{collection}`)
   - Paginated data grid
   - Filter builder UI
   - Sort controls
   - Inline editing
   - Bulk operations
   - Export to CSV/JSON

6. **Query Builder** (`/query`)
   - Visual filter builder
   - Field selector with autocomplete
   - Query preview (shows generated API call)
   - Execute query and view results
   - Save/Load query templates

7. **API Tester** (`/api-test`)
   - Interactive API endpoint tester
   - Request builder (method, path, body)
   - Response viewer with syntax highlighting
   - Save common requests
   - cURL command generator

8. **Logs** (`/logs`)
   - View application logs
   - Filter by level, source, date
   - Real-time log streaming
   - Export logs

9. **Settings** (`/settings`)
   - Application settings
   - Default adapter configuration
   - Performance tuning
   - Feature flags

#### Key Components

**Services:**
```csharp
namespace DataAbstractionAPI.UI.Services
{
    public class ApiClientService
    {
        private readonly HttpClient _httpClient;
        
        // Wraps API calls for UI consumption
        public Task<ListResponseDto> GetRecordsAsync(string connection, string collection, ...);
        public Task<CollectionSchemaDto> GetSchemaAsync(string connection, string collection);
        // ... other API methods
    }

    public class ConnectionService
    {
        // Manages connection CRUD operations
        // Persists to connections.json
    }

    public class NotificationService
    {
        // Toast notifications
        // Error handling
    }
}
```

**Shared Components:**
```razor
@* Components/DataGrid.razor *@
@* Reusable data grid with sorting, filtering, pagination *@

@* Components/FilterBuilder.razor *@
@* Visual filter construction UI *@

@* Components/SchemaEditor.razor *@
@* Field definition editor *@

@* Components/ConnectionCard.razor *@
@* Connection display and quick actions *@
```

**Project Structure:**
```
DataAbstractionAPI.UI/
├── Pages/
│   ├── Index.razor
│   ├── Connections.razor
│   ├── Collections.razor
│   ├── SchemaEditor.razor
│   ├── DataBrowser.razor
│   ├── QueryBuilder.razor
│   ├── ApiTester.razor
│   ├── Logs.razor
│   └── Settings.razor
├── Components/
│   ├── DataGrid.razor
│   ├── FilterBuilder.razor
│   ├── SchemaEditor.razor
│   └── ConnectionCard.razor
├── Services/
│   ├── ApiClientService.cs
│   ├── ConnectionService.cs
│   └── NotificationService.cs
├── Models/
│   └── (UI-specific view models)
├── wwwroot/
│   ├── css/
│   └── js/
├── App.razor
├── _Imports.razor
└── Program.cs
```

**Dependencies:**
- Microsoft.AspNetCore.Components
- MudBlazor (6.11.0) - Material Design component library
- Blazored.LocalStorage
- Microsoft.Extensions.Http

---

## Configuration System

### Connection Configuration

**connections.json:**
```json
{
  "connections": [
    {
      "id": "csv-local",
      "name": "Local CSV Storage",
      "type": "Csv",
      "isActive": true,
      "settings": {
        "baseDirectory": "./data",
        "delimiter": ",",
        "hasHeaders": "true",
        "encoding": "UTF-8",
        "enableCaching": "true"
      },
      "createdAt": "2025-10-26T10:00:00Z"
    }
  ]
}
```

### Application Configuration

**appsettings.json (API):**
```json
{
  "ApiSettings": {
    "MaxPageSize": 100,
    "DefaultPageSize": 10,
    "EnableSwagger": true,
    "CorsOrigins": ["http://localhost:5001"]
  },
  "CsvAdapter": {
    "BaseDirectory": "./data",
    "MaxFileSizeMb": 100,
    "EnableAutoBackup": true,
    "BackupDirectory": "./backups"
  }
}
```

**appsettings.json (UI):**
```json
{
  "ApiBaseUrl": "http://localhost:5000/api",
  "UI": {
    "ItemsPerPage": 20,
    "EnableRealTimeLogs": true,
    "Theme": "Light"
  }
}
```

---

## Testing Strategy

### Unit Tests

**DataAbstractionAPI.Core.Tests:**
- Model validation
- Filter parsing logic
- Type conversion logic
- Default generation patterns

**DataAbstractionAPI.Adapters.Tests:**
- CSV read/write operations
- Schema persistence
- Query execution
- Bulk operations

**DataAbstractionAPI.Services.Tests:**
- FilterEvaluator logic
- ValidationService rules
- ConnectionManager behavior

### Integration Tests

**DataAbstractionAPI.Integration.Tests:**
- End-to-end API workflows
- CSV file manipulation
- Multi-connection scenarios
- Error handling and recovery

**Test Data:**
```
tests/TestData/
├── sample-users.csv
├── sample-orders.csv
├── sample-products.csv
└── schemas/
```

---

## Development Workflow

### Initial Setup

1. **Create Solution:**
   ```bash
   dotnet new sln -n DataAbstractionAPI
   ```

2. **Create Projects:**
   ```bash
   dotnet new classlib -n DataAbstractionAPI.Core -f net8.0
   dotnet new classlib -n DataAbstractionAPI.Services -f net8.0
   dotnet new classlib -n DataAbstractionAPI.Adapters.Csv -f net8.0
   dotnet new webapi -n DataAbstractionAPI.API -f net8.0
   dotnet new blazorserver -n DataAbstractionAPI.UI -f net8.0
   ```

3. **Add Projects to Solution:**
   ```bash
   dotnet sln add **/*.csproj
   ```

4. **Add Project References:**
   ```bash
   # Services references Core
   dotnet add DataAbstractionAPI.Services reference DataAbstractionAPI.Core
   
   # Adapters reference Core and Services
   dotnet add DataAbstractionAPI.Adapters.Csv reference DataAbstractionAPI.Core
   dotnet add DataAbstractionAPI.Adapters.Csv reference DataAbstractionAPI.Services
   
   # API references everything
   dotnet add DataAbstractionAPI.API reference DataAbstractionAPI.Core
   dotnet add DataAbstractionAPI.API reference DataAbstractionAPI.Services
   dotnet add DataAbstractionAPI.API reference DataAbstractionAPI.Adapters.Csv
   
   # UI references Core and Services
   dotnet add DataAbstractionAPI.UI reference DataAbstractionAPI.Core
   dotnet add DataAbstractionAPI.UI reference DataAbstractionAPI.Services
   ```

5. **Install NuGet Packages:**
   ```bash
   # Core - no external dependencies
   
   # Services
   dotnet add DataAbstractionAPI.Services package Microsoft.Extensions.Configuration.Abstractions
   dotnet add DataAbstractionAPI.Services package Microsoft.Extensions.Logging.Abstractions
   
   # CSV Adapter
   dotnet add DataAbstractionAPI.Adapters.Csv package CsvHelper
   dotnet add DataAbstractionAPI.Adapters.Csv package Microsoft.Extensions.Logging.Abstractions
   
   # API
   dotnet add DataAbstractionAPI.API package Swashbuckle.AspNetCore
   
   # UI
   dotnet add DataAbstractionAPI.UI package MudBlazor
   dotnet add DataAbstractionAPI.UI package Blazored.LocalStorage
   ```

### Running the Application

**Development Mode:**

1. **Terminal 1 - API:**
   ```bash
   cd src/DataAbstractionAPI.API
   dotnet run
   ```
   API runs on: `http://localhost:5000`
   Swagger UI: `http://localhost:5000/swagger`

2. **Terminal 2 - UI:**
   ```bash
   cd src/DataAbstractionAPI.UI
   dotnet run
   ```
   UI runs on: `http://localhost:5001`

**Docker Compose (Future):**
```yaml
version: '3.8'
services:
  api:
    build:
      context: .
      dockerfile: src/DataAbstractionAPI.API/Dockerfile
    ports:
      - "5000:8080"
    volumes:
      - ./data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
  
  ui:
    build:
      context: .
      dockerfile: src/DataAbstractionAPI.UI/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ApiBaseUrl=http://api:8080/api
    depends_on:
      - api
```

---

## Implementation Phases

### Phase 1: Core Foundation (Week 1-2)

**Deliverables:**
- ✅ Solution structure created
- ✅ Core models and interfaces defined
- ✅ Basic CSV adapter implementation
- ✅ Unit tests for core functionality
- ✅ Sample CSV files for testing

**Key Tasks:**
1. Define all interfaces in Core project
2. Implement domain models with records
3. Create CsvAdapter with basic CRUD operations
4. Implement CSV file reading/writing with CsvHelper
5. Create schema persistence (JSON files)
6. Write unit tests for models and adapter

### Phase 2: Services Layer (Week 3)

**Deliverables:**
- ✅ DefaultGenerator implementation
- ✅ TypeConverter implementation
- ✅ FilterParser and FilterEvaluator
- ✅ ValidationService
- ✅ ConnectionManager

**Key Tasks:**
1. Implement pattern-based default generation
2. Create type conversion with all strategies
3. Build filter parsing from JSON
4. Implement filter evaluation logic
5. Create connection management with persistence
6. Add comprehensive unit tests

### Phase 3: REST API (Week 4)

**Deliverables:**
- ✅ All API endpoints implemented
- ✅ DTOs with proper JSON serialization
- ✅ Swagger/OpenAPI documentation
- ✅ Error handling middleware
- ✅ CORS configuration
- ✅ Integration tests

**Key Tasks:**
1. Create DataController with all endpoints
2. Create SchemaController with all endpoints
3. Implement DTO mapping
4. Add global error handling
5. Configure Swagger generation
6. Write integration tests for API
7. Test with Postman/curl

### Phase 4: Management UI (Week 5-6)

**Deliverables:**
- ✅ Blazor Server application
- ✅ All pages implemented
- ✅ Reusable components
- ✅ API client service
- ✅ Responsive design

**Key Tasks:**
1. Set up Blazor project with MudBlazor
2. Create ApiClientService wrapper
3. Implement Dashboard page
4. Implement Connections management
5. Implement Data Browser with grid
6. Implement Schema Browser/Editor
7. Implement Query Builder
8. Implement API Tester
9. Add real-time features (logs, notifications)
10. Polish UI/UX

### Phase 5: Polish & Documentation (Week 7)

**Deliverables:**
- ✅ Complete documentation
- ✅ Performance optimization
- ✅ Error handling improvements
- ✅ Sample datasets
- ✅ Deployment guide

**Key Tasks:**
1. Write comprehensive README
2. Create user guide for UI
3. Document CSV file format requirements
4. Add performance monitoring
5. Optimize CSV operations (caching, indexing)
6. Create sample datasets and scenarios
7. Write deployment documentation
8. Security review

---

## CSV Adapter Implementation Details

### File Organization

```
{BaseDirectory}/
├── users.csv                    # Data files
├── orders.csv
├── products.csv
├── .schema/                     # Schema definitions
│   ├── users.json
│   ├── orders.json
│   └── products.json
├── .locks/                      # File locks for concurrency
│   └── users.lock
└── .backups/                    # Automatic backups (optional)
    ├── users_20251026_100000.csv
    └── users_20251026_110000.csv
```

### Schema File Format

**.schema/users.json:**
```json
{
  "name": "users",
  "fields": [
    {
      "name": "id",
      "type": "Integer",
      "nullable": false,
      "default": null
    },
    {
      "name": "email",
      "type": "String",
      "nullable": false,
      "default": "user@example.com"
    },
    {
      "name": "status",
      "type": "String",
      "nullable": false,
      "default": "active",
      "allowedValues": ["active", "inactive", "pending"]
    },
    {
      "name": "created_at",
      "type": "DateTime",
      "nullable": false,
      "default": "current_timestamp"
    }
  ],
  "version": "1.0",
  "createdAt": "2025-10-26T10:00:00Z",
  "updatedAt": "2025-10-26T10:00:00Z"
}
```

### CSV Format

**users.csv:**
```csv
id,email,status,created_at
1,alice@example.com,active,2025-01-15T10:30:00Z
2,bob@example.com,inactive,2025-02-20T14:20:00Z
3,charlie@example.com,pending,2025-03-10T09:15:00Z
```

### Concurrency Handling

**File Locking Strategy:**
```csharp
public class CsvFileLock : IDisposable
{
    private readonly FileStream _lockStream;
    
    public CsvFileLock(string collectionPath)
    {
        var lockPath = Path.Combine(
            Path.GetDirectoryName(collectionPath),
            ".locks",
            Path.GetFileName(collectionPath) + ".lock"
        );
        
        Directory.CreateDirectory(Path.GetDirectoryName(lockPath));
        
        _lockStream = new FileStream(
            lockPath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 1,
            FileOptions.DeleteOnClose
        );
    }
    
    public void Dispose()
    {
        _lockStream?.Dispose();
    }
}

// Usage:
using (var fileLock = new CsvFileLock(csvPath))
{
    // Perform file operations
}
```

### Caching Strategy

```csharp
public class CsvCache
{
    private readonly MemoryCache _cache;
    private readonly TimeSpan _defaultDuration;
    
    public void Set(string collection, List<Record> records)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaultDuration,
            Size = records.Count
        };
        
        _cache.Set($"data:{collection}", records, options);
    }
    
    public void InvalidateCollection(string collection)
    {
        _cache.Remove($"data:{collection}");
        _cache.Remove($"schema:{collection}");
    }
}
```

### Query Optimization

**In-Memory Filtering:**
```csharp
public class CsvQueryExecutor
{
    public List<Record> ExecuteQuery(
        List<Record> allRecords,
        Filter filter,
        string[] fields,
        string sort,
        int limit,
        int offset)
    {
        IEnumerable<Record> query = allRecords;
        
        // Apply filter
        if (filter != null)
        {
            query = query.Where(r => _filterEvaluator.Evaluate(filter, r));
        }
        
        // Apply sorting
        if (!string.IsNullOrEmpty(sort))
        {
            query = ApplySort(query, sort);
        }
        
        // Apply pagination
        query = query.Skip(offset).Take(limit);
        
        // Project fields
        if (fields != null && fields.Length > 0)
        {
            query = query.Select(r => ProjectFields(r, fields));
        }
        
        return query.ToList();
    }
}
```

### Backup Strategy

```csharp
public class CsvBackupManager
{
    private readonly string _backupDirectory;
    
    public async Task BackupBeforeModification(string collection)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(
            _backupDirectory,
            $"{collection}_{timestamp}.csv"
        );
        
        var sourcePath = GetCollectionPath(collection);
        
        await using var source = File.OpenRead(sourcePath);
        await using var destination = File.Create(backupPath);
        await source.CopyToAsync(destination);
        
        // Keep only last N backups
        await CleanOldBackups(collection, maxBackups: 10);
    }
}
```

---

## Error Handling Strategy

### Global Exception Handler

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred");
        
        var errorResponse = exception switch
        {
            CollectionNotFoundException ex => new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "COLLECTION_NOT_FOUND",
                    Message = ex.Message
                }
            },
            RecordNotFoundException ex => new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "RECORD_NOT_FOUND",
                    Message = ex.Message
                }
            },
            ValidationException ex => new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "VALIDATION_ERROR",
                    Message = ex.Message,
                    Details = ex.ValidationErrors
                }
            },
            _ => new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred"
                }
            }
        };
        
        httpContext.Response.StatusCode = GetStatusCode(exception);
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        
        return true;
    }
}
```

### Custom Exceptions

```csharp
namespace DataAbstractionAPI.Core.Exceptions
{
    public class CollectionNotFoundException : Exception
    {
        public string CollectionName { get; }
        
        public CollectionNotFoundException(string collectionName)
            : base($"Collection '{collectionName}' not found")
        {
            CollectionName = collectionName;
        }
    }
    
    public class RecordNotFoundException : Exception
    {
        public string CollectionName { get; }
        public string RecordId { get; }
        
        public RecordNotFoundException(string collectionName, string recordId)
            : base($"Record '{recordId}' not found in collection '{collectionName}'")
        {
            CollectionName = collectionName;
            RecordId = recordId;
        }
    }
    
    public class ValidationException : Exception
    {
        public Dictionary<string, string[]> ValidationErrors { get; }
        
        public ValidationException(Dictionary<string, string[]> errors)
            : base("Validation failed")
        {
            ValidationErrors = errors;
        }
    }
    
    public class SchemaConflictException : Exception
    {
        public SchemaConflictException(string message) : base(message) { }
    }
    
    public class ConversionException : Exception
    {
        public string FieldName { get; }
        public object Value { get; }
        
        public ConversionException(string fieldName, object value, string message)
            : base(message)
        {
            FieldName = fieldName;
            Value = value;
        }
    }
}
```

---

## Performance Considerations

### CSV Adapter Optimizations

1. **Lazy Loading:**
   ```csharp
   // Don't load entire file for count operations
   public async Task<int> GetCountAsync(string collection)
   {
       var csvPath = GetCollectionPath(collection);
       
       // Quick line count without parsing
       return await File.ReadLinesAsync(csvPath).CountAsync() - 1; // -1 for header
   }
   ```

2. **Streaming for Large Files:**
   ```csharp
   public async IAsyncEnumerable<Record> StreamRecordsAsync(
       string collection,
       [EnumeratorCancellation] CancellationToken ct = default)
   {
       using var reader = new StreamReader(GetCollectionPath(collection));
       using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
       
       await csv.ReadAsync();
       csv.ReadHeader();
       
       while (await csv.ReadAsync())
       {
           ct.ThrowIfCancellationRequested();
           yield return ParseRecord(csv);
       }
   }
   ```

3. **Index Files (Optional):**
   ```csharp
   // Create B-tree index for ID lookups
   public class CsvIndexBuilder
   {
       public async Task BuildIndexAsync(string collection, string fieldName)
       {
           var index = new Dictionary<object, long>(); // value -> file offset
           
           // Build index
           // Store in .index/{collection}/{fieldName}.idx
       }
   }
   ```

4. **Batch Operations:**
   ```csharp
   // Optimize bulk inserts by writing once
   public async Task BulkCreateAsync(string collection, List<Record> records)
   {
       using var fileLock = new CsvFileLock(GetCollectionPath(collection));
       using var writer = new StreamWriter(GetCollectionPath(collection), append: true);
       using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
       
       foreach (var record in records)
       {
           await csv.WriteRecordAsync(record);
       }
       
       await csv.FlushAsync();
   }
   ```

### API Performance

1. **Response Compression:**
   ```csharp
   builder.Services.AddResponseCompression(options =>
   {
       options.EnableForHttps = true;
       options.Providers.Add<GzipCompressionProvider>();
   });
   ```

2. **Async All the Way:**
   ```csharp
   // All IO operations use async/await
   public async Task<ListResult> ListAsync(...)
   {
       var records = await _csvAdapter.LoadRecordsAsync(collection);
       // ...
   }
   ```

3. **Pagination:**
   ```csharp
   // Always enforce max page size
   limit = Math.Min(limit, _maxPageSize);
   ```

---

## Security Considerations

### Current Phase (CSV Only)

1. **File System Access:**
   - Restrict base directory to configured path
   - Validate collection names (no path traversal)
   - Use Path.GetFullPath() to normalize paths

   ```csharp
   public string GetCollectionPath(string collection)
   {
       // Prevent path traversal
       if (collection.Contains("..") || collection.Contains("/") || collection.Contains("\\"))
       {
           throw new ArgumentException("Invalid collection name", nameof(collection));
       }
       
       var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, $"{collection}.csv"));
       
       // Ensure path is within base directory
       if (!fullPath.StartsWith(Path.GetFullPath(_baseDirectory)))
       {
           throw new SecurityException("Path traversal detected");
       }
       
       return fullPath;
   }
   ```

2. **Input Validation:**
   - Sanitize all user inputs
   - Validate field names, collection names
   - Limit request sizes

3. **CORS Configuration:**
   - Configure allowed origins in production
   - Don't use AllowAny in production

### Future Enhancements

1. **Authentication** (Extension Point):
   - JWT bearer tokens
   - API keys
   - OAuth2 integration

2. **Authorization** (Extension Point):
   - Role-based access control (RBAC)
   - Collection-level permissions
   - Record-level access control

3. **Encryption**:
   - HTTPS enforcement
   - Encryption at rest for sensitive data
   - Secure credential storage

---

## Deployment Guide

### Development Environment

**Prerequisites:**
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git

**Setup Steps:**
1. Clone repository
2. Restore NuGet packages: `dotnet restore`
3. Build solution: `dotnet build`
4. Run tests: `dotnet test`
5. Run API: `dotnet run --project src/DataAbstractionAPI.API`
6. Run UI: `dotnet run --project src/DataAbstractionAPI.UI`

### Production Deployment

**Option 1: Self-Hosted (IIS/Nginx)**

1. **Publish API:**
   ```bash
   dotnet publish src/DataAbstractionAPI.API -c Release -o ./publish/api
   ```

2. **Publish UI:**
   ```bash
   dotnet publish src/DataAbstractionAPI.UI -c Release -o ./publish/ui
   ```

3. **Configure Web Server:**
   - Set up reverse proxy
   - Configure SSL certificates
   - Set environment variables

**Option 2: Docker**

```dockerfile
# API Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/DataAbstractionAPI.API/", "DataAbstractionAPI.API/"]
COPY ["src/DataAbstractionAPI.Core/", "DataAbstractionAPI.Core/"]
COPY ["src/DataAbstractionAPI.Services/", "DataAbstractionAPI.Services/"]
COPY ["src/DataAbstractionAPI.Adapters.Csv/", "DataAbstractionAPI.Adapters.Csv/"]

RUN dotnet restore "DataAbstractionAPI.API/DataAbstractionAPI.API.csproj"
RUN dotnet build "DataAbstractionAPI.API/DataAbstractionAPI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataAbstractionAPI.API/DataAbstractionAPI.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataAbstractionAPI.API.dll"]
```

**Option 3: Azure App Service**

1. Create App Service plan
2. Deploy using Visual Studio or Azure CLI
3. Configure application settings
4. Enable Application Insights for monitoring

---

## Monitoring & Logging

### Logging Configuration

```csharp
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsProduction())
{
    builder.Logging.AddApplicationInsights();
}

builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("DataAbstractionAPI", LogLevel.Information);
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("csv_storage", () =>
    {
        var baseDir = configuration["CsvAdapter:BaseDirectory"];
        return Directory.Exists(baseDir)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Storage directory not accessible");
    });

app.MapHealthChecks("/health");
```

### Metrics

```csharp
// Track API usage
public class MetricsMiddleware
{
    private static readonly Counter RequestCounter = Metrics.CreateCounter(
        "api_requests_total",
        "Total API requests",
        new CounterConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "status" }
        }
    );
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context);
        
        RequestCounter.WithLabels(
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode.ToString()
        ).Inc();
    }
}
```

---

## Sample Data & Use Cases

### Sample Datasets

**users.csv:**
```csv
id,name,email,status,created_at,age,department
1,Alice Johnson,alice@example.com,active,2025-01-15T10:30:00Z,28,Engineering
2,Bob Smith,bob@example.com,active,2025-02-20T14:20:00Z,35,Sales
3,Charlie Brown,charlie@example.com,inactive,2025-03-10T09:15:00Z,42,Marketing
4,Diana Prince,diana@example.com,active,2025-04-05T11:45:00Z,31,Engineering
5,Eve Davis,eve@example.com,pending,2025-05-12T08:30:00Z,26,HR
```

**orders.csv:**
```csv
id,user_id,product,quantity,price,status,order_date
1,1,Laptop,1,999.99,completed,2025-06-01T10:00:00Z
2,2,Mouse,2,29.99,completed,2025-06-02T11:30:00Z
3,1,Keyboard,1,79.99,pending,2025-06-03T09:15:00Z
4,3,Monitor,1,299.99,completed,2025-06-04T14:20:00Z
5,4,Headphones,1,149.99,shipped,2025-06-05T16:45:00Z
```

### Common Use Cases

**Use Case 1: LLM Data Analysis**
```
1. LLM calls GET /schema to discover available collections
2. LLM calls GET /data/users?sample=3 to understand data structure
3. LLM calls GET /data/users?fields=department&summary for department breakdown
4. LLM calls POST /data/users/aggregate to get average age by department
5. LLM generates insights from aggregated data
```

**Use Case 2: Data Migration**
```
1. Export from source system to CSV
2. Upload CSV file to /data directory
3. Use UI to create schema definition
4. Validate data through Data Browser
5. Access via API for downstream applications
```

**Use Case 3: Rapid Prototyping**
```
1. Use UI to create new collection with schema
2. Manually add sample records through Data Browser
3. Test API queries with Query Builder
4. Export API calls for integration
5. Switch to SQL adapter when ready for production
```

---

## Future Enhancements

### Phase 6: SQL Adapter
- PostgreSQL, SQL Server, MySQL support
- Connection string configuration
- Query translation
- Transaction support

### Phase 7: NoSQL Adapter
- MongoDB support
- Document-based operations
- Flexible schema handling

### Phase 8: Advanced Features
- Full-text search
- Relationships between collections
- Computed fields and views
- Data versioning
- Audit trails
- Webhook notifications
- GraphQL endpoint (optional)

### Phase 9: Enterprise Features
- Multi-tenancy
- Advanced RBAC
- Data encryption
- Compliance logging
- High availability
- Horizontal scaling

---

## Success Criteria

### Functional Requirements
✅ All API endpoints from spec implemented
✅ CSV adapter fully functional
✅ Management UI provides full CRUD operations
✅ Schema modifications work correctly
✅ Filter syntax supported
✅ Intelligent defaults generated
✅ Error handling comprehensive

### Non-Functional Requirements
✅ API response time < 200ms for typical queries
✅ Support CSV files up to 100MB
✅ Handle 100+ concurrent requests
✅ Comprehensive test coverage (>80%)
✅ Clear documentation
✅ Cross-platform compatibility (Windows, Linux, macOS)

### User Experience
✅ UI intuitive and responsive
✅ API testable through Swagger
✅ Clear error messages
✅ Helpful logging and diagnostics

---

## Appendix

### Key NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| CsvHelper | 33.0.1 | CSV parsing and writing |
| MudBlazor | 6.11.0 | UI component library |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI |
| Microsoft.Extensions.Caching.Memory | 8.0.0 | In-memory caching |
| Blazored.LocalStorage | 4.4.0 | Browser storage for UI |

### Useful Commands

```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run API with watch (hot reload)
dotnet watch run --project src/DataAbstractionAPI.API

# Run UI with watch
dotnet watch run --project src/DataAbstractionAPI.UI

# Create migration (future SQL support)
dotnet ef migrations add InitialCreate

# Format code
dotnet format

# Generate code coverage
dotnet test /p:CollectCoverage=true
```

### Recommended VS Code Extensions
- C# Dev Kit
- .NET Extension Pack
- REST Client (for API testing)
- Thunder Client (alternative API client)

### Project Timeline Summary

| Phase | Duration | Key Deliverable |
|-------|----------|-----------------|
| Phase 1 | 2 weeks | Core + CSV Adapter |
| Phase 2 | 1 week | Services Layer |
| Phase 3 | 1 week | REST API |
| Phase 4 | 2 weeks | Management UI |
| Phase 5 | 1 week | Polish & Docs |
| **Total** | **7 weeks** | **Production-ready MVP** |

---

## Contact & Support

**Documentation:** `/docs` folder in repository  
**Issues:** GitHub Issues  
**License:** MIT (recommended)

---

*This specification provides a complete roadmap for implementing the Data Abstraction REST API in C# .NET Core with CSV support and a management UI. Follow the phases sequentially for best results.*