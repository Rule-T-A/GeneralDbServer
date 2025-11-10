using Microsoft.AspNetCore.Mvc;
using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.API.Mapping;

namespace DataAbstractionAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IDataAdapter _adapter;
    private readonly IWebHostEnvironment _environment;

    public DataController(IDataAdapter adapter, IWebHostEnvironment environment)
    {
        _adapter = adapter;
        _environment = environment;
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

    [HttpGet]
    public async Task<ActionResult<string[]>> ListCollections(CancellationToken cancellationToken = default)
    {
        var collections = await _adapter.ListCollectionsAsync(cancellationToken);
        return Ok(collections);
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

