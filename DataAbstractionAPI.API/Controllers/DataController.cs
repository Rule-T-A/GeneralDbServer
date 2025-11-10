using Microsoft.AspNetCore.Mvc;
using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.API.Mapping;
using DataAbstractionAPI.API.Configuration;

namespace DataAbstractionAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IDataAdapter _adapter;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public DataController(IDataAdapter adapter, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _adapter = adapter;
        _environment = environment;
        _configuration = configuration;
    }

    private string GetDataPath()
    {
        return Path.Combine(_environment.ContentRootPath, "..", "testdata");
    }

    /// <summary>
    /// Lists records from a collection with optional pagination.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="limit">Maximum number of records to return (default: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of records with pagination information</returns>
    [HttpGet("{collection}")]
    [ProducesResponseType(typeof(ListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponseDto>> GetCollection(string collection, [FromQuery] int? limit = 100, CancellationToken cancellationToken = default)
    {
        var options = new QueryOptions { Limit = limit ?? 100 };
        var result = await _adapter.ListAsync(collection, options, cancellationToken);
        return Ok(result.ToDto());
    }

    /// <summary>
    /// Gets a single record by ID.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="id">The unique identifier of the record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The requested record</returns>
    [HttpGet("{collection}/{id}")]
    [ProducesResponseType(typeof(RecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordDto>> GetRecord(string collection, string id, CancellationToken cancellationToken = default)
    {
        var record = await _adapter.GetAsync(collection, id, cancellationToken);
        return Ok(record.ToDto());
    }

    /// <summary>
    /// Creates a new record in the collection.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="data">The record data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created record with its ID</returns>
    [HttpPost("{collection}")]
    [ProducesResponseType(typeof(CreateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateResponseDto>> CreateRecord(string collection, [FromBody] Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        var result = await _adapter.CreateAsync(collection, data, cancellationToken);
        return CreatedAtAction(nameof(GetRecord), new { collection, id = result.Id }, result.ToDto());
    }

    /// <summary>
    /// Updates an existing record in the collection.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="id">The unique identifier of the record</param>
    /// <param name="data">The fields to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated fields</returns>
    [HttpPut("{collection}/{id}")]
    [ProducesResponseType(typeof(UpdateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UpdateResponseDto>> UpdateRecord(string collection, string id, [FromBody] Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        await _adapter.UpdateAsync(collection, id, data, cancellationToken);
        return Ok(new UpdateResponseDto
        {
            UpdatedFields = data,
            Success = true
        });
    }

    /// <summary>
    /// Deletes a record from the collection.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="id">The unique identifier of the record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation of deletion</returns>
    [HttpDelete("{collection}/{id}")]
    [ProducesResponseType(typeof(DeleteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResponseDto>> DeleteRecord(string collection, string id, CancellationToken cancellationToken = default)
    {
        await _adapter.DeleteAsync(collection, id, cancellationToken);
        return Ok(new DeleteResponseDto
        {
            Success = true,
            Id = id
        });
    }

    /// <summary>
    /// Gets the schema (structure) of a collection.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The collection schema</returns>
    [HttpGet("{collection}/schema")]
    [ProducesResponseType(typeof(SchemaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SchemaResponseDto>> GetSchema(string collection, CancellationToken cancellationToken = default)
    {
        var schema = await _adapter.GetSchemaAsync(collection, cancellationToken);
        return Ok(schema.ToDto());
    }

    /// <summary>
    /// Lists all available collections.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of collection names</returns>
    [HttpGet]
    public async Task<ActionResult<string[]>> ListCollections(CancellationToken cancellationToken = default)
    {
        var collections = await _adapter.ListCollectionsAsync(cancellationToken);
        return Ok(collections);
    }

    /// <summary>
    /// Discovery endpoint for agents and automated clients.
    /// Provides machine-readable information about the API structure and usage.
    /// This endpoint is always available in both Development and Production environments,
    /// unlike Swagger which is development-only.
    /// </summary>
    /// <returns>Discovery information including endpoints, authentication, and quick start guide</returns>
    [HttpGet("help")]
    [ProducesResponseType(typeof(DiscoveryResponseDto), StatusCodes.Status200OK)]
    public ActionResult<DiscoveryResponseDto> GetHelp()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api";
        var isDevelopment = _environment.IsDevelopment();
        
        var apiKeyOptions = _configuration.GetSection("ApiKeyAuthentication").Get<ApiKeyAuthenticationOptions>() 
            ?? new ApiKeyAuthenticationOptions();
        var isApiKeyRequired = IsApiKeyRequired(apiKeyOptions);

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
                Header = apiKeyOptions.HeaderName,
                Required = isApiKeyRequired,
                Description = isApiKeyRequired 
                    ? "API key authentication is required. Provide the API key in the X-API-Key header."
                    : "Optional API key authentication. Configured via appsettings.json"
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
                "POST /api/data/{collection}/aggregate - Perform complex aggregations with grouping",
                "POST /api/data/upload - Upload a CSV file to create or replace a collection"
            },
            Endpoints = new EndpointsInfoDto
            {
                Collections = new EndpointInfoDto
                {
                    Endpoint = "GET /api/data",
                    Description = "List all available collections"
                },
                Data = new DataEndpointsInfoDto
                {
                    List = "GET /api/data/{collection}",
                    Get = "GET /api/data/{collection}/{id}",
                    Create = "POST /api/data/{collection}",
                    Update = "PUT /api/data/{collection}/{id}",
                    Delete = "DELETE /api/data/{collection}/{id}",
                    Summary = "GET /api/data/{collection}/summary?field={fieldName}",
                    Bulk = "POST /api/data/{collection}/bulk",
                    Aggregate = "POST /api/data/{collection}/aggregate"
                },
                Schema = new EndpointInfoDto
                {
                    Endpoint = "GET /api/data/{collection}/schema",
                    Description = "Get collection schema (field definitions)"
                },
                Upload = new EndpointInfoDto
                {
                    Endpoint = "POST /api/data/upload",
                    Description = "Upload CSV file to create or replace a collection"
                }
            }
        };
        
        return Ok(response);
    }

    /// <summary>
    /// Determines if API key authentication is required based on configuration.
    /// </summary>
    private bool IsApiKeyRequired(ApiKeyAuthenticationOptions options)
    {
        return options.Enabled && 
               options.ValidApiKeys != null && 
               options.ValidApiKeys.Length > 0;
    }

    /// <summary>
    /// Performs bulk operations (create, update, delete) on multiple records.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="request">The bulk operation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk operation result</returns>
    [HttpPost("{collection}/bulk")]
    [ProducesResponseType(typeof(BulkResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BulkResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BulkResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BulkResponseDto>> BulkOperation(string collection, [FromBody] BulkOperationRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new BulkResponseDto { Success = false, Error = "Request body is required" });
        }

        // Convert DTO to Core model
        var coreRequest = new BulkOperationRequest
        {
            Action = request.Action,
            Atomic = request.Atomic,
            Records = request.Records,
            UpdateData = request.UpdateData
        };

        var result = await _adapter.BulkOperationAsync(collection, coreRequest, cancellationToken);
        var dto = result.ToDto();

        // Return appropriate status code based on result
        if (request.Atomic)
        {
            if (result.Success && request.Action.ToLower() == "create")
            {
                return CreatedAtAction(nameof(GetCollection), new { collection }, dto);
            }
            else if (!result.Success)
            {
                return BadRequest(dto);
            }
        }

        return Ok(dto);
    }

    /// <summary>
    /// Gets a summary (count) of values for a specific field.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="field">The field name to summarize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping field values to their counts</returns>
    [HttpGet("{collection}/summary")]
    [ProducesResponseType(typeof(SummaryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SummaryResponseDto>> GetSummary(string collection, [FromQuery] string field, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return BadRequest(new { error = "Field parameter is required" });
        }

        var result = await _adapter.GetSummaryAsync(collection, field, cancellationToken);
        return Ok(result.ToDto());
    }

    /// <summary>
    /// Performs complex aggregations with grouping and multiple aggregate functions.
    /// </summary>
    /// <param name="collection">The name of the collection</param>
    /// <param name="request">The aggregate request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated results</returns>
    [HttpPost("{collection}/aggregate")]
    [ProducesResponseType(typeof(AggregateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AggregateResponseDto>> Aggregate(string collection, [FromBody] AggregateRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        if (request.Aggregates == null || request.Aggregates.Count == 0)
        {
            return BadRequest(new { error = "At least one aggregate function must be specified" });
        }

        // Convert DTO to Core model
        var coreRequest = new AggregateRequest
        {
            GroupBy = request.GroupBy,
            Filter = request.Filter,
            Aggregates = request.Aggregates.Select(a => new AggregateFunction
            {
                Field = a.Field,
                Function = a.Function,
                Alias = a.Alias
            }).ToList()
        };

        var result = await _adapter.AggregateAsync(collection, coreRequest, cancellationToken);
        return Ok(result.ToDto());
    }

    /// <summary>
    /// Upload a CSV file to create or replace a collection.
    /// </summary>
    /// <param name="request">The upload request containing collection name and file</param>
    /// <returns>Success message with collection name</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResponse>> UploadCsvFile([FromForm] UploadCsvRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Collection))
        {
            return BadRequest(new { error = "Collection name is required" });
        }

        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new { error = "CSV file is required" });
        }

        // Validate file extension
        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (extension != ".csv")
        {
            return BadRequest(new { error = "Only CSV files are allowed" });
        }

        // Validate collection name (prevent path traversal)
        if (request.Collection.Contains("..") || request.Collection.Contains("/") || request.Collection.Contains("\\") || Path.IsPathRooted(request.Collection))
        {
            return BadRequest(new { error = "Invalid collection name" });
        }

        var dataPath = GetDataPath();
        
        // Ensure the data directory exists
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        // Save the file
        var csvPath = Path.Combine(dataPath, $"{request.Collection}.csv");
        using (var stream = new FileStream(csvPath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        return Ok(new UploadResponse
        { 
            Message = $"CSV file uploaded successfully as collection '{request.Collection}'",
            Collection = request.Collection,
            FilePath = csvPath
        });
    }
}

/// <summary>
/// Request model for CSV file upload
/// </summary>
public class UploadCsvRequest
{
    public string Collection { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}

/// <summary>
/// Response model for CSV file upload
/// </summary>
public class UploadResponse
{
    public string Message { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

