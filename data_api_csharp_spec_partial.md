## Security Considerations

### API Authentication (Phase 3 - MVP)

**API Key Authentication:**
- Simple, stateless authentication via `X-API-Key` header
- Multiple API keys supported (for different clients/environments)
- Can be disabled for local development
- Keys stored in configuration (for production, use Azure Key Vault or similar)

**Example Usage:**
```bash
# With authentication
curl -H "X-API-Key: dev-key-12345" http://localhost:5000/api/data/users

# Swagger UI includes API key input field
```

**Production Considerations:**
- Store API keys in environment variables or secure vault
- Rotate keys periodically
- Log authentication failures
- Consider rate limiting per API key

### Current Phase (CSV Only)

1. **File System Access:**
   - Restrict base directory to configured path
   - Validate collection names (no path traversal)
   - Use Path.GetFullPath() to normalize paths

   ```csh# Data Abstraction REST API - C# .NET Core Implementation Specification

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
- DataAbstractionAPI.Core (interfaces only - dependency inversion)
- CsvHelper (33.0.1)
- System.Text.Json
- Microsoft.Extensions.Logging.Abstractions

**Architecture Note:**
The CSV adapter depends only on `Core` for interfaces (`IDefaultGenerator`, `ITypeConverter`). 
The actual service implementations are injected at runtime by the API's dependency injection 
container. This maintains clean separation and allows adapters to remain implementation-agnostic.

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
        private readonly IServiceProvider _serviceProvider;
        private readonly string _connectionsFilePath;
        private readonly Dictionary<string, IDataAdapter> _adapterCache = new();
        
        // Manages multiple data source connections
        // Dynamically instantiates adapters based on ConnectionConfig
        // Serves as single source of truth for adapter configuration
        
        public async Task<IDataAdapter> GetAdapterAsync(string connectionId, CancellationToken ct = default)
        {
            if (_adapterCache.TryGetValue(connectionId, out var cached))
                return cached;
            
            var config = await GetConnectionAsync(connectionId, ct);
            
            var adapter = config.Type switch
            {
                StorageType.Csv => CreateCsvAdapter(config),
                StorageType.Sql => CreateSqlAdapter(config),
                StorageType.NoSql => CreateNoSqlAdapter(config),
                _ => throw new NotSupportedException($"Storage type {config.Type} not supported")
            };
            
            _adapterCache[connectionId] = adapter;
            return adapter;
        }
        
        private IDataAdapter CreateCsvAdapter(ConnectionConfig config)
        {
            var adapterConfig = new CsvAdapterConfig(
                BaseDirectory: config.Settings["baseDirectory"],
                Delimiter: config.Settings.GetValueOrDefault("delimiter", ","),
                HasHeaders: bool.Parse(config.Settings.GetValueOrDefault("hasHeaders", "true")),
                Encoding: config.Settings.GetValueOrDefault("encoding", "UTF-8"),
                EnableCaching: bool.Parse(config.Settings.GetValueOrDefault("enableCaching", "true"))
            );
            
            return new CsvAdapter(
                adapterConfig,
                _serviceProvider.GetRequiredService<IDefaultGenerator>(),
                _serviceProvider.GetRequiredService<ITypeConverter>(),
                _serviceProvider.GetRequiredService<ILogger<CsvAdapter>>()
            );
        }
        
        // Future: CreateSqlAdapter, CreateNoSqlAdapter
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
- Microsoft.Extensions.Caching.Memory
- System.Text.Json

**Architecture Note:**
The Services layer provides concrete implementations of interfaces defined in Core. 
These implementations are injected into adapters and other components at runtime.

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
    
    // Add API Key authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication using X-API-Key header",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() 
        ?? new[] { "http://localhost:5001" };
    
    options.AddPolicy("ConfiguredOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register core services
builder.Services.AddSingleton<IDefaultGenerator, DefaultGenerator>();
builder.Services.AddSingleton<ITypeConverter, TypeConverter>();
builder.Services.AddSingleton<IConnectionManager>(sp =>
{
    var connectionsPath = builder.Configuration["Connections:StoragePath"] ?? "./config/connections.json";
    return new ConnectionManager(sp, connectionsPath);
});
builder.Services.AddScoped<FilterParser>();
builder.Services.AddScoped<FilterEvaluator>();
builder.Services.AddScoped<ValidationService>();

// API Key Authentication
builder.Services.AddSingleton<ApiKeyValidator>(sp =>
{
    var apiKeys = builder.Configuration.GetSection("ApiKeys").Get<string[]>() ?? Array.Empty<string>();
    var requireAuth = builder.Configuration.GetValue<bool>("RequireAuthentication", true);
    return new ApiKeyValidator(apiKeys, requireAuth);
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ConfiguredOrigins");

// API Key Authentication Middleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

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
    // DTO Design Philosophy:
    // - Internal names are clear and descriptive (e.g., ListResponseDto, CreateResponseDto)
    //   for code readability and IntelliSense support
    // - JSON keys are compact (using [JsonPropertyName]) to minimize token usage for LLM clients
    // - This gives us the best of both worlds:
    //   * Developer-friendly code: response.Data, response.Total
    //   * Token-efficient API: {"d": [...], "t": 150, "more": true}
    
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
  "ApiSettings": {
    "MaxPageSize": 100,
    "DefaultPageSize": 10
  },
  "Connections": {
    "StoragePath": "./config/connections.json",
    "DefaultConnectionId": "csv-local"
  },
  "CorsOrigins": [
    "http://localhost:5001",
    "http://localhost:5002"
  ],
  "ApiKeys": [
    "dev-key-12345",
    "admin-key-67890"
  ],
  "RequireAuthentication": true
}
```

**Note on Configuration:**
- `CsvAdapter` configuration has been removed from `appsettings.json`
- Adapter-specific settings now live in `connections.json` (see Configuration System section)
- `ConnectionManager` dynamically loads and configures adapters based on connection definitions
- This allows multiple connections of the same type with different configurations

**Dependencies:**
- DataAbstractionAPI.Core
- DataAbstractionAPI.Services
- DataAbstractionAPI.Adapters.Csv
- Microsoft.AspNetCore.OpenApi
- Swashbuckle.AspNetCore

**Security Components:**

```csharp
namespace DataAbstractionAPI.API.Middleware
{
    public class ApiKeyValidator
    {
        private readonly HashSet<string> _validKeys;
        private readonly bool _requireAuthentication;
        
        public ApiKeyValidator(string[] apiKeys, bool requireAuthentication)
        {
            _validKeys = new HashSet<string>(apiKeys);
            _requireAuthentication = requireAuthentication;
        }
        
        public bool IsValid(string apiKey)
        {
            if (!_requireAuthentication) return true;
            return !string.IsNullOrEmpty(apiKey) && _validKeys.Contains(apiKey);
        }
        
        public bool RequiresAuthentication => _requireAuthentication;
    }
    
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiKeyValidator _validator;
        private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
        
        public ApiKeyAuthenticationMiddleware(
            RequestDelegate next,
            ApiKeyValidator validator,
            ILogger<ApiKeyAuthenticationMiddleware> logger)
        {
            _next = next;
            _validator = validator;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for health checks and Swagger
            if (context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }
            
            if (!_validator.RequiresAuthentication)
            {
                await _next(context);
                return;
            }
            
            if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
            {
                _logger.LogWarning("API request without API key from {RemoteIp}", 
                    context.Connection.RemoteIpAddress);
                    
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "UNAUTHORIZED",
                        message = "API key is required. Include X-API-Key header."
                    }
                });
                return;
            }
            
            if (!_validator.IsValid(apiKey))
            {
                _logger.LogWarning("Invalid API key attempt from {RemoteIp}", 
                    context.Connection.RemoteIpAddress);
                    
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "UNAUTHORIZED",
                        message = "Invalid API key"
                    }
                });
                return;
            }
            
            await _next(context);
        }
    }
}
```

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
        private readonly string _apiBaseUrl;
        
        // ✅ UI communicates ONLY via HTTP REST calls
        // ✅ No direct references to Core, Services, or Adapters
        public async Task<ListResponseDto> GetRecordsAsync(string collection, QueryParams query)
        {
            var response = await _httpClient.GetFromJsonAsync<ListResponseDto>(
                $"{_apiBaseUrl}/data/{collection}?{query.ToQueryString()}");
            return response;
        }
        
        public async Task<CollectionSchemaDto> GetSchemaAsync(string collection)
        {
            return await _httpClient.GetFromJsonAsync<CollectionSchemaDto>(
                $"{_apiBaseUrl}/schema/{collection}");
        }
        
        // ... other API methods using HttpClient
    }

    public class ConnectionService
    {
        // ✅ UI manages its own local connections.json
        // Persists connection configurations for UI display
        // API has separate connections.json for actual adapters
        private readonly string _localConnectionsPath = "./ui-connections.json";
        
        public Task<List<ConnectionViewModel>> GetConnectionsAsync() { }
        public Task SaveConnectionAsync(ConnectionViewModel conn) { }
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
- **NO dependencies on Core, Services, or Adapters** (separated via REST API)

---

## Separation Architecture: UI vs Database Layer

### Key Design Principle: Loose Coupling

The UI and Database/API layers are **completely independent components** that can be developed, deployed, and expanded separately.

### Architecture Layers

```
┌─────────────────────────────────────────┐
│         UI Layer (Blazor Server)        │
│  - No references to Core/Services       │
│  - Communicates via HTTP only           │
│  - Can be replaced with any frontend    │
└────────────────┬────────────────────────┘
                 │ HTTP/REST (JSON)
                 ▼
┌─────────────────────────────────────────┐
│       API Layer (ASP.NET Core)           │
│  - References: Core, Services, Adapters  │
│  - Exposes REST API                     │
│  - Business logic lives here             │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│     Database Layer (Adapters)          │
│  - CSV, SQL, NoSQL implementations     │
│  - Can swap backends independently      │
└─────────────────────────────────────────┘
```

### Why This Separation Matters

#### 1. **Independent Development**
- UI team can build features without waiting for API changes
- API team can add new endpoints without touching UI
- Database adapters can be swapped (CSV → SQL → MongoDB) without UI impact

#### 2. **Independent Deployment**
- Deploy API to production without redeploying UI
- Use different UI frameworks (React, Vue, Angular) against same API
- Scale API and UI independently

#### 3. **Independent Expansion**
- Add new storage backends (PostgreSQL, MongoDB) → no UI changes
- Build mobile app or CLI tool → reuses same API
- Replace Blazor UI with React → no backend changes

### Communication Contract

**UI → API:**
```csharp
// ✅ CORRECT: UI uses HttpClient
public class ApiClientService 
{
    private readonly HttpClient _client;
    
    public async Task<ListResponseDto> GetUsersAsync()
    {
        return await _client.GetFromJsonAsync<ListResponseDto>(
            "http://localhost:5000/api/data/users");
    }
}
```

**UI ← API:**
```csharp
// ❌ WRONG: UI should NOT reference Core
using DataAbstractionAPI.Core; // NO!
using DataAbstractionAPI.Services; // NO!
```

### Configuration Independence

**UI has its own config:**
```json
// UI appsettings.json
{
  "ApiBaseUrl": "http://localhost:5000/api",
  "UI": {
    "Theme": "Light",
    "ItemsPerPage": 20
  }
}
```

**API has its own config:**
```json
// API appsettings.json
{
  "Connections": {
    "StoragePath": "./config/connections.json"
  },
  "ApiKeys": ["dev-key-12345"]
}
```

### Deployment Independence

**Docker Compose shows separation:**
```yaml
services:
  api:  # ✅ Can scale/deploy independently
    ports: ["5000:8080"]
    env: ["CsvBaseDirectory=/app/data"]
    
  ui:   # ✅ Can scale/deploy independently
    ports: ["5001:8080"]
    env: ["ApiBaseUrl=http://api:8080/api"]
    depends_on: ["api"]  # Only for startup order
```

### Future Expansion Examples

**Expanding Database Layer:**
```csharp
// Add new adapter (no UI changes needed)
DataAbstractionAPI.Adapters.Sql/     // ✅ New SQL adapter
DataAbstractionAPI.Adapters.Redis/   // ✅ New cache adapter
```

**Expanding UI Layer:**
```csharp
// Add new UI (no API changes needed)
DataAbstractionAPI.MobileApp/         // ✅ Xamarin/MAUI app
DataAbstractionAPI.CLI/              // ✅ Command-line tool
DataAbstractionAPI.AdminPanel/       // ✅ Separate admin UI
```

**All use the same API endpoints:**
```
POST /api/data/users
GET /api/schema/products
PATCH /api/data/orders/{id}
```

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
   
   # API references everything (this is the server layer)
   dotnet add DataAbstractionAPI.API reference DataAbstractionAPI.Core
   dotnet add DataAbstractionAPI.API reference DataAbstractionAPI.Services
   dotnet add DataAbstractionAPI.API reference DataAbstractionAPI.Adapters.Csv
   
   # ✅ UI does NOT reference Core or Services (separated via HTTP)
   # UI communicates with API via HTTP/REST only
   ```

5. **Install NuGet Packages:**
   ```bash
   # Core - no external dependencies
   
   # Services
   dotnet add DataAbstractionAPI.Services package Microsoft.Extensions.Configuration.Abstractions
   dotnet add DataAbstractionAPI.Services package Microsoft.Extensions.Logging.Abstractions
   dotnet add DataAbstractionAPI.Services package Microsoft.Extensions.Caching.Memory
   
   # CSV Adapter - Only references Core, not Services
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

> **📋 For detailed step-by-step implementation plan with TDD and checkboxes, see [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md)**
> 
> The detailed plan includes:
> - ✅ Step-by-step tasks with checkboxes
> - ✅ Test-Driven Development (TDD) approach for each step
> - ✅ Phase gate policy (don't start next phase without discussion)
> - ✅ Validation commands for each phase
> - ✅ Quick reference commands

### Phase 1: Core Foundation (Week 1-2)

**Deliverables:**
- ✅ Solution structure created
- ✅ Core models and interfaces defined
- ✅ Basic CSV adapter implementation
- ✅ Unit tests for core functionality
- ✅ Sample CSV files for testing

**Key Tasks:**
1. Create solution file (`dotnet new sln`)
2. Create Core project with all interfaces (IDataAdapter, IConnectionManager, IDefaultGenerator, ITypeConverter)
3. Create domain models using C# records
4. Create CSV adapter project with CsvHelper dependency
5. Implement CsvAdapter.GetCollectionPath() with security validation
6. Implement basic ListAsync(), GetAsync() with ID generation
7. Create CsvSchemaManager for .schema/{collection}.json files
8. Implement CsvFileLock for concurrency
9. Write unit tests for CsvAdapter and models
10. Create sample test data (users.csv, orders.csv)

**Technical Debt Notes:**
- Ignore caching in Phase 1 (implement as needed in Phase 5)
- Only support Create/Read in Phase 1, postpone Update/Delete filtering
- Use System.Text.Json for schema serialization (don't over-engineer)

### Phase 2: Services Layer (Week 3)

**Deliverables:**
- ✅ DefaultGenerator implementation
- ✅ TypeConverter implementation
- ✅ FilterParser and FilterEvaluator
- ✅ ValidationService
- ✅ ConnectionManager

**Key Tasks:**
1. Create Services project
2. Implement DefaultGenerator.DetermineStrategy() with pattern matching (lines 629-648 in API spec)
3. Implement DefaultGenerator.GenerateDefault() with context analysis
4. Implement TypeConverter.Convert() with strategies: Cast, Truncate, FailOnError, SetNull
5. Create FilterParser.ParseFilter() to convert JSON string → Filter object
6. Implement FilterEvaluator.Evaluate() to test records against filters
7. Create ValidationService.ValidateField() for type/required checks
8. Implement ConnectionManager with JSON persistence to connections.json
9. Create ConnectionManager.CreateCsvAdapter() factory method
10. Write unit tests for all services

**Implementation Notes:**
- DefaultGenerator uses reflection on field naming patterns
- TypeConverter throws ConversionException with field name and value on failure
- FilterEvaluator uses LINQ Where() with Func<Record, bool> predicates
- ValidationService called before CreateAsync/UpdateAsync in controllers

### Phase 3: REST API (Week 4)

**Deliverables:**
- ✅ All API endpoints implemented
- ✅ DTOs with proper JSON serialization
- ✅ Swagger/OpenAPI documentation
- ✅ **API Key Authentication**
- ✅ Error handling middleware
- ✅ CORS configuration
- ✅ Integration tests

**Key Tasks:**
1. Create API project (ASP.NET Core Web API)
2. Register Services in DI container (lines 650-660 in spec)
3. Create DTOs in API/Dtos with [JsonPropertyName] attributes
4. Implement DataController.GET(ListAsync), POST(CreateAsync), PATCH(UpdateAsync), DELETE(DeleteAsync)
5. Implement SchemaController.GET(GetSchemaAsync), POST(AddFieldAsync), etc.
6. Create GlobalExceptionHandler middleware (lines 1712-1768 in spec)
7. Implement ApiKeyAuthenticationMiddleware (lines 955-1023 in spec)
8. Add appsettings.json with ApiKeys array and RequireAuthentication flag
9. Configure Swagger with API key security (lines 611-633 in spec)
10. Add POST endpoints: /data/{collection}/bulk, /data/{collection}/query, /data/{collection}/aggregate
11. Implement GET endpoints: /data/{collection}/summary, /schema (list collections)
12. Write integration tests using TestServer or HttpClient

**Authentication Implementation:**
- Simple API key authentication via `X-API-Key` header
- Configurable through `appsettings.json`
- Can be disabled for local development (`RequireAuthentication: false`)
- Swagger UI includes API key input
- Health check and Swagger endpoints bypass authentication
- Invalid keys return proper 401 responses with error details

**Implementation Notes:**
- Use IActionResult with ActionResult<T> return types
- Map domain exceptions to HTTP status codes (404, 400, 500)
- Implement pagination with limit/maxPageSize validation
- Use CancellationToken parameters for async operations

### Phase 4: Management UI (Week 5-6)

**Deliverables:**
- ✅ Blazor Server application
- ✅ All pages implemented
- ✅ Reusable components
- ✅ API client service
- ✅ Responsive design

**Key Tasks:**
1. Create Blazor Server project
2. Install MudBlazor package and configure theme
3. Create ApiClientService with HttpClient injection
4. Create shared DataGrid component with pagination/sorting
5. Implement Dashboard.razor with connection health cards
6. Implement Connections.razor with CRUD form
7. Create FilterBuilder.razor component for visual filters
8. Implement DataBrowser.razor with DataGrid and inline editing
9. Implement SchemaEditor.razor with field definitions table
10. Create QueryBuilder.razor with POST /query endpoint integration
11. Implement ApiTester.razor with HttpClient testing UI
12. Add NotificationService for toast messages
13. Polish CSS/responsive design

**Implementation Notes:**
- ✅ **CRITICAL**: UI MUST NOT reference Core, Services, or Adapters
- ✅ UI communicates ONLY via HTTP using ApiClientService + HttpClient
- Use MudTable for data grids
- Use MudDialog for confirmations
- Store API credentials in Blazored.LocalStorage
- Use SignalR for real-time logs (optional, can use polling in Phase 5)
- Focus on core functionality first, polish in Phase 5

**Separation Validation Checklist:**
- [ ] UI project .csproj has NO reference to Core, Services, or Adapters
- [ ] All data access goes through ApiClientService using HttpClient
- [ ] UI has separate appsettings.json pointing to API base URL
- [ ] UI can start and display error if API is unavailable
- [ ] API can be tested independently via Swagger without UI running

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