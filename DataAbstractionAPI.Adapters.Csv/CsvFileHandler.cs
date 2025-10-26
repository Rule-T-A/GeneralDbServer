namespace DataAbstractionAPI.Adapters.Csv;

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

/// <summary>
/// Handles reading CSV files using CsvHelper library.
/// </summary>
public class CsvFileHandler
{
    private readonly string _filePath;

    public CsvFileHandler(string filePath)
    {
        _filePath = filePath;
        
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"CSV file not found: {filePath}");
        }
    }

    /// <summary>
    /// Reads the header row from the CSV file.
    /// </summary>
    /// <returns>Array of header names</returns>
    public string[] ReadHeaders()
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        if (csv.Read())
        {
            csv.ReadHeader();
            return csv.HeaderRecord ?? Array.Empty<string>();
        }
        
        return Array.Empty<string>();
    }

    /// <summary>
    /// Reads all records from the CSV file as dictionaries.
    /// </summary>
    /// <returns>List of dictionaries where keys are column names and values are cell values</returns>
    public List<Dictionary<string, object>> ReadRecords()
    {
        var records = new List<Dictionary<string, object>>();
        
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        // Read header
        if (csv.Read())
        {
            csv.ReadHeader();
            var headers = csv.HeaderRecord;
            
            if (headers != null)
            {
                // Read data rows
                while (csv.Read())
                {
                    var record = new Dictionary<string, object>();
                    
                    foreach (var header in headers)
                    {
                        var value = csv.GetField<string>(header) ?? string.Empty;
                        record[header] = value;
                    }
                    
                    records.Add(record);
                }
            }
        }
        
        return records;
    }

    /// <summary>
    /// Appends a record to the CSV file.
    /// </summary>
    /// <param name="record">Record to append as dictionary</param>
    public void AppendRecord(Dictionary<string, object> record)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = File.Exists(_filePath) && new FileInfo(_filePath).Length > 0
        };

        var appendMode = File.Exists(_filePath) && new FileInfo(_filePath).Length > 0;

        using var writer = new StreamWriter(_filePath, appendMode);
        using var csv = new CsvWriter(writer, config);

        if (!appendMode)
        {
            // Write headers
            foreach (var key in record.Keys)
            {
                csv.WriteField(key);
            }
            csv.NextRecord();
        }

        // Write record
        foreach (var value in record.Values)
        {
            csv.WriteField(value?.ToString() ?? string.Empty);
        }
        csv.NextRecord();
    }
}

