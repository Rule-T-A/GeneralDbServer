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
}

