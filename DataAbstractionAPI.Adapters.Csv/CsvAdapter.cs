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
    private readonly IDefaultGenerator? _defaultGenerator;
    private readonly ITypeConverter? _typeConverter;
    private readonly IFilterEvaluator? _filterEvaluator;
    private readonly RetryOptions _retryOptions;
    private readonly CsvSchemaManager? _schemaManager;

    /// <summary>
    /// Initializes a new instance of CsvAdapter with optional service dependencies.
    /// </summary>
    /// <param name="baseDirectory">The base directory for CSV files</param>
    /// <param name="defaultGenerator">Optional default value generator</param>
    /// <param name="typeConverter">Optional type converter</param>
    /// <param name="filterEvaluator">Optional filter evaluator</param>
    /// <param name="retryOptions">Optional retry options for concurrent write operations</param>
    /// <param name="schemaManager">Optional schema manager for schema file operations</param>
    public CsvAdapter(string baseDirectory, IDefaultGenerator? defaultGenerator = null, ITypeConverter? typeConverter = null, IFilterEvaluator? filterEvaluator = null, RetryOptions? retryOptions = null, CsvSchemaManager? schemaManager = null)
    {
        _baseDirectory = baseDirectory;
        _defaultGenerator = defaultGenerator;
        _typeConverter = typeConverter;
        _filterEvaluator = filterEvaluator;
        _retryOptions = retryOptions ?? new RetryOptions();
        _schemaManager = schemaManager ?? new CsvSchemaManager(baseDirectory);
    }

    /// <summary>
    /// Lists records from a CSV collection with optional filters and pagination.
    /// </summary>
    public async Task<ListResult> ListAsync(string collection, QueryOptions options, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async
        ct.ThrowIfCancellationRequested();

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        ct.ThrowIfCancellationRequested();
        var handler = new CsvFileHandler(csvPath);
        var allRecords = handler.ReadRecords();

        ct.ThrowIfCancellationRequested();
        // Convert to Record objects
        var records = allRecords.Select((dict, index) => new Record
        {
            Id = dict.ContainsKey("id") ? dict["id"].ToString() ?? index.ToString() : index.ToString(),
            Data = dict
        }).ToList();

        // Apply filtering
        if (options.Filter != null && options.Filter.Count > 0)
        {
            if (_filterEvaluator != null)
            {
                // Use FilterEvaluator service if available (supports operator-based and compound filters)
                records = records.Where(record => _filterEvaluator.Evaluate(record, options.Filter)).ToList();
            }
            else
            {
                // Fall back to simple filter logic (backward compatibility)
                records = FilterRecords(records, options.Filter);
            }
        }

        var total = records.Count;

        ct.ThrowIfCancellationRequested();
        // Apply sorting
        if (!string.IsNullOrEmpty(options.Sort))
        {
            records = SortRecords(records, options.Sort);
        }

        ct.ThrowIfCancellationRequested();
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
        ct.ThrowIfCancellationRequested();

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        ct.ThrowIfCancellationRequested();
        var handler = new CsvFileHandler(csvPath);
        var allRecords = handler.ReadRecords();

        ct.ThrowIfCancellationRequested();
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
        ct.ThrowIfCancellationRequested();

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        ct.ThrowIfCancellationRequested();
        // Generate a unique ID for the new record
        var newId = GenerateId();
        
        // Add ID to the data dictionary
        var recordData = new Dictionary<string, object>(data)
        {
            ["id"] = newId
        };

        ct.ThrowIfCancellationRequested();
        // Append the record to the CSV file with file locking to prevent concurrent write conflicts
        var lockPath = csvPath + ".lock";
        await RetryFileOperationAsync(async () =>
        {
            using (var fileLock = new CsvFileLock(lockPath))
            {
                ct.ThrowIfCancellationRequested();
                var handler = new CsvFileHandler(csvPath);
                handler.AppendRecord(recordData);
            }
        }, ct);

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
        ct.ThrowIfCancellationRequested();

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        ct.ThrowIfCancellationRequested();
        // Read all records from the CSV file
        var handler = new CsvFileHandler(csvPath);
        var headers = handler.ReadHeaders();
        var allRecords = handler.ReadRecords();

        ct.ThrowIfCancellationRequested();
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

        ct.ThrowIfCancellationRequested();
        // Update the record with new data (merge updates into existing data)
        var existingRecord = allRecords[recordIndex];
        var newFields = new List<string>();
        
        foreach (var kvp in data)
        {
            ct.ThrowIfCancellationRequested();
            if (existingRecord.ContainsKey(kvp.Key))
            {
                existingRecord[kvp.Key] = kvp.Value;
            }
            else
            {
                // New field being added
                existingRecord.Add(kvp.Key, kvp.Value);
                if (!headers.Contains(kvp.Key))
                {
                    newFields.Add(kvp.Key);
                }
            }
        }
        allRecords[recordIndex] = existingRecord;

        // Update headers if new fields were added
        if (newFields.Count > 0)
        {
            // Add new fields to headers (append to end to preserve order)
            var updatedHeaders = new List<string>(headers);
            foreach (var newField in newFields)
            {
                if (!updatedHeaders.Contains(newField))
                {
                    updatedHeaders.Add(newField);
                }
            }
            headers = updatedHeaders.ToArray();

            // Add new fields to all existing records with intelligent default values
            foreach (var newField in newFields)
            {
                // Get the value from the updated record to infer type
                var firstValue = existingRecord.ContainsKey(newField) ? existingRecord[newField] : null;
                
                // Infer field type from the first value
                var fieldType = InferFieldType(firstValue);
                
                // Generate default value using DefaultGenerator if available
                object defaultValue;
                if (_defaultGenerator != null)
                {
                    var context = new DefaultGenerationContext
                    {
                        CollectionName = collection
                    };
                    defaultValue = _defaultGenerator.GenerateDefault(newField, fieldType, context);
                }
                else
                {
                    // Fall back to empty string if DefaultGenerator not available (backward compatible)
                    defaultValue = string.Empty;
                }
                
                // Apply default to all existing records (except the one being updated, which already has the value)
                foreach (var record in allRecords)
                {
                    if (!record.ContainsKey(newField))
                    {
                        record[newField] = defaultValue;
                    }
                }
            }
            
            // Update schema file if it exists
            if (_schemaManager != null && newFields.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var currentSchema = _schemaManager.LoadSchema(collection);
                if (currentSchema != null)
                {
                    // Add new field definitions to schema
                    foreach (var newField in newFields)
                    {
                        var firstValue = existingRecord.ContainsKey(newField) ? existingRecord[newField] : null;
                        var fieldType = InferFieldType(firstValue);
                        
                        var newFieldDef = new FieldDefinition
                        {
                            Name = newField,
                            Type = fieldType,
                            Nullable = true,
                            Default = _defaultGenerator != null 
                                ? _defaultGenerator.GenerateDefault(newField, fieldType, new DefaultGenerationContext { CollectionName = collection })
                                : null
                        };
                        
                        if (currentSchema.Fields == null)
                        {
                            currentSchema.Fields = new List<FieldDefinition>();
                        }
                        
                        // Only add if not already present
                        if (!currentSchema.Fields.Any(f => f.Name == newField))
                        {
                            currentSchema.Fields.Add(newFieldDef);
                        }
                    }
                    
                    _schemaManager.SaveSchema(collection, currentSchema);
                }
            }
        }

        ct.ThrowIfCancellationRequested();
        // Write all records back to the CSV file
        var lockPath = csvPath + ".lock";
        await RetryFileOperationAsync(async () =>
        {
            using (var fileLock = new CsvFileLock(lockPath))
            {
                using (var fileStream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(fileStream))
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
                        ct.ThrowIfCancellationRequested();
                        foreach (var header in headers)
                        {
                            var value = record.ContainsKey(header) ? record[header]?.ToString() : "";
                            csv.WriteField(value);
                        }
                        csv.NextRecord();
                    }
                }
            }
        }, ct);
    }

    public async Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async
        ct.ThrowIfCancellationRequested();

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        ct.ThrowIfCancellationRequested();
        // Read all records from the CSV file
        var handler = new CsvFileHandler(csvPath);
        var headers = handler.ReadHeaders();
        var allRecords = handler.ReadRecords();

        ct.ThrowIfCancellationRequested();
        // Find the record with matching ID
        var recordToDeleteIndex = -1;
        for (int i = 0; i < allRecords.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
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

        ct.ThrowIfCancellationRequested();
        // Remove the record from the list
        allRecords.RemoveAt(recordToDeleteIndex);

        ct.ThrowIfCancellationRequested();
        // Write all remaining records back to the CSV file
        var lockPath = csvPath + ".lock";
        await RetryFileOperationAsync(async () =>
        {
            using (var fileLock = new CsvFileLock(lockPath))
            {
                using (var fileStream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(fileStream))
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
                        ct.ThrowIfCancellationRequested();
                        foreach (var header in headers)
                        {
                            var value = record.ContainsKey(header) ? record[header]?.ToString() : "";
                            csv.WriteField(value);
                        }
                        csv.NextRecord();
                    }
                }
            }
        }, ct);
    }

    public async Task<CollectionSchema> GetSchemaAsync(string collection, CancellationToken ct = default)
    {
        await Task.Yield(); // Make async
        ct.ThrowIfCancellationRequested();

        var csvPath = GetCsvPath(collection);
        
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        ct.ThrowIfCancellationRequested();
        // Read headers from CSV file (source of truth)
        var handler = new CsvFileHandler(csvPath);
        var headers = handler.ReadHeaders();
        var allRecords = handler.ReadRecords();

        ct.ThrowIfCancellationRequested();
        // Try to load schema file for metadata (optional)
        CollectionSchema? schemaFile = null;
        if (_schemaManager != null)
        {
            schemaFile = _schemaManager.LoadSchema(collection);
        }

        // Create FieldDefinitions from headers, enriched with schema file metadata if available
        var fields = new List<FieldDefinition>();
        foreach (var header in headers)
        {
            // Find field definition from schema file if it exists
            var schemaField = schemaFile?.Fields?.FirstOrDefault(f => f.Name == header);
            
            // Infer type from data if not in schema file
            var inferredType = schemaField?.Type ?? InferFieldTypeFromData(allRecords, header);
            
            fields.Add(new FieldDefinition
            {
                Name = header,
                Type = inferredType,
                Nullable = schemaField?.Nullable ?? true, // Default to nullable
                Default = schemaField?.Default
            });
        }

        // Add any fields from schema file that aren't in CSV headers (for backward compatibility)
        if (schemaFile?.Fields != null)
        {
            foreach (var schemaField in schemaFile.Fields)
            {
                if (!headers.Contains(schemaField.Name))
                {
                    fields.Add(schemaField);
                }
            }
        }

        return new CollectionSchema
        {
            Name = collection,
            Fields = fields
        };
    }

    public async Task<string[]> ListCollectionsAsync(CancellationToken ct = default)
    {
        await Task.Yield(); // Make async
        ct.ThrowIfCancellationRequested();

        // Get all CSV files in the base directory
        if (!Directory.Exists(_baseDirectory))
        {
            return Array.Empty<string>();
        }

        ct.ThrowIfCancellationRequested();
        var csvFiles = Directory.GetFiles(_baseDirectory, "*.csv", SearchOption.TopDirectoryOnly);
        ct.ThrowIfCancellationRequested();
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
        // Deduplicate fields while preserving order (first occurrence)
        var distinctFields = fields.Distinct().ToArray();
        
        return records.Select(record => new Record
        {
            Id = record.Id,
            Data = distinctFields
                .Where(f => record.Data.ContainsKey(f))
                .ToDictionary(f => f, f => record.Data[f])
        }).ToList();
    }

    /// <summary>
    /// Retries a file operation with exponential backoff if it fails due to file locking.
    /// </summary>
    /// <param name="operation">The file operation to retry</param>
    /// <param name="ct">Cancellation token</param>
    private async Task RetryFileOperationAsync(Func<Task> operation, CancellationToken ct)
    {
        if (!_retryOptions.Enabled)
        {
            await operation();
            return;
        }

        int attempt = 0;
        while (attempt <= _retryOptions.MaxRetries)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                await operation();
                return; // Success
            }
            catch (IOException ex) when (attempt < _retryOptions.MaxRetries && IsLockException(ex))
            {
                attempt++;
                var delayMs = _retryOptions.BaseDelayMs * (int)Math.Pow(2, attempt - 1);
                await Task.Delay(delayMs, ct);
            }
        }

        // If we get here, all retries failed - throw the last exception
        await operation();
    }

    /// <summary>
    /// Determines if an IOException is due to file locking (should be retried).
    /// </summary>
    private static bool IsLockException(IOException ex)
    {
        // Check if the exception message indicates a lock issue
        var message = ex.Message.ToLowerInvariant();
        return message.Contains("locked") || 
               message.Contains("being used by another process") ||
               message.Contains("file is locked");
    }

    /// <summary>
    /// Infers the FieldType from a value object.
    /// </summary>
    private static FieldType InferFieldType(object? value)
    {
        if (value == null)
        {
            return FieldType.String; // Default to String for null values
        }

        return value switch
        {
            string => FieldType.String,
            int or long or short => FieldType.Integer,
            double or float or decimal => FieldType.Float,
            bool => FieldType.Boolean,
            DateTime => FieldType.DateTime,
            System.Collections.IEnumerable enumerable when !(value is string) => FieldType.Array,
            _ => FieldType.Object
        };
    }

    /// <summary>
    /// Infers the FieldType from data in records for a specific field.
    /// </summary>
    private static FieldType InferFieldTypeFromData(List<Dictionary<string, object>> records, string fieldName)
    {
        // Look through records to find a non-null value to infer type
        foreach (var record in records)
        {
            if (record.ContainsKey(fieldName) && record[fieldName] != null)
            {
                return InferFieldType(record[fieldName]);
            }
        }
        
        // Default to String if no data found
        return FieldType.String;
    }
}

