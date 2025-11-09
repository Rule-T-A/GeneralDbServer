using System.Net;
using System.Text.Json;
using DataAbstractionAPI.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DataAbstractionAPI.API.Tests.Integration;

/// <summary>
/// Integration tests for error handling via HTTP requests.
/// These tests verify that the GlobalExceptionHandlerMiddleware correctly handles exceptions
/// and returns appropriate HTTP status codes and error responses.
/// </summary>
public class ErrorHandlingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _tempTestDir;

    public ErrorHandlingIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task ErrorHandling_GetCollection_WithInvalidCollection_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/data/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal(404, errorResponse.StatusCode);
        Assert.Contains("not found", errorResponse.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ErrorHandling_GetRecord_WithInvalidId_Returns404()
    {
        // Arrange - Create a collection first
        var csvPath = Path.Combine(_tempTestDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,name\n1,Test");

        // Act
        var response = await _client.GetAsync("/api/data/test/nonexistent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal(404, errorResponse.StatusCode);
    }

    [Fact]
    public async Task ErrorHandling_UpdateRecord_WithInvalidId_Returns404()
    {
        // Arrange - Create a collection first
        var csvPath = Path.Combine(_tempTestDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,name\n1,Test");

        var updateData = new Dictionary<string, object> { { "name", "Updated" } };
        var json = JsonSerializer.Serialize(updateData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/data/test/nonexistent-id", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandling_DeleteRecord_WithInvalidId_Returns404()
    {
        // Arrange - Create a collection first
        var csvPath = Path.Combine(_tempTestDir, "test.csv");
        await File.WriteAllTextAsync(csvPath, "id,name\n1,Test");

        // Act
        var response = await _client.DeleteAsync("/api/data/test/nonexistent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandling_GetSchema_WithInvalidCollection_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/data/nonexistent/schema");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandling_InvalidJson_Returns400()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/test", content);

        // Assert
        // Should return 400 Bad Request for invalid JSON
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.UnsupportedMediaType);
    }
}

