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
        using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);
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
        
        using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);
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
        var appendMode = File.Exists(_filePath) && new FileInfo(_filePath).Length > 0;

        if (!appendMode)
        {
            // Write headers first
            using var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer1 = new StreamWriter(fileStream);
            using var csv1 = new CsvWriter(writer1, CultureInfo.InvariantCulture);
            
            foreach (var key in record.Keys)
            {
                csv1.WriteField(key);
            }
            csv1.NextRecord();
            
            // Write the record itself
            foreach (var value in record.Values)
            {
                csv1.WriteField(value?.ToString() ?? string.Empty);
            }
            csv1.NextRecord();
        }
        else
        {
            // Read existing headers to maintain field order
            var headers = ReadHeaders();
            var orderedValues = new List<string>();
            
            foreach (var header in headers)
            {
                var value = record.ContainsKey(header) ? record[header]?.ToString() ?? string.Empty : string.Empty;
                orderedValues.Add(value);
            }
            
            // Append record maintaining header order
            using var fileStream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer2 = new StreamWriter(fileStream);
            using var csv2 = new CsvWriter(writer2, CultureInfo.InvariantCulture);
            
            foreach (var value in orderedValues)
            {
                csv2.WriteField(value);
            }
            csv2.NextRecord();
        }
    }
}

