namespace DataAbstractionAPI.Adapters.Csv;

using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;

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

    public async Task<CreateResult> CreateAsync(string collection, Dictionary<string, object> data, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        // Generate a unique ID for the new record
        var newId = GenerateId();
        
        // Add ID to the data dictionary
        var recordData = new Dictionary<string, object>(data)
        {
            ["id"] = newId
        };

        // Append the record to the CSV file
        var handler = new CsvFileHandler(csvPath);
        handler.AppendRecord(recordData);

        var record = new Record
        {
            Id = newId,
            Data = recordData
        };

        return new CreateResult
        {
            Record = record,
            Id = newId
        };
    }

    public async Task UpdateAsync(string collection, string id, Dictionary<string, object> data, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        // Read all records from the CSV file
        var handler = new CsvFileHandler(csvPath);
        var headers = handler.ReadHeaders();
        var allRecords = handler.ReadRecords();

        // Find the record with matching ID
        var recordIndex = -1;
        for (int i = 0; i < allRecords.Count; i++)
        {
            if (allRecords[i].ContainsKey("id") && allRecords[i]["id"]?.ToString() == id)
            {
                recordIndex = i;
                break;
            }
        }

        if (recordIndex == -1)
        {
            throw new KeyNotFoundException($"Record with ID '{id}' not found in collection '{collection}'");
        }

        // Update the record with new data (merge updates into existing data)
        var existingRecord = allRecords[recordIndex];
        foreach (var kvp in data)
        {
            if (existingRecord.ContainsKey(kvp.Key))
            {
                existingRecord[kvp.Key] = kvp.Value;
            }
            else
            {
                // New field being added
                existingRecord.Add(kvp.Key, kvp.Value);
            }
        }
        allRecords[recordIndex] = existingRecord;

        // Write all records back to the CSV file
        var lockPath = csvPath + ".lock";
        using (var fileLock = new CsvFileLock(lockPath))
        {
            using (var writer = new StreamWriter(csvPath, false))
            using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                // Write headers
                foreach (var header in headers)
                {
                    csv.WriteField(header);
                }
                csv.NextRecord();

                // Write records
                foreach (var record in allRecords)
                {
                    foreach (var header in headers)
                    {
                        var value = record.ContainsKey(header) ? record[header]?.ToString() : "";
                        csv.WriteField(value);
                    }
                    csv.NextRecord();
                }
            }
        }
    }

    public async Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        // Read all records from the CSV file
        var handler = new CsvFileHandler(csvPath);
        var headers = handler.ReadHeaders();
        var allRecords = handler.ReadRecords();

        // Find the record with matching ID
        var recordToDeleteIndex = -1;
        for (int i = 0; i < allRecords.Count; i++)
        {
            if (allRecords[i].ContainsKey("id") && allRecords[i]["id"]?.ToString() == id)
            {
                recordToDeleteIndex = i;
                break;
            }
        }

        if (recordToDeleteIndex == -1)
        {
            throw new KeyNotFoundException($"Record with ID '{id}' not found in collection '{collection}'");
        }

        // Remove the record from the list
        allRecords.RemoveAt(recordToDeleteIndex);

        // Write all remaining records back to the CSV file
        var lockPath = csvPath + ".lock";
        using (var fileLock = new CsvFileLock(lockPath))
        {
            using (var writer = new StreamWriter(csvPath, false))
            using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                // Write headers
                foreach (var header in headers)
                {
                    csv.WriteField(header);
                }
                csv.NextRecord();

                // Write remaining records
                foreach (var record in allRecords)
                {
                    foreach (var header in headers)
                    {
                        var value = record.ContainsKey(header) ? record[header]?.ToString() : "";
                        csv.WriteField(value);
                    }
                    csv.NextRecord();
                }
            }
        }
    }

    public async Task<CollectionSchema> GetSchemaAsync(string collection, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        // Read headers from CSV file
        var handler = new CsvFileHandler(csvPath);
        var headers = handler.ReadHeaders();

        // Create FieldDefinitions from headers
        var fields = headers.Select(header => new FieldDefinition
        {
            Name = header,
            Type = FieldType.String, // Default to String type
            Nullable = true, // All CSV fields are nullable by default
            Default = null
        }).ToList();

        return new CollectionSchema
        {
            Name = collection,
            Fields = fields
        };
    }

    public async Task<string[]> ListCollectionsAsync(CancellationToken ct = default)
    {
        await Task.Yield(); // Make async

        // Get all CSV files in the base directory
        if (!Directory.Exists(_baseDirectory))
        {
            return Array.Empty<string>();
        }

        var csvFiles = Directory.GetFiles(_baseDirectory, "*.csv", SearchOption.TopDirectoryOnly);
        var collections = csvFiles
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .ToArray();

        return collections;
    }

    /// <summary>
    /// Generates a unique ID for a new record (using GUID).
    /// </summary>
    /// <returns>A unique string ID</returns>
    public string GenerateId()
    {
        return Guid.NewGuid().ToString("N"); // Format without hyphens
    }

    private string GetCsvPath(string collection)
    {
        // Security validation: prevent path traversal attacks
        ValidateCollectionName(collection);
        
        var csvPath = Path.Combine(_baseDirectory, $"{collection}.csv");
        
        // Additional security: ensure the resolved path is still within base directory
        var resolvedPath = Path.GetFullPath(csvPath);
        var resolvedBase = Path.GetFullPath(_baseDirectory);
        
        if (!resolvedPath.StartsWith(resolvedBase, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Invalid collection name: {collection}");
        }
        
        return csvPath;
    }

    /// <summary>
    /// Validates collection name to prevent path traversal attacks.
    /// </summary>
    private static void ValidateCollectionName(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentException("Collection name cannot be empty", nameof(collection));
        }

        // Reject path traversal attempts
        if (collection.Contains("..") || collection.Contains("../"))
        {
            throw new ArgumentException("Collection name cannot contain '..' (path traversal)", nameof(collection));
        }

        // Reject directory separators
        if (collection.Contains('/') || collection.Contains('\\'))
        {
            throw new ArgumentException("Collection name cannot contain directory separators", nameof(collection));
        }

        // Reject absolute paths
        if (Path.IsPathRooted(collection))
        {
            throw new ArgumentException("Collection name cannot be an absolute path", nameof(collection));
        }
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

