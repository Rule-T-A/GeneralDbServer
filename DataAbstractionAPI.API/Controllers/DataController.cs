using Microsoft.AspNetCore.Mvc;
using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;

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

    [HttpGet("{collection}")]
    public async Task<ActionResult<ListResult>> GetCollection(string collection, [FromQuery] int? limit = 100, CancellationToken cancellationToken = default)
    {
        var options = new QueryOptions { Limit = limit ?? 100 };
        var result = await _adapter.ListAsync(collection, options, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{collection}/{id}")]
    public async Task<ActionResult<Record>> GetRecord(string collection, string id, CancellationToken cancellationToken = default)
    {
        var record = await _adapter.GetAsync(collection, id, cancellationToken);
        return Ok(record);
    }

    [HttpPost("{collection}")]
    public async Task<ActionResult<CreateResult>> CreateRecord(string collection, [FromBody] Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        var result = await _adapter.CreateAsync(collection, data, cancellationToken);
        return CreatedAtAction(nameof(GetRecord), new { collection, id = result.Id }, result);
    }

    [HttpPut("{collection}/{id}")]
    public async Task<IActionResult> UpdateRecord(string collection, string id, [FromBody] Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        await _adapter.UpdateAsync(collection, id, data, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{collection}/{id}")]
    public async Task<IActionResult> DeleteRecord(string collection, string id, CancellationToken cancellationToken = default)
    {
        await _adapter.DeleteAsync(collection, id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{collection}/schema")]
    public async Task<ActionResult<CollectionSchema>> GetSchema(string collection, CancellationToken cancellationToken = default)
    {
        var schema = await _adapter.GetSchemaAsync(collection, cancellationToken);
        return Ok(schema);
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

