namespace DataAbstractionAPI.Adapters.Csv;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;

/// <summary>
/// CSV storage adapter implementing IDataAdapter for CSV file storage.
/// </summary>
public class CsvAdapter : IDataAdapter
{
    private readonly string _baseDirectory;

    public CsvAdapter(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    /// <summary>
    /// Lists records from a CSV collection with optional filters and pagination.
    /// </summary>
    public async Task<ListResult> ListAsync(string collection, QueryOptions options, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        var handler = new CsvFileHandler(csvPath);
        var allRecords = handler.ReadRecords();

        // Convert to Record objects
        var records = allRecords.Select((dict, index) => new Record
        {
            Id = dict.ContainsKey("id") ? dict["id"].ToString() ?? index.ToString() : index.ToString(),
            Data = dict
        }).ToList();

        // Apply filtering (basic - just checking if field exists and matches)
        if (options.Filter != null && options.Filter.Count > 0)
        {
            records = FilterRecords(records, options.Filter);
        }

        var total = records.Count;

        // Apply sorting
        if (!string.IsNullOrEmpty(options.Sort))
        {
            records = SortRecords(records, options.Sort);
        }

        // Apply pagination
        var offset = options.Offset;
        var limit = options.Limit;
        var hasMore = offset + limit < total;
        var paginatedRecords = records.Skip(offset).Take(limit).ToList();

        // Apply field selection if specified
        if (options.Fields != null && options.Fields.Length > 0)
        {
            paginatedRecords = SelectFields(paginatedRecords, options.Fields);
        }

        return new ListResult
        {
            Data = paginatedRecords,
            Total = total,
            More = hasMore
        };
    }

    public async Task<Record> GetAsync(string collection, string id, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        var handler = new CsvFileHandler(csvPath);
        var allRecords = handler.ReadRecords();

        // Find the record with matching ID
        var recordData = allRecords.FirstOrDefault(dict => 
            dict.ContainsKey("id") && dict["id"]?.ToString() == id);

        if (recordData == null)
        {
            throw new FileNotFoundException($"Record with ID '{id}' not found in collection '{collection}'");
        }

        return new Record
        {
            Id = id,
            Data = recordData
        };
    }

    public Task<CreateResult> CreateAsync(string collection, Dictionary<string, object> data, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(string collection, string id, Dictionary<string, object> data, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<CollectionSchema> GetSchemaAsync(string collection, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<string[]> ListCollectionsAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private string GetCsvPath(string collection)
    {
        return Path.Combine(_baseDirectory, $"{collection}.csv");
    }

    private List<Record> FilterRecords(List<Record> records, Dictionary<string, object> filter)
    {
        return records.Where(record =>
        {
            foreach (var kvp in filter)
            {
                if (!record.Data.ContainsKey(kvp.Key))
                {
                    return false;
                }

                var recordValue = record.Data[kvp.Key]?.ToString();
                var filterValue = kvp.Value?.ToString();

                if (recordValue != filterValue)
                {
                    return false;
                }
            }
            return true;
        }).ToList();
    }

    private List<Record> SortRecords(List<Record> records, string sort)
    {
        // Parse sort: "name:asc" or "name:desc"
        var parts = sort.Split(':');
        if (parts.Length != 2)
        {
            return records;
        }

        var field = parts[0];
        var direction = parts[1].ToLower();

        if (direction == "asc")
        {
            return records.OrderBy(r => r.Data.ContainsKey(field) ? r.Data[field]?.ToString() : "").ToList();
        }
        else if (direction == "desc")
        {
            return records.OrderByDescending(r => r.Data.ContainsKey(field) ? r.Data[field]?.ToString() : "").ToList();
        }

        return records;
    }

    private List<Record> SelectFields(List<Record> records, string[] fields)
    {
        return records.Select(record => new Record
        {
            Id = record.Id,
            Data = fields
                .Where(f => record.Data.ContainsKey(f))
                .ToDictionary(f => f, f => record.Data[f])
        }).ToList();
    }
}

