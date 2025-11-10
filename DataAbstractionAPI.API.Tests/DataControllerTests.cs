namespace DataAbstractionAPI.API.Tests;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DataAbstractionAPI.API.Controllers;
using DataAbstractionAPI.Core.Interfaces;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Adapters.Csv;
using DataAbstractionAPI.API.Models.DTOs;
using Xunit;
using Moq;
using Record = DataAbstractionAPI.Core.Models.Record;

public class DataControllerTests : IDisposable
{
    private readonly string _tempTestDir;
    private readonly IDataAdapter _adapter;
    private readonly DataController _controller;

    public DataControllerTests()
    {
        _tempTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestDir);
        
        // Copy test CSV to temp directory
        var testDataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "testdata");
        File.Copy(
            Path.Combine(testDataDir, "users.csv"),
            Path.Combine(_tempTestDir, "users.csv"),
            true
        );

        _adapter = new CsvAdapter(_tempTestDir);
        
        // Create mock IWebHostEnvironment
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(e => e.ContentRootPath).Returns(AppContext.BaseDirectory);
        
        _controller = new DataController(_adapter, mockEnvironment.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempTestDir))
        {
            Directory.Delete(_tempTestDir, true);
        }
    }

    [Fact]
    public async Task DataController_GetCollection_Returns200_WithListResult()
    {
        // Act
        var result = await _controller.GetCollection("users", 100);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var listResponse = Assert.IsType<ListResponseDto>(okObjectResult.Value);
        
        Assert.NotNull(listResponse);
        Assert.True(listResponse.Data.Count > 0);
        Assert.True(listResponse.Total > 0);
    }

    [Fact]
    public async Task DataController_GetCollection_WithInvalidCollection_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _controller.GetCollection("nonexistent", 100)
        );
    }

    [Fact]
    public async Task DataController_GetCollection_RespectsLimitParameter()
    {
        // Act
        var result = await _controller.GetCollection("users", 2);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var listResponse = Assert.IsType<ListResponseDto>(okObjectResult.Value);
        
        Assert.True(listResponse.Data.Count <= 2);
    }

    [Fact]
    public async Task DataController_GetRecord_Returns200_WithRecord()
    {
        // Arrange - Get first record ID
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var recordId = listResult.Data.First().Id;

        // Act
        var result = await _controller.GetRecord("users", recordId);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var recordDto = Assert.IsType<RecordDto>(okObjectResult.Value);
        
        Assert.NotNull(recordDto);
        Assert.Equal(recordId, recordDto.Id);
        Assert.NotNull(recordDto.Data);
    }

    [Fact]
    public async Task DataController_GetRecord_WithInvalidId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _controller.GetRecord("users", "nonexistent-id")
        );
    }

    [Fact]
    public async Task DataController_CreateRecord_Returns201_WithLocationHeader()
    {
        // Arrange
        var newRecord = new Dictionary<string, object>
        {
            { "name", "Test User" },
            { "email", "test@example.com" },
            { "age", "30" }
        };

        // Act
        var result = await _controller.CreateRecord("users", newRecord);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.NotNull(createdAtActionResult.RouteValues);
        Assert.Equal("users", createdAtActionResult.RouteValues["collection"]);
        
        var createResponse = Assert.IsType<CreateResponseDto>(createdAtActionResult.Value);
        Assert.NotNull(createResponse.Id);
        Assert.NotNull(createResponse.Record);
    }

    [Fact]
    public async Task DataController_CreateRecord_WithInvalidCollection_ThrowsException()
    {
        // Arrange
        var newRecord = new Dictionary<string, object>
        {
            { "name", "Test User" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _controller.CreateRecord("nonexistent", newRecord)
        );
    }

    [Fact]
    public async Task DataController_UpdateRecord_Returns200_WithUpdateResponse()
    {
        // Arrange - Get existing record
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var recordId = listResult.Data.First().Id;
        
        var updates = new Dictionary<string, object>
        {
            { "name", "Updated Name" }
        };

        // Act
        var result = await _controller.UpdateRecord("users", recordId, updates);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var updateResponse = Assert.IsType<UpdateResponseDto>(okObjectResult.Value);
        
        Assert.Equal(200, okObjectResult.StatusCode);
        Assert.True(updateResponse.Success);
        Assert.True(updateResponse.UpdatedFields.ContainsKey("name"));
        Assert.Equal("Updated Name", updateResponse.UpdatedFields["name"].ToString());
        
        // Verify update was applied
        var updatedRecord = await _adapter.GetAsync("users", recordId);
        Assert.Equal("Updated Name", updatedRecord.Data["name"].ToString());
    }

    [Fact]
    public async Task DataController_UpdateRecord_WithInvalidId_ThrowsException()
    {
        // Arrange
        var updates = new Dictionary<string, object>
        {
            { "name", "Updated Name" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.UpdateRecord("users", "nonexistent-id", updates)
        );
    }

    [Fact]
    public async Task DataController_DeleteRecord_Returns200_WithDeleteResponse()
    {
        // Arrange - Get existing record
        var listResult = await _adapter.ListAsync("users", new QueryOptions { Limit = 1 });
        var recordId = listResult.Data.First().Id;

        // Act
        var result = await _controller.DeleteRecord("users", recordId);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var deleteResponse = Assert.IsType<DeleteResponseDto>(okObjectResult.Value);
        
        Assert.Equal(200, okObjectResult.StatusCode);
        Assert.True(deleteResponse.Success);
        Assert.Equal(recordId, deleteResponse.Id);
        
        // Verify record was deleted
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _adapter.GetAsync("users", recordId)
        );
    }

    [Fact]
    public async Task DataController_DeleteRecord_WithInvalidId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.DeleteRecord("users", "nonexistent-id")
        );
    }

    [Fact]
    public async Task DataController_GetSchema_Returns200_WithSchema()
    {
        // Act
        var result = await _controller.GetSchema("users");

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var schemaResponse = Assert.IsType<SchemaResponseDto>(okObjectResult.Value);
        
        Assert.NotNull(schemaResponse);
        Assert.Equal("users", schemaResponse.Name);
        Assert.True(schemaResponse.Fields.Count > 0);
    }

    [Fact]
    public async Task DataController_GetSchema_WithInvalidCollection_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _controller.GetSchema("nonexistent")
        );
    }

    [Fact]
    public async Task DataController_ListCollections_Returns200_WithArray()
    {
        // Act
        var result = await _controller.ListCollections();

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var collections = Assert.IsType<string[]>(okObjectResult.Value);
        
        Assert.NotNull(collections);
        Assert.Contains("users", collections);
    }

    [Fact]
    public async Task DataController_ListCollections_ReturnsEmptyArray_WhenNoCollections()
    {
        // Arrange - Create adapter with empty directory
        var emptyDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(emptyDir);
        var emptyAdapter = new CsvAdapter(emptyDir);
        
        // Create mock IWebHostEnvironment for empty controller
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(e => e.ContentRootPath).Returns(AppContext.BaseDirectory);
        
        var emptyController = new DataController(emptyAdapter, mockEnvironment.Object);

        try
        {
            // Act
            var result = await emptyController.ListCollections();

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var collections = Assert.IsType<string[]>(okObjectResult.Value);
            
            Assert.NotNull(collections);
            Assert.Empty(collections);
        }
        finally
        {
            Directory.Delete(emptyDir, true);
        }
    }
}

