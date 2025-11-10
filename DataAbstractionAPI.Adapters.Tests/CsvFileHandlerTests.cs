namespace DataAbstractionAPI.Adapters.Tests;

using DataAbstractionAPI.Adapters.Csv;
using Xunit;

public class CsvFileHandlerTests
{
    private readonly string _testDataDir;

    public CsvFileHandlerTests()
    {
        _testDataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "testdata");
    }

    [Fact]
    public void CsvFileHandler_ReadsHeaders_FromCsvFile()
    {
        // Arrange
        var csvPath = Path.Combine(_testDataDir, "users.csv");
        var handler = new CsvFileHandler(csvPath);

        // Act
        var headers = handler.ReadHeaders();

        // Assert
        Assert.NotNull(headers);
        Assert.Equal(5, headers.Length);
        Assert.Equal("id", headers[0]);
        Assert.Equal("name", headers[1]);
        Assert.Equal("email", headers[2]);
        Assert.Equal("age", headers[3]);
        Assert.Equal("active", headers[4]);
    }

    [Fact]
    public void CsvFileHandler_ReadsRecords_AsDictionary()
    {
        // Arrange
        var csvPath = Path.Combine(_testDataDir, "users.csv");
        var handler = new CsvFileHandler(csvPath);

        // Act
        var records = handler.ReadRecords();

        // Assert
        Assert.NotNull(records);
        Assert.Equal(3, records.Count);
        
        // Check first record
        Assert.Equal("1", records[0]["id"]);
        Assert.Equal("Alice Johnson", records[0]["name"]);
        Assert.Equal("alice@example.com", records[0]["email"]);
        Assert.Equal("30", records[0]["age"]);
        Assert.Equal("true", records[0]["active"]);
        
        // Check second record
        Assert.Equal("2", records[1]["id"]);
        Assert.Equal("Bob Smith", records[1]["name"]);
    }

    [Fact]
    public void CsvFileHandler_HandlesEmptyFile_Gracefully()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var emptyCsv = Path.Combine(tempDir, "empty.csv");
        File.WriteAllText(emptyCsv, "header1,header2\n"); // Just headers, no data
        
        var handler = new CsvFileHandler(emptyCsv);

        // Act
        var records = handler.ReadRecords();

        // Assert
        Assert.NotNull(records);
        Assert.Empty(records);
        
        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_HandlesMissingFile_ThrowsException()
    {
        // Arrange
        var nonexistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nonexistent.csv");
        
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new CsvFileHandler(nonexistentPath));
    }

    // ============================================
    // Task 1.2.1: AppendRecord Edge Cases
    // ============================================

    [Fact]
    public void CsvFileHandler_AppendRecord_ToEmptyFile_WritesHeaders()
    {
        // Arrange - Create empty file (exists but has no content)
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.Create(csvPath).Close(); // Create empty file
        
        var handler = new CsvFileHandler(csvPath);
        var record = new Dictionary<string, object>
        {
            { "id", "1" },
            { "name", "Test User" },
            { "email", "test@example.com" }
        };

        // Act
        handler.AppendRecord(record);

        // Assert - Headers should be written
        var content = File.ReadAllText(csvPath);
        Assert.Contains("id,name,email", content);
        Assert.Contains("1,Test User,test@example.com", content);
        
        // Verify headers can be read
        var headers = handler.ReadHeaders();
        Assert.Equal(3, headers.Length);
        Assert.Equal("id", headers[0]);
        Assert.Equal("name", headers[1]);
        Assert.Equal("email", headers[2]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_AppendRecord_ToExistingFile_AppendsCorrectly()
    {
        // Arrange - Create file with existing data
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email\n1,Alice,alice@example.com\n");
        
        var handler = new CsvFileHandler(csvPath);
        var newRecord = new Dictionary<string, object>
        {
            { "id", "2" },
            { "name", "Bob" },
            { "email", "bob@example.com" }
        };

        // Act
        handler.AppendRecord(newRecord);

        // Assert - New record should be appended
        var records = handler.ReadRecords();
        Assert.Equal(2, records.Count);
        Assert.Equal("1", records[0]["id"]);
        Assert.Equal("Alice", records[0]["name"]);
        Assert.Equal("2", records[1]["id"]);
        Assert.Equal("Bob", records[1]["name"]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_AppendRecord_WithNewFields_HandlesGracefully()
    {
        // Arrange - Create file with existing headers
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "id,name\n1,Alice\n");
        
        var handler = new CsvFileHandler(csvPath);
        // Record has fields not in existing headers (email, age)
        var newRecord = new Dictionary<string, object>
        {
            { "id", "2" },
            { "name", "Bob" },
            { "email", "bob@example.com" }, // New field
            { "age", "25" } // New field
        };

        // Act
        handler.AppendRecord(newRecord);

        // Assert - Should only append fields that match existing headers
        var records = handler.ReadRecords();
        Assert.Equal(2, records.Count);
        Assert.Equal("2", records[1]["id"]);
        Assert.Equal("Bob", records[1]["name"]);
        // New fields should not be in the record (only existing headers are used)
        Assert.False(records[1].ContainsKey("email"));
        Assert.False(records[1].ContainsKey("age"));

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_AppendRecord_WithNullValues_ConvertsToEmpty()
    {
        // Arrange - Create empty file
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.Create(csvPath).Close();
        
        var handler = new CsvFileHandler(csvPath);
        var record = new Dictionary<string, object>
        {
            { "id", "1" },
            { "name", (object?)null! }, // Null value
            { "email", "test@example.com" }
        };

        // Act
        handler.AppendRecord(record);

        // Assert - Null values should be converted to empty strings
        var records = handler.ReadRecords();
        Assert.Single(records);
        Assert.Equal("1", records[0]["id"]);
        Assert.Equal("", records[0]["name"]); // Null converted to empty string
        Assert.Equal("test@example.com", records[0]["email"]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_AppendRecord_MaintainsHeaderOrder()
    {
        // Arrange - Create file with headers in specific order
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email,age\n1,Alice,alice@example.com,30\n");
        
        var handler = new CsvFileHandler(csvPath);
        // Record with fields in different order
        var newRecord = new Dictionary<string, object>
        {
            { "age", "25" },
            { "name", "Bob" },
            { "id", "2" },
            { "email", "bob@example.com" }
        };

        // Act
        handler.AppendRecord(newRecord);

        // Assert - Should maintain header order (id, name, email, age)
        var records = handler.ReadRecords();
        Assert.Equal(2, records.Count);
        var secondRecord = records[1];
        var keys = secondRecord.Keys.ToArray();
        Assert.Equal("id", keys[0]);
        Assert.Equal("name", keys[1]);
        Assert.Equal("email", keys[2]);
        Assert.Equal("age", keys[3]);
        
        // Values should match regardless of input order
        Assert.Equal("2", secondRecord["id"]);
        Assert.Equal("Bob", secondRecord["name"]);
        Assert.Equal("bob@example.com", secondRecord["email"]);
        Assert.Equal("25", secondRecord["age"]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_AppendRecord_WithMissingFields_PadsWithEmpty()
    {
        // Arrange - Create file with multiple headers
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email,age\n1,Alice,alice@example.com,30\n");
        
        var handler = new CsvFileHandler(csvPath);
        // Record missing some fields (missing "email" and "age")
        var newRecord = new Dictionary<string, object>
        {
            { "id", "2" },
            { "name", "Bob" }
        };

        // Act
        handler.AppendRecord(newRecord);

        // Assert - Missing fields should be padded with empty strings
        var records = handler.ReadRecords();
        Assert.Equal(2, records.Count);
        var secondRecord = records[1];
        Assert.Equal("2", secondRecord["id"]);
        Assert.Equal("Bob", secondRecord["name"]);
        Assert.Equal("", secondRecord["email"]); // Missing field padded with empty
        Assert.Equal("", secondRecord["age"]); // Missing field padded with empty

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    // ============================================
    // Task 1.2.2: ReadHeaders Edge Cases
    // ============================================

    [Fact]
    public void CsvFileHandler_ReadHeaders_FromEmptyFile_ReturnsEmpty()
    {
        // Arrange - Create empty file (no content at all)
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.Create(csvPath).Close(); // Empty file
        
        var handler = new CsvFileHandler(csvPath);

        // Act
        var headers = handler.ReadHeaders();

        // Assert - Should return empty array
        Assert.NotNull(headers);
        Assert.Empty(headers);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_ReadHeaders_WithOnlyHeaders_ReturnsHeaders()
    {
        // Arrange - File with headers but no data rows
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email\n"); // Only headers, no data
        
        var handler = new CsvFileHandler(csvPath);

        // Act
        var headers = handler.ReadHeaders();

        // Assert - Should return headers
        Assert.NotNull(headers);
        Assert.Equal(3, headers.Length);
        Assert.Equal("id", headers[0]);
        Assert.Equal("name", headers[1]);
        Assert.Equal("email", headers[2]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_ReadHeaders_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange - Headers with special characters (quotes, spaces)
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "\"id\",\"first name\",\"email address\"\n1,Alice,alice@example.com\n");
        
        var handler = new CsvFileHandler(csvPath);

        // Act
        var headers = handler.ReadHeaders();

        // Assert - Should handle quoted headers correctly
        Assert.NotNull(headers);
        Assert.Equal(3, headers.Length);
        // CsvHelper should unquote the headers
        Assert.Equal("id", headers[0]);
        Assert.Equal("first name", headers[1]);
        Assert.Equal("email address", headers[2]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    // ============================================
    // Task 1.2.3: ReadRecords Edge Cases
    // ============================================

    [Fact]
    public void CsvFileHandler_ReadRecords_WithMissingValues_HandlesGracefully()
    {
        // Arrange - CSV with empty cells
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email\n1,,alice@example.com\n2,Bob,\n");
        
        var handler = new CsvFileHandler(csvPath);

        // Act
        var records = handler.ReadRecords();

        // Assert - Empty cells should be empty strings
        Assert.Equal(2, records.Count);
        Assert.Equal("1", records[0]["id"]);
        Assert.Equal("", records[0]["name"]); // Missing value
        Assert.Equal("alice@example.com", records[0]["email"]);
        
        Assert.Equal("2", records[1]["id"]);
        Assert.Equal("Bob", records[1]["name"]);
        Assert.Equal("", records[1]["email"]); // Missing value

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_ReadRecords_WithExtraColumns_IgnoresExtra()
    {
        // Arrange - CSV with more columns in row than headers
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "id,name\n1,Alice,extra1,extra2\n");
        
        var handler = new CsvFileHandler(csvPath);

        // Act
        var records = handler.ReadRecords();

        // Assert - Should only read columns matching headers
        Assert.Single(records);
        Assert.Equal("1", records[0]["id"]);
        Assert.Equal("Alice", records[0]["name"]);
        // Extra columns should be ignored
        Assert.False(records[0].ContainsKey("extra1"));
        Assert.False(records[0].ContainsKey("extra2"));

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_ReadRecords_WithFewerColumns_ThrowsException()
    {
        // Arrange - CSV with fewer columns than headers
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email,age\n1,Alice\n");
        
        var handler = new CsvFileHandler(csvPath);

        // Act & Assert - Should throw MissingFieldException when fields are missing
        Assert.Throws<CsvHelper.MissingFieldException>(() => handler.ReadRecords());

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_ReadRecords_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange - CSV with special characters (quotes, commas, newlines)
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        // Use CsvHelper to write properly escaped CSV
        using (var fileStream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        using (var writer = new StreamWriter(fileStream))
        using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        {
            csv.WriteField("id");
            csv.WriteField("name");
            csv.WriteField("description");
            csv.NextRecord();
            
            csv.WriteField("1");
            csv.WriteField("Alice");
            csv.WriteField("Has \"quotes\" and, commas");
            csv.NextRecord();
        }
        
        var handler = new CsvFileHandler(csvPath);

        // Act
        var records = handler.ReadRecords();

        // Assert - Special characters should be handled correctly
        Assert.Single(records);
        Assert.Equal("1", records[0]["id"]);
        Assert.Equal("Alice", records[0]["name"]);
        Assert.Equal("Has \"quotes\" and, commas", records[0]["description"]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void CsvFileHandler_ReadRecords_WithNewlinesInFields_HandlesCorrectly()
    {
        // Arrange - CSV with newlines in field values
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "test.csv");
        // Use CsvHelper to write properly escaped CSV with newlines
        using (var fileStream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        using (var writer = new StreamWriter(fileStream))
        using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        {
            csv.WriteField("id");
            csv.WriteField("name");
            csv.WriteField("description");
            csv.NextRecord();
            
            csv.WriteField("1");
            csv.WriteField("Alice");
            csv.WriteField("Line 1\nLine 2\nLine 3");
            csv.NextRecord();
        }
        
        var handler = new CsvFileHandler(csvPath);

        // Act
        var records = handler.ReadRecords();

        // Assert - Newlines in fields should be preserved
        Assert.Single(records);
        Assert.Equal("1", records[0]["id"]);
        Assert.Equal("Alice", records[0]["name"]);
        Assert.Contains("Line 1", records[0]["description"]?.ToString() ?? "");
        Assert.Contains("Line 2", records[0]["description"]?.ToString() ?? "");

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}

