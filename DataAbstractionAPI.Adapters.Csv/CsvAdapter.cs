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
            
            // Update schema file if it exists, or create it if it doesn't
            if (_schemaManager != null && newFields.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var currentSchema = _schemaManager.LoadSchema(collection);
                
                // Create new schema if it doesn't exist
                if (currentSchema == null)
                {
                    currentSchema = new CollectionSchema
                    {
                        Name = collection,
                        Fields = new List<FieldDefinition>()
                    };
                }
                
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

    public async Task<BulkResult> BulkOperationAsync(string collection, BulkOperationRequest request, CancellationToken ct = default)
    {
        await Task.Yield();
        ct.ThrowIfCancellationRequested();

        // Validate action
        if (string.IsNullOrWhiteSpace(request.Action) || 
            !new[] { "create", "update", "delete" }.Contains(request.Action.ToLower()))
        {
            throw new ArgumentException("Action must be 'create', 'update', or 'delete'", nameof(request));
        }

        if (request.Records == null || request.Records.Count == 0)
        {
            throw new ArgumentException("Records cannot be empty", nameof(request));
        }

        var csvPath = GetCsvPath(collection);
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        var action = request.Action.ToLower();
        var lockPath = csvPath + ".lock";

        if (request.Atomic)
        {
            // Atomic mode: all succeed or all fail
            BulkResult? result = null;
            int attempt = 0;
            while (attempt <= _retryOptions.MaxRetries)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    using (var fileLock = new CsvFileLock(lockPath))
                    {
                        ct.ThrowIfCancellationRequested();
                        var handler = new CsvFileHandler(csvPath);
                        var headers = handler.ReadHeaders();
                        var allRecords = handler.ReadRecords();

                        var createdIds = new List<string>();
                        int failedIndex = -1;
                        string? failedError = null;

                        try
                        {
                            for (int i = 0; i < request.Records.Count; i++)
                            {
                                ct.ThrowIfCancellationRequested();
                                var record = request.Records[i];

                                if (action == "create")
                                {
                                    var newId = GenerateId();
                                    var recordData = new Dictionary<string, object>(record) { ["id"] = newId };
                                    
                                    // Ensure all headers are present
                                    foreach (var header in headers)
                                    {
                                        if (!recordData.ContainsKey(header))
                                        {
                                            recordData[header] = string.Empty;
                                        }
                                    }
                                    
                                    allRecords.Add(recordData);
                                    createdIds.Add(newId);
                                }
                                else if (action == "update")
                                {
                                    if (!record.ContainsKey("id"))
                                    {
                                        throw new ArgumentException($"Record at index {i} must contain an 'id' field for update operations");
                                    }

                                    var id = record["id"]?.ToString();
                                    if (string.IsNullOrEmpty(id))
                                    {
                                        throw new ArgumentException($"Record at index {i} has invalid 'id' field");
                                    }

                                    var recordIndex = allRecords.FindIndex(r => r.ContainsKey("id") && r["id"]?.ToString() == id);
                                    if (recordIndex == -1)
                                    {
                                        throw new KeyNotFoundException($"Record with ID '{id}' not found at index {i}");
                                    }

                                    // Merge update data
                                    var updateData = request.UpdateData ?? record;
                                    foreach (var kvp in updateData)
                                    {
                                        if (kvp.Key != "id")
                                        {
                                            allRecords[recordIndex][kvp.Key] = kvp.Value;
                                        }
                                    }
                                }
                                else if (action == "delete")
                                {
                                    if (!record.ContainsKey("id"))
                                    {
                                        throw new ArgumentException($"Record at index {i} must contain an 'id' field for delete operations");
                                    }

                                    var id = record["id"]?.ToString();
                                    if (string.IsNullOrEmpty(id))
                                    {
                                        throw new ArgumentException($"Record at index {i} has invalid 'id' field");
                                    }

                                    var recordIndex = allRecords.FindIndex(r => r.ContainsKey("id") && r["id"]?.ToString() == id);
                                    if (recordIndex == -1)
                                    {
                                        throw new KeyNotFoundException($"Record with ID '{id}' not found at index {i}");
                                    }

                                    allRecords.RemoveAt(recordIndex);
                                }
                            }

                            // All operations succeeded, write the file
                            ct.ThrowIfCancellationRequested();
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

                            if (action == "create")
                            {
                                result = new BulkResult
                                {
                                    Success = true,
                                    Succeeded = request.Records.Count,
                                    Failed = 0,
                                    Ids = createdIds
                                };
                            }
                            else
                            {
                                result = new BulkResult
                                {
                                    Success = true,
                                    Succeeded = request.Records.Count,
                                    Failed = 0
                                };
                            }
                            break; // Success, exit retry loop
                        }
                        catch (Exception ex)
                        {
                            // Rollback: don't write the file
                            result = new BulkResult
                            {
                                Success = false,
                                Error = $"Transaction rolled back: {ex.Message}",
                                FailedIndex = failedIndex >= 0 ? failedIndex : request.Records.Count - 1,
                                FailedError = failedError ?? ex.Message
                            };
                            throw; // Re-throw to trigger retry if it's a lock exception
                        }
                    }
                }
                catch (IOException ex) when (attempt < _retryOptions.MaxRetries && IsLockException(ex))
                {
                    attempt++;
                    var delayMs = _retryOptions.BaseDelayMs * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delayMs, ct);
                }
                catch (Exception) when (result != null && !result.Success)
                {
                    // Non-lock exception in atomic mode - return failure result
                    return result;
                }
            }

            // If we get here and result is null, all retries failed
            if (result == null)
            {
                throw new IOException("Failed to acquire file lock after retries");
            }

            return result;
        }
        else
        {
            // Best-effort mode: process each record individually
            var results = new List<BulkOperationItemResult>();
            int succeeded = 0;
            int failed = 0;

            for (int i = 0; i < request.Records.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var record = request.Records[i];
                var itemResult = new BulkOperationItemResult { Index = i };

                try
                {
                    if (action == "create")
                    {
                        var createResult = await CreateAsync(collection, record, ct);
                        itemResult.Success = true;
                        itemResult.Id = createResult.Id;
                        succeeded++;
                    }
                    else if (action == "update")
                    {
                        if (!record.ContainsKey("id"))
                        {
                            throw new ArgumentException("Record must contain an 'id' field for update operations");
                        }

                        var id = record["id"]?.ToString() ?? string.Empty;
                        var updateData = request.UpdateData ?? record;
                        // Remove id from update data
                        var cleanUpdateData = updateData.Where(kvp => kvp.Key != "id").ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        
                        await UpdateAsync(collection, id, cleanUpdateData, ct);
                        itemResult.Success = true;
                        itemResult.Id = id;
                        succeeded++;
                    }
                    else if (action == "delete")
                    {
                        if (!record.ContainsKey("id"))
                        {
                            throw new ArgumentException("Record must contain an 'id' field for delete operations");
                        }

                        var id = record["id"]?.ToString() ?? string.Empty;
                        await DeleteAsync(collection, id, ct);
                        itemResult.Success = true;
                        itemResult.Id = id;
                        succeeded++;
                    }
                }
                catch (Exception ex)
                {
                    itemResult.Success = false;
                    itemResult.Error = ex.Message;
                    failed++;
                }

                results.Add(itemResult);
            }

            return new BulkResult
            {
                Success = succeeded > 0,
                Succeeded = succeeded,
                Failed = failed,
                Results = results
            };
        }
    }

    public async Task<SummaryResult> GetSummaryAsync(string collection, string field, CancellationToken ct = default)
    {
        await Task.Yield();
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(field))
        {
            throw new ArgumentException("Field name cannot be empty", nameof(field));
        }

        var csvPath = GetCsvPath(collection);
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        ct.ThrowIfCancellationRequested();
        var handler = new CsvFileHandler(csvPath);
        var allRecords = handler.ReadRecords();

        // Group by field value and count
        var counts = allRecords
            .Where(r => r.ContainsKey(field))
            .GroupBy(r => r[field]?.ToString() ?? "null")
            .ToDictionary(g => g.Key, g => g.Count());

        return new SummaryResult { Counts = counts };
    }

    public async Task<AggregateResult> AggregateAsync(string collection, AggregateRequest request, CancellationToken ct = default)
    {
        await Task.Yield();
        ct.ThrowIfCancellationRequested();

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Aggregates == null || request.Aggregates.Count == 0)
        {
            throw new ArgumentException("At least one aggregate function must be specified", nameof(request));
        }

        var csvPath = GetCsvPath(collection);
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Collection '{collection}' not found at {csvPath}");
        }

        ct.ThrowIfCancellationRequested();
        var handler = new CsvFileHandler(csvPath);
        var allRecords = handler.ReadRecords();

        // Convert to Record objects for filtering
        var records = allRecords.Select((dict, index) => new Record
        {
            Id = dict.ContainsKey("id") ? dict["id"].ToString() ?? index.ToString() : index.ToString(),
            Data = dict
        }).ToList();

        // Apply filter if provided
        if (request.Filter != null && request.Filter.Count > 0)
        {
            if (_filterEvaluator != null)
            {
                records = records.Where(record => _filterEvaluator.Evaluate(record, request.Filter)).ToList();
            }
            else
            {
                records = FilterRecords(records, request.Filter);
            }
        }

        ct.ThrowIfCancellationRequested();

        // Group by fields if specified
        IEnumerable<IGrouping<string, Record>> groupedRecords;
        if (request.GroupBy != null && request.GroupBy.Length > 0)
        {
            // Multi-level grouping: create composite key
            groupedRecords = records.GroupBy(r =>
            {
                var keys = request.GroupBy.Select(field =>
                {
                    if (r.Data.ContainsKey(field))
                    {
                        return r.Data[field]?.ToString() ?? "null";
                    }
                    return "null";
                });
                return string.Join("|", keys);
            });
        }
        else
        {
            // No grouping: treat all records as one group
            groupedRecords = records.GroupBy(r => "all");
        }

        // Apply aggregate functions to each group
        var result = new AggregateResult();
        foreach (var group in groupedRecords)
        {
            ct.ThrowIfCancellationRequested();
            var groupResult = new Dictionary<string, object>();

            // Add group-by field values
            if (request.GroupBy != null && request.GroupBy.Length > 0)
            {
                var firstRecord = group.First();
                var groupKeys = group.Key.Split('|');
                for (int i = 0; i < request.GroupBy.Length; i++)
                {
                    groupResult[request.GroupBy[i]] = groupKeys[i];
                }
            }

            // Apply aggregate functions
            foreach (var agg in request.Aggregates)
            {
                var field = agg.Field;
                var function = agg.Function.ToLower();
                var alias = string.IsNullOrWhiteSpace(agg.Alias) ? $"{field}_{function}" : agg.Alias;

                object? value = null;
                var fieldValues = group
                    .Where(r => r.Data.ContainsKey(field) && r.Data[field] != null)
                    .Select(r => r.Data[field])
                    .ToList();

                switch (function)
                {
                    case "count":
                        value = group.Count();
                        break;

                    case "sum":
                        value = fieldValues
                            .Select(v => ConvertToNumeric(v))
                            .Where(v => v.HasValue)
                            .Sum(v => v!.Value);
                        break;

                    case "avg":
                        var numericValues = fieldValues
                            .Select(v => ConvertToNumeric(v))
                            .Where(v => v.HasValue)
                            .Select(v => v!.Value)
                            .ToList();
                        value = numericValues.Count > 0 ? numericValues.Average() : 0;
                        break;

                    case "min":
                        var minValues = fieldValues
                            .Select(v => ConvertToNumeric(v))
                            .Where(v => v.HasValue)
                            .Select(v => v!.Value)
                            .ToList();
                        value = minValues.Count > 0 ? minValues.Min() : null;
                        break;

                    case "max":
                        var maxValues = fieldValues
                            .Select(v => ConvertToNumeric(v))
                            .Where(v => v.HasValue)
                            .Select(v => v!.Value)
                            .ToList();
                        value = maxValues.Count > 0 ? maxValues.Max() : null;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported aggregate function: {function}");
                }

                groupResult[alias] = value ?? 0;
            }

            result.Data.Add(groupResult);
        }

        return result;
    }

    private double? ConvertToNumeric(object? value)
    {
        if (value == null) return null;

        return value switch
        {
            int i => i,
            long l => l,
            short s => s,
            double d => d,
            float f => f,
            decimal dec => (double)dec,
            string str => double.TryParse(str, out var parsed) ? parsed : null,
            _ => null
        };
    }
}

