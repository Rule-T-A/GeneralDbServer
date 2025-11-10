using System.Net;
using System.Text.Json;
using DataAbstractionAPI.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataAbstractionAPI.API.Tests.Integration;

/// <summary>
/// Integration tests for DTO serialization and JSON property names.
/// These tests verify that DTOs are correctly serialized with proper JSON property names.
/// </summary>
public class DtoIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _tempTestDir;

    public DtoIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _tempTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestDir);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace IDataAdapter with one using temp directory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DataAbstractionAPI.Core.Interfaces.IDataAdapter));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<DataAbstractionAPI.Core.Interfaces.IDataAdapter>(
                    new DataAbstractionAPI.Adapters.Csv.CsvAdapter(_tempTestDir));
            });
        });

        _client = _factory.CreateClient();
        
        // Create test collection
        var csvPath = Path.Combine(_tempTestDir, "test.csv");
        File.WriteAllText(csvPath, "id,name,email\n1,Alice,alice@example.com\n2,Bob,bob@example.com");
    }

    public void Dispose()
    {
        _client?.Dispose();
        if (Directory.Exists(_tempTestDir))
        {
            try
            {
                Directory.Delete(_tempTestDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task GetCollection_ReturnsJsonWithCorrectPropertyNames()
    {
        // Act
        var response = await _client.GetAsync("/api/data/test");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Verify compact property names
        Assert.True(root.TryGetProperty("d", out var dataArray)); // Compact "d" for data
        Assert.True(root.TryGetProperty("t", out var total)); // Compact "t" for total
        Assert.True(root.TryGetProperty("more", out var more));
        
        Assert.True(dataArray.ValueKind == JsonValueKind.Array);
        Assert.True(total.GetInt32() >= 0);
    }

    [Fact]
    public async Task GetRecord_ReturnsJsonWithCorrectPropertyNames()
    {
        // Arrange - Get first record ID
        var listResponse = await _client.GetAsync("/api/data/test");
        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listDoc = JsonDocument.Parse(listJson);
        var firstRecord = listDoc.RootElement.GetProperty("d")[0];
        var recordId = firstRecord.GetProperty("id").GetString();

        // Act
        var response = await _client.GetAsync($"/api/data/test/{recordId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Verify property names
        Assert.True(root.TryGetProperty("id", out var id));
        Assert.True(root.TryGetProperty("d", out var data)); // Compact "d" for data
        Assert.Equal(recordId, id.GetString());
    }

    [Fact]
    public async Task CreateRecord_ReturnsJsonWithCorrectPropertyNames()
    {
        // Arrange
        var newRecord = new Dictionary<string, object>
        {
            { "name", "Charlie" },
            { "email", "charlie@example.com" }
        };
        var json = JsonSerializer.Serialize(newRecord);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/test", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        // Verify property names
        Assert.True(root.TryGetProperty("id", out var id));
        Assert.True(root.TryGetProperty("d", out var record)); // Compact "d" for record
        Assert.NotNull(id.GetString());
    }

    [Fact]
    public async Task UpdateRecord_ReturnsJsonWithCorrectPropertyNames()
    {
        // Arrange - Get first record ID
        var listResponse = await _client.GetAsync("/api/data/test");
        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listDoc = JsonDocument.Parse(listJson);
        var firstRecord = listDoc.RootElement.GetProperty("d")[0];
        var recordId = firstRecord.GetProperty("id").GetString();

        var updates = new Dictionary<string, object> { { "name", "Updated Name" } };
        var json = JsonSerializer.Serialize(updates);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/data/test/{recordId}", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        // Verify property names
        Assert.True(root.TryGetProperty("success", out var success));
        Assert.True(root.TryGetProperty("d", out var updatedFields)); // Compact "d" for updated fields
        Assert.True(success.GetBoolean());
    }

    [Fact]
    public async Task DeleteRecord_ReturnsJsonWithCorrectPropertyNames()
    {
        // Arrange - Get first record ID
        var listResponse = await _client.GetAsync("/api/data/test");
        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listDoc = JsonDocument.Parse(listJson);
        var firstRecord = listDoc.RootElement.GetProperty("d")[0];
        var recordId = firstRecord.GetProperty("id").GetString();

        // Act
        var response = await _client.DeleteAsync($"/api/data/test/{recordId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Verify property names
        Assert.True(root.TryGetProperty("success", out var success));
        Assert.True(root.TryGetProperty("id", out var id));
        Assert.True(success.GetBoolean());
        Assert.Equal(recordId, id.GetString());
    }

    [Fact]
    public async Task GetSchema_ReturnsJsonWithCorrectPropertyNames()
    {
        // Act
        var response = await _client.GetAsync("/api/data/test/schema");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Verify property names
        Assert.True(root.TryGetProperty("name", out var name));
        Assert.True(root.TryGetProperty("fields", out var fields));
        Assert.Equal("test", name.GetString());
        Assert.True(fields.ValueKind == JsonValueKind.Array);
        
        // Verify field property names
        if (fields.GetArrayLength() > 0)
        {
            var firstField = fields[0];
            Assert.True(firstField.TryGetProperty("name", out _));
            Assert.True(firstField.TryGetProperty("type", out _));
            Assert.True(firstField.TryGetProperty("nullable", out _));
        }
    }
}

