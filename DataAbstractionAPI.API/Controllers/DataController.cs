using Microsoft.AspNetCore.Mvc;
using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;

namespace DataAbstractionAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IDataAdapter _adapter;

    public DataController(IDataAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpGet("{collection}")]
    public async Task<ActionResult<ListResult>> GetCollection(string collection, [FromQuery] int? limit = 100)
    {
        var options = new QueryOptions { Limit = limit ?? 100 };
        var result = await _adapter.ListAsync(collection, options);
        return Ok(result);
    }

    [HttpGet("{collection}/{id}")]
    public async Task<ActionResult<Record>> GetRecord(string collection, string id)
    {
        var record = await _adapter.GetAsync(collection, id);
        return Ok(record);
    }

    [HttpPost("{collection}")]
    public async Task<ActionResult<CreateResult>> CreateRecord(string collection, [FromBody] Dictionary<string, object> data)
    {
        var result = await _adapter.CreateAsync(collection, data);
        return CreatedAtAction(nameof(GetRecord), new { collection, id = result.Id }, result);
    }

    [HttpPut("{collection}/{id}")]
    public async Task<IActionResult> UpdateRecord(string collection, string id, [FromBody] Dictionary<string, object> data)
    {
        await _adapter.UpdateAsync(collection, id, data);
        return NoContent();
    }

    [HttpDelete("{collection}/{id}")]
    public async Task<IActionResult> DeleteRecord(string collection, string id)
    {
        await _adapter.DeleteAsync(collection, id);
        return NoContent();
    }

    [HttpGet("{collection}/schema")]
    public async Task<ActionResult<CollectionSchema>> GetSchema(string collection)
    {
        var schema = await _adapter.GetSchemaAsync(collection);
        return Ok(schema);
    }

    [HttpGet]
    public async Task<ActionResult<string[]>> ListCollections()
    {
        var collections = await _adapter.ListCollectionsAsync();
        return Ok(collections);
    }
}

